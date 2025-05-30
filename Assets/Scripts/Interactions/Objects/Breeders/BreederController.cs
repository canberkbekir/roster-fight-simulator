using System;
using Interactions.Base;
using UI;
using UnityEngine;

namespace Interactions.Objects.Breeders
{
    public class BreederController : InteractableBase
    {
        [SerializeField] private Breeder breeder;
        public Breeder Breeder => breeder; 
        
        
        private PlayerUIHandler _playerUIHandler;

        public override void OnStartServer()
        {
            base.OnStartServer();
            if (!Breeder)
            {
                Debug.LogError("Breeder is not assigned in the inspector.", this);
            }
            _playerUIHandler = PlayerUIHandler.Instance;
        }
 
        
        public override void OnInteract(GameObject interactor)
        {
            base.OnInteract(interactor); 
            OpenBreederUI();
        }
 
        private void OpenBreederUI()
        {
            if (!_playerUIHandler)
            {
                Debug.LogError("PlayerUIHandler is not initialized.");
                return;
            }

            if (_playerUIHandler.BreederUI == null)
            {
                Debug.LogError("BreederUI is not assigned in PlayerUIHandler.");
                return;
            }

            _playerUIHandler.BreederUI.Show();
            _playerUIHandler.BreederUI.Init(breeder.CurrentRoosters);
            Debug.Log("Breeder UI opened.");
        }
    }
}
