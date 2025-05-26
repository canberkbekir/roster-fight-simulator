using System;
using Interactions.Base;
using Mirror;
using Players;
using TMPro;
using UnityEngine;

namespace UI
{
    public class InteractUI : BaseUI
    { 
        [SerializeField] private TMP_Text promptText;

        private PlayerReferenceHandler _playerReferenceHandler;
        private PlayerInteraction _playerInteraction; 
        
        protected override void Awake()
        {
            base.Awake();
            if (promptText == null)
            {
                Debug.LogError("TextMeshProUGUI component is not assigned in the inspector.");
            } 
        }

        private void Start()
        {
            if (!NetworkClient.active) return;
            var localPlayer = NetworkClient.localPlayer;
            if (localPlayer == null) return; 
         }

        private void OnEnable()
        {
            PlayerReferenceHandler.LocalPlayerReady += OnLocalPlayerReady;
        }
        
        private void OnDisable()
        {
            PlayerReferenceHandler.LocalPlayerReady -= OnLocalPlayerReady;
            if (_playerInteraction == null) return;
            _playerInteraction.OnHoverEnter -= OnHoverEnter;
            _playerInteraction.OnHoverExit -= OnHoverExit;
        }

        private void OnLocalPlayerReady(PlayerReferenceHandler obj)
        {
            _playerReferenceHandler = obj ?? throw new System.Exception("PlayerReferenceHandler is null.");
            _playerInteraction = _playerReferenceHandler.PlayerInteraction 
                                 ?? throw new System.Exception("PlayerInteraction is null.");
            
            _playerInteraction.OnHoverEnter += OnHoverEnter;
            _playerInteraction.OnHoverExit += OnHoverExit;
        }

        private void OnHoverExit(InteractableBase obj)
        { 
            Hide();
        }

        private void OnHoverEnter(InteractableBase obj)
        {
            if (!obj || obj.InteractionPrompt == null)
            {
                Debug.LogWarning("InteractableBase or InteractionPrompt is null in OnHoverEnter.");
                return;
            } 
            
            promptText.text = obj.InteractionPrompt; 
            Show();
        }
        
        public override void Hide()
        {
            base.Hide(); 
        }  
    }
}