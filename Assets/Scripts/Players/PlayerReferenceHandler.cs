using System;
using Mirror;
using UnityEngine;

namespace Players
{
    public class PlayerReferenceHandler : NetworkBehaviour
    {
        [Header("References")] [SerializeField]
        private Camera playerCamera; 

        [SerializeField]
        private PlayerInventory playerInventory;
        
        [SerializeField] private PlayerInteraction playerInteraction;
        
        public static event Action<PlayerReferenceHandler> LocalPlayerReady; 
        
        public Camera PlayerCamera => playerCamera; 
        public PlayerInventory PlayerInventory => playerInventory;
        public PlayerInteraction PlayerInteraction => playerInteraction; 
        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            if (!playerCamera)
            {
                Debug.LogError("Player camera is not assigned in the inspector.");
                return;
            }
            if (!playerInventory)
            {
                Debug.LogError("Player inventory is not assigned in the inspector.");
                return;
            }
            if (!playerInteraction)
            {
                Debug.LogError("Player interaction is not assigned in the inspector.");
                return;
            }
            
            LocalPlayerReady?.Invoke(this);
        }
    }
}