using System.Collections.Generic;
using Creatures.Eggs;
using Creatures.Genes;
using Creatures.Genes.Base;
using Mirror;
using UnityEngine;

namespace Managers
{ 
    public class EggManager : NetworkBehaviour
    {
        [Header("Egg Settings")]
        [Tooltip("Drag in your Egg prefab here. It must have an `Egg` component on it.")]
        [SerializeField]
        private GameObject eggPrefab; 
        
        public static EggManager Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }
        
        public void SpawnEggWithGenes(Vector3 spawnPosition, GeneSync[] geneSyncs)
        {
            if (!isServer)
            {
                Debug.LogError("EggManager.SpawnEggWithGenes called on client! Must be run on server.");
                return;
            }

            if (eggPrefab == null)
            {
                Debug.LogError("EggManager: eggPrefab is not assigned in the inspector.");
                return;
            }

            // 1) Instantiate the egg locally on the server
            var eggObject = Instantiate(eggPrefab, spawnPosition, Quaternion.identity);
            var eggComponent = eggObject.GetComponent<Egg>();
            if (eggComponent == null)
            {
                Debug.LogError("EggManager: The eggPrefab has no Egg component attached.");
                Destroy(eggObject);
                return;
            }
 
            eggComponent.Genes.Clear();
            foreach (var gs in geneSyncs)
            {
                eggComponent.Genes.Add(gs);
            } 
            
            NetworkServer.Spawn(eggObject);
        }
    }
}
