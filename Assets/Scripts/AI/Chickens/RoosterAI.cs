﻿using System;
using AI.Base;
using Creatures.Chickens.Base;
using Creatures.Chickens.Hens.Components;
using Creatures.Chickens.Roosters.Components;
using Mirror;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using Utils;
using Random = UnityEngine.Random;

namespace AI.Chickens
{
    public enum RoosterState
    {
        Idle,
        Wander,
        SeekMate,
        Breed
    } 
    public class RoosterAI : BaseAI
    {
        [FormerlySerializedAs("_entity")] [SerializeField]
        private RoosterEntity entity; 
        [Header("Rooster AI Settings")]
        [SerializeField] private float wanderRadius = 5f;
        [SerializeField] private Color wanderColor = Color.white;
        [Space]
        [SerializeField] private float mateSearchRadius = 10f;
        [SerializeField] private Color mateSearchColor = Color.red;
        [Space]
        [SerializeField] private float breedingDistance = 2f;
        [SerializeField] private Color breedingDistanceColor = Color.yellow;
        [Space]
        [SerializeField] private LayerMask chickenLayer;

        #region Wander Helpers

        private Vector3 _wanderDestination;
        private bool _hasWanderDestination = false;
        private float _wanderTimer = 0f;
        private const float NewWanderInterval = 2f;

        #endregion
        
        private RoosterState _currentState = RoosterState.Wander;
        private ChickenEntity _targetChicken;

        // Buffer for non-alloc overlap checks
        private const int MaxChickenOverlap = 30;
        private readonly Collider[] _chickenOverlapBuffer = new Collider[MaxChickenOverlap];

        public RoosterState CurrentState => _currentState;

        private void Start()
        {
            if (!isServer)
            {
                enabled = false;
                return;
            } 
            if (!entity)
            {
                Debug.LogError($"RoosterAI: No RoosterEntity on {name}");
                enabled = false;
                return;
            }

            _currentState = RoosterState.Wander;
        }

        protected override void StateTransition()
        {
            switch (_currentState)
            {
                case RoosterState.Wander:
                    TryFindMate();
                    break;

                case RoosterState.SeekMate:
                    EvaluateSeekMate();
                    break;

                case RoosterState.Breed: 
                    _currentState = RoosterState.Wander;
                    break;

                case RoosterState.Idle:
                    _currentState = RoosterState.Wander;
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected override void StateTick()
        {
            switch (_currentState)
            {
                case RoosterState.Wander:
                    HandleWander();
                    break;

                case RoosterState.SeekMate:
                    HandleSeekMate();
                    break;

                case RoosterState.Breed:
                    HandleBreed();
                    break;

                case RoosterState.Idle:
                    StopMoving();
                    break;
            }
        }

        #region State Handlers

        private void HandleWander()
        {
            _wanderTimer -= Time.deltaTime;
            if (!_hasWanderDestination || _wanderTimer <= 0f)
            {
                var randomDir = Random.insideUnitSphere * wanderRadius + transform.position;
                if (NavMesh.SamplePosition(randomDir, out var hit, wanderRadius, NavMesh.AllAreas))
                {
                    _wanderDestination = hit.position;
                    _hasWanderDestination = true;
                    _wanderTimer = NewWanderInterval;
                    MoveTo(_wanderDestination);
                }
                else
                {
                    _hasWanderDestination = false;
                    _wanderTimer = 0.1f;
                    StopMoving();
                }
            }
            else if (HasReached(_wanderDestination))
            {
                _hasWanderDestination = false;
            }
        }

        private void HandleSeekMate()
        {
            if (_targetChicken)
                MoveTo(_targetChicken.transform.position);
        }

        private void HandleBreed()
        {
            if (_targetChicken)
            {
                var chickenRepro = _targetChicken.Reproduction;
                var selfRepro = entity.Reproduction;
                if (chickenRepro && selfRepro)
                {
                    selfRepro.TryBreedWith(chickenRepro);
                }
            }

            _targetChicken = null;
        }

        #endregion

        #region State Transitions

        private void TryFindMate()
        { 
            var count = Physics.OverlapSphereNonAlloc(transform.position, mateSearchRadius, _chickenOverlapBuffer);
            for (var i = 0; i < count; i++)
            {
                var col = _chickenOverlapBuffer[i];
                if (!chickenLayer.Contains(col.gameObject.layer))
                    continue;

                var chickenEnt = col.GetComponent<HenEntity>();
                if (!chickenEnt)
                    continue;

                var chickenRepro = chickenEnt.Reproduction;
                if (!chickenRepro || chickenRepro.IsPregnant)
                    continue;
                
                var chickenAI = chickenEnt.ChickenAI as HenAI;
                if (!chickenAI || chickenAI.Entity.CurrentState != HenState.Wander)
                    continue; 

                _targetChicken = chickenEnt;
                _currentState = RoosterState.SeekMate;
                return;
            }
        }

        private void EvaluateSeekMate()
        {
            if (!_targetChicken)
            {
                _currentState = RoosterState.Wander;
                return;
            }

            var chickenRepro = _targetChicken.Reproduction;
            if (!chickenRepro || chickenRepro.IsPregnant)
            {
                _targetChicken = null;
                _currentState = RoosterState.Wander;
                return;
            }

            float dist = Vector3.Distance(transform.position, _targetChicken.transform.position);
            if (dist <= breedingDistance)
                _currentState = RoosterState.Breed;
        }

        #endregion

        [Server]
        public void ForceToWander()
        {
            _currentState = RoosterState.Wander;
            StopMoving();
            Debug.Log($"[RoosterAI:{name}] Forced → Wander.");
        }

        private void OnDrawGizmosSelected()
        {
            // Wander radius
            Gizmos.color = wanderColor;
            Gizmos.DrawWireSphere(transform.position, wanderRadius);

            // Mate search radius
            Gizmos.color = mateSearchColor;
            Gizmos.DrawWireSphere(transform.position, mateSearchRadius);

            // Breeding distance
            Gizmos.color = breedingDistanceColor;
            Gizmos.DrawWireSphere(transform.position, breedingDistance);
        }
    }
}
