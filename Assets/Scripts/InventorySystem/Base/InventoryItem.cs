﻿using System;
using Creatures.Chickens.Base; 
using UnityEngine;

namespace InventorySystem.Base
{
    [Serializable]
    public readonly struct InventoryItem : IEquatable<InventoryItem>
    {
        [SerializeField] private readonly string _itemId;
        [SerializeField] private readonly ItemType _itemType;
        [SerializeField] private readonly int    _quantity;
        [SerializeField] private readonly Chicken _chicken;

        public string ItemId    => _itemId;       // matches ItemData.ItemId
        public ItemType Type    => _itemType;     // e.g. Resource, Equipment, Rooster
        public int Quantity     => _quantity;     // how many in this stack
        public Chicken Chicken  => _chicken;     // optional JSON blob 
        public bool IsChicken   => _itemType == ItemType.Chicken;
        public bool IsStackable => !IsChicken;     
        public bool IsEmpty     => string.IsNullOrEmpty(_itemId) && !IsChicken;

        public static InventoryItem Empty => default;

        // Primary constructor
        public InventoryItem(string itemId, ItemType type, int quantity = 1, Chicken chicken = null)
        {
            _itemId   = itemId;
            _itemType = type;
            _quantity = quantity;
            _chicken = chicken;
        }

        // Returns a copy with a different quantity
        public InventoryItem WithQuantity(int newQty)
            => new InventoryItem(_itemId, _itemType, newQty, _chicken); 
        public override string ToString()
            => $"{_itemType}:{_itemId} x{_quantity}" + (IsChicken ? " [meta]" : "");

        // Equality implementations for SyncList lookups
        public bool Equals(InventoryItem other)
            => _itemId   == other._itemId
               && _itemType == other._itemType
               && _quantity == other._quantity
               && _chicken == other._chicken;

        public override bool Equals(object obj)
            => obj is InventoryItem other && Equals(other);

        public override int GetHashCode()
            => HashCode.Combine(_itemId, _itemType, _quantity, _chicken);
    }
}
