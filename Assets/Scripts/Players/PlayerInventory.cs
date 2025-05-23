using System.Linq;
using InventorySystem.Base;
using Managers;
using Mirror;
using Roosters;
using UnityEngine;

namespace Players
{
    public class PlayerInventory : NetworkBehaviour
    {
        [Header("Inventory Settings")] [SerializeField, Tooltip("Max distinct slots the inventory can hold.")]
        private int maxSlots = 20;

        [SerializeField, Tooltip("Max items per stack (ignored for roosters).")]
        private int maxStackSize = 99;

        [Header("Drop Settings")] [SerializeField, Tooltip("Prefab with ItemWorld for dropping roosters.")]
        private GameObject roosterItemWorldPrefab;

        [SerializeField, Tooltip("Distance in front of the player to drop items.")]
        private float dropDistance = 1.5f;

        public int MaxSlots => maxSlots;

        // Tracks the currently selected slot (0-based)
        public int SelectedSlot { get; private set; } = 0;

        public class SyncListInv : SyncList<InventoryItem>
        {
        }

        public readonly SyncListInv items = new SyncListInv();

        private void Update()
        {
            if (!isLocalPlayer) return;
 
            for (int i = 0; i < MaxSlots && i < 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                    SelectedSlot = i;
            }

            // Mouse wheel to cycle slots
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll > 0f)
                SelectedSlot = (SelectedSlot + 1) % MaxSlots;
            else if (scroll < 0f)
                SelectedSlot = (SelectedSlot - 1 + MaxSlots) % MaxSlots;

            // Drop with Q
            if (Input.GetKeyDown(KeyCode.Q))
            {
                CmdDropSlot(SelectedSlot);
            }
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (isLocalPlayer)
                items.Callback += OnInvChanged;
        }

        private void OnInvChanged(SyncList<InventoryItem>.Operation op, int idx,
            InventoryItem oldItem, InventoryItem newItem)
        {
            // Clamp selected slot when inventory shrinks
            if (SelectedSlot >= items.Count)
                SelectedSlot = Mathf.Max(items.Count - 1, 0);

            Debug.Log($"Inventory changed: {op} at {idx}, selected={SelectedSlot}");
        }

        [Server]
        public void AddItem(string itemId, int qty, Rooster rooster)
        {
            if (items.Count >= maxSlots &&
                items.All(i => !(i.ItemId == itemId && i.Rooster == rooster && i.IsStackable)))
            {
                Debug.LogWarning($"Inventory full – cannot add {itemId}");
                return;
            }

            var existing = items.FirstOrDefault(i =>
                i.ItemId == itemId && i.Rooster == rooster && i.IsStackable);

            if (!existing.IsEmpty)
            {
                int newQty = Mathf.Min(existing.Quantity + qty, maxStackSize);
                items[items.IndexOf(existing)] = existing.WithQuantity(newQty);
            }
            else
            {
                int addQty = Mathf.Min(qty, maxStackSize);
                items.Add(new InventoryItem(itemId, ItemType.Resource, addQty, rooster));
            }
        }

        [Server]
        public void AddRooster(Rooster rooster)
        {
            if (items.Count >= maxSlots)
            {
                Debug.LogWarning("Inventory full – cannot add new rooster");
                return;
            }

            items.Add(new InventoryItem(string.Empty, ItemType.Rooster, 1, rooster));
        }

        [Server]
        public void RemoveItem(string itemId, int qty, Rooster rooster)
        {
            var existing = items.FirstOrDefault(i => i.ItemId == itemId && i.Rooster == rooster);
            if (existing.IsEmpty) return;

            int keep = existing.Quantity - qty;
            int idx = items.IndexOf(existing);
            if (keep > 0)
                items[idx] = existing.WithQuantity(keep);
            else
                items.RemoveAt(idx);
        }

        [Command]
        public void CmdAddItem(string id, int q, Rooster r) => AddItem(id, q, r);

        [Command]
        public void CmdAddRooster(Rooster r) => AddRooster(r);

        [Command]
        public void CmdRemoveItem(string id, int q, Rooster r) => RemoveItem(id, q, r);

        [Command]
        public void CmdDropSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= items.Count) return;
            var item = items[slotIndex];
            var dropPos = transform.position + transform.forward * dropDistance;

            if (item.IsRooster)
            {
                // only spawn via your Networked spawner
                GameManager.Instance.RoosterSpawnerManager.RequestSpawnRoosterAt(dropPos, item.Rooster);
            }
            else
            {
                var data = GameManager.Instance.ContainerManager.ItemDataContainer.Get(item.ItemId);
                Instantiate(data.WorldPrefab, dropPos, Quaternion.identity);
            }

            RemoveItem(item.ItemId, 1, item.Rooster);
        }

    }
}