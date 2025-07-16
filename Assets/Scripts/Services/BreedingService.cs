using Creatures.Chickens.Eggs;
using Creatures.Chickens.Hens.Components;
using Creatures.Chickens.Roosters.Components; 
using Creatures.Genes.Base.ScriptableObjects;
using Interactions.Objects.Nests;
using Managers;
using Mirror;
using UnityEngine;
using Utils; 

namespace Services
{
    public class BreedingService : NetworkBehaviour
    {
        public static BreedingService Instance { get; private set; }

        private GeneDataContainer _geneDataContainer;
        private EggService       _eggService;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _geneDataContainer = GameManager.Instance.ContainerService.GeneDataContainer;
            _eggService        = GameManager.Instance.EggService;
        }
 
        [Server]
        public Egg SpawnEggAndAssignToNest(HenEntity mother, RoosterEntity father, Nest nest)
        {
            if (!mother || !father || !nest)
            {
                Debug.LogError("[BreedingManager] SpawnEggAndAssignToNest: one argument was null.");
                return null;
            }
 
            var mixed = GeneHelper.CrossGenes(
                mother.Chicken.Genes,
                father.Chicken.Genes
            );
            
            var mixedSync = GeneHelper.GeneToGeneSync(mixed);
 
            var newEggNetId = _eggService.SpawnEggWithGenes(nest, mixedSync);
            if (newEggNetId == 0)
            {
                Debug.LogError("[BreedingManager] Failed to spawn egg via EggManager.");
                return null;
            }
 
            var spawnedEgg = nest.CurrentEgg;
            if (spawnedEgg == null)
            {
                Debug.LogError("[BreedingManager] After spawning, nest.CurrentEgg is still null!");
            }
            return spawnedEgg;
        }

        [Server]
        public Egg SpawnEggAndAssignToNest(HenEntity mother, Nest nest)
        {
            if (!mother || !nest)
            {
                Debug.LogError("[BreedingManager] SpawnEggAndAssignToNest: one argument was null.");
                return null;
            }

            var passedGene = GeneHelper.GetPassedGene(mother.Chicken.Genes);
            var passedGeneSync = GeneHelper.GeneToGeneSync(passedGene); 
 
            var newEggNetId = _eggService.SpawnEggWithGenes(nest, passedGeneSync);
            if (newEggNetId == 0)
            {
                Debug.LogError("[BreedingManager] Failed to spawn egg via EggManager.");
                return null;
            }
 
            var spawnedEgg = nest.CurrentEgg;
            if (spawnedEgg == null)
            {
                Debug.LogError("[BreedingManager] After spawning, nest.CurrentEgg is still null!");
            }
            return spawnedEgg;
        }   
    }
}
