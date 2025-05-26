using System;
using InventorySystem.Base;
using Mirror;
using Players;
using TMPro;
using UnityEngine;

namespace UI
{
    public class RoosterInspectorUI : BaseUI
    {
        [SerializeField] private TextMeshProUGUI text;

        private PlayerReferenceHandler _playerReferenceHandler;
        private PlayerInventory _playerInventory; 

        private void OnEnable()
        {
            PlayerReferenceHandler.LocalPlayerReady += OnLocalPlayerReady;
        }

        private void OnDisable()
        {
            PlayerReferenceHandler.LocalPlayerReady -= OnLocalPlayerReady;
            if (_playerInventory != null)
            {
                _playerInventory.OnSelectedSlotChanged -= OnSelectedSlotChanged;
            }
        }

        private void OnLocalPlayerReady(PlayerReferenceHandler obj)
        {
            _playerReferenceHandler = obj;
            _playerInventory = _playerReferenceHandler.PlayerInventory
                               ?? throw new System.Exception("PlayerInventory is null.");
            if (text == null)
            {
                Debug.LogError("TextMeshProUGUI component is not assigned in the inspector.");
                return;
            }

            text.text = "";

            if (_playerInventory != null)
            {
                _playerInventory.OnSelectedSlotChanged += OnSelectedSlotChanged;
            }
        }

        private void OnSelectedSlotChanged(int index, InventoryItem item)
        {
            text.text = "";

            if (item.IsRooster)
            {
                var rooster = item.Rooster;
                if (rooster == null)
                {
                    Debug.LogWarning("Selected item is a rooster but Rooster data is null.");
                    return;
                }

                var bufferText = "";
                bufferText += $"<b>Rooster Name:</b> {item.Rooster.Name}\n";
                foreach (var gene in rooster.Genes)
                {
                    bufferText += $"<b>Gene:</b> {gene.GeneName} - %{gene.GenePassingChance}\n";
                    if (!string.IsNullOrEmpty(gene.GeneDescription))
                    {
                        bufferText += $"<b>Description:</b> {gene.GeneDescription}\n";
                    }

                    foreach (var feature in gene.GeneFeatures)
                    {
                        bufferText += $"<b>Feature:</b> {feature.Name}\n";
                    }
                }

                text.text = bufferText;

                Show();
            }
            else
            {
                Hide();
            }
        }

        public override void Hide()
        {
            base.Hide();
            if (text != null)
            {
                text.text = "";
            }
            else
            {
                Debug.LogWarning("TextMeshProUGUI component is not assigned in the inspector.");
            }
 
        } 
    }
}