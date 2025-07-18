using Creatures.Chickens.Eggs.Components;
using Creatures.Chickens.Hens.Components;
using Creatures.Chickens.Roosters.Components;
using Creatures.Genes.Base;
using Interactions.Objects.Nests;
using Mirror;
using UnityEngine;
using Utils;

namespace Services
{
    public class ReproductionService : NetworkBehaviour
    {
        [Header("Egg Settings")]
        [Tooltip("Must be registered in the NetworkManager’s Spawnable Prefabs.")]
        [SerializeField] private GameObject eggPrefab;

        // Disable this entire component on clients
        [ClientCallback]
        private void Start()
        {
            if (!isServer)
                enabled = false;
        }

        // Only runs on server
        [ServerCallback]
        private void Awake()
        {
            if (eggPrefab == null)
                Debug.LogError("[ReproductionService] eggPrefab is not assigned.");
        }

        [Server]
        private EggEntity SpawnEgg(Nest nest, Gene[] genes)
        {
            if (nest == null)
            {
                Debug.LogError("[ReproductionService] SpawnEgg: nest is null!");
                return null;
            }

            var eggObj = Instantiate(eggPrefab, nest.SpawnTransform.position, Quaternion.identity);
            if (!eggObj.TryGetComponent<EggEntity>(out var egg))
            {
                Debug.LogError("[ReproductionService] eggPrefab has no EggEntity component!");
                Destroy(eggObj);
                return null;
            }

            egg.SetGenes(genes);
            NetworkServer.Spawn(eggObj);
            nest.AssignEgg(egg.netId);

            return egg;
        }

        /// <summary>
        /// Lay a fertilized egg (server only).
        /// </summary>
        [Server]
        public EggEntity LayEgg(HenEntity mother, RoosterEntity father, Nest nest)
        {
            if (mother == null || father == null || nest == null)
            {
                Debug.LogError("[ReproductionService] LayEgg: one argument was null.");
                return null;
            }

            var mixed = GeneHelper.GetCrossGenes(mother.Chicken.Genes, father.Chicken.Genes);
            return SpawnEgg(nest, mixed);
        }

        /// <summary>
        /// Lay an unfertilized egg (server only).
        /// </summary>
        [Server]
        public EggEntity LayEgg(HenEntity mother, Nest nest)
        {
            if (mother == null || nest == null)
            {
                Debug.LogError("[ReproductionService] LayEgg (unfertilized): one argument was null.");
                return null;
            }

            var passed = GeneHelper.GetPassedGene(mother.Chicken.Genes);
            return SpawnEgg(nest, passed);
        }
    }
}
