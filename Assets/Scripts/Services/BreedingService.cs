using System.Collections.Generic;
using System.Linq;
using Creatures.Chickens.Eggs;
using Creatures.Chickens.Hens.Components;
using Creatures.Chickens.Roosters.Components;
using Creatures.Genes;
using Creatures.Genes.Base;
using Creatures.Genes.Base.ScriptableObjects;
using Interactions.Objects.Nests;
using Managers;
using Mirror;
using UnityEngine;

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
 
            var mixed = CrossGenes(
                mother.Chicken.Genes,
                father.Chicken.Genes
            );
 
            var newEggNetId = _eggService.SpawnEggWithGenes(nest, mixed);
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
 
        public static GeneSync[] CrossGenes(Gene[] momGenes, Gene[] dadGenes)
        {
            var lookup = new Dictionary<int, Gene>();

            foreach (var gm in momGenes)
            {
                lookup[gm.GeneId] = gm;
            }

            foreach (var gf in dadGenes)
            {
                if (!lookup.ContainsKey(gf.GeneId))
                {
                    lookup[gf.GeneId] = gf;
                }
                else
                {
                    var chosen = (Random.value < 0.5f) ? lookup[gf.GeneId] : gf;
                    lookup[gf.GeneId] = chosen;
                }
            }

            return lookup.Select(kvp => new GeneSync(kvp.Value)).ToArray();
        }
    }
}
