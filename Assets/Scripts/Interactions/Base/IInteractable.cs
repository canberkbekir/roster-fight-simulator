using UnityEngine;

namespace Interactions.Base
{
    public interface IInteractable { 
        string InteractionPrompt { get; }
 
        void OnInteract(GameObject interactor);
    }
}
