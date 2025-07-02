using Creatures.Chickens.Base;
using System.Collections.Generic;
using Interactions.Base;
using InventorySystem.Base;
using Managers;
using Mirror;
using Players;
using Services;
using UnityEngine;

namespace Interactions.Objects.Breeders
{ 
    public class Breeder : InteractableBase
    {
        #region Inspector Fields

        [Header("Settings")]
        [Tooltip("Maximum number of chickens this breeder can hold")]
        [SerializeField] private int maxChickens = 10;
 

        #endregion

        #region Private Fields

        private ChickenSpawnerService _spawnerService;
        private readonly List<ChickenEntity> _spawnedChickens = new List<ChickenEntity>();

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets currently spawned chickens.
        /// </summary>
        public IReadOnlyList<ChickenEntity> CurrentChickens => _spawnedChickens.AsReadOnly();

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            _spawnerService = GameManager.Instance.ChickenSpawnerService;
            if (!_spawnerService)
            {
                Debug.LogError("ChickenSpawnerService not found on GameManager");
            }
        }

        #endregion

        #region Interaction

        /// <summary>
        /// Called when player interacts with breeder.
        /// Attempts to spawn a chicken if conditions are met.
        /// </summary>
        /// <param name="interactor">Player game object</param>
        public override void OnInteract(GameObject interactor)
        {
            base.OnInteract(interactor);

            if (!CanSpawnChicken(interactor, out var inventory, out var selectedChicken, out var spawnPosition))
                return;

            SpawnChicken(inventory, selectedChicken, spawnPosition);
        }

        #endregion

        #region Spawning Logic

        /// <summary>
        /// Checks all conditions required to spawn a chicken.
        /// </summary>
        private bool CanSpawnChicken(GameObject interactor, 
                                     out PlayerInventory inventory, 
                                     out Chicken chickenData, 
                                     out Vector3 spawnPosition)
        {
            inventory = null;
            chickenData = null;
            spawnPosition = Vector3.zero;
 
            if (!interactor.TryGetComponent<PlayerReferenceHandler>(out var handler) ||
                !(inventory = handler.PlayerInventory))
            {
                Debug.LogError("PlayerInventory not found on interactor");
                return false;
            }
 
            if (_spawnedChickens.Count >= maxChickens)
            {
                Debug.LogWarning("Breeder is at full capacity");
                return false;
            }
 
            var item = inventory.SelectedItem;
            if (!item.IsChicken || item.Equals(InventoryItem.Empty))
            {
                Debug.LogWarning("No chicken selected in inventory");
                return false;
            }

            chickenData = item.Chicken;
            if (chickenData == null)
            {
                Debug.LogError("Selected chicken data is null");
                return false;
            }
 
            if (!CalculateSpawnPosition(handler.PlayerCamera, out spawnPosition))
            {
                Debug.LogError("Failed to determine spawn position");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Spawns a chicken on the server and tracks it.
        /// </summary>
        private void SpawnChicken(PlayerInventory inventory, Chicken chickenData, Vector3 position)
        {
            var entity = _spawnerService.SpawnChickenServer(position, chickenData);
            if (!entity) return;

            entity.AssignBreeder(this);
            _spawnedChickens.Add(entity);

            var tracker = entity.gameObject.AddComponent<BreederChickenTracker>();
            tracker.Init(this, entity);

            // Remove chicken item from inventory
            inventory.RemoveItem(inventory.SelectedItem.ItemId, 1, chickenData);
        }

        /// <summary>
        /// Calculates a valid spawn position based on camera view or default distance.
        /// </summary>
        private bool CalculateSpawnPosition(Camera playerCamera, out Vector3 position)
        {
            position = Vector3.zero;
            if (!playerCamera) return false;

            var ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            if (Physics.Raycast(ray, out var hit))
            {
                position = hit.point;
            }
            else
            {
                position = playerCamera.transform.position + playerCamera.transform.forward * 2f;
            }
            return true;
        }

        #endregion

        #region Server Methods

        /// <summary>
        /// Removes a chicken from tracking when it is destroyed or removed.
        /// </summary>
        [Server]
        public void UnregisterChicken(ChickenEntity entity)
        {
            if (_spawnedChickens.Contains(entity))
                _spawnedChickens.Remove(entity);
        }

        #endregion
    }
}
