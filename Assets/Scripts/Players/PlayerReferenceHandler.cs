using UI;
using UnityEngine;

namespace Players
{
    public class PlayerReferenceHandler : MonoBehaviour
    {
        [Header("References")] [SerializeField]
        private Camera playerCamera;

        [Space] [Header("UI References")] [SerializeField]
        private InteractUI interactUI;

        [Space] [Header("References")] [SerializeField]
        private PlayerInventory playerInventory;

        public Camera PlayerCamera => playerCamera;
        public InteractUI InteractUI => interactUI;
        public PlayerInventory PlayerInventory => playerInventory;

        private void Awake()
        {
            if (playerCamera == null)
            {
                Debug.LogError("Player camera is not assigned in the inspector.");
            }

            if (interactUI == null)
            {
                Debug.LogError("Interact UI is not assigned in the inspector.");
            }

            if (playerInventory == null)
            {
                Debug.LogError("Player inventory is not assigned in the inspector.");
            }
        }
    }
}