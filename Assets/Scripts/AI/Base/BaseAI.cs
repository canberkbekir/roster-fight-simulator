using UnityEngine;
using UnityEngine.AI;
using Mirror;
using Sirenix.OdinInspector;
using Utils;   

namespace AI.Base
{  
    public abstract class BaseAI : NetworkBehaviour
    {
        /// <summary>
        /// How often (in seconds) the AI update loop (StateTransition + StateTick) should run.
        /// </summary>
        [PropertyRange(0.1f, 5f)]
        [SerializeField]
        private float tickInterval = 0.1f;

        /// <summary>
        /// Reference to the NavMeshAgent used for pathfinding and movement.
        /// </summary>
        [SerializeField]
        protected NavMeshAgent agent;
 
        /// <summary>
        /// Internal cooldown tracker to enforce the <see cref="tickInterval"/>.
        /// </summary>
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

        /// <summary>
        /// Decide whether to switch to a different AI state.  
        /// Implement this in your derived AI to inspect conditions and call <c>ChangeState(...)</c>.
        /// </summary>
        protected abstract void StateTransition();

        /// <summary>
        /// Execute the logic for the current AI state.  
        /// This runs every tick after <see cref="StateTransition"/>.
        /// </summary>
        protected abstract void StateTick();

        #region Movement Helpers

        /// <summary>
        /// Orders the NavMeshAgent to move toward the specified world position.
        /// </summary>
        /// <param name="position">Target destination.</param>
        protected void MoveTo(Vector3 position)
        {
            if (!agent || !agent.isOnNavMesh) return;
            agent.isStopped = false;
            agent.SetDestination(position);
        }

        /// <summary>
        /// Returns true if the agent has reached (or is within <paramref name="threshold"/> of) the target.
        /// </summary>
        /// <param name="position">Destination to check.</param>
        /// <param name="threshold">How close is "close enough" (in Unity units)?</param>
        protected bool HasReached(Vector3 position, float threshold = 0.5f)
        {
            if (!agent || !agent.isOnNavMesh) return false;
            if (agent.pathPending) return false;
            return agent.remainingDistance <= threshold;
        }

        /// <summary>
        /// Immediately stops the agent’s current path and movement.
        /// </summary>
        protected void StopMoving()
        {
            if (!agent || !agent.isOnNavMesh) return;
            agent.ResetPath();
        }

        #endregion
    }
}
