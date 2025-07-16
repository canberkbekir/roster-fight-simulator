using System;
using Mirror;
using UnityEngine;
using Creatures.Chickens.Base;
using Creatures.Chickens.Base.Components;
using Creatures.Chickens.Eggs;
using Creatures.Chickens.Roosters.Components;
using Interactions.Objects.Nests;
using Managers;
using Services;

namespace Creatures.Chickens.Hens.Components
{
    public class HenEggHandler : ChickenComponentBase
    {
        [Header("Egg Timing")]
        [Tooltip("Seconds required to form one egg.")]
        [SerializeField] private float eggFormationDuration = 15f;

        [SyncVar] private float progressTime;
        [SyncVar] private bool isProgressing;
        [SyncVar] private bool isEggFertilized;
        [SyncVar] private bool eggReady;
        [SyncVar] private RoosterEntity fertilizedBy;
        [SyncVar] private EggEntity _layedEggEntity;
        
        public EggEntity LayedEggEntity => _layedEggEntity;
        public float Progress => Mathf.Clamp01(progressTime / eggFormationDuration);
        public bool IsEggReady => eggReady;
        
        public event Action OnEggFormed;
        public event Action OnEggLaid;
        public event Action<RoosterEntity> OnEggFertilized;
        
        
        private ReproductionService _reproductionService;

        private void Awake()
        {
            _reproductionService = GameManager.Instance.ReproductionService;
        }

        public override void Init(ChickenEntity owner)
        {
            base.Init(owner);
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
            isEggFertilized = false;
            progressTime  = 0f;
            fertilizedBy  = null;
            
            StartProgress();
        }

        [Server]
        public void LayEggOnNest(Nest nest)
        {
            if (nest == null || !eggReady)
            {
                Debug.LogError("Cannot lay egg: either nest is null or egg is not ready.");
                return;
            }

            if (isEggFertilized)
            {
               _layedEggEntity = _reproductionService.LayFertilizedEgg(Owner as HenEntity, fertilizedBy, nest);
            }
            else
            { 
                _layedEggEntity = _reproductionService.LayUnfertilizedEgg(Owner as HenEntity, nest);
            }
            OnEggLaid?.Invoke();
            ResetProgress();
        }
        
        [Server]
        public void FertilizeEgg(RoosterEntity rooster)
        {
            if (rooster == null)
            {
                Debug.LogError("Cannot fertilize egg: Rooster is null.");
                return;
            }

            isEggFertilized = true;
            fertilizedBy    = rooster;

            OnEggFertilized?.Invoke(fertilizedBy);
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
