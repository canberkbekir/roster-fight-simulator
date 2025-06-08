using System.Collections.Generic;
using System.Linq;
using Creatures.Chickens.Roosters;
using Creatures.Chickens.Roosters.Components; 
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
        [Header("Settings")]
        [SerializeField] private int maxRoosters = 10;

        [Space] [Header("Spawn Settings")]
        [SerializeField] private List<Transform> spawnPoints;
        [SerializeField] private Transform parentForRoosterEntities;
        public List<Transform> SpawnPoints => spawnPoints;
        public int MaxRoosters => maxRoosters; 
        private ChickenSpawnerService _chickenSpawnerService; 

        public RoosterEntity[] RoosterEntities => parentForRoosterEntities.GetComponentsInChildren<RoosterEntity>();
        public Rooster[] CurrentRoosters =>null;

        private void Awake()
        {
            _chickenSpawnerService = GameManager.Instance.ChickenSpawnerService;
            
            if (_chickenSpawnerService == null)
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

            if (!selectedItem.IsRooster)
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
 
            // AddRooster(rooster);
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
        
        public void AddRooster(Rooster rooster)
        {
            if (RoosterEntities.Length >= maxRoosters)
            {
                Debug.LogWarning("Cannot add more roosters, max limit reached.");
                return;
            }
 
            var parentIdentity = parentForRoosterEntities
                .GetComponent<NetworkIdentity>();
            if (parentIdentity == null)
            {
                Debug.LogError("Parent GameObject has no NetworkIdentity!");
                return;
            }
 
            // _roosterSpawnerManager.RequestSpawnRoosterWithParentAt(
            //     GetRandomSpawnPoint(),
            //     rooster,
            //     parentIdentity,
            //     Quaternion.identity
            // );
        }
        
        private Vector3 GetRandomSpawnPoint()
        {
            if (spawnPoints.Count == 0)
            {
                Debug.LogError("No spawn points available.");
                return Vector3.zero;
            }

            int randomIndex = Random.Range(0, spawnPoints.Count);
            return spawnPoints[randomIndex].position;
        }
    }
}