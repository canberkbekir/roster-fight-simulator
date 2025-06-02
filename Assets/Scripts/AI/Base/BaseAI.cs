using UnityEngine;
using UnityEngine.AI;
using Mirror;
using Sirenix.OdinInspector;

namespace AI.Base
{
    [RequireComponent(typeof(NavMeshAgent))]
    public abstract class BaseAI : NetworkBehaviour
    {
        [FoldoutGroup("AI Settings")]
        [FoldoutGroup("AI Settings/Timing"), LabelText("Tick Interval"), Tooltip("Seconds between each AI state update.")]
        [PropertyRange(0.1f, 5f)]
        [SerializeField]
        private float tickInterval = 1f;

        [FoldoutGroup("AI Settings/Debug"), Sirenix.OdinInspector.ShowInInspector, Sirenix.OdinInspector.ReadOnly]
        [LabelText("NavMesh Agent"), Tooltip("Cached NavMeshAgent component.")]
        protected NavMeshAgent Agent;

        private float _tickTimer;

        protected virtual void Awake()
        {
            Agent = GetComponent<NavMeshAgent>();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            _tickTimer = tickInterval;
        }

        private void Update()
        {
            if (!isServer) return;

            _tickTimer -= Time.deltaTime;
            if (_tickTimer > 0f) return;

            _tickTimer = tickInterval;
            StateTransition();
            StateTick();
        }

        protected abstract void StateTransition();
        protected abstract void StateTick();

        #region Movement Helpers

        /// <summary>
        /// Only call SetDestination if the agent is actually on a NavMesh.
        /// </summary>
        protected void MoveTo(Vector3 position)
        {
            if (!Agent || !Agent.isOnNavMesh) return;
            Agent.isStopped = false;
            Agent.SetDestination(position);
        }

        /// <summary>
        /// Only check remainingDistance if the agent is on a NavMesh.
        /// </summary>
        protected bool HasReached(Vector3 position, float threshold = 0.5f)
        {
            if (!Agent || !Agent.isOnNavMesh) return false;
            if (Agent.pathPending) return false;
            return Agent.remainingDistance <= threshold;
        }

        /// <summary>
        /// Only call ResetPath if the agent is on a NavMesh.
        /// </summary>
        protected void StopMoving()
        {
            if (!Agent || !Agent.isOnNavMesh) return;
            Agent.ResetPath();
        }

        #endregion
    }
}
