using UnityEngine;
using Mirror;
using Interactions.Base;
using Managers;
using Players; // for GameManager
using Roosters.Components; // for RoosterState

namespace InventorySystem.Base
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Collider))]
    public class ItemWorld : InteractableBase
    {
        [Tooltip("Static ItemData ID; leave blank for dynamic items.")]
        [SerializeField] private string itemId;

        [Tooltip("JSON metadata for dynamic items (serialized state).")]
        [TextArea]
        [SerializeField] private string metaJson;

        private ItemData _data;
        private ItemDataContainer _db;

        private void Awake()
        {
            _db = GameManager.Instance.ContainerManager.ItemDataContainer;
            if (!string.IsNullOrEmpty(itemId))
                _data = _db.Get(itemId);
        }

        public override string InteractionPrompt
        {
            get
            {
                if (_data != null)
                    return $"Pick up {_data.DisplayName}";

                if (!string.IsNullOrEmpty(metaJson))
                {
                    var state = JsonUtility.FromJson<RoosterState>(metaJson);
                    return $"Pick up {state.Name}";
                }

                return "Pick up";
            }
        }

        [Server]
        public override void OnInteract(GameObject interactor)
        {
            base.OnInteract(interactor);
            var inv = interactor.GetComponent<Players.PlayerReferenceHandler>()?.PlayerInventory;
            if (inv == null) return;

            if (_data != null)
                inv.AddItem(_data.ItemId, 1, null);
            else
                inv.AddRooster(metaJson);

            NetworkServer.Destroy(gameObject);
        }

        /// <summary>
        /// Server-only setter for dynamic item JSON before spawning.
        /// </summary>
        [Server]
        public void SetMetaJson(string json)
        {
            itemId   = null;
            _data    = null;
            metaJson = json;
        }
    }
}