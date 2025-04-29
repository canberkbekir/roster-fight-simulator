using Interactions.Base;
using Managers;
using UnityEngine;

namespace Interactions.Objects
{
    public class RoosterGeneratorMachine : InteractableBase
    {
        [Header("Settings")]
        [SerializeField] private Transform spawnPoint;
        
        public override void OnInteract(GameObject interactor)
        {
            base.OnInteract(interactor);
            GameManager.Instance.RoosterSpawnerManager.RequestSpawnAt(spawnPoint);   
        }
    }
}
