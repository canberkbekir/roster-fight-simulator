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
        public ChickenState CurrentState => _currentState;

        private ChickenState _currentState = ChickenState.Wander;
        private Vector3 _wanderDestination;
        private bool _hasWanderDestination;
        private float _wanderTimer;
        private const float WanderInterval = 2f;

        private const int NestOverlapMax = 30;
        private readonly Collider[] _overlapNestsBuffer = new Collider[NestOverlapMax];

        private BreedingManager _breedingManager;

        private Nest _targetNest;
        private RoosterEntity _pregnantBy;

        // **YENİ: Event callback referansı (abone/çıkarma için saklıyoruz)**
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

            _currentState = ChickenState.Wander;
            Debug.Log($"[ChickenAI:{name}] Initialized in state: {_currentState}");

            _breedingManager = GameManager.Instance.BreedingManager;
            if (_breedingManager == null)
                Debug.LogError($"[ChickenAI:{name}] BreedingManager not found!", this);
        }

        [Server]
        public void BecomePregnant(RoosterEntity father)
        {
            if (_entity.Reproduction.IsPregnant || father == null) return;

            _entity.Reproduction.MarkPregnant(father.netId);
            _pregnantBy = father;
            Debug.Log($"[ChickenAI:{name}] Became pregnant by {father.name}");

            _currentState = ChickenState.SeekNest;
            _hasWanderDestination = false;
            _targetNest = null;
            Agent.ResetPath();
            Debug.Log($"[ChickenAI:{name}] Transition → SeekNest");
        }

        protected override void StateTransition()
        {
            switch (_currentState)
            {
                case ChickenState.Wander:
                    if (_entity.Reproduction.IsPregnant)
                    {
                        _currentState = ChickenState.SeekNest;
                        _hasWanderDestination = false;
                        Agent.ResetPath(); 
                    }
                    break;

                case ChickenState.SeekNest: 
                    if (!_entity.Reproduction.IsPregnant)
                    {
                        _currentState = ChickenState.Wander;
                        Agent.ResetPath();  
                    }
                    break;

                case ChickenState.LayEgg:
                    _currentState = ChickenState.Incubate; 
                    break;

                case ChickenState.Incubate: 
                    break;

                case ChickenState.Idle:
                    _currentState = ChickenState.Wander;
                    Agent.ResetPath(); 
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override void StateTick()
        {
            switch (_currentState)
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
            if (_targetNest == null)
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

                if (bestNest != null)
                {
                    bestNest.Assign(_entity.netId);
                    _targetNest = bestNest;
                    _entity.Reproduction.AssignNest(bestNest.netId);
                }
                else
                {
                    _currentState = ChickenState.Wander;
                    Agent.ResetPath();
                    _hasWanderDestination = false;
                    return;
                }
            }

            if (_targetNest != null && NetworkServer.spawned.TryGetValue(_targetNest.netId, out var nestObj))
            {
                var nestPos = nestObj.transform.position;
                MoveTo(nestPos);

                if (Vector3.Distance(transform.position, nestPos) <= layEggDistance)
                {
                    _currentState = ChickenState.LayEgg;
                    Agent.ResetPath();
                }
            }
            else
            {
                _entity.Reproduction.AssignNest(0);
                _targetNest = null;
                _currentState = ChickenState.Wander;
                Agent.ResetPath();
                _hasWanderDestination = false;
            }
        }

        private void DoLayEgg()
        {
            if (!_entity.Reproduction.IsPregnant || _pregnantBy == null)
            {
                Debug.LogWarning($"[ChickenAI:{name}] Cannot lay egg: not pregnant or no father.");
                _currentState = ChickenState.Wander;
                Agent.ResetPath();
                return;
            }

            if (_targetNest == null || !NetworkServer.spawned.TryGetValue(_targetNest.netId, out var nestObj))
            {
                Debug.LogWarning($"[ChickenAI:{name}] No valid nest to lay egg in.");
                _currentState = ChickenState.Wander;
                Agent.ResetPath();
                return;
            }

            // 1) Egg spawn ve Nest.AssignEgg() akışı
            _breedingManager.RequestBreed(_entity, _pregnantBy, _targetNest);

            // 2) Hamilelik sil, incubate durumuna geç
            _entity.Reproduction.UnmarkPregnant();
            _currentState = ChickenState.Incubate;
            Debug.Log($"[ChickenAI:{name}] Egg laid and transitioning to Incubate state.");

            // **YENİ:** Nest’in OnEggHatched event’ine abone olalım
            if (_onNestEggHatchedHandler == null)
            {
                _onNestEggHatchedHandler = OnNestEggHatched;
            }
            _targetNest.OnEggHatched += _onNestEggHatchedHandler;

            // **YENİ:** Egg referansını al, incubate’ı başlat
            var eggComponent = _targetNest.CurrentEgg;
            if (eggComponent != null)
            {
                eggComponent.StartIncubation();  // Sunucuda isIncubating=true set eder, istemcide VFX tetikler
            }
        }

        private void DoIncubate()
        {
            if (_targetNest == null || !NetworkServer.spawned.TryGetValue(_targetNest.netId, out var nestObj))
            {
                Debug.LogWarning($"[ChickenAI:{name}] No valid nest to incubate egg in.");
                _currentState = ChickenState.Wander;
                Agent.ResetPath();
                return;
            }

            if (Vector3.Distance(transform.position, _targetNest.transform.position) <= layEggDistance)
            {
                _entity.Reproduction.SitOnEgg();
                transform.position = _targetNest.transform.position + new Vector3(0f, incubateYOffset, 0f);
                Debug.Log($"[ChickenAI:{name}] Sitting on egg at {_targetNest.name}");
            }
        }

        #endregion

        /// <summary>
        /// Bu metod, Nest.OnEggHatched event’i tetiklendiğinde çağrılır.
        /// Yuvadaki yumurta hatch olduğunda tavuk tekrar wander’a dönsün.
        /// </summary>
        [Server]
        private void OnNestEggHatched(Nest nest)
        {
            // 1) Aboneliği kaldır:
            if (_onNestEggHatchedHandler != null && _targetNest != null)
            {
                _targetNest.OnEggHatched -= _onNestEggHatchedHandler;
            }

            // 2) Yuva bilgisini sıfırla
            _targetNest = null;
            _entity.Reproduction.AssignNest(0);

            // 3) Tavuk wander durumuna dönsün
            _currentState = ChickenState.Wander;
            Agent.ResetPath();
            _hasWanderDestination = false;

            Debug.Log($"[ChickenAI:{name}] Egg hatched → back to Wander.");
        }

        /// <summary>
        /// Eğer tavuk obje yeniden spawn olduysa, kuluçka sürecine geri bağlamak için kullanılır.
        /// </summary>
        [Server]
        public void ForceSetNestAndIncubate(Nest nest)
        {
            if (nest == null)
                return;

            _targetNest = nest;
            _entity.Reproduction.AssignNest(nest.netId);

            // Eğer hâlâ yuvada bir yumurta varsa incubate durumuna geç
            if (nest.CurrentEgg != null)
            {
                _currentState = ChickenState.Incubate;
                Agent.ResetPath();

                // Event aboneliğini tekrar kur
                if (_onNestEggHatchedHandler == null)
                    _onNestEggHatchedHandler = OnNestEggHatched;
                nest.OnEggHatched += _onNestEggHatchedHandler;

                // Tavuk otomatik olarak yumurtanın üzerine otur
                _entity.Reproduction.SitOnEgg();
                transform.position = nest.transform.position + new Vector3(0f, incubateYOffset, 0f);

                // Sunucuda incubate flag’ini set et (eğer henüz set edilmemişse)
                var eggComponent = nest.CurrentEgg;
                if (eggComponent != null && !eggComponent.isIncubating)
                {
                    eggComponent.StartIncubation();
                }
            }
            else
            {
                // Eğer yuvada yumurta yoksa direkt wander’a geç
                _currentState = ChickenState.Wander;
                Agent.ResetPath();
                _hasWanderDestination = false;
            }
        }

        [Server]
        public void ForceSetPregnantSeekNest()
        {
            _currentState = ChickenState.SeekNest;
            _hasWanderDestination = false;
            _targetNest = null;
            Agent.ResetPath();
            Debug.Log($"[ChickenAI:{name}] Forced → SeekNest (Pregnant).");
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = nestSearchColor;
            Gizmos.DrawWireSphere(transform.position, nestSearchRadius);
            Gizmos.color = wanderColor;
            Gizmos.DrawWireSphere(transform.position, wanderRadius);

            if (_targetNest != null && NetworkServer.spawned.TryGetValue(_targetNest.netId, out var nestObj))
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, nestObj.transform.position);
            }
        }
    }
}
