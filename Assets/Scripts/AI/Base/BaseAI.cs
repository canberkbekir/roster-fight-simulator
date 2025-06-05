using UnityEngine;
using UnityEngine.AI;
using Mirror;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;

namespace AI.Base
{ 
    public abstract class BaseAI : NetworkBehaviour
    {
        [PropertyRange(0.1f, 5f)]
        [SerializeField]
        private float tickInterval = 0.1f; 
        [SerializeField]
        protected NavMeshAgent agent;

        private float _tickTimer;

        protected virtual void Awake()
        {
            if (agent) return;
            agent = GetComponent<NavMeshAgent>();
            if (!agent)
            {
                Debug.LogError("NavMeshAgent is not assigned and could not be found on the GameObject.");
            }
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
            if (!agent || !agent.isOnNavMesh) return;
            agent.isStopped = false;
            agent.SetDestination(position);
        }

        /// <summary>
        /// Only check remainingDistance if the agent is on a NavMesh.
        /// </summary>
        protected bool HasReached(Vector3 position, float threshold = 0.5f)
        {
            if (!agent || !agent.isOnNavMesh) return false;
            if (agent.pathPending) return false;
            return agent.remainingDistance <= threshold;
        }

        /// <summary>
        /// Only call ResetPath if the agent is on a NavMesh.
        /// </summary>
        protected void StopMoving()
        {
            if (!agent || !agent.isOnNavMesh) return;
            agent.ResetPath();
        }

        #endregion
    }
}
