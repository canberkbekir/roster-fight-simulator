using Creatures.Chickens.Base;
using Interactions.Base;
using InventorySystem.Base;
using Managers;
using Players;
using Services;
using UnityEngine;

namespace Interactions.Objects.Breeders
{
    public class Breeder : InteractableBase
    {
        [Header("Settings")]
        [SerializeField] private int maxChickens = 10;

        [Header("Spawn Settings")]
        [SerializeField] private Transform chickenContainer; 

        private ChickenSpawnerService _spawner;

        public ChickenEntity[] CurrentChickens => 
            chickenContainer.GetComponentsInChildren<ChickenEntity>();

        void Awake()
        {
            _spawner = GameManager.Instance.ChickenSpawnerService;
            if (_spawner == null)
                Debug.LogError("ChickenSpawnerService not initialized");
        }

        public override void OnInteract(GameObject interactor)
        {
            base.OnInteract(interactor);
 
            if (!TryPrepareSpawn(interactor, out var inventory, out var chicken, out var spawnPos))
                return;
 
            _spawner.SpawnChickenServer(spawnPos, chicken);
            inventory.RemoveItem(inventory.SelectedItem.ItemId, 1, chicken);
        }

        private bool TryPrepareSpawn(GameObject interactor, 
            out PlayerInventory inventory,
            out Chicken chicken, 
            out Vector3 spawnPos)
        {
            inventory = null;
            chicken    = null;
            spawnPos   = default;
 
            if (!interactor.TryGetComponent<PlayerReferenceHandler>(out var handler) ||
                (inventory = handler.PlayerInventory) == null)
            {
                Debug.LogError("PlayerInventory not found");
                return false;
            }
 
            if (CurrentChickens.Length >= maxChickens)
            {
                Debug.LogWarning("Breeder is full");
                return false;
            }
 
            var item = inventory.SelectedItem;
            if (!item.IsChicken || item.Equals(InventoryItem.Empty))
            {
                Debug.LogWarning("No chicken selected");
                return false;
            }
 
            chicken = item.Chicken;
            if (chicken == null)
            {
                Debug.LogError("Selected chicken data is null");
                return false;
            }
 
            if (!TryGetSpawnPosition(handler.PlayerCamera, out spawnPos))
            {
                Debug.LogError("Could not compute spawn position");
                return false;
            }

            return true;
        }

        private bool TryGetSpawnPosition(Camera cam, out Vector3 pos)
        {
            pos = default;
            if (!cam) 
                return false;

            var ray = new Ray(cam.transform.position, cam.transform.forward);
            if (Physics.Raycast(ray, out var hitInfo))
            {
                pos = hitInfo.point;
            }
            else
            {
                pos = cam.transform.position + cam.transform.forward * 2f;
            }
            return true;
        }
    }
}
