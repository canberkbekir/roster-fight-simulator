// Egg.cs
using System.Collections.Generic;
using Creatures.Genes;
using Creatures.Genes.Base;
using Creatures.Genes.Base.ScriptableObjects;
using Creatures.Roosters;
using Creatures.Roosters.Components;
using Managers;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;

namespace Creatures.Eggs
{
    public class Egg : NetworkBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float hatchTime = 5f;
        [SerializeField] private GameObject chickPrefab;

        // SyncList of GeneSync so the egg carries its combined genes
        public class SyncGeneList : SyncList<GeneSync> { }
        public readonly SyncGeneList Genes = new SyncGeneList();
        
        [SyncVar] public bool isIncubating = false;

        private float _hatchTimer;
        private bool _hasHatched;
        private GeneDataContainer _geneDataContainer;

        public override void OnStartServer()
        {
            base.OnStartServer();
            _hatchTimer = hatchTime;
            _hasHatched = false;
            isIncubating = false;
 
            _geneDataContainer = GameManager.Instance.ContainerManager.GeneDataContainer;
        }

        [ServerCallback]
        private void Update()
        {
            if (_hasHatched) return;
 
            if (!isIncubating) return;

            _hatchTimer -= Time.deltaTime;
            if (!(_hatchTimer <= 0f)) return;
            _hasHatched = true;
            HatchEgg();
        }

        [Server]
        private void HatchEgg()
        {
            if (!chickPrefab)
            {
                Debug.LogError("Egg: chickPrefab is not assigned in inspector.", this);
                return;
            }

            var chickObject = Instantiate(chickPrefab, transform.position, Quaternion.identity);
            var babyEntity = chickObject.GetComponent<RoosterEntity>();
            if (!babyEntity)
            {
                Debug.LogError("Egg: chickPrefab has no RoosterEntity component!");
                Destroy(chickObject);
                return;
            }

            // Build the new Rooster data from synced genes
            var babyData = new Rooster
            {
                Gender = (Random.value < 0.5f) ? RoosterGender.Male : RoosterGender.Female,
                Genes = new List<Gene>().ToArray()
            };

            var geneList = new List<Gene>();
            foreach (var sync in Genes)
            {
                var data = _geneDataContainer.GetGeneById(sync.id);
                if (!data) continue;
                var newGene = new Gene(data);
                newGene.OverridePassingChance(sync.currentPassingChance);
                geneList.Add(newGene);
            }
            babyData.Genes = geneList.ToArray();

            babyEntity.Init(babyData);

            NetworkServer.Spawn(chickObject);
            NetworkServer.Destroy(gameObject);
        }
    }
}
