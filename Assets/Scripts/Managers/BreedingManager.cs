using System.Collections.Generic;
using System.Linq;
using Creatures.Eggs;
using Creatures.Genes;
using Creatures.Genes.Base;
using Creatures.Genes.Base.ScriptableObjects;
using Creatures.Roosters.Components;
using Interactions.Objects.Nests;
using Mirror;
using UnityEngine;

namespace Managers
{
    public class BreedingManager : NetworkBehaviour
    {
        public static BreedingManager Instance { get; private set; }

        private GeneDataContainer _geneDataContainer;
        private EggManager       _eggManager;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _geneDataContainer = GameManager.Instance.ContainerManager.GeneDataContainer;
            _eggManager        = GameManager.Instance.EggManager;
        }
 
        [Server]
        public Egg SpawnEggAndAssignToNest(RoosterEntity mother, RoosterEntity father, Nest nest)
        {
            if (mother == null || father == null || nest == null)
            {
                Debug.LogError("[BreedingManager] SpawnEggAndAssignToNest: one argument was null.");
                return null;
            }
 
            var mixed = CrossGenes(
                mother.Rooster.Genes,
                father.Rooster.Genes
            );
 
            var newEggNetId = _eggManager.SpawnEggWithGenes(nest, mixed);
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
