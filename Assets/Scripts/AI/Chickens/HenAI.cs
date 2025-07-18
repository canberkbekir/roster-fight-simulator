using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Mirror;
using AI.Base;
using Creatures.Chickens.Hens.Components;
using Managers;
using Services;

namespace AI.Chickens
{
    public enum HenAIState
    {
        Wander,     
        SearchNest, 
        MoveToNest, 
        LayEgg,     
        Incubate    
    }

    [RequireComponent(typeof(NavMeshAgent))]
    public class HenAI : BaseAI
    {
        [Header("References")]
        [SerializeField] private HenEntity entity;
        public HenEntity Entity => entity;

        [SyncVar] private HenAIState _currentState = HenAIState.Wander;
        public HenAIState CurrentState => _currentState;

        [Header("Settings")]
        [SerializeField] private float wanderRadius      = 3f;
        [SerializeField] private Color wanderColor       = Color.blue;
        [Space]
        [SerializeField] private float nestSearchRadius  = 15f;
        [SerializeField] private Color nestSearchColor   = Color.green;
        [Space]
        [SerializeField] private float layEggDistance    = 1f;
        [SerializeField] private float incubateYOffset   = 0.5f;

        private ReproductionService _reproductionService;

        private void Start()
        {
            if (!isServer || !entity)
            {
                enabled = false;
                return;
            }

            _currentState = HenAIState.Wander;
            _reproductionService = GameManager.Instance.ReproductionService;
            if (!_reproductionService)
                Debug.LogError("[HenAI] ReproductionService not found!", this);
        }

        protected override void StateTransition()
        {
            switch (_currentState)
            {
                case HenAIState.Wander:
                    if (IsHenAssignedToNest())
                    {
                        _currentState = (IsThereAnyFertilizedEggInNest() || IsHenReadyToLayEgg())
                            ? HenAIState.MoveToNest
                            : HenAIState.Wander;
                    }
                    else
                    {
                        _currentState = IsThereAnyEmptyNest()
                            ? HenAIState.SearchNest
                            : HenAIState.Wander;
                    }
                    break;

                case HenAIState.SearchNest:
                    _currentState = IsThereAnyEmptyNest()
                        ? HenAIState.MoveToNest
                        : HenAIState.Wander;
                    if (_currentState == HenAIState.MoveToNest)
                        AssignHenToNest();
                    break;

                case HenAIState.MoveToNest:
                    if (HasReached(entity.HenNestHandler.AssignedNest.transform.position) || IsHenReadyToLayEgg())
                        _currentState = HenAIState.LayEgg;
                    break;

                case HenAIState.LayEgg:
                    _currentState = IsThereAnyFertilizedEggInNest()
                        ? HenAIState.Incubate
                        : HenAIState.Wander;
                    break;

                case HenAIState.Incubate:
                    // stay incubating until external event fires
                    break;
            }
        }

        protected override void StateTick()
        {
            switch (_currentState)
            {
                case HenAIState.Wander:
                    WanderTick();
                    break;
                case HenAIState.SearchNest:
                    SearchNestTick();
                    break;
                case HenAIState.MoveToNest:
                    MoveToNestTick();
                    break;
                case HenAIState.LayEgg:
                    LayEggTick();
                    break;
                case HenAIState.Incubate:
                    IncubateTick();
                    break;
            }
        }

        #region Tick Methods

        private void WanderTick()
        {
            var randomPoint = agent.transform.position + Random.insideUnitSphere * wanderRadius;
            if (HasReached(randomPoint))
                MoveTo(randomPoint);
        }

        private void SearchNestTick()
        { 
            AssignHenToNest();
            var nest = entity.HenNestHandler.AssignedNest;
            if (nest)
                MoveTo(nest.transform.position);
        }

        private void MoveToNestTick()
        {
            if (entity.HenEggHandler.CanLayEgg())
            {
                var nest = entity.HenNestHandler.AssignedNest;
                if (nest)
                    MoveTo(nest.transform.position);
            }
        }

        private void LayEggTick()
        {
            if (entity.HenEggHandler.CanLayEgg())
            { 
                LayEgg();
                StopMoving();
            }
            else if (IsHenReadyToLayEgg())
            { 
                StopMoving();
            } 
        }

        private void IncubateTick()
        {
            Incubate();
        }

        #endregion

        #region Helpers

        public bool IsHenAssignedToNest()
            => entity.HenNestHandler.AssignedNest != null;

        public bool IsThereAnyFertilizedEggInNest()
            => entity.HenNestHandler.AssignedNest?.CurrentEggEntities
                   .Any(e => e.IsFertilized) ?? false;

        public bool IsHenReadyToLayEgg()
            => entity.HenEggHandler.IsEggReady;

        public bool IsThereAnyEmptyNest()
            => entity.Breeder?.Nests.Any(n => !n.IsOccupied) ?? false;

        public void AssignHenToNest()
        {
            var nest = entity.Breeder.Nests.FirstOrDefault(n => !n.IsOccupied);
            if (nest != null)
                entity.HenNestHandler.AssignHenToNest(nest);
            else
                Debug.LogWarning("[HenAI] No empty nests to assign!", this);
        }

        public void LayEgg()
            => entity.HenEggHandler.LayEggOnNest();

        public void Incubate()
            => entity.HenNestHandler.Incubate();

        public void StopIncubating()
            => entity.HenNestHandler.StopIncubating();

        #endregion

        //───────────────────────────────────────────────────────────────────────────
        // Debug Gizmos
        //───────────────────────────────────────────────────────────────────────────
        private void OnDrawGizmosSelected()
        {
            // Wander radius
            Gizmos.color = wanderColor;
            Gizmos.DrawWireSphere(agent.transform.position, wanderRadius);

            // Nest-search radius
            Gizmos.color = nestSearchColor;
            Gizmos.DrawWireSphere(agent.transform.position, nestSearchRadius);

            // Lay-egg distance
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(agent.transform.position, layEggDistance);

            // If assigned to a nest, draw a marker and line to it
            if (entity == null || entity.HenNestHandler.AssignedNest == null) return;
            var nestPos = entity.HenNestHandler.AssignedNest.transform.position;
            var incubatePos = nestPos + Vector3.up * incubateYOffset;

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(incubatePos, 0.2f);
            Gizmos.DrawLine(agent.transform.position, incubatePos);
        }
    }
}
