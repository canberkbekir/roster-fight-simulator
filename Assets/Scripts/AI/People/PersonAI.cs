using System;
using AI.Base;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace AI.People
{
    /// <summary>
    /// Simple FSM for an NPC “Person”:
    /// - Idle for a random interval
    /// - Wander to a random point
    /// - Repeat
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class PersonAI : BaseAI
    {
        private enum PersonState
        {
            Idle,
            Wander
        }

        [Header("Person AI Settings")]
        [SerializeField] private float wanderRadius = 8f;
        [SerializeField] private float idleTimeMin = 2f;
        [SerializeField] private float idleTimeMax = 5f;

        private PersonState _currentState = PersonState.Idle;
        private float _idleTimer = 0f;

        private Vector3 _wanderDestination;
        private bool _hasWanderDestination = false;

        private void Start()
        {
            if (!isServer) return;
            _currentState = PersonState.Idle;
            ResetIdleTimer();
        }

        protected override void StateTransition()
        {
            switch (_currentState)
            {
                case PersonState.Idle:
                    if (_idleTimer <= 0f)
                    {
                        _currentState = PersonState.Wander;
                        _hasWanderDestination = false;
                    }
                    break;

                case PersonState.Wander:
                    if (_hasWanderDestination && HasReached(_wanderDestination))
                    {
                        _currentState = PersonState.Idle;
                        ResetIdleTimer();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override void StateTick()
        {
            switch (_currentState)
            {
                case PersonState.Idle:
                    _idleTimer -= Time.deltaTime;
                    StopMoving();
                    break;

                case PersonState.Wander:
                    WanderBehavior();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ResetIdleTimer()
        {
            _idleTimer = Random.Range(idleTimeMin, idleTimeMax);
        }

        private void WanderBehavior()
        {
            // If we don't have a destination, pick one
            if (!_hasWanderDestination)
            {
                var randomDir = Random.insideUnitSphere * wanderRadius;
                randomDir += transform.position;
                if (NavMesh.SamplePosition(randomDir, out var hit, wanderRadius, NavMesh.AllAreas))
                {
                    _wanderDestination = hit.position;
                    _hasWanderDestination = true;
                    MoveTo(_wanderDestination);
                }
                else
                {
                    // Failed to find point; just idle and retry next tick
                    _hasWanderDestination = false;
                    ResetIdleTimer();
                    _currentState = PersonState.Idle;
                }
            }
            else
            {
                if (!HasReached(_wanderDestination)) return;
                _currentState = PersonState.Idle;
                ResetIdleTimer();
            }
        }
    }
}
