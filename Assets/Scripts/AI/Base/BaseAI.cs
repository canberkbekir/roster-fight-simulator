using UnityEngine;
using UnityEngine.AI;
using Mirror;
using Sirenix.OdinInspector;
using Utils;   

namespace AI.Base
{ 
    public abstract class BaseAI : NetworkBehaviour
    {
        [PropertyRange(0.1f, 5f)]
        [SerializeField]
        private float tickInterval = 0.1f;

        [SerializeField]
        protected NavMeshAgent agent;
 
        [InlineProperty]
        [HideLabel]
        [SerializeField]
        private Cooldown tickCooldown;

        protected virtual void Awake()
        { 
            if (!agent) agent = GetComponent<NavMeshAgent>();
            if (!agent) Debug.LogError("NavMeshAgent missing on " + name);
 
            tickCooldown = new Cooldown(tickInterval);
        }

        public override void OnStartServer()
        {
            base.OnStartServer(); 
            tickCooldown.Start();
        }

        private void Update()
        {
            if (!isServer) return;
 
            tickCooldown.Tick(Time.deltaTime);

            if (!tickCooldown.IsReady) return;
            tickCooldown.Start();

            StateTransition();
            StateTick();
        }

        protected abstract void StateTransition();
        protected abstract void StateTick();

        #region Movement Helpers

        protected void MoveTo(Vector3 position)
        {
            if (!agent || !agent.isOnNavMesh) return;
            agent.isStopped = false;
            agent.SetDestination(position);
        }

        protected bool HasReached(Vector3 position, float threshold = 0.5f)
        {
            if (!agent || !agent.isOnNavMesh) return false;
            if (agent.pathPending) return false;
            return agent.remainingDistance <= threshold;
        }

        protected void StopMoving()
        {
            if (!agent || !agent.isOnNavMesh) return;
            agent.ResetPath();
        }

        #endregion
    }
}
