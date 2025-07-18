﻿using System;
using System.Linq;
using Creatures.Chickens.Base;
using Creatures.Chickens.Roosters; 
using InventorySystem.Base;
using Managers;
using Mirror;
using UnityEngine;

namespace Players
{
    public class PlayerInventory : NetworkBehaviour
    {
        [Header("Inventory Settings")]
        [SerializeField, Tooltip("Max distinct slots the inventory can hold.")]
        private int maxSlots = 20;

        [SerializeField, Tooltip("Max items per stack (ignored for roosters).")]
        private int maxStackSize = 99;

        [Header("Drop Settings")]
        [SerializeField, Tooltip("Prefab with ItemWorld for dropping roosters.")]
        private GameObject roosterItemWorldPrefab;

        [SerializeField, Tooltip("Distance in front of the player to drop items.")]
        private float dropDistance = 1.5f;

        public int MaxSlots => maxSlots;
        public int SelectedSlot { get; private set; } = 0;
        
        public InventoryItem SelectedItem => 
            SelectedSlot >= 0 && SelectedSlot < items.Count ? items[SelectedSlot] : InventoryItem.Empty;

        #region Events
        public event Action<InventoryItem> OnItemDropped;
        public event Action<InventoryItem> OnItemAdded;
        public event Action<InventoryItem> OnItemRemoved;
        public event Action<int,InventoryItem> OnSelectedSlotChanged;
        #endregion

        // Mirror-synced list of fixed-size inventory slots
        public class SyncListInv : SyncList<InventoryItem> { }
        public readonly SyncListInv items = new SyncListInv();

        public override void OnStartServer()
        {
            base.OnStartServer();
            items.Clear();
            for (int i = 0; i < maxSlots; i++)
                items.Add(InventoryItem.Empty);
            
            ChangeSelectedSlot(0);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (isLocalPlayer)
                items.Callback += OnInvChanged;
        }

        private void Update()
        {
            if (!isLocalPlayer) return;

            for (int i = 0; i < MaxSlots && i < 9; i++)
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                    ChangeSelectedSlot(i);

            // Mouse wheel
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll > 0f)
                ChangeSelectedSlot((SelectedSlot + 1) % MaxSlots);
            else if (scroll < 0f)
                ChangeSelectedSlot((SelectedSlot - 1 + MaxSlots) % MaxSlots);

            // Drop item
            if (Input.GetKeyDown(KeyCode.Q))
                CmdDropSlot(SelectedSlot);
        }

        private void OnInvChanged(SyncList<InventoryItem>.Operation op, int idx,
                                  InventoryItem oldItem, InventoryItem newItem)
        { 
            ChangeSelectedSlot(SelectedSlot);
        }

        private void ChangeSelectedSlot(int index)
        {
            if (index < 0 || index >= MaxSlots) return;
            SelectedSlot = index;
            OnSelectedSlotChanged?.Invoke(SelectedSlot, items[SelectedSlot]);
        }
        public InventoryItem GetItemAt(int index)
        {
            if (index < 0 || index >= items.Count) return InventoryItem.Empty;
            return items[index];
        }  

        [Server]
        public void AddItem(string itemId, int qty, Chicken chicken)
        { 
            var existing = items.FirstOrDefault(i =>
                i.ItemId == itemId && i.Chicken == chicken && i.IsStackable && !i.IsEmpty);

            if (!existing.IsEmpty)
            {
                var idx = items.IndexOf(existing);
                var newQty = Mathf.Min(existing.Quantity + qty, maxStackSize);
                items[idx] = existing.WithQuantity(newQty);
            }
            else
            { 
                var idx = items.FindIndex(i => i.IsEmpty);
                if (idx < 0)
                {
                    Debug.LogWarning($"Inventory full – cannot add {itemId}");
                    return;
                }
                var addQty = Mathf.Min(qty, maxStackSize);
                items[idx] = new InventoryItem(itemId, ItemType.Resource, addQty, chicken);
            }

            OnItemAdded?.Invoke(new InventoryItem(itemId, ItemType.Resource, qty, chicken));
        }

        [Server]
        public void AddChicken(Chicken chicken)
        {
            var idx = items.FindIndex(i => i.IsEmpty);
            if (idx < 0)
            {
                Debug.LogWarning("Inventory full – cannot add new rooster");
                return;
            }

            items[idx] = new InventoryItem(string.Empty, ItemType.Chicken, 1, chicken);
            OnItemAdded?.Invoke(items[idx]);
        }

        [Server]
        public void RemoveItem(string itemId, int qty, Chicken chicken)
        {
            var existing = items.FirstOrDefault(i =>
                i.ItemId == itemId && i.Chicken == chicken && !i.IsEmpty);
            if (existing.IsEmpty) return;

            var idx = items.IndexOf(existing);
            var keep = existing.Quantity - qty;
            if (keep > 0)
            {
                items[idx] = existing.WithQuantity(keep);
            }
            else
            {
                // replace with empty slot
                items[idx] = InventoryItem.Empty;
            }

            OnItemRemoved?.Invoke(existing);
        }
        

        [Command]
        public void CmdDropSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= items.Count) return;
            var item = items[slotIndex];
            if (item.IsEmpty) return;

            var dropPos = transform.position + transform.forward * dropDistance;

            if (item.IsChicken)
                GameManager.Instance.ChickenSpawnerService.SpawnChickenServer(dropPos, item.Chicken);
            else
            {
                var data = GameManager.Instance.ContainerService.ItemDataContainer.Get(item.ItemId);
                Instantiate(data.WorldPrefab, dropPos, Quaternion.identity);
            }

            RemoveItem(item.ItemId, 1, item.Chicken);
            OnItemDropped?.Invoke(item);
        }
    }
}
