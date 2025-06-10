using System;
using System.Collections.Generic;
using Creatures.Chickens.Base;
using Creatures.Chickens.Chicks.Components;
using Creatures.Chickens.Chicks;
using Creatures.Genes;
using Creatures.Genes.Base;
using Creatures.Genes.Base.ScriptableObjects;
using Managers;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Creatures.Chickens.Eggs
{
    public class Egg : NetworkBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float hatchTime = 5f;
        [SerializeField] private GameObject chickPrefab;
 
        public class SyncGeneList : SyncList<GeneSync> { }
        public readonly SyncGeneList Genes = new SyncGeneList();
        
        [SyncVar] public bool isIncubating = false;

        private float _hatchTimer;
        private bool _hasHatched;
        private GeneDataContainer _geneDataContainer;
 
        public event Action<Egg> OnHatched;   

        public override void OnStartServer()
        {
            base.OnStartServer();
            _hatchTimer = hatchTime;
            _hasHatched = false;
            isIncubating = false;
            _geneDataContainer = GameManager.Instance.ContainerService.GeneDataContainer;
        }

        [ServerCallback]
        private void Update()
        {
            if (_hasHatched) return;
            if (!isIncubating) return;

            _hatchTimer -= Time.deltaTime;
            if (_hatchTimer <= 0f)
            {
                _hasHatched = true; 
                OnHatched?.Invoke(this); 
                HatchEgg();
            }
        }
 
        [Server]
        public void StartIncubation()
        {
            if (isIncubating) return;
            isIncubating = true; 
            RpcPlayIncubationVFX();
        }

        /// <summary>
        /// Plays the incubation visual effects on all clients.
        /// </summary>
        [ClientRpc]
        private void RpcPlayIncubationVFX()
        { 
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
            var babyEntity = chickObject.GetComponent<ChickEntity>();
            if (!babyEntity)
            {
                Debug.LogError("Egg: chickPrefab has no ChickEntity component!");
                Destroy(chickObject);
                return;
            }

            var babyData = new Chick
            {
                Gender = (Random.value < 0.5f) ? ChickenGender.Male : ChickenGender.Female,
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
