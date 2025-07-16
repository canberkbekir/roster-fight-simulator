using System;
using Creatures.Genes.Base; 
using Managers;
using Mirror;
using Services;
using UnityEngine; 

namespace Creatures.Chickens.Eggs
{
    public class EggEntity : NetworkBehaviour
    {
        [Header("Settings")] [SerializeField] private float hatchTime = 5f;
        [SerializeField] private float minHatchTime = 1f;
        [SerializeField] private float maxHatchTime = 60f;

        private Gene[] _genes;
        public Gene[] Genes => _genes ?? Array.Empty<Gene>();

        [SyncVar] public bool IsIncubating;

        private float _hatchTimer;
        private bool _isInitialized;

        public event Action<EggEntity> OnHatched;

        public float RemainingHatchTime => Mathf.Max(0f, _hatchTimer);
        public bool IsInitialized => _isInitialized;
        public float HatchTime => hatchTime;

        private ChickenSpawnerService _chickenSpawnerService;

        private void Awake()
        {
            _chickenSpawnerService = GameManager.Instance?.ChickenSpawnerService;
            if (_chickenSpawnerService == null)
                Debug.LogError($"[Egg:{name}] Missing ChickenSpawnerService.");
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            Clamp(ref hatchTime, minHatchTime, maxHatchTime);
            Init();
        }

        [ServerCallback]
        private void Update()
        {
            if (!_isInitialized || !IsIncubating) return;

            if ((_hatchTimer -= Time.deltaTime) <= 0f)
            {
                OnHatched?.Invoke(this);
                Hatch();
            }
        }

        [Server]
        public void StartIncubation()
        {
            if (!_isInitialized || IsIncubating) return;
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
            try
            {
                _chickenSpawnerService.SpawnChickFromEggServer(transform.position, _genes);
                NetworkServer.Destroy(gameObject);
                Debug.Log($"[Egg:{name}] Hatched");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Egg:{name}] Hatch error: {ex.Message}");
            }
        }

        private void Init()
        {
            if (_isInitialized) return;
            _hatchTimer = hatchTime;
            _isInitialized = true;
            Debug.Log($"[Egg:{name}] Initialized ({hatchTime}s).");
        }

        public void SetGenes(Gene[] newGenes)
        {
            _genes = newGenes ?? Array.Empty<Gene>();
        }

        private static void Clamp(ref float v, float min, float max) =>
            v = Mathf.Clamp(v, min, max);
    }
}