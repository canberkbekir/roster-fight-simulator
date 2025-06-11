using System;
using UnityEngine;

namespace InventorySystem.Base
{ 
    [CreateAssetMenu(menuName = "Items/Item Data")]
    public class ItemData : ScriptableObject
    {
        [SerializeField, HideInInspector]
        private string itemId;
        /// <summary>
        /// Unique identifier, generated automatically in the Unity Editor.
        /// </summary>
        public string ItemId => itemId;

        [Header("Item Info")]
        [SerializeField]
        private string displayName;
        /// <summary>
        /// The name shown to the player.
        /// </summary>
        public string DisplayName => displayName;

        [SerializeField]
        private Sprite icon;
        /// <summary>
        /// Icon used in UI inventories.
        /// </summary>
        public Sprite Icon => icon;

        [SerializeField]
        private ItemType itemType;
        /// <summary>
        /// Category of this item.
        /// </summary>
        public ItemType ItemType => itemType;

        [Header("Stacking")]
        [SerializeField]
        private bool isStackable = true;
        /// <summary>
        /// Can multiple of this item occupy one inventory slot?
        /// </summary>
        public bool IsStackable => isStackable;

        [SerializeField, Min(1)]
        private int maxStack = 99;
        /// <summary>
        /// Maximum number of items per stack. Always 1 if not stackable.
        /// </summary>
        public int MaxStack => isStackable ? maxStack : 1;

        [Header("World")]
        [SerializeField]
        private GameObject worldPrefab;
        /// <summary>
        /// Prefab spawned when dropping this item into the world.
        /// </summary>
        public GameObject WorldPrefab => worldPrefab;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(itemId))
            {
                itemId = Guid.NewGuid().ToString("N");
                UnityEditor.EditorUtility.SetDirty(this);
                Debug.Log($"[ItemData] Generated new ItemId: {itemId} for {name}", this);
            }

            maxStack = !isStackable ? 1 : Mathf.Max(1, maxStack);
        }
#endif
    }

    /// <summary>
    /// Types of items supported in the inventory.
    /// </summary>
    public enum ItemType
    {
        Resource,
        Consumable,
        Equipment,
        Chicken
    }
}
