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
        [SerializeField] private int maxChicken = 10;

        [Space] [Header("Spawn Settings")] 
        [SerializeField] private Transform parentForRoosterEntities; 
        public int MaxChicken => maxChicken;  
        public ChickenEntity[] CurrentChickens =>parentForRoosterEntities.GetComponentsInChildren<ChickenEntity>();
        
        private ChickenSpawnerService _chickenSpawnerService;   

        private void Awake()
        {
            _chickenSpawnerService = GameManager.Instance.ChickenSpawnerService;
            
            if (!_chickenSpawnerService)
            {
                Debug.LogError("RoosterSpawnerManager is not initialized.");
            }  
        } 
        public override void OnInteract(GameObject interactor)
        {
            base.OnInteract(interactor);

            if (!TryGetInventory(interactor, out var inventory))
                return;

            if (CurrentChickens.Length >= maxChicken)
            {
                Debug.LogWarning("Breeder reached maximum capacity.");
                return;
            }

            var selectedItem = inventory.SelectedItem;
            if (selectedItem.Equals(InventoryItem.Empty))
            {
                Debug.LogWarning("No item selected in player inventory.");
                return;
            }

            if (!selectedItem.IsChicken)
            {
                Debug.LogWarning("Selected item is not a chicken.");
                return;
            }

            var chicken = selectedItem.Chicken;
            if (chicken == null)
            {
                Debug.LogError("Selected chicken data is null.");
                return;
            }

            if (!TryGetSpawnPosition(interactor, out var spawnPos))
            {
                Debug.LogError("Unable to determine spawn position.");
                return;
            }

            _chickenSpawnerService.SpawnChickenServer(spawnPos, chicken);

            inventory.RemoveItem(selectedItem.ItemId, 1, chicken);
        }

        private bool TryGetSpawnPosition(GameObject interactor, out Vector3 pos)
        {
            pos = Vector3.zero;
            if (!interactor.TryGetComponent<PlayerReferenceHandler>(out var handler))
                return false;

            var cam = handler.PlayerCamera;
            if (cam == null)
                return false;

            var ray = new Ray(cam.transform.position, cam.transform.forward);
            if (Physics.Raycast(ray, out var hit))
            {
                pos = hit.point;
            }
            else
            {
                pos = cam.transform.position + cam.transform.forward * 2f;
            }

            return true;
        }
        
        private bool TryGetInventory(GameObject newInteractor, out PlayerInventory inventory)
        {
            if (!newInteractor.TryGetComponent<PlayerReferenceHandler>(out var handler))
            {
                Debug.LogError("PlayerReferenceHandler not found on interactor.");
                inventory = null;
                return false;
            }

            inventory = handler.PlayerInventory;
            if (inventory) return true;
            Debug.LogError("PlayerInventory not found on PlayerReferenceHandler.");
            return false; 
        }  
    }
}