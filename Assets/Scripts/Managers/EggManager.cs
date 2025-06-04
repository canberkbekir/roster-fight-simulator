using Creatures.Eggs;
using Creatures.Genes;
using Creatures.Genes.Base;
using Interactions.Objects.Nests;
using Mirror;
using UnityEngine;

namespace Managers
{
    public class EggManager : NetworkBehaviour
    {
        [Header("Egg Settings")]
        [Tooltip("Drag in your Egg prefab here. It must have an `Egg` component on it.")]
        [SerializeField] private GameObject eggPrefab;

        public static EggManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (eggPrefab == null)
                Debug.LogError("EggManager: eggPrefab is not assigned in the inspector.");
        } 
        [Server]
        public uint SpawnEggWithGenes(Nest nest, GeneSync[] geneSyncs)
        {
            if (!nest)
            {
                Debug.LogError("[EggManager] SpawnEggWithGenes: nest is null!");
                return 0;
            }
  
            var pos = nest.SpawnTransform.position;
            var eggObject = Instantiate(eggPrefab, pos, Quaternion.identity);
            var eggComponent = eggObject.GetComponent<Egg>();
            if (!eggComponent)
            {
                Debug.LogError("[EggManager] The eggPrefab has no Egg component attached.");
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
                Debug.LogError("[EggManager] Spawned egg has no NetworkIdentity!");
                return 0;
            }
            var newEggNetId = nid.netId;
 
            nest.AssignEgg(newEggNetId); 
            return newEggNetId;
        }
    }
}
