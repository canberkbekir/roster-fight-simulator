// InteractableBase.cs
using UnityEngine;
using Mirror;

namespace Interactions.Base
{
    [RequireComponent(typeof(NetworkIdentity))]
    public class InteractableBase : NetworkBehaviour, IInteractable
    {
        [SerializeField] private string interactionPrompt = "Press E to interact";
        public virtual string InteractionPrompt => interactionPrompt;

        [Server]
        public virtual void OnInteract(GameObject interactor)
        {
            Debug.Log($"[InteractableBase] {interactor.name} interacted with {name}");
        }

        [Command(requiresAuthority = false)]
        public void CmdInteract()
        {
            if (!isServer) return;
            var playerGO = connectionToClient.identity.gameObject;
            OnInteract(playerGO);
            RpcOnInteract();
        }

        [ClientRpc]
        public virtual void RpcOnInteract() { }
    }
}