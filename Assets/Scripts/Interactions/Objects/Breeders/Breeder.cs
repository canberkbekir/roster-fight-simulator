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
 
            var selectedItem = inventory.SelectedItem;
            if (selectedItem.Equals(InventoryItem.Empty))
            {
                Debug.LogWarning("No item selected in player inventory.");
                return;
            }

            if (!selectedItem.IsChicken)
            {
                Debug.LogWarning("Selected item is not a rooster.");
                return;
            }
 
            var rooster = selectedItem.Chicken;
            if (rooster == null)
            {
                Debug.LogError("Selected rooster data is null.");
                return;
            }
  
            inventory.RemoveItem(selectedItem.ItemId, 1, rooster);
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