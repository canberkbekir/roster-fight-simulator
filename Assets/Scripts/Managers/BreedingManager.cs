using System.Collections.Generic;
using System.Linq;
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
        private EggManager _eggManager;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            _geneDataContainer = GameManager.Instance.ContainerManager.GeneDataContainer;
            _eggManager = GameManager.Instance.EggManager;
        }

        /// <summary>
        /// Called by client‐side code to request that two networked roosters breed.
        /// Issues a [Command] to the server with each parent's netId + spawn location.
        /// </summary>
        public void RequestBreed(RoosterEntity mother, RoosterEntity father, Nest nest)
        {
            if (!isClient) return;

            if (mother == null || father == null)
            {
                Debug.LogError("BreedingManager: A null parent was passed into RequestBreed.");
                return;
            }
 
            CmdBreedOnServer(mother.netId, father.netId, nest.netId);
        }

        [Command(requiresAuthority = false)]
        private void CmdBreedOnServer(uint motherNetId, uint fatherNetId, uint nestNetId)
        { 
            if (!NetworkServer.spawned.TryGetValue(motherNetId, out var momObj) ||
                !NetworkServer.spawned.TryGetValue(fatherNetId, out var dadObj))
            {
                Debug.LogError($"BreedingManager: Could not find parent netIds {motherNetId} or {fatherNetId} on the server.");
                return;
            }
            
            if (!NetworkServer.spawned.TryGetValue(nestNetId, out var nestObj))
            {
                Debug.LogError($"BreedingManager: Could not find nest netId {nestNetId} on the server.");
                return;
            }

            var motherEntity = momObj.GetComponent<RoosterEntity>();
            var fatherEntity = dadObj.GetComponent<RoosterEntity>();
            var nestObject = nestObj.GetComponent<Nest>();
            
            if (!motherEntity || fatherEntity == null)
            {
                Debug.LogError("BreedingManager: One of the netIds is not a RoosterEntity.");
                return;
            }
 
            var mixed = CrossGenes(motherEntity.Rooster.Genes, fatherEntity.Rooster.Genes);
 
            if (!_eggManager)
            {
                Debug.LogError("BreedingManager: eggManager reference not set in inspector.");
                return;
            }

            _eggManager.SpawnEggWithGenes(nestObject.SpawnTransform.position, mixed);
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
