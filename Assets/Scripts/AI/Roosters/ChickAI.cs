using AI.Base;
using Creatures.Roosters.Components;
using UnityEngine;
using UnityEngine.AI;

namespace AI.Roosters
{
    public enum ChickState
    {
        Idle,
        Wander
    }
    
    [RequireComponent(typeof(RoosterEntity))]
    public class ChickAI : BaseAI
    { 

        [Header("Chick AI Settings")]
        [SerializeField] private float wanderRadius = 2f;

        private ChickState _currentState = ChickState.Idle;

        private Vector3 _wanderDestination;
        private bool _hasWanderDestination = false;
        private float _wanderTimer = 0f;
        private const float WanderInterval = 2f;

        public ChickState CurrentState => _currentState;
        private void Start()
        {
            if (!isServer) return;
            _currentState = ChickState.Wander;
        }

        protected override void StateTransition()
        {
            // Always stay in Wander for now
            if (_currentState == ChickState.Idle)
                _currentState = ChickState.Wander;
        }

        protected override void StateTick()
        {
            if (_currentState == ChickState.Wander)
            {
                WanderBehavior();
            }
        }

        private void WanderBehavior()
        {
            _wanderTimer -= Time.deltaTime;
            var randomDir = Random.insideUnitSphere * wanderRadius;
            if (!_hasWanderDestination || _wanderTimer <= 0f)
            {
                randomDir += transform.position;
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
                    StopMoving();
                }
            }
            else
            {
                if (HasReached(_wanderDestination))
                {
                    _hasWanderDestination = false;
                }
            }
        }
    }
}
