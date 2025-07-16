using System; 
using System.Linq;
using Creatures.Chickens.Base;
using Creatures.Chickens.Chicks;
using Creatures.Chickens.Chicks.Components;
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
        [SerializeField] private float minHatchTime = 1f;
        [SerializeField] private float maxHatchTime = 60f;

        public class SyncGeneList : SyncList<GeneSync> { }
        public readonly SyncGeneList Genes = new SyncGeneList();

        [SyncVar] public bool IsIncubating;

        private float _hatchTimer;
        private bool _hasHatched, _isInitialized;
        private GeneDataContainer _geneDataContainer;

        public event Action<Egg> OnHatched;

        public float RemainingHatchTime => Mathf.Max(0f, _hatchTimer);
        public bool HasHatched        => _hasHatched;
        public bool IsInitialized    => _isInitialized;
        public float HatchTime       => hatchTime;

        public override void OnStartServer()
        {
            base.OnStartServer();
            Clamp(ref hatchTime, minHatchTime, maxHatchTime);
            if (!chickPrefab || chickPrefab.GetComponent<ChickEntity>() == null)
                Debug.LogError($"[Egg:{name}] Invalid chickPrefab.");
            Initialize();
        }

        [ServerCallback]
        private void Update()
        {
            if (!_isInitialized || _hasHatched || !IsIncubating) return;

            if ((_hatchTimer -= Time.deltaTime) <= 0f)
            {
                _hasHatched = true;
                OnHatched?.Invoke(this);
                Hatch();
            }
        }

        [Server]
        public void StartIncubation()
        {
            if (!_isInitialized || _hasHatched || IsIncubating) return;
            _hatchTimer = hatchTime;
            IsIncubating = true;
            RpcPlayIncubationVFX();
            Debug.Log($"[Egg:{name}] Incubation started for {hatchTime}s.");
        }

        [ClientRpc]
        private void RpcPlayIncubationVFX() =>
            Debug.Log($"[Egg:{name}] Playing incubation VFX.");

        [Server]
        private void Hatch()
        {
            if (!chickPrefab || !_geneDataContainer) return;

            try
            {
                var obj = Instantiate(chickPrefab, transform.position, Quaternion.identity);
                var entity = obj.GetComponent<ChickEntity>();
                if (entity == null) { Destroy(obj); return; }

                var data = CreateChickData();
                if (data == null) { Destroy(obj); return; }

                entity.Init(data);
                NetworkServer.Spawn(obj);
                NetworkServer.Destroy(gameObject);
                Debug.Log($"[Egg:{name}] Hatched {data.Name}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Egg:{name}] Hatch error: {ex.Message}");
            }
        }

        private void Initialize()
        {
            if (_isInitialized) return;
            _geneDataContainer = GameManager.Instance?.ContainerService?.GeneDataContainer;
            if (_geneDataContainer == null)
                Debug.LogError($"[Egg:{name}] Missing GeneDataContainer.");
            _hatchTimer = hatchTime;
            _isInitialized = true;
            Debug.Log($"[Egg:{name}] Initialized ({hatchTime}s).");
        }

        private Chick CreateChickData()
        {
            var baby = new Chick { Gender = Random.value < .5f ? ChickenGender.Male : ChickenGender.Female };
            var list = Genes
                .Where(g => g.id > 0)
                .Select(g =>
                {
                    g.currentPassingChance = Mathf.Clamp01(g.currentPassingChance);
                    var def = _geneDataContainer.GetGeneById(g.id);
                    return def != null ? new Gene(def).ApplyChance(g.currentPassingChance) : null;
                })
                .Where(g => g != null)
                .ToArray();
            baby.Genes = list;
            return baby;
        }

        [Server]
        public bool AddGene(int id, float chance = 1f)
        {
            if (id <= 0) return false;
            chance = Mathf.Clamp01(chance);
            if (Genes.Any(g => g.id == id)) return false;
            Genes.Add(new GeneSync { id = id, currentPassingChance = chance });
            Debug.Log($"[Egg:{name}] Gene {id} @ {chance}");
            return true;
        }

        private static void Clamp(ref float v, float min, float max) =>
            v = Mathf.Clamp(v, min, max);
    }

    public static class GeneExtensions
    {
        public static Gene ApplyChance(this Gene g, float chance)
        {
            g.OverridePassingChance(chance);
            return g;
        }
    }
}
