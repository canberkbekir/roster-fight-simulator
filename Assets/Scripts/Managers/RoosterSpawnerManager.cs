using System;
using System.Collections.Generic;
using System.Linq;
using Creatures.Genes;
using Creatures.Genes.Base;
using Creatures.Genes.Base.ScriptableObjects;
using Creatures.Roosters;
using Creatures.Roosters.Components;
using Mirror;
using UnityEngine;

namespace Managers
{
    public class RoosterSpawnerManager : NetworkBehaviour
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

        /// <summary>
        /// Client → Server: spawn a specific rooster with provided Rooster data.
        /// </summary>
        public void RequestSpawnRoosterAt(Vector3 spawnPos, Rooster rooster, Quaternion? spawnRot = null)
        {
            if (!isClient) return;
            var rot = spawnRot ?? Quaternion.identity;

            var geneSyncs = rooster.Genes.Select(g => new GeneSync(g)).ToArray();
            rooster.Gender = RoosterGender.Male; // enforce male for Rooster
            CmdSpawnRooster(spawnPos, rooster, geneSyncs, rot);
        }

        /// <summary>
        /// Client → Server: spawn a specific chicken (female) with provided Rooster data.
        /// </summary>
        public void RequestSpawnChickenAt(Vector3 spawnPos, Rooster chickenData, Quaternion? spawnRot = null)
        {
            if (!isClient) return;
            var rot = spawnRot ?? Quaternion.identity;

            chickenData.Gender = RoosterGender.Female;
            var geneSyncs = chickenData.Genes.Select(g => new GeneSync(g)).ToArray();
            CmdSpawnChicken(spawnPos, chickenData, geneSyncs, rot);
        }

        /// <summary>
        /// Client → Server: spawn a specific chick with given Rooster data (baby genes).
        /// </summary>
        public void RequestSpawnChickAt(Vector3 spawnPos, Rooster chickData, Quaternion? spawnRot = null)
        {
            if (!isClient) return;
            var rot = spawnRot ?? Quaternion.identity;

            var geneSyncs = chickData.Genes.Select(g => new GeneSync(g)).ToArray();
            CmdSpawnChick(spawnPos, chickData, geneSyncs, rot);
        } 

        #endregion

        #region Commands

        [Command(requiresAuthority = false)]
        private void CmdSpawnRandomCreature(Vector3 spawnPos, Quaternion spawnRot, CreatureType type)
        { 
            var randomGene = geneDataContainer.GetRandomGene();
            var ro = new Rooster
            {
                Genes = new[] { new Gene(randomGene) },
                Gender = (RoosterGender)UnityEngine.Random.Range(0, Enum.GetValues(typeof(RoosterGender)).Length)
            };

            switch (type)
            {
                case CreatureType.Rooster:
                    ro.Gender = RoosterGender.Male;
                    {
                        var entity = CreateRoosterEntity(ro);
                        SpawnEntityInternal(entity, spawnPos, spawnRot);
                    }
                    break;

                case CreatureType.Chicken:
                    ro.Gender = RoosterGender.Female;
                    {
                        var entity = CreateChickenEntity(ro);
                        SpawnEntityInternal(entity, spawnPos, spawnRot);
                    }
                    break;

                case CreatureType.Chick:
                    // randomize chick gender
                    ro.Gender = (RoosterGender)UnityEngine.Random.Range(0, 2);
                    {
                        var entity = CreateChickEntity(ro);
                        SpawnEntityInternal(entity, spawnPos, spawnRot);
                    }
                    break; 
            }
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
            rooster.Gender = RoosterGender.Male; // enforce male

            var entity = CreateRoosterEntity(rooster);
            SpawnEntityInternal(entity, spawnPos, spawnRot);
        }

        [Command(requiresAuthority = false)]
        private void CmdSpawnChicken(Vector3 spawnPos, Rooster chickenData, GeneSync[] geneSyncs, Quaternion spawnRot)
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
            chickenData.Gender = RoosterGender.Female;

            var entity = CreateChickenEntity(chickenData);
            SpawnEntityInternal(entity, spawnPos, spawnRot);
        }

        [Command(requiresAuthority = false)]
        private void CmdSpawnChick(Vector3 spawnPos, Rooster chickData, GeneSync[] geneSyncs, Quaternion spawnRot)
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
            SpawnEntityInternal(entity, spawnPos, spawnRot);
        } 
         
        #endregion

        #region Helpers
 
        private void SpawnEntityInternal(RoosterEntity entity, Vector3 pos, Quaternion rot)
        {
            entity.transform.SetPositionAndRotation(pos, rot);
 
            if (entity.TryGetComponent(out InventorySystem.Base.ItemWorld itemWorld))
                itemWorld.SetRooster(entity.Rooster);

            NetworkServer.Spawn(entity.gameObject, connectionToClient);
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

            var rooster = new Rooster { Genes = new[] { new Gene(geneData) }, Gender = RoosterGender.Male };
            ent.Init(rooster);
            return ent;
        }

        private RoosterEntity CreateRoosterEntity(Rooster rooster)
        {
            if (roosterEntityPrefab == null)
                throw new UnassignedReferenceException("roosterEntityPrefab must be assigned.");

            if (rooster == null)
                throw new ArgumentNullException(nameof(rooster));

            rooster.Genes = rooster.Genes.Select(g =>
            {
                var data = geneDataContainer.GetGeneById(g.GeneId)
                           ?? throw new ArgumentException($"Gene ID {g.GeneId} not found.");
                return new Gene(data);
            }).ToArray();

            rooster.Gender = RoosterGender.Male;

            var go = Instantiate(roosterEntityPrefab);
            var ent = go.GetComponent<RoosterEntity>();
            if (ent == null)
                throw new MissingComponentException($"Prefab '{roosterEntityPrefab.name}' lacks RoosterEntity.");

            ent.Init(rooster);
            return ent;
        }

        private RoosterEntity CreateChickenEntity(GeneData geneData)
        {
            if (chickenEntityPrefab == null)
                throw new UnassignedReferenceException("chickenEntityPrefab must be assigned.");

            var go = Instantiate(chickenEntityPrefab);
            var ent = go.GetComponent<RoosterEntity>();
            if (ent == null)
                throw new MissingComponentException($"Prefab '{chickenEntityPrefab.name}' lacks RoosterEntity.");

            var chicken = new Rooster { Genes = new[] { new Gene(geneData) }, Gender = RoosterGender.Female };
            ent.Init(chicken);
            return ent;
        }

        private RoosterEntity CreateChickenEntity(Rooster chickenData)
        {
            if (chickenEntityPrefab == null)
                throw new UnassignedReferenceException("chickenEntityPrefab must be assigned.");

            if (chickenData == null)
                throw new ArgumentNullException(nameof(chickenData));

            chickenData.Genes = chickenData.Genes.Select(g =>
            {
                var data = geneDataContainer.GetGeneById(g.GeneId)
                           ?? throw new ArgumentException($"Gene ID {g.GeneId} not found.");
                return new Gene(data);
            }).ToArray();

            chickenData.Gender = RoosterGender.Female;

            var go = Instantiate(chickenEntityPrefab);
            var ent = go.GetComponent<RoosterEntity>();
            if (ent == null)
                throw new MissingComponentException($"Prefab '{chickenEntityPrefab.name}' lacks RoosterEntity.");

            ent.Init(chickenData);
            return ent;
        }

        private RoosterEntity CreateChickEntity(Rooster chickData)
        {
            if (chickEntityPrefab == null)
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
            var ent = go.GetComponent<RoosterEntity>();
            if (ent == null)
                throw new MissingComponentException($"Prefab '{chickEntityPrefab.name}' lacks RoosterEntity.");

            ent.Init(chickData);
            return ent;
        }

        #endregion
    }
}
