using System;
using System.Linq;
using Creatures.Genes;
using Creatures.Genes.Base;
using Interactions.Objects.Nests;
using Managers;
using Mirror;
using Services;
using UnityEngine;
using Utils;

namespace Creatures.Chickens.Eggs.Components
{
    public class EggEntity : NetworkBehaviour
    {
        #region Settings
        [Header("Settings")]
        [SerializeField, Min(1f)] private float hatchTime      = 5f;
        [SerializeField]           private float minHatchTime   = 1f;
        [SerializeField]           private float maxHatchTime   = 60f;
        #endregion

        #region SyncLists
        public class SyncGeneList : SyncList<GeneSync> { }
        public readonly SyncGeneList SyncedGenes = new SyncGeneList();
        #endregion

        #region SyncVars
        [SyncVar] private bool _isIncubating;
        [SyncVar] private Nest _assignedNest;
        [SyncVar] private bool _isEggFertilized;
        #endregion

        #region Public Properties
        public Gene[]                Genes               => GeneHelper.GeneSyncToGene(SyncedGenes.ToArray());
        public bool                  IsIncubating        => _isIncubating;
        public Nest                  AssignedNest        => _assignedNest;
        public bool                  IsFertilized        => _isEggFertilized;
        public bool                  IsAssignedToNest    => _assignedNest != null;
        public float                 RemainingHatchTime  => Mathf.Max(0f, _hatchTimer);
        public bool                  IsInitialized       { get; private set; }
        #endregion

        #region Events
        public event Action<EggEntity> OnHatched;
        #endregion

        #region Private Fields
        private ChickenSpawnerService _chickenSpawnerService;
        private float                 _hatchTimer;
        #endregion

        #region Unity Callbacks
        public override void OnStartServer()
        {
            base.OnStartServer();
            hatchTime = Mathf.Clamp(hatchTime, minHatchTime, maxHatchTime);
        }

        [ServerCallback]
        private void Update()
        {
            if (!IsInitialized || !_isIncubating) return;

            _hatchTimer -= Time.deltaTime;
            if (_hatchTimer > 0f) return;

            OnHatched?.Invoke(this);
            Hatch();
        }
        #endregion

        #region Server Methods
        [Server]
        public void Init(Nest nest)
        {
            if (IsInitialized) return;

            _chickenSpawnerService = GameManager.Instance?.ChickenSpawnerService
                ?? throw new InvalidOperationException("ChickenSpawnerService not found");
            _assignedNest = nest;
            _hatchTimer   = hatchTime;
            IsInitialized = true;
            Debug.Log($"[Egg:{name}] Initialized for {hatchTime:F1}s");
        }

        [Server]
        public void StartIncubation()
        {
            if (!IsInitialized || _isIncubating) return;

            _hatchTimer    = hatchTime;
            _isIncubating  = true;
            RpcPlayIncubationVFX();
            Debug.Log($"[Egg:{name}] Incubation started ({hatchTime:F1}s)");
        }

        [Server]
        public void StopIncubation()
        {
            if (!IsInitialized || !_isIncubating) return;

            _isIncubating = false;
            Debug.Log($"[Egg:{name}] Incubation stopped");
        }

        [Server]
        public void SetGenes(Gene[] genes, bool isFertilizing = true)
        {
            SyncedGenes.Clear();
            if (genes == null || genes.Length == 0) return;

            foreach (var sync in GeneHelper.GeneToGeneSync(genes))
                SyncedGenes.Add(sync);

            _isEggFertilized = isFertilizing;
        }

        [Server]
        public void SetNest(Nest nest)
        {
            _assignedNest = nest ?? throw new ArgumentNullException(nameof(nest));
            Debug.Log($"[Egg:{name}] Nest set to {_assignedNest.name}");
        }
        #endregion

        #region Hatch Logic
        [Server]
        private void Hatch()
        {
            try
            {
                _chickenSpawnerService.SpawnChickFromEggServer(transform.position, Genes);
                NetworkServer.Destroy(gameObject);
                Debug.Log($"[Egg:{name}] Hatched");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Egg:{name}] Hatch error: {ex}");
            }
        }
        #endregion

        #region Client RPCs
        [ClientRpc]
        private void RpcPlayIncubationVFX() =>
            Debug.Log($"[Egg:{name}] Playing incubation VFX");
        #endregion
    }
}
