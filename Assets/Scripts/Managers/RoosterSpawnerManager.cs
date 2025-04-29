using System;
using Genes.Base;
using InventorySystem.Base;
using Mirror;
using Roosters.Components;
using UnityEngine;

namespace Managers
{
    public class RoosterSpawnerManager : NetworkBehaviour
    {
        [SerializeField] private GeneDataContainer geneDataContainer;
        [SerializeField] private GameObject roosterEntityPrefab;

        public void RequestSpawnAt(Transform spawnPoint)
        {
            if (!isClient) return;
            CmdSpawnRandomRooster(spawnPoint.position, spawnPoint.rotation);
        }

        [Command(requiresAuthority = false)]
        private void CmdSpawnRandomRooster(Vector3 spawnPos, Quaternion spawnRot)
        {
            var randomGene = geneDataContainer.GetRandomGene();
            var tempEntity = CreateTemporaryRooster(randomGene);
            var stateJson = JsonUtility.ToJson(new RoosterState(tempEntity));
            Destroy(tempEntity.gameObject);
            SpawnItemWorld(spawnPos, spawnRot, stateJson);
        }

        private RoosterEntity CreateTemporaryRooster(GeneData gene)
        {
            if (roosterEntityPrefab == null)
                throw new UnassignedReferenceException("roosterEntityPrefab is not assigned in RoosterSpawnerManager");
            var tempGO = Instantiate(roosterEntityPrefab);
            var entity = tempGO.GetComponent<RoosterEntity>();
            entity.preReadyGenes = new[] { gene };
            entity.Init();
            return entity;
        }

        private void SpawnItemWorld(Vector3 position, Quaternion rotation, string metaJson)
        {
            if (roosterEntityPrefab == null)
                throw new UnassignedReferenceException("roosterEntityPrefab is not assigned in RoosterSpawnerManager");
            var itemGO = Instantiate(roosterEntityPrefab, position, rotation);
            var entity = itemGO.GetComponent<RoosterEntity>();
            entity.Init();
            var stateJson = JsonUtility.ToJson(new RoosterState(entity));
            var itemWorld = itemGO.GetComponent<ItemWorld>();
            itemWorld.SetMetaJson(stateJson);
            NetworkServer.Spawn(itemGO, connectionToClient);
        }
    }
}
