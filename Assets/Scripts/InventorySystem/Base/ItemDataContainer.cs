using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem.Base
{
    [CreateAssetMenu(fileName = "ItemDataContainer", menuName = "Items/ItemDataContainer", order = 0)]
    public class ItemDataContainer : ScriptableObject
    {
        [Tooltip("Drag all your ItemData assets here.")]
        public List<ItemData> allItems = new List<ItemData>();

        private Dictionary<string, ItemData> _lookup;

        private void OnEnable()
        {
            _lookup = new Dictionary<string, ItemData>(allItems.Count);
            foreach (var item in allItems)
            {
                if (!_lookup.ContainsKey(item.ItemId))
                    _lookup.Add(item.ItemId, item);
                else
                    Debug.LogWarning($"Duplicate ItemId '{item.ItemId}' in database", this);
            }
        }

        /// <summary>
        /// Returns the ItemData for a given ID, or null if not found.
        /// </summary>
        public ItemData Get(string itemId)
        {
            if (_lookup != null && _lookup.TryGetValue(itemId, out var data))
                return data;

            Debug.LogError($"[ItemDatabaseSO] No item with ID '{itemId}'", this);
            return null;
        }
    }
}