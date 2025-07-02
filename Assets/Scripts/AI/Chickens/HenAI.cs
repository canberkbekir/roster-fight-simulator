using System;
using AI.Base;
using Creatures.Chickens.Hens.Components;
using Interactions.Objects.Nests;
using Managers;
using Mirror;
using Services;
using UnityEngine; 

namespace AI.Chickens
{
    public enum ChickenState
    {
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
        public LayerMask nestLayer;
        public bool IsPregnant => entity.Reproduction.IsPregnant;
        public ChickenState CurrentState { get; private set; } = ChickenState.Wander;

        private Vector3 _wanderDestination;
        private bool _hasWanderDestination;
        private float _wanderTimer;
        private const float WanderInterval = 2f;

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
            
            if (!entity)
            {
                Debug.LogError($"[ChickenAI:{name}] Missing RoosterEntity!", this);
                enabled = false;
                return;
            }

            CurrentState = ChickenState.Wander; 

            _breedingService = GameManager.Instance.BreedingService;
            if (!_breedingService)
                Debug.LogError($"[ChickenAI:{name}] BreedingManager not found!", this);
        } 
        protected override void StateTransition()
        {
            switch (CurrentState)
            {
                case ChickenState.Wander:
                    if (entity.Reproduction.IsPregnant)
                    {
                        CurrentState = ChickenState.SeekNest;
                        _hasWanderDestination = false;
                        agent.ResetPath(); 
                    }
                    break;

                case ChickenState.SeekNest: 
                    if (!entity.Reproduction.IsPregnant)
                    {
                        CurrentState = ChickenState.Wander;
                        agent.ResetPath();  
                    }
                    break;

                case ChickenState.LayEgg:
                case ChickenState.Incubate:
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
                    // DoWander();
                    break;
                case ChickenState.SeekNest:
                    // DoSeekNest();
                    break;
                case ChickenState.LayEgg:
                    // DoLayEgg();
                    break;
                case ChickenState.Incubate:
                    // DoIncubate();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        [Server]
        private void OnNestEggHatched(Nest nest)
        {
            if (_onNestEggHatchedHandler != null && _targetNest)
                _targetNest.OnEggHatched -= _onNestEggHatchedHandler;

            _targetNest = null;  
            CurrentState = ChickenState.Wander;
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
