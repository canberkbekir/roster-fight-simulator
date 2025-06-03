using System.Collections.Generic;
using System;
using Creatures.Genes;
using Creatures.Genes.Base;
using Creatures.Genes.Base.ScriptableObjects;
using Creatures.Roosters;
using Creatures.Roosters.Components;
using Managers;
using Mirror;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

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

        /// <summary>
        /// Yumurtanın kuluçka süresi dolduğunda abone olanları bilgilendirecek event.
        /// </summary>
        public event Action<Egg> OnHatched;  // <<< **YENİ** event bildirimi

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
            if (_hatchTimer <= 0f)
            {
                _hasHatched = true; 
                OnHatched?.Invoke(this); 
                HatchEgg();
            }
        }

        /// <summary>
        /// Kuluçkayı sunucu tarafında başlatmak için helper.
        /// Hem SyncVar isIncubating=true set eder, hem RPC ile istemcilerde VFX oynatmayı tetikler.
        /// </summary>
        [Server]
        public void StartIncubation()
        {
            if (isIncubating) return;
            isIncubating = true;
            // İstemcilerde görsel bir efekt çalıştırmak isterseniz:
            RpcPlayIncubationVFX();
        }

        /// <summary>
        /// İstemci tarafında, incubate başladığında VFX ya da animasyon oynatmak isterseniz burayı kullanabilirsiniz.
        /// </summary>
        [ClientRpc]
        private void RpcPlayIncubationVFX()
        {
            // Örneğin: 
            // Instantiate(incubationVfxPrefab, transform.position, Quaternion.identity);
            // Veya bir animatör trigger’ı tetikleyebilirsiniz.
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
