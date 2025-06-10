using Creatures.Chickens.Base;
using Creatures.Chickens.Base.Components;
using Managers;
using Mirror;
using Services;
using UnityEngine;
using Utils;

namespace Creatures.Chickens.Chicks.Components
{
    public class ChickGrowthHandler : ChickenComponentBase
    {
        [SerializeField] private float growthInterval = 5f;
        [SerializeField] private Cooldown growthCooldown;

        private ChickenSpawnerService _chickenSpawnerService;
        [ServerCallback]
        private void Start()
        {
            _chickenSpawnerService = GameManager.Instance.ChickenSpawnerService;
            growthCooldown = new Cooldown(growthInterval);
            growthCooldown.OnFinished += OnGrowthCooldownFinished;
            growthCooldown.Start();
        }

        [ServerCallback]
        private void Update()
        {
            growthCooldown.Tick(Time.deltaTime);
        }

        public override void Init(ChickenEntity entity)
        {
            base.Init(entity);
        }
        
        
        [Server]
        private void OnGrowthCooldownFinished()
        {
            Debug.Log("Growth cooldown finished");
            if (!Owner) return;

            _chickenSpawnerService.RequestSpawnChickenAt(transform.position, Owner.Chicken);
            NetworkServer.Destroy(Owner.gameObject);
        }

        [ServerCallback]
        public void OnDestroy()
        {
            if (growthCooldown != null)
            {
                growthCooldown.OnFinished -= OnGrowthCooldownFinished;
            }
        }
        
        
    }
}
