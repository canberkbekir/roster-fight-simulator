using System;
using Mirror;
using UnityEngine;
using Creatures.Chickens.Base;
using Interactions.Objects.Nests;
using Managers;
using Services;

namespace Creatures.Chickens.Hens.Components
{
    /// <summary>
    /// Server‑authoritative egg formation using delta‑time in Update,
    /// fires an event when done.
    /// </summary>
    public class HenEggHandler : NetworkBehaviour
    {
        [Header("Egg Timing")]
        [Tooltip("Seconds required to form one egg.")]
        [SerializeField] private float eggFormationDuration = 15f;

        // 0…eggFormationDuration, synced to clients
        [SyncVar] private float progressTime;
        [SyncVar] private bool isProgressing;
        [SyncVar] private bool eggReady;

        /// <summary>
        /// Normalized [0…1] progress for UI binding.
        /// </summary>
        public float Progress => Mathf.Clamp01(progressTime / eggFormationDuration);

        /// <summary>
        /// True once the timer has elapsed.  You can poll this
        /// and then call your own LayEggAt(...) or whatever.
        /// </summary>
        public bool IsEggReady => eggReady;

        /// <summary>
        /// Fired on the server when an egg is fully formed.
        /// </summary>
        public event Action OnEggFormed;
        public event Action OnEggLaid;

        
        private ReproductionService _reproductionService;

        private void Awake()
        {
            _reproductionService = GameManager.Instance.ReproductionService;
        }

        public void Init(ChickenEntity newOwner)
        {
            // owner reference if you need it later
            ResetProgress();
        }

        [ServerCallback]
        private void Update()
        {
            if (!isProgressing || eggReady)
                return;

            progressTime += Time.deltaTime;
            if (progressTime < eggFormationDuration)
                return;

            CompleteEggFormation();
        }

        [Server]
        public void StartProgress()
        {
            eggReady      = false;
            isProgressing = true;
        }

        [Server]
        public void StopProgress() => isProgressing = false;

        [Server]
        public void ResetProgress()
        {
            isProgressing = false;
            eggReady      = false;
            progressTime  = 0f;
        }

        [Server]
        public void LayEggOnNest(Nest nest)
        {
            if (nest == null || !eggReady)
            {
                Debug.LogError("Cannot lay egg: either nest is null or egg is not ready.");
                return;
            }

            // Lay the egg at the nest's position
            // nest.AssignEgg();
            OnEggLaid?.Invoke();

            // Reset progress after laying the egg
            ResetProgress();
            
        }

        [Server]
        private void CompleteEggFormation()
        {
            eggReady      = true;
            isProgressing = false;
            progressTime  = eggFormationDuration;

            OnEggFormed?.Invoke();

            RpcOnEggFormed();
        }

        [ClientRpc]
        private void RpcOnEggFormed()
        {
            // e.g. play sound/VFX on clients
        }
    }
}
