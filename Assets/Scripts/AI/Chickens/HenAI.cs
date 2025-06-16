// ChickenAI.cs

using System;
using AI.Base;
using Creatures.Chickens.Hens.Components;
using Interactions.Objects.Nests;
using Managers;
using Mirror;
using Services;
using UnityEngine;
using UnityEngine.AI;
using Utils;
using Random = UnityEngine.Random;

namespace AI.Chickens
{
    public enum ChickenState
    {
        Idle,
        Wander,
        SeekNest,
        LayEgg,
        Incubate
    }
 
    public class HenAI : BaseAI
    {
        [Header("References")]
        [SerializeField]
        private HenEntity entity;

        [Space] 
        public float wanderRadius = 3f;
        public Color wanderColor = Color.blue;
        public float nestSearchRadius = 15f;
        public Color nestSearchColor = Color.green;
        public float layEggDistance = 1f;
        public float incubateYOffset = 0.5f;
        public float unfertilizedLayInterval = 60f;
        public LayerMask nestLayer;
        public bool IsPregnant => entity.Reproduction.IsPregnant;
        public ChickenState CurrentState { get; private set; } = ChickenState.Wander;

        private Vector3 _wanderDestination;
        private bool _hasWanderDestination;
        private float _wanderTimer;
        private const float WanderInterval = 2f;

        private float _unfertilizedTimer;
        private bool _layUnfertilized;

        private const int NestOverlapMax = 30;
        private readonly Collider[] _overlapNestsBuffer = new Collider[NestOverlapMax];

        private BreedingService _breedingService; 
        private Nest _targetNest; 
 
        private Action<Nest> _onNestEggHatchedHandler;

        private void Start()
        {
            if (!isServer)
            {
                enabled = false;
                return;
            } 
            
            if (entity == null)
            {
                Debug.LogError($"[ChickenAI:{name}] Missing RoosterEntity!", this);
                enabled = false;
                return;
            }

            CurrentState = ChickenState.Wander;

            _breedingService = GameManager.Instance.BreedingService;
            if (_breedingService == null)
                Debug.LogError($"[ChickenAI:{name}] BreedingManager not found!", this);

            _unfertilizedTimer = unfertilizedLayInterval;
            _layUnfertilized = false;
        }
        protected override void StateTransition()
        {
            switch (CurrentState)
            {
                case ChickenState.Wander:
                    if (entity.Reproduction.IsPregnant || _layUnfertilized)
                    {
                        CurrentState = ChickenState.SeekNest;
                        _hasWanderDestination = false;
                        agent.ResetPath();
                    }
                    break;

                case ChickenState.SeekNest:
                    if (!entity.Reproduction.IsPregnant && !_layUnfertilized)
                    {
                        CurrentState = ChickenState.Wander;
                        agent.ResetPath();
                    }
                    break;

                case ChickenState.LayEgg:
                case ChickenState.Incubate:
                    break;

                case ChickenState.Idle:
                    CurrentState = ChickenState.Wander;
                    agent.ResetPath(); 
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
                    agent.ResetPath();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #region State Behaviors

        private void DoWander()
        {
            if (!entity.Reproduction.IsPregnant)
            {
                _unfertilizedTimer -= Time.deltaTime;
                if (_unfertilizedTimer <= 0f)
                {
                    _layUnfertilized = true;
                }
            }

            _wanderTimer -= Time.deltaTime;
            if (!_hasWanderDestination || _wanderTimer <= 0f)
            {
                var randomDir = Random.insideUnitSphere * wanderRadius + transform.position;
                if (NavMesh.SamplePosition(randomDir, out var hit, wanderRadius, NavMesh.AllAreas))
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
                    agent.ResetPath();
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
                    if (!nestComp || nestComp.CurrentHen) continue;

                    var d = Vector3.Distance(transform.position, col.transform.position);
                    if (!(d < bestDist)) continue;
                    bestDist = d;
                    bestNest = nestComp;
                }

                if (bestNest)
                {
                    bestNest.Assign(entity.netId);
                    _targetNest = bestNest;
                    entity.Reproduction.AssignNest(bestNest.netId);
                }
                else
                {
                    CurrentState = ChickenState.Wander;
                    agent.ResetPath();
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
                agent.ResetPath();
            }
            else
            {
                entity.Reproduction.AssignNest(0);
                _targetNest = null;
                CurrentState = ChickenState.Wander;
                agent.ResetPath();
                _hasWanderDestination = false;
            }
        }

        private void DoLayEgg()
        {
            if (!_targetNest)
            {
                Debug.LogWarning($"[ChickenAI:{name}] Cannot lay egg: targetNest is null.");
                CurrentState = ChickenState.Wander;
                agent.ResetPath();
                return;
            }

            bool fertilized = entity.Reproduction.IsPregnant && entity.Reproduction.PregnantBy;
            Egg newEgg;

            if (fertilized)
            {
                newEgg = _breedingService.SpawnEggAndAssignToNest(
                    entity,
                    entity.Reproduction.PregnantBy,
                    _targetNest
                );
                entity.Reproduction.UnmarkPregnant();
            }
            else
            {
                newEgg = _breedingService.SpawnUnfertilizedEgg(entity, _targetNest);
                _layUnfertilized = false;
                _unfertilizedTimer = unfertilizedLayInterval;
            }

            if (!newEgg)
            {
                Debug.LogError($"[ChickenAI:{name}] Something went wrong: newEgg was null.");
                CurrentState = ChickenState.Wander;
                agent.ResetPath();
                _targetNest = null;
                return;
            }

            if (fertilized)
            {
                Debug.Log($"[ChickenAI:{name}] Egg laid (netId={newEgg.netId}). Transitioning to Incubate.");
                _onNestEggHatchedHandler ??= OnNestEggHatched;
                _targetNest.OnEggHatched += _onNestEggHatchedHandler;
                newEgg.StartIncubation();
                CurrentState = ChickenState.Incubate;
            }
            else
            {
                Debug.Log($"[ChickenAI:{name}] Unfertilized egg laid (netId={newEgg.netId}). Returning to Wander.");
                entity.Reproduction.ClearNestReferences();
                _targetNest = null;
                CurrentState = ChickenState.Wander;
                agent.ResetPath();
            }
        }

        private void DoIncubate()
        { 
            if (!_targetNest || !_targetNest.CurrentEgg)
            {
                CurrentState = ChickenState.Wander;
                agent.ResetPath();
                return;
            }
 
            var dist = Vector3.Distance(transform.position, _targetNest.transform.position);
            if (dist > layEggDistance)
            {
                MoveTo(_targetNest.transform.position);
                return;
            }
 
            entity.Reproduction.SitOnEgg();
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
            agent.ResetPath();
        }
 
        [Server]
        public void ForceSetNestAndIncubate(Nest nest)
        {
            if (!nest)
                return;

            _targetNest = nest;
            entity.Reproduction.AssignNest(nest.netId);
 
            if (nest.CurrentEgg)
            {
                CurrentState = ChickenState.Incubate;
                agent.ResetPath();
 
                _onNestEggHatchedHandler ??= OnNestEggHatched;
                nest.OnEggHatched += _onNestEggHatchedHandler;
 
                entity.Reproduction.SitOnEgg();
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
                agent.ResetPath();
                _hasWanderDestination = false;
            }
        }

        [Server]
        public void ForceSetPregnantSeekNest()
        {
            CurrentState = ChickenState.SeekNest;
            _hasWanderDestination = false;
            _targetNest = null;
            agent.ResetPath(); 
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
