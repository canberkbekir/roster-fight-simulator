using System;
using Creatures.Roosters;
using UnityEngine;

namespace InventorySystem.Base
{
    [Serializable]
    public readonly struct InventoryItem : IEquatable<InventoryItem>
    {
        [SerializeField] private readonly string _itemId;
        [SerializeField] private readonly ItemType _itemType;
        [SerializeField] private readonly int    _quantity;
        [SerializeField] private readonly Rooster _rooster;

        public string ItemId    => _itemId;       // matches ItemData.ItemId
        public ItemType Type    => _itemType;     // e.g. Resource, Equipment, Rooster
        public int Quantity     => _quantity;     // how many in this stack
        public Rooster Rooster  => _rooster;     // optional JSON blob

        public bool HasRooster => Rooster != null;
        public bool IsRooster   => _itemType == ItemType.Rooster;
        public bool IsStackable => !IsRooster;    // roosters are unique/non-stackable
        public bool IsEmpty     => string.IsNullOrEmpty(_itemId) && !HasRooster;

        public static InventoryItem Empty => default;

        // Primary constructor
        public InventoryItem(string itemId, ItemType type, int quantity = 1, Rooster rooster = null)
        {
            _itemId   = itemId;
            _itemType = type;
            _quantity = quantity;
            _rooster = rooster;
        }

        // Returns a copy with a different quantity
        public InventoryItem WithQuantity(int newQty)
            => new InventoryItem(_itemId, _itemType, newQty, _rooster); 
        public override string ToString()
            => $"{_itemType}:{_itemId} x{_quantity}" + (HasRooster ? " [meta]" : "");

        // Equality implementations for SyncList lookups
        public bool Equals(InventoryItem other)
            => _itemId   == other._itemId
               && _itemType == other._itemType
               && _quantity == other._quantity
               && _rooster == other._rooster;

        public override bool Equals(object obj)
            => obj is InventoryItem other && Equals(other);

        public override int GetHashCode()
            => HashCode.Combine(_itemId, _itemType, _quantity, _rooster);
    }
}
