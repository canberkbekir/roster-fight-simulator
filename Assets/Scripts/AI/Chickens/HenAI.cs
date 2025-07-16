using System;
using UnityEngine;
using UnityEngine.AI;
using Mirror;
using AI.Base;
using Creatures.Chickens.Hens.Components;
using Interactions.Objects.Nests;
using Managers;
using Services;

namespace AI.Chickens
{
    public enum HenAIState
    {
        Wander,
        CheckEggProgress,
        LayAnywhere,
        SearchNest,
        GoToNest,
        Incubate,
    }

    [RequireComponent(typeof(NavMeshAgent))]
    public class HenAI : BaseAI
    {
        [Header("References")]
        [SerializeField] private HenEntity entity;
        public HenEntity Entity => entity;

        [SyncVar] private HenAIState _currentState = HenAIState.Wander;

        [Header("Settings")]
        [SerializeField] private float wanderRadius = 3f;
        [SerializeField] private Color wanderColor = Color.blue;
        [Space]
        [SerializeField] private float nestSearchRadius = 15f;
        [SerializeField] private Color nestSearchColor = Color.green;
        [Space]
        [SerializeField] private float layEggDistance = 1f;
        [SerializeField] private float incubateYOffset = 0.5f;

        private BreedingService _breedingService;
        private Nest _targetNest;
        private NavMeshAgent _agent;

        private void Start()
        {
            if (!isServer)
            {
                enabled = false;
                return;
            }

            if (entity == null)
            {
                Debug.LogError("[HenAI] Missing HenEntity!", this);
                enabled = false;
                return;
            }

            _breedingService = GameManager.Instance.BreedingService;
            if (_breedingService == null)
                Debug.LogError("[HenAI] BreedingService not found!", this);

            _agent = GetComponent<NavMeshAgent>(); 
            OnEnterState(_currentState);
        }

        protected override void StateTransition()
        {
            var next = _currentState;

            switch (_currentState)
            {
                case HenAIState.Wander:
                    // if (entity.EggProgress > 0f)
                    //     next = HenAIState.CheckEggProgress;
                    // else if (entity.CanLayEgg())
                    //     next = HenAIState.LayAnywhere;
                    break;

                case HenAIState.LayAnywhere:
                    next = HenAIState.CheckEggProgress;
                    break;

                case HenAIState.CheckEggProgress:
                    // if (entity.EggProgress <= 0f)
                    //     next = HenAIState.Wander;
                    // else if (entity.AssignedNest != null)
                    //     next = HenAIState.GoToNest;
                    // else if (entity.IsEggFertilized && FindEmptyNest(out _targetNest))
                    //     next = HenAIState.SearchNest;
                    // else
                    //     next = HenAIState.LayAnywhere;
                    break;

                case HenAIState.SearchNest:
                    if (_targetNest != null)
                        next = HenAIState.GoToNest;
                    break;

                case HenAIState.GoToNest:
                    if (Vector3.Distance(transform.position, _targetNest.transform.position) <= layEggDistance)
                        next = HenAIState.Incubate;
                    break;

                case HenAIState.Incubate:
                    // if (_targetNest.Egg == null || _targetNest.Egg.IsHatched)
                    //     next = HenAIState.Wander;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (next != _currentState)
                ChangeState(next);
        }

        protected override void StateTick()
        {
            switch (_currentState)
            {
                case HenAIState.Wander:
                    HandleWander();
                    break;
                case HenAIState.LayAnywhere:
                    HandleLayAnywhere();
                    break;
                case HenAIState.CheckEggProgress:
                    HandleCheckEggProgress();
                    break;
                case HenAIState.SearchNest:
                    HandleSearchNest();
                    break;
                case HenAIState.GoToNest:
                    HandleGoToNest();
                    break;
                case HenAIState.Incubate:
                    HandleIncubate();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ChangeState(HenAIState newState)
        {
            OnExitState(_currentState);
            _currentState = newState;
            OnEnterState(_currentState);
        }

        private void OnEnterState(HenAIState state)
        {
            switch (state)
            {
                case HenAIState.Wander:
                    PickNewWanderTarget();
                    break;

                case HenAIState.SearchNest:
                    // look for a nest right away
                    FindEmptyNest(out _targetNest);
                    break;

                case HenAIState.GoToNest:
                    if (_targetNest != null)
                        _agent.SetDestination(_targetNest.transform.position + Vector3.up * incubateYOffset);
                    break;

                case HenAIState.Incubate:
                    // entity.StartIncubation(_targetNest);
                    break;
            }
        }

        private void OnExitState(HenAIState state)
        {
            switch (state)
            {
                case HenAIState.Wander:
                    _agent.ResetPath();
                    break;
            }
        }

        // ─── State Logic ───────────────────────────────────────────────────────────

        private void HandleWander()
        {
            // when we get close to our wander target, pick a new one
            if (!_agent.pathPending && _agent.remainingDistance < 0.5f)
                PickNewWanderTarget();
        }

        private void HandleCheckEggProgress()
        {
            // just idle; transition logic will pick the next state
        }

        private void HandleLayAnywhere()
        {
            // entity.LayEggAt(transform.position);
        }

        private void HandleSearchNest()
        {
            if (FindEmptyNest(out _targetNest))
                ChangeState(HenAIState.GoToNest);
        }

        private void HandleGoToNest()
        {
            if (!_agent.hasPath && _targetNest != null)
                _agent.SetDestination(_targetNest.transform.position + Vector3.up * incubateYOffset);
        }

        private void HandleIncubate()
        {
            // incubation kicked off in OnEnterState; you can play an animation here
        }

        // ─── Helpers ────────────────────────────────────────────────────────────────

        private void PickNewWanderTarget()
        {
            var rnd = (Vector3)UnityEngine.Random.insideUnitCircle * wanderRadius + transform.position;
            if (NavMesh.SamplePosition(rnd, out var hit, wanderRadius, NavMesh.AllAreas))
                _agent.SetDestination(hit.position);
        }

        private bool FindEmptyNest(out Nest nest)
        {
            // nest = _breedingService.FindNearestEmpty(transform.position, nestSearchRadius);
            // return nest != null;
            nest = null;
            return false;
        }

        [Server]
        private void OnNestEggHatched(Nest nest)
        {
            if (nest == _targetNest)
                ChangeState(HenAIState.Wander);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = nestSearchColor;
            Gizmos.DrawWireSphere(transform.position, nestSearchRadius);

            Gizmos.color = wanderColor;
            Gizmos.DrawWireSphere(transform.position, wanderRadius);

            if (_targetNest != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, _targetNest.transform.position);
            }
        }
    }
}
