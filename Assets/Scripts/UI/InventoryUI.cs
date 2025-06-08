using System;
using InventorySystem.Base;
using Managers;
using Mirror;
using Players;
using UnityEngine;

namespace UI
{
    public class InventoryUI : BaseUI
    {

        [SerializeField, Tooltip("Slot prefab with InventorySlotUI component.")]
        private InventorySlotUI slotPrefab; 

        [SerializeField, Tooltip("Parent transform for instantiated slots.")]
        private Transform slotsParent;

        [Header("Settings")]
        [Tooltip("Icon to use for rooster items.")]
        [SerializeField] private Sprite defaultRoosterIcon;
        
        private InventorySlotUI[] _slots;
        private PlayerInventory   _inventory;
        private PlayerReferenceHandler _playerReferenceHandler;


        private void Start()
        {
            if (!NetworkClient.active) return;
            var localPlayer = NetworkClient.localPlayer;
            if (localPlayer == null) return; 
        }

        private void OnEnable()
        {
            PlayerReferenceHandler.LocalPlayerReady += OnLocalPlayerReady;
        }
        
        private void OnDisable()
        {
            PlayerReferenceHandler.LocalPlayerReady -= OnLocalPlayerReady; 
        }

        private void OnLocalPlayerReady(PlayerReferenceHandler obj)
        {
            _playerReferenceHandler = obj ?? throw new System.Exception("PlayerReferenceHandler is null.");
            _inventory = _playerReferenceHandler.PlayerInventory 
                         ?? throw new System.Exception("PlayerInventory is null.");

            _slots = new InventorySlotUI[_inventory.MaxSlots];
            for (int i = 0; i < _slots.Length; i++)
            {
                _slots[i] = Instantiate(slotPrefab, slotsParent);
                _slots[i].Clear();
            }
 
            _inventory.items.Callback += OnInventoryChanged;
            _inventory.OnSelectedSlotChanged += OnSelectedSlotChanged;
            RefreshAll();
        }

        private void OnSelectedSlotChanged(int obj, InventoryItem item)
        { 
            for (var i = 0; i < _slots.Length; i++)
            {
                if (i == obj)
                    _slots[i].Highlight();
                else
                    _slots[i].Unhighlight();
            }
            
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
                        .ContainerService.ItemDataContainer
                        .Get(item.ItemId);
                    var icon = data != null ? data.Icon : null;
                    _slots[i].SetItem(icon, item.Quantity);
                }

                i++;
            }
        }
    }
}
