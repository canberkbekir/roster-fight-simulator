// ChickenAI.cs
using System;
using AI.Base;
using Creatures.Eggs;
using Creatures.Roosters.Components;
using Interactions.Objects.Nests;
using Managers;
using Mirror;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;
using Utils;
using Random = UnityEngine.Random;

namespace AI.Roosters
{
    public enum ChickenState
    {
        Idle,
        Wander,
        SeekNest,
        LayEgg,
        Incubate
    }

    [RequireComponent(typeof(RoosterEntity))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class ChickenAI : BaseAI
    {
        private RoosterEntity _entity;

        [FoldoutGroup("Chicken AI Settings")]
        [FoldoutGroup("Chicken AI Settings/Wander"), LabelText("Wander Radius"), PropertyRange(0.5f, 10f)]
        [Tooltip("Max distance for random wandering.")]
        public float wanderRadius = 3f;

        [FoldoutGroup("Chicken AI Settings/Wander"), LabelText("Wander Color")]
        public Color wanderColor = Color.blue;

        [FoldoutGroup("Chicken AI Settings/Nest Search"), LabelText("Nest Search Radius"), PropertyRange(5f, 50f)]
        [Tooltip("Radius to look for a free nest.")]
        public float nestSearchRadius = 15f;

        [FoldoutGroup("Chicken AI Settings/Nest Search"), LabelText("Nest Search Color")]
        public Color nestSearchColor = Color.green;

        [FoldoutGroup("Chicken AI Settings/Egg"), LabelText("Lay Egg Distance"), PropertyRange(0.2f, 3f)]
        [Tooltip("Distance threshold for laying an egg.")]
        public float layEggDistance = 1f;

        [FoldoutGroup("Chicken AI Settings/Egg"), LabelText("Incubate Y-Offset"), PropertyRange(0f, 1f)]
        [Tooltip("Vertical offset when sitting on the egg.")]
        public float incubateYOffset = 0.5f;

        [FoldoutGroup("Chicken AI Settings/Layers"), LabelText("Nest Layer Mask")]
        [Tooltip("LayerMask used to identify nest GameObjects.")]
        public LayerMask nestLayer;

        [FoldoutGroup("Chicken AI Settings/Debug"), Sirenix.OdinInspector.ReadOnly, LabelText("Is Pregnant")]
        [Tooltip("True if this chicken is carrying genes to lay an egg.")]
        public bool IsPregnant => _entity.Reproduction.IsPregnant;
        public ChickenState CurrentState { get; private set; } = ChickenState.Wander;

        private Vector3 _wanderDestination;
        private bool _hasWanderDestination;
        private float _wanderTimer;
        private const float WanderInterval = 2f;

        private const int NestOverlapMax = 30;
        private readonly Collider[] _overlapNestsBuffer = new Collider[NestOverlapMax];

        private BreedingManager _breedingManager; 
        private Nest _targetNest; 
 
        private Action<Nest> _onNestEggHatchedHandler;

        private void Start()
        {
            if (!isServer)
            {
                enabled = false;
                return;
            }

            _entity = GetComponent<RoosterEntity>();
            if (_entity == null)
            {
                Debug.LogError($"[ChickenAI:{name}] Missing RoosterEntity!", this);
                enabled = false;
                return;
            }

            CurrentState = ChickenState.Wander; 

            _breedingManager = GameManager.Instance.BreedingManager;
            if (_breedingManager == null)
                Debug.LogError($"[ChickenAI:{name}] BreedingManager not found!", this);
        } 
        protected override void StateTransition()
        {
            switch (CurrentState)
            {
                case ChickenState.Wander:
                    if (_entity.Reproduction.IsPregnant)
                    {
                        CurrentState = ChickenState.SeekNest;
                        _hasWanderDestination = false;
                        Agent.ResetPath(); 
                    }
                    break;

                case ChickenState.SeekNest: 
                    if (!_entity.Reproduction.IsPregnant)
                    {
                        CurrentState = ChickenState.Wander;
                        Agent.ResetPath();  
                    }
                    break;

                case ChickenState.LayEgg:
                case ChickenState.Incubate:
                    break;

                case ChickenState.Idle:
                    CurrentState = ChickenState.Wander;
                    Agent.ResetPath(); 
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override void StateTick()
        {
            switch (CurrentState)
            {
                case ChickenState.Wander:
                    DoWander();
                    break;
                case ChickenState.SeekNest:
                    DoSeekNest();
                    break;
                case ChickenState.LayEgg:
                    DoLayEgg();
                    break;
                case ChickenState.Incubate:
                    DoIncubate();
                    break;
                case ChickenState.Idle:
                    Agent.ResetPath();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #region State Behaviors

        private void DoWander()
        {
            _wanderTimer -= Time.deltaTime;
            if (!_hasWanderDestination || _wanderTimer <= 0f)
            {
                var randomDir = Random.insideUnitSphere * wanderRadius + transform.position;
                if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
                {
                    _wanderDestination = hit.position;
                    _hasWanderDestination = true;
                    _wanderTimer = WanderInterval;
                    MoveTo(_wanderDestination);
                }
                else
                {
                    _hasWanderDestination = false;
                    _wanderTimer = 0.1f;
                    Agent.ResetPath();
                }
            }
            else if (HasReached(_wanderDestination))
            {
                _hasWanderDestination = false;
            }
        }

        private void DoSeekNest()
        {
            if (!_targetNest)
            {
                var count = Physics.OverlapSphereNonAlloc(transform.position, nestSearchRadius, _overlapNestsBuffer);
                var bestDist = float.MaxValue;
                Nest bestNest = null;

                for (var i = 0; i < count; i++)
                {
                    var col = _overlapNestsBuffer[i];
                    if (!nestLayer.Contains(col.gameObject.layer)) continue;

                    var nestComp = col.GetComponent<Nest>();
                    if (!nestComp || nestComp.CurrentChicken) continue;

                    var d = Vector3.Distance(transform.position, col.transform.position);
                    if (!(d < bestDist)) continue;
                    bestDist = d;
                    bestNest = nestComp;
                }

                if (bestNest)
                {
                    bestNest.Assign(_entity.netId);
                    _targetNest = bestNest;
                    _entity.Reproduction.AssignNest(bestNest.netId);
                }
                else
                {
                    CurrentState = ChickenState.Wander;
                    Agent.ResetPath();
                    _hasWanderDestination = false;
                    return;
                }
            }

            if (_targetNest && NetworkServer.spawned.TryGetValue(_targetNest.netId, out var nestObj))
            {
                var nestPos = nestObj.transform.position;
                MoveTo(nestPos);

                if (!(Vector3.Distance(transform.position, nestPos) <= layEggDistance)) return;
                CurrentState = ChickenState.LayEgg;
                Agent.ResetPath();
            }
            else
            {
                _entity.Reproduction.AssignNest(0);
                _targetNest = null;
                CurrentState = ChickenState.Wander;
                Agent.ResetPath();
                _hasWanderDestination = false;
            }
        }

          private void DoLayEgg()
        { 
            if (!_entity.Reproduction.IsPregnant || !_entity.Reproduction.PregnantBy)
            {
                Debug.LogWarning($"[ChickenAI:{name}] Cannot lay egg: not pregnant or no father.");
                CurrentState = ChickenState.Wander;
                Agent.ResetPath();
                return;
            }
 
            if (!_targetNest)
            {
                Debug.LogWarning($"[ChickenAI:{name}] Cannot lay egg: targetNest is null.");
                CurrentState = ChickenState.Wander;
                Agent.ResetPath();
                return;
            }
 
            var newEgg = _breedingManager.SpawnEggAndAssignToNest(
                _entity, 
                _entity.Reproduction.PregnantBy, 
                _targetNest
            );
 
            _entity.Reproduction.UnmarkPregnant();
            Debug.Log($"[ChickenAI:{name}] Egg laid (netId={newEgg?.netId ?? 0}). Transitioning to Incubate.");
 
            _onNestEggHatchedHandler ??= OnNestEggHatched;
            _targetNest.OnEggHatched += _onNestEggHatchedHandler;
 
            if (newEgg)
            {
                newEgg.StartIncubation();
                CurrentState = ChickenState.Incubate;
            }
            else
            { 
                Debug.LogError($"[ChickenAI:{name}] Something went wrong: newEgg was null.");
                CurrentState = ChickenState.Wander;
                Agent.ResetPath();
            }
        }

        private void DoIncubate()
        { 
            if (!_targetNest || !_targetNest.CurrentEgg)
            {
                CurrentState = ChickenState.Wander;
                Agent.ResetPath();
                return;
            }
 
            var dist = Vector3.Distance(transform.position, _targetNest.transform.position);
            if (dist > layEggDistance)
            {
                MoveTo(_targetNest.transform.position);
                return;
            }
 
            _entity.Reproduction.SitOnEgg();
            transform.position = _targetNest.transform.position + new Vector3(0f, incubateYOffset, 0f);
        }

        #endregion 
        
        [Server]
        private void OnNestEggHatched(Nest nest)
        {
            if (_onNestEggHatchedHandler != null && _targetNest)
                _targetNest.OnEggHatched -= _onNestEggHatchedHandler;

            _targetNest = null;  
            CurrentState = ChickenState.Wander;
            Agent.ResetPath();
        }
 
        [Server]
        public void ForceSetNestAndIncubate(Nest nest)
        {
            if (!nest)
                return;

            _targetNest = nest;
            _entity.Reproduction.AssignNest(nest.netId);
 
            if (nest.CurrentEgg)
            {
                CurrentState = ChickenState.Incubate;
                Agent.ResetPath();
 
                _onNestEggHatchedHandler ??= OnNestEggHatched;
                nest.OnEggHatched += _onNestEggHatchedHandler;
 
                _entity.Reproduction.SitOnEgg();
                transform.position = nest.transform.position + new Vector3(0f, incubateYOffset, 0f);
 
                var eggComponent = nest.CurrentEgg;
                if (eggComponent && !eggComponent.isIncubating)
                {
                    eggComponent.StartIncubation();
                }
            }
            else
            { 
                CurrentState = ChickenState.Wander;
                Agent.ResetPath();
                _hasWanderDestination = false;
            }
        }

        [Server]
        public void ForceSetPregnantSeekNest()
        {
            CurrentState = ChickenState.SeekNest;
            _hasWanderDestination = false;
            _targetNest = null;
            Agent.ResetPath(); 
        }
         
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = nestSearchColor;
            Gizmos.DrawWireSphere(transform.position, nestSearchRadius);
            Gizmos.color = wanderColor;
            Gizmos.DrawWireSphere(transform.position, wanderRadius);

            if (_targetNest == null || !NetworkServer.spawned.TryGetValue(_targetNest.netId, out var nestObj)) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, nestObj.transform.position);
        }
    }
}
