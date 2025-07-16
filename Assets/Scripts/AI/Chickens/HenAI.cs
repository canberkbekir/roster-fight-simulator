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

        private ReproductionService _reproductionService;
        private Nest _targetNest; 

        private void Start()
        {
            if (!isServer)
            {
                enabled = false;
                return;
            }

            if (!entity)
            {
                Debug.LogError("[HenAI] Missing HenEntity!", this);
                enabled = false;
                return;
            }

            _reproductionService = GameManager.Instance.ReproductionService;
            if (!_reproductionService)
                Debug.LogError("[HenAI] ReproductionService not found!", this); 
        }

        protected override void StateTransition()
        {
             
        }

        protected override void StateTick()
        {
           
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

        public bool IsThereAnyEggAssignedToHen()
        {
            var eggHandler = entity.HenEggHandler;

            return eggHandler.LayedEggEntity != null;
        }

        public bool IsAssignedEggInNest()
        {
            return false;
        }
    }
}
