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
            var tempEntity = CreateTemporaryRoosterEntity(randomGene);
            var rooster = new Rooster(tempEntity); 
            Destroy(tempEntity.gameObject);
            SpawnItemWorld(spawnPos, spawnRot, rooster);
        }

        private RoosterEntity CreateTemporaryRoosterEntity(GeneData geneData)
        {
            if (roosterEntityPrefab == null)
                throw new UnassignedReferenceException("roosterEntityPrefab must be assigned in RoosterSpawnerManager.");

            GameObject instance = Instantiate(roosterEntityPrefab);
            var rooster = instance.GetComponent<RoosterEntity>();
            if (rooster == null)
                throw new MissingComponentException($"RoosterEntity component missing on prefab '{roosterEntityPrefab.name}'.");

            rooster.Init(new[] { new Gene(geneData) });
            return rooster;
        }

        private void SpawnItemWorld(Vector3 position, Quaternion rotation, Rooster rooster)
        {
            if (roosterEntityPrefab == null)
                throw new UnassignedReferenceException("roosterEntityPrefab is not assigned in RoosterSpawnerManager");
            var itemGO = Instantiate(roosterEntityPrefab, position, rotation);
            var entity = itemGO.GetComponent<RoosterEntity>(); 
            var itemWorld = itemGO.GetComponent<ItemWorld>();
            // itemWorld.SetMetaJson(stateJson);
            NetworkServer.Spawn(itemGO, connectionToClient);
        }
    }
}
