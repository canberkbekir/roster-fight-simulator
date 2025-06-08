using Creatures.Chickens.Base.Components;
using Managers;
using Mirror;
using Services;
using UnityEngine;
using Utils;

namespace Creatures.Chickens.Chicks.Components
{
    public class ChickGrowthHandler : NetworkBehaviour, IChickenComponent
    {
        private ChickEntity _owner;
        [SerializeField] private float growthInterval = 5f; 
        [SerializeField] private Cooldown growthCooldown;

        private ChickenSpawnerService _chickenSpawnerService;
        private void Start()
        { 
            if (!isServer) return;
            _chickenSpawnerService = GameManager.Instance.ChickenSpawnerService;
            growthCooldown = new Cooldown(growthInterval);
            growthCooldown.OnFinished += OnGrowthCooldownFinished;
            growthCooldown.Start();
        } 

        private void Update()
        {
            if (!isServer) return;

            growthCooldown.Tick(Time.deltaTime);  
        }

        public void Init(ChickEntity entity)
        {
            _owner = entity;  
        }
        
        
        private void OnGrowthCooldownFinished()
        {
            Debug.Log("Growth cooldown finished");  
            if (!_owner) return;

            _chickenSpawnerService.RequestSpawnChickenAt(transform.position,_owner.Chicken);
            Destroy(_owner.gameObject);
        }

        public void OnDestroy()
        {
            if (growthCooldown != null)
            {
                growthCooldown.OnFinished -= OnGrowthCooldownFinished;
            }
        }
        
        
    }
}
