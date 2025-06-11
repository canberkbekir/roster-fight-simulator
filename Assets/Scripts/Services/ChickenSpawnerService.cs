using System;
using System.Collections.Generic;
using System.Linq;
using Creatures.Chickens.Base;
using Creatures.Chickens.Hens;
using Creatures.Chickens.Hens.Components;
using Creatures.Chickens.Roosters;
using Creatures.Chickens.Roosters.Components;
using Creatures.Chickens.Chicks;
using Creatures.Chickens.Chicks.Components;
using Creatures.Genes;
using Creatures.Genes.Base;
using Creatures.Genes.Base.ScriptableObjects;
using Mirror;
using UnityEngine;

namespace Services
{
    public class ChickenSpawnerService : NetworkBehaviour
    {
        #region Inspector Fields

        [SerializeField] private GeneDataContainer geneDataContainer;
        [SerializeField] private GameObject roosterEntityPrefab;
        [SerializeField] private GameObject chickenEntityPrefab;
        [SerializeField] private GameObject chickEntityPrefab;

        #endregion

        #region Public Requests

        /// <summary>
        /// Client → Server: spawn a random creature of the given type at spawnPoint.
        /// </summary>
        public void RequestSpawnRandomAt(Transform spawnPoint, CreatureType type)
        {
            if (!isClient) return;
            CmdSpawnRandomCreature(spawnPoint.position, spawnPoint.rotation, type);
        }

        public void RequestSpawnChickenAt(Vector3 spawnPos, Chicken chicken, Quaternion? spawnRot = null)
        {
            if (!isClient) return;
            var rot = spawnRot ?? Quaternion.identity;

            var geneSyncs = chicken.Genes.Select(g => new GeneSync(g)).ToArray();
            switch (chicken)
            { 
                case Rooster rooster:
                    CmdSpawnRooster(spawnPos, rooster, geneSyncs, rot);
                    break;
                case Hen hen:
                    CmdSpawnHen(spawnPos, hen, geneSyncs, rot);
                    break;
                default:
                    Debug.LogError("Unknown chicken type for spawning.");
                    break;
            } 
        }

        /// <summary>
        /// Client → Server: spawn a specific rooster with provided Rooster data.
        /// </summary>
        public void RequestSpawnRoosterAt(Vector3 spawnPos, Rooster rooster, Quaternion? spawnRot = null)
        {
            if (!isClient) return;
            var rot = spawnRot ?? Quaternion.identity;

            var geneSyncs = rooster.Genes.Select(g => new GeneSync(g)).ToArray(); 
            CmdSpawnRooster(spawnPos, rooster, geneSyncs, rot);
        }

        /// <summary>
        /// Client → Server: spawn a specific chicken (female) with provided Rooster data.
        /// </summary>
        public void RequestSpawnHenAt(Vector3 spawnPos, Hen chickenData, Quaternion? spawnRot = null)
        {
            if (!isClient) return;
            var rot = spawnRot ?? Quaternion.identity; 
            var geneSyncs = chickenData.Genes.Select(g => new GeneSync(g)).ToArray();
            CmdSpawnHen(spawnPos, chickenData, geneSyncs, rot);
        }

        /// <summary>
        /// Client → Server: spawn a specific chick with given Chick data (baby genes).
        /// </summary>
        public void RequestSpawnChickAt(Vector3 spawnPos, Chick chickData, Quaternion? spawnRot = null)
        {
            if (!isClient) return;
            var rot = spawnRot ?? Quaternion.identity;

            var geneSyncs = chickData.Genes.Select(g => new GeneSync(g)).ToArray();
            CmdSpawnChick(spawnPos, chickData, geneSyncs, rot);
        }

        #endregion

        #region Server Spawns

        [Server]
        public void SpawnRandomServer(Vector3 spawnPos, CreatureType type, Quaternion? spawnRot = null)
        {
            var rot = spawnRot ?? Quaternion.identity;
            SpawnRandomInternal(spawnPos, rot, type, null);
        }

        [Server]
        public void SpawnChickenServer(Vector3 spawnPos, Chicken chicken, Quaternion? spawnRot = null)
        {
            var rot = spawnRot ?? Quaternion.identity;

            switch (chicken)
            {
                case Rooster rooster:
                    SpawnRoosterServer(spawnPos, rooster, rot);
                    break;
                case Hen hen:
                    SpawnHenServer(spawnPos, hen, rot);
                    break;
                case Chick chick:
                    SpawnChickServer(spawnPos, chick, rot);
                    break;
                default:
                    Debug.LogError("Unknown chicken type for spawning.");
                    break;
            }
        }

        [Server]
        public void SpawnRoosterServer(Vector3 spawnPos, Rooster rooster, Quaternion? spawnRot = null)
        {
            var rot = spawnRot ?? Quaternion.identity;
            var entity = CreateRoosterEntity(rooster);
            SpawnEntityInternal(entity, spawnPos, rot, null);
        }

        [Server]
        public void SpawnHenServer(Vector3 spawnPos, Hen hen, Quaternion? spawnRot = null)
        {
            var rot = spawnRot ?? Quaternion.identity;
            var entity = CreateHenEntity(hen);
            SpawnEntityInternal(entity, spawnPos, rot, null);
        }

        [Server]
        public void SpawnChickServer(Vector3 spawnPos, Chick chickData, Quaternion? spawnRot = null)
        {
            var rot = spawnRot ?? Quaternion.identity;
            var entity = CreateChickEntity(chickData);
            SpawnEntityInternal(entity, spawnPos, rot, null);
        }

        private void SpawnRandomInternal(Vector3 spawnPos, Quaternion spawnRot, CreatureType type, NetworkConnectionToClient owner)
        {
            var randomGene = geneDataContainer.GetRandomGene();
            var genes = new[] { new Gene(randomGene) };

            switch (type)
            {
                case CreatureType.Rooster:
                    var roster = new Rooster
                    {
                        Genes = genes,
                    };
                    var roosterEntity = CreateRoosterEntity(roster);
                    SpawnEntityInternal(roosterEntity, spawnPos, spawnRot, owner);
                    break;

                case CreatureType.Chicken:
                    var hen = new Hen
                    {
                        Genes = new[] { new Gene(randomGene) },
                        Gender = (ChickenGender)UnityEngine.Random.Range(0,
                            Enum.GetValues(typeof(ChickenGender)).Length)
                    };

                    var henEntity = CreateHenEntity(hen);
                    SpawnEntityInternal(henEntity, spawnPos, spawnRot, owner);
                    break;

                case CreatureType.Chick:
                    break;
            }
        }

        #endregion

        #region Commands

        [Command(requiresAuthority = false)]
        private void CmdSpawnRandomCreature(Vector3 spawnPos, Quaternion spawnRot, CreatureType type)
        {
            SpawnRandomInternal(spawnPos, spawnRot, type, connectionToClient);
        }

        [Command(requiresAuthority = false)]
        private void CmdSpawnRooster(Vector3 spawnPos, Rooster rooster, GeneSync[] geneSyncs, Quaternion spawnRot)
        {
            if (rooster == null)
            {
                Debug.LogError("CmdSpawnRooster: Rooster data is null.");
                return;
            }

            var validGenes = new List<Gene>(geneSyncs.Length);
            foreach (var sync in geneSyncs)
            {
                var data = geneDataContainer.GetGeneById(sync.id);
                if (data == null)
                {
                    Debug.LogError($"CmdSpawnRooster: Gene ID {sync.id} not found.");
                    continue;
                }

                var gene = new Gene(data);
                gene.OverridePassingChance(sync.currentPassingChance);
                validGenes.Add(gene);
            }

            rooster.Genes = validGenes.ToArray(); 

            var entity = CreateRoosterEntity(rooster);
            SpawnEntityInternal(entity, spawnPos, spawnRot, connectionToClient);
        }

        [Command(requiresAuthority = false)]
        private void CmdSpawnHen(Vector3 spawnPos, Hen chickenData, GeneSync[] geneSyncs, Quaternion spawnRot)
        {
            if (chickenData == null)
            {
                Debug.LogError("CmdSpawnChicken: chickenData is null.");
                return;
            }

            var validGenes = new List<Gene>(geneSyncs.Length);
            foreach (var sync in geneSyncs)
            {
                var data = geneDataContainer.GetGeneById(sync.id);
                if (data == null)
                {
                    Debug.LogError($"CmdSpawnChicken: Gene ID {sync.id} not found.");
                    continue;
                }

                var gene = new Gene(data);
                gene.OverridePassingChance(sync.currentPassingChance);
                validGenes.Add(gene);
            }

            chickenData.Genes = validGenes.ToArray(); 

            var entity = CreateHenEntity(chickenData);
            SpawnEntityInternal(entity, spawnPos, spawnRot, connectionToClient);
        }

        [Command(requiresAuthority = false)]
        private void CmdSpawnChick(Vector3 spawnPos, Chick chickData, GeneSync[] geneSyncs, Quaternion spawnRot)
        {
            if (chickData == null)
            {
                Debug.LogError("CmdSpawnChick: chickData is null.");
                return;
            }

            var validGenes = new List<Gene>(geneSyncs.Length);
            foreach (var sync in geneSyncs)
            {
                var data = geneDataContainer.GetGeneById(sync.id);
                if (data == null)
                {
                    Debug.LogError($"CmdSpawnChick: Gene ID {sync.id} not found.");
                    continue;
                }

                var gene = new Gene(data);
                gene.OverridePassingChance(sync.currentPassingChance);
                validGenes.Add(gene);
            }

            chickData.Genes = validGenes.ToArray();

            var entity = CreateChickEntity(chickData);
            SpawnEntityInternal(entity, spawnPos, spawnRot, connectionToClient);
        }

        #endregion

        #region Helpers

        private void SpawnEntityInternal(ChickenEntity entity, Vector3 pos, Quaternion rot, NetworkConnectionToClient owner)
        {
            entity.transform.SetPositionAndRotation(pos, rot);

            if (entity.TryGetComponent(out InventorySystem.Base.ItemWorld itemWorld))
                itemWorld.SetChicken(entity.Chicken);

            NetworkServer.Spawn(entity.gameObject, owner);
        }

        #endregion

        #region Entity Creation

        private RoosterEntity CreateRoosterEntity(GeneData geneData)
        {
            if (roosterEntityPrefab == null)
                throw new UnassignedReferenceException("roosterEntityPrefab must be assigned.");

            var go = Instantiate(roosterEntityPrefab);
            var ent = go.GetComponent<RoosterEntity>();
            if (ent == null)
                throw new MissingComponentException($"Prefab '{roosterEntityPrefab.name}' lacks RoosterEntity.");

            var rooster = new Rooster();
            rooster.Genes = new[] { new Gene(geneData) };
            ent.Init(rooster);
            return ent;
        }

        private RoosterEntity CreateRoosterEntity(Rooster rooster)
        {
            if (!roosterEntityPrefab)
                throw new UnassignedReferenceException("roosterEntityPrefab must be assigned.");

            if (rooster == null)
                throw new ArgumentNullException(nameof(rooster));

            rooster.Genes = rooster.Genes.Select(g =>
            {
                var data = geneDataContainer.GetGeneById(g.GeneId)
                           ?? throw new ArgumentException($"Gene ID {g.GeneId} not found.");
                return new Gene(data);
            }).ToArray();
  
            var go = Instantiate(roosterEntityPrefab);
            var ent = go.GetComponent<RoosterEntity>();
            if (!ent)
                throw new MissingComponentException($"Prefab '{roosterEntityPrefab.name}' lacks RoosterEntity.");

            ent.Init(rooster);
            return ent;
        }

        private HenEntity CreateHenEntity(GeneData geneData)
        {
            if (chickenEntityPrefab == null)
                throw new UnassignedReferenceException("chickenEntityPrefab must be assigned.");

            var go = Instantiate(chickenEntityPrefab);
            var ent = go.GetComponent<HenEntity>();
            if (ent == null)
                throw new MissingComponentException($"Prefab '{chickenEntityPrefab.name}' lacks RoosterEntity.");

            var chicken = new Hen { Genes = new[] { new Gene(geneData) } };
            ent.Init(chicken);
            return ent;
        }

        private HenEntity CreateHenEntity(Hen henData)
        {
            if (chickenEntityPrefab == null)
                throw new UnassignedReferenceException("chickenEntityPrefab must be assigned.");

            if (henData == null)
                throw new ArgumentNullException(nameof(henData));

            henData.Genes = henData.Genes.Select(g =>
            {
                var data = geneDataContainer.GetGeneById(g.GeneId)
                           ?? throw new ArgumentException($"Gene ID {g.GeneId} not found.");
                return new Gene(data);
            }).ToArray(); 
            var go = Instantiate(chickenEntityPrefab);
            var ent = go.GetComponent<HenEntity>();
            if (ent == null)
                throw new MissingComponentException($"Prefab '{chickenEntityPrefab.name}' lacks RoosterEntity.");

            ent.Init(henData);
            return ent;
        }

        private ChickEntity CreateChickEntity(Chick chickData)
        {
            if (!chickEntityPrefab)
                throw new UnassignedReferenceException("chickEntityPrefab must be assigned.");

            if (chickData == null)
                throw new ArgumentNullException(nameof(chickData));

            chickData.Genes = chickData.Genes.Select(g =>
            {
                var data = geneDataContainer.GetGeneById(g.GeneId)
                           ?? throw new ArgumentException($"Gene ID {g.GeneId} not found.");
                return new Gene(data);
            }).ToArray();

            // Gender may already be set on chickData
            var go = Instantiate(chickEntityPrefab);
            var ent = go.GetComponent<ChickEntity>();
            if (ent == null)
                throw new MissingComponentException($"Prefab '{chickEntityPrefab.name}' lacks ChickEntity.");

            ent.Init(chickData);
            return ent;
        }

        #endregion
    }
}
