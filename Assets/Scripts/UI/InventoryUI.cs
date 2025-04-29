using InventorySystem.Base;
using Managers;
using Mirror;
using Players;
using UnityEngine;

namespace UI
{
    public class InventoryUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private PlayerReferenceHandler playerReferenceHandler;

        [SerializeField, Tooltip("Slot prefab with InventorySlotUI component.")]
        private InventorySlotUI slotPrefab; 

        [SerializeField, Tooltip("Parent transform for instantiated slots.")]
        private Transform slotsParent;

        [Header("Settings")]
        [Tooltip("Icon to use for rooster items.")]
        [SerializeField] private Sprite defaultRoosterIcon;
        
        private InventorySlotUI[] _slots;
        private PlayerInventory   _inventory;

        private void Start()
        {
            if (!NetworkClient.active) return;
            var localPlayer = NetworkClient.localPlayer;
            if (localPlayer == null) return;

            _inventory = playerReferenceHandler.PlayerInventory 
                         ?? throw new System.Exception("PlayerInventory is null.");

            _slots = new InventorySlotUI[_inventory.MaxSlots];
            for (int i = 0; i < _slots.Length; i++)
            {
                _slots[i] = Instantiate(slotPrefab, slotsParent);
                _slots[i].Clear();
            }
 
            _inventory.items.Callback += OnInventoryChanged;
            RefreshAll();
        }

        private void OnDestroy()
        {
            if (_inventory != null)
                _inventory.items.Callback -= OnInventoryChanged;
        }

        private void OnInventoryChanged(SyncList<InventoryItem>.Operation op, int index,
                                        InventoryItem oldItem, InventoryItem newItem)
        {
            RefreshAll();
        }

        private void RefreshAll()
        {
            // Clear all slots
            foreach (var slot in _slots)
                slot.Clear();
 
            int i = 0;
            foreach (var item in _inventory.items)
            {
                if (i >= _slots.Length) break;

                if (item.IsRooster)
                {
                    _slots[i].SetItem(defaultRoosterIcon,null);
                }
                else
                {
                    var data = GameManager.Instance
                        .ContainerManager.ItemDataContainer
                        .Get(item.ItemId);
                    var icon = data != null ? data.Icon : null;
                    _slots[i].SetItem(icon, item.Quantity);
                }

                i++;
            }
        }
    }
}
