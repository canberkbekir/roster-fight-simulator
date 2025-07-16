using Creatures.Chickens.Eggs;
using Creatures.Chickens.Hens.Components;
using Creatures.Chickens.Roosters.Components;
using Creatures.Genes;
using Interactions.Objects.Nests;
using Mirror;
using UnityEngine;
using Utils;

namespace Services
{
    public class ReproductionService : NetworkBehaviour
    {
        [Header("Egg Settings")]
        [Tooltip("Drag in your Egg prefab here. It must have an `Egg` component on it.")]
        [SerializeField] private GameObject eggPrefab;

        public static ReproductionService Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (eggPrefab == null)
                Debug.LogError("[ReproductionService] eggPrefab is not assigned in the inspector.");
        }

        [Server]
        public Egg BreedWithParents(HenEntity mother, RoosterEntity father, Nest nest)
        {
            if (!mother || !father || !nest)
            {
                Debug.LogError("[ReproductionService] BreedWithParents: one argument was null.");
                return null;
            }

            var mixed = GeneHelper.CrossGenes(mother.Chicken.Genes, father.Chicken.Genes);
            var mixedSync = GeneHelper.GeneToGeneSync(mixed);

            var newEggNetId = SpawnEggWithGenes(nest, mixedSync);
            if (newEggNetId == 0)
            {
                Debug.LogError("[ReproductionService] Failed to spawn egg.");
                return null;
            }

            var spawnedEgg = nest.CurrentEgg;
            if (spawnedEgg == null)
                Debug.LogError("[ReproductionService] After spawning, nest.CurrentEgg is still null!");

            return spawnedEgg;
        }

        [Server]
        public Egg LayUnfertilizedEgg(HenEntity mother, Nest nest)
        {
            if (!mother || !nest)
            {
                Debug.LogError("[ReproductionService] LayUnfertilizedEgg: one argument was null.");
                return null;
            }

            var passedGenes = GeneHelper.GetPassedGene(mother.Chicken.Genes);
            var passedGeneSync = GeneHelper.GeneToGeneSync(passedGenes);

            var newEggNetId = SpawnEggWithGenes(nest, passedGeneSync);
            if (newEggNetId == 0)
            {
                Debug.LogError("[ReproductionService] Failed to spawn egg.");
                return null;
            }

            var spawnedEgg = nest.CurrentEgg;
            if (spawnedEgg == null)
                Debug.LogError("[ReproductionService] After spawning, nest.CurrentEgg is still null!");

            return spawnedEgg;
        }

        [Server]
        public uint SpawnEggWithGenes(Nest nest, GeneSync[] geneSyncs)
        {
            if (!nest)
            {
                Debug.LogError("[ReproductionService] SpawnEggWithGenes: nest is null!");
                return 0;
            }

            var pos = nest.SpawnTransform.position;
            var eggObject = Instantiate(eggPrefab, pos, Quaternion.identity);
            var eggComponent = eggObject.GetComponent<Egg>();
            if (!eggComponent)
            {
                Debug.LogError("[ReproductionService] The eggPrefab has no Egg component attached.");
                Destroy(eggObject);
                return 0;
            }
            eggComponent.Genes.Clear();
            foreach (var gs in geneSyncs)
                eggComponent.Genes.Add(gs);

            NetworkServer.Spawn(eggObject);

            var nid = eggObject.GetComponent<NetworkIdentity>();
            if (!nid)
            {
                Debug.LogError("[ReproductionService] Spawned egg has no NetworkIdentity!");
                return 0;
            }
            var newEggNetId = nid.netId;

            nest.AssignEgg(newEggNetId);
            return newEggNetId;
        }
    }
}
