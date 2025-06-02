using System;
using AI.Base;
using Creatures.Roosters;
using Creatures.Roosters.Components;
using UnityEngine;
using UnityEngine.AI;
using Utils;
using Random = UnityEngine.Random;

namespace AI.Roosters
{
    public enum RoosterState
    {
        Idle,
        Wander,
        SeekMate,
        Breed
    }

    [RequireComponent(typeof(RoosterEntity))]
    public class RoosterAI : BaseAI
    {
        private RoosterEntity _entity;

        
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

        private RoosterState _currentState = RoosterState.Wander;
        private RoosterEntity _targetChicken;

        private void Start()
        {
            if (!isServer) return;
            _entity = GetComponent<RoosterEntity>();
            if (_entity == null)
            {
                Debug.LogError($"RoosterAI: No RoosterEntity on {name}");
                enabled = false;
                return;
            }

            // Start in Wander
            _currentState = RoosterState.Wander;
        }

        protected override void StateTransition()
        {
            switch (_currentState)
            {
                case RoosterState.Wander: 
                    var hits = Physics.OverlapSphere(transform.position, mateSearchRadius);
                    foreach (var col in hits)
                    {  
                        if (!chickenLayer.Contains(col.gameObject.layer))
                            continue;
                        
                        var chickenEnt = col.GetComponent<RoosterEntity>();
                        if (!chickenEnt || chickenEnt.Gender != RoosterGender.Female) continue;
                        var chickenAI = chickenEnt.GetComponent<ChickenAI>();
                        if (!chickenAI || chickenAI.IsPregnant) continue;
                        _targetChicken = chickenEnt;
                        _currentState = RoosterState.SeekMate;
                        return;
                    } 
                    break;

                case RoosterState.SeekMate:
                    if (!_targetChicken)
                    {
                        _currentState = RoosterState.Wander;
                        return;
                    }

                    var cAi = _targetChicken.GetComponent<ChickenAI>();
                    if (!cAi || cAi.IsPregnant)
                    { 
                        _targetChicken = null;
                        _currentState = RoosterState.Wander;
                        return;
                    }
 
                    var dist = Vector3.Distance(transform.position, _targetChicken.transform.position);
                    if (dist <= breedingDistance)
                    {
                        _currentState = RoosterState.Breed;
                        return;
                    } 
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
                    // Pick a random point (once) and move; when reached or after a while pick a new one
                    WanderBehavior();
                    break;

                case RoosterState.SeekMate:
                    if (_targetChicken != null)
                    {
                        MoveTo(_targetChicken.transform.position);
                    }
                    break;

                case RoosterState.Breed:
                    if (_targetChicken != null)
                    {
                        var chickenAI = _targetChicken.GetComponent<ChickenAI>();
                        if (chickenAI)
                        {
                            chickenAI.BecomePregnant(_entity);
                        }
                    } 
                    _targetChicken = null;
                    break;

                case RoosterState.Idle: 
                    StopMoving();
                    break;
            }
        }

        #region Wander Helper

        private Vector3 _wanderDestination;
        private bool _hasWanderDestination = false;
        private float _wanderTimer = 0f;
        private const float NewWanderInterval = 2f;

        private void WanderBehavior()
        {
            _wanderTimer -= Time.deltaTime;
            if (!_hasWanderDestination || _wanderTimer <= 0f)
            {
                // Pick a new random NavMesh point
                Vector3 randomDir = Random.insideUnitSphere * wanderRadius;
                randomDir += transform.position;
                if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
                {
                    _wanderDestination = hit.position;
                    _hasWanderDestination = true;
                    _wanderTimer = NewWanderInterval;
                    MoveTo(_wanderDestination);
                }
                else
                {
                    // If we failed to find a point, stay put and retry next frame
                    _hasWanderDestination = false;
                    _wanderTimer = 0.1f;
                    StopMoving();
                }
            }
            else
            {
                // Continue moving toward the current wanderDestination
                if (HasReached(_wanderDestination))
                {
                    _hasWanderDestination = false; // pick a new one next tick
                }
            }
        }

        #endregion

        private void OnDrawGizmosSelected()
        {
            // Draw wander radius
            Gizmos.color = wanderColor;
            Gizmos.DrawWireSphere(transform.position, wanderRadius);
            // Draw search radius
            Gizmos.color = mateSearchColor;
            Gizmos.DrawWireSphere(transform.position, mateSearchRadius);
            // Draw breeding distance
            Gizmos.color = breedingDistanceColor;
            Gizmos.DrawWireSphere(transform.position, breedingDistance);
        }
    }
}
