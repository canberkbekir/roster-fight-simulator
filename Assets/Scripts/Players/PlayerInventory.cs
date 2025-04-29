using System.Linq;
using InventorySystem.Base;
using Managers;
using Mirror;
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
        public void AddItem(string itemId, int qty, string metaJson)
        {
            if (items.Count >= maxSlots &&
                items.All(i => !(i.ItemId == itemId && i.MetaJson == metaJson && i.IsStackable)))
            {
                Debug.LogWarning($"Inventory full – cannot add {itemId}");
                return;
            }

            var existing = items.FirstOrDefault(i =>
                i.ItemId == itemId && i.MetaJson == metaJson && i.IsStackable);

            if (!existing.IsEmpty)
            {
                int newQty = Mathf.Min(existing.Quantity + qty, maxStackSize);
                items[items.IndexOf(existing)] = existing.WithQuantity(newQty);
            }
            else
            {
                int addQty = Mathf.Min(qty, maxStackSize);
                items.Add(new InventoryItem(itemId, ItemType.Resource, addQty, metaJson));
            }
        }

        [Server]
        public void AddRooster(string metaJson)
        {
            if (items.Count >= maxSlots)
            {
                Debug.LogWarning("Inventory full – cannot add new rooster");
                return;
            }

            items.Add(new InventoryItem(string.Empty, ItemType.Rooster, 1, metaJson));
        }

        [Server]
        public void RemoveItem(string itemId, int qty, string metaJson)
        {
            var existing = items.FirstOrDefault(i => i.ItemId == itemId && i.MetaJson == metaJson);
            if (existing.IsEmpty) return;

            int keep = existing.Quantity - qty;
            int idx = items.IndexOf(existing);
            if (keep > 0)
                items[idx] = existing.WithQuantity(keep);
            else
                items.RemoveAt(idx);
        }

        [Command]
        public void CmdAddItem(string id, int q, string m) => AddItem(id, q, m);

        [Command]
        public void CmdAddRooster(string m) => AddRooster(m);

        [Command]
        public void CmdRemoveItem(string id, int q, string m) => RemoveItem(id, q, m);

        [Command]
        public void CmdDropSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= items.Count) return;
            var item = items[slotIndex];

            Vector3 dropPos = transform.position + transform.forward * dropDistance;
            GameObject worldGO;
            if (item.IsRooster)
            {
                if (roosterItemWorldPrefab == null)
                    throw new UnassignedReferenceException("roosterItemWorldPrefab not assigned");
                worldGO = Instantiate(roosterItemWorldPrefab, dropPos, Quaternion.identity);
                worldGO.GetComponent<ItemWorld>().SetMetaJson(item.MetaJson);
            }
            else
            {
                var data = GameManager.Instance.ContainerManager.ItemDataContainer.Get(item.ItemId);
                worldGO = Instantiate(data.WorldPrefab, dropPos, Quaternion.identity);
            }

            NetworkServer.Spawn(worldGO, connectionToClient);
            RemoveItem(item.ItemId, 1, item.MetaJson);
        }
    }
}