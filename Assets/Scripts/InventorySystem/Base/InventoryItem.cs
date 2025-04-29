using System;
using UnityEngine;

namespace InventorySystem.Base
{
    [Serializable]
    public readonly struct InventoryItem : IEquatable<InventoryItem>
    {
        [SerializeField] private readonly string _itemId;
        [SerializeField] private readonly ItemType _itemType;
        [SerializeField] private readonly int    _quantity;
        [SerializeField] private readonly string _metaJson;

        public string ItemId    => _itemId;       // matches ItemData.ItemId
        public ItemType Type    => _itemType;     // e.g. Resource, Equipment, Rooster
        public int Quantity     => _quantity;     // how many in this stack
        public string MetaJson  => _metaJson;     // optional JSON blob

        public bool HasMetadata => !string.IsNullOrEmpty(_metaJson);
        public bool IsRooster   => _itemType == ItemType.Rooster;
        public bool IsStackable => !IsRooster;    // roosters are unique/non-stackable
        public bool IsEmpty     => string.IsNullOrEmpty(_itemId) && !HasMetadata;

        // Primary constructor
        public InventoryItem(string itemId, ItemType type, int quantity = 1, string metaJson = null)
        {
            _itemId   = itemId;
            _itemType = type;
            _quantity = quantity;
            _metaJson = metaJson;
        }

        // Returns a copy with a different quantity
        public InventoryItem WithQuantity(int newQty)
            => new InventoryItem(_itemId, _itemType, newQty, _metaJson);

        // Returns a copy with updated metadata
        public InventoryItem WithMetadata(string metaJson)
            => new InventoryItem(_itemId, _itemType, _quantity, metaJson);

        public override string ToString()
            => $"{_itemType}:{_itemId} x{_quantity}" + (HasMetadata ? " [meta]" : "");

        // Equality implementations for SyncList lookups
        public bool Equals(InventoryItem other)
            => _itemId   == other._itemId
               && _itemType == other._itemType
               && _quantity == other._quantity
               && _metaJson == other._metaJson;

        public override bool Equals(object obj)
            => obj is InventoryItem other && Equals(other);

        public override int GetHashCode()
            => HashCode.Combine(_itemId, _itemType, _quantity, _metaJson);
    }
}
