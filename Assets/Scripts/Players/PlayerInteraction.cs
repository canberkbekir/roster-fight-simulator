using System;
using UnityEngine;
using Mirror;
using Interactions.Base;
using UI;
using Unity.VisualScripting;

namespace Players
{
    public class PlayerInteraction : NetworkBehaviour
    {
        [SerializeField] private PlayerReferenceHandler refs;
        [SerializeField] private float range = 3f;
        [SerializeField] private LayerMask mask;

        private IInteractable current;
        private InteractableBase currentBase;
        private Camera cam;
        private InteractUI ui;
        
        public event Action<InteractableBase> OnHoverEnter;
        public event Action<InteractableBase> OnHoverExit; 
        public event Action<InteractableBase> OnInteracted;

        void Update()
        {
            if (!isLocalPlayer) return;

            var ray = new Ray(cam.transform.position, cam.transform.forward);
            if (Physics.Raycast(ray, out var hit, range, mask)
                && hit.collider.TryGetComponent<IInteractable>(out var next))
            {
                if (next != current)
                {
                    current = next; 
                    currentBase = hit.collider.GetComponent<InteractableBase>();
                    OnHoverEnter?.Invoke(currentBase); 
                }

                if (Input.GetKeyDown(KeyCode.E)
                    && hit.collider.TryGetComponent<InteractableBase>(out currentBase))
                { 
                    CmdInteractWith(currentBase.netIdentity.netId);
                }
            }
            else if (current != null)
            {
                current = null;
                currentBase = null;
                OnHoverExit?.Invoke(currentBase); 
            }
        }

        public override void OnStartLocalPlayer()
        {
            base.OnStartLocalPlayer();
            cam = refs.PlayerCamera; 
        }

        [Command]
        public void CmdInteractWith(uint targetNetId)
        { 
            if (!NetworkServer.spawned.TryGetValue(targetNetId, out var ni)) return;
            var interactable = ni.GetComponent<InteractableBase>();
            if (interactable == null) return;
 
            interactable.OnInteract(connectionToClient.identity.gameObject); 
            interactable.RpcOnInteract();
            OnInteracted?.Invoke(interactable);
        }
    }
}