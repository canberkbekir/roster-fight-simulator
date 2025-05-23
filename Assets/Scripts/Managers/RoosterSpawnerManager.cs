using System;
using System.Collections.Generic;
using System.Linq;
using Genes;
using Genes.Base;
using Genes.Base.ScriptableObjects;
using InventorySystem.Base;
using Mirror;
using Roosters;
using Roosters.Components;
using UnityEngine;

namespace Managers
{
    public class RoosterSpawnerManager : NetworkBehaviour
    {
        #region Inspector Fields
        [SerializeField] private GeneDataContainer geneDataContainer;
        [SerializeField] private GameObject        roosterEntityPrefab;
        #endregion

        #region Public Requests
        public void RequestSpawnRandomRoosterAt(Transform spawnPoint)
        {
            if (!isClient) return;
            CmdSpawnRandomRooster(spawnPoint.position, spawnPoint.rotation);
        }

        public void RequestSpawnRoosterAt(Vector3 spawnPos, Rooster rooster,Quaternion? spawnRot = null)
        {
            if (!isClient) return;
            var rot = spawnRot ?? Quaternion.identity; 
            
            var geneSyncs = rooster.Genes.Select(g => new GeneSync(g)).ToArray();
            
            CmdSpawnRooster(spawnPos, rooster,geneSyncs, rot);
        }
        #endregion

        #region Commands
        [Command(requiresAuthority = false)]
        private void CmdSpawnRandomRooster(Vector3 spawnPos, Quaternion spawnRot)
        {
            var gene   = geneDataContainer.GetRandomGene();
            var entity = CreateRoosterEntity(gene);
            SpawnRoosterInternal(entity, spawnPos, spawnRot);
        }

        [Command(requiresAuthority = false)]
        private void CmdSpawnRooster(Vector3 spawnPos,Rooster rooster,GeneSync[] geneSyncs,Quaternion spawnRot)
        {
            if (rooster == null)
            {
                Debug.LogError("Rooster is null.");
                return;
            }
 
            var validGenes = new List<Gene>(geneSyncs.Length);
            foreach (var sync in geneSyncs)
            {
                var data = geneDataContainer.GetGeneById(sync.id);
                if (data == null)
                {
                    Debug.LogError($"Gene with ID {sync.id} not found.");
                    continue;
                }

                var gene = new Gene(data);
                gene.OverridePassingChance(sync.currentPassingChance);
                validGenes.Add(gene);
            }
            rooster.Genes = validGenes.ToArray();
 
            var entity = CreateRoosterEntity(rooster);
            SpawnRoosterInternal(entity, spawnPos, spawnRot);
        }
        #endregion

        #region Helpers
        private void SpawnRoosterInternal(RoosterEntity entity, Vector3 pos, Quaternion rot)
        {
            entity.transform.SetPositionAndRotation(pos, rot);

            if (entity.TryGetComponent<ItemWorld>(out var itemWorld))
                itemWorld.SetRooster(entity.Rooster);
            else
                Debug.LogError("ItemWorld component not found on RoosterEntity prefab.");

            NetworkServer.Spawn(entity.gameObject, connectionToClient);
        }
        #endregion

        #region Entity Creation
        private RoosterEntity CreateRoosterEntity(GeneData geneData)
        {
            if (roosterEntityPrefab == null)
                throw new UnassignedReferenceException("roosterEntityPrefab must be assigned in RoosterSpawnerManager.");

            var go  = Instantiate(roosterEntityPrefab);
            var ent = go.GetComponent<RoosterEntity>();
            if (ent == null)
                throw new MissingComponentException($"RoosterEntity component missing on prefab '{roosterEntityPrefab.name}'.");

            var rooster = new Rooster { Genes = new[] { new Gene(geneData) } };
            ent.Init(rooster);
            return ent;
        }

        private RoosterEntity CreateRoosterEntity(Rooster rooster)
        {
            if (roosterEntityPrefab == null)
                throw new UnassignedReferenceException("roosterEntityPrefab must be assigned in RoosterSpawnerManager.");
            
            if (rooster == null)
                throw new ArgumentNullException(nameof(rooster), "Rooster cannot be null.");

            rooster.Genes = rooster.Genes.Select(g =>
                {
                    var data = geneDataContainer.GetGeneById(g.GeneId)
                               ?? throw new ArgumentException($"Gene ID {g.GeneId} not found.");
                    return new Gene(data);
                }).ToArray();

            var go  = Instantiate(roosterEntityPrefab);
            var ent = go.GetComponent<RoosterEntity>();
            if (ent == null)
                throw new MissingComponentException($"RoosterEntity component missing on prefab '{roosterEntityPrefab.name}'.");

            ent.Init(rooster);
            return ent;
        }
        #endregion
    }
}
