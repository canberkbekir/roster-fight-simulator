using Creatures.Chickens.Base;
using Creatures.Chickens.Base.Components;
using Creatures.Chickens.Hens;
using Creatures.Chickens.Roosters;
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

            Chicken adult = Owner.Chicken.Gender == ChickenGender.Male
                ? new Rooster(Owner)
                : new Hen(Owner);

            _chickenSpawnerService.SpawnChickenServer(transform.position, adult);
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
