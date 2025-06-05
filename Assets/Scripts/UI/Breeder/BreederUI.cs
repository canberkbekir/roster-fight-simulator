using System;
using System.Linq;
using Creatures.Chickens.Roosters;  
using UnityEngine;

namespace UI.Breeder
{
    public class BreederUI : BaseUI
    {
        [Header("References")] 
        [SerializeField, Tooltip("Parent transform for instantiated slots.")] private Transform roostersSlotsParent;
        [SerializeField] private BreederInformationUI breederInformationUI;
        [SerializeField] private BreederGenesUI breederGenesUI;

        [SerializeField, Tooltip("Slot prefab with InventorySlotUI component.")]
        private InventorySlotUI slotPrefab;

        [Space] [Header("Information Tab References")] 
        [SerializeField] private BaseUI roosterInformationTab;
        [SerializeField] private BaseUI roosterGenesTab;
        [SerializeField] private BaseUI roosterSkillsTab;

        private Rooster _selectedRooster;

        public event Action<Rooster> OnRoosterSelected;

        protected override void Awake()
        {
            base.Awake();
            OnRoosterSelected += RoosterChanged;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Hide();
            }
        }

        public void Init(Rooster[] roosters)
        {
            SetRoosterList(roosters);
            
            if (roosters is { Length: > 0 })
            {
                SelectRooster(roosters[0]);
            }
            else
            {
                Debug.LogWarning("No roosters provided to BreederUI.");
            }
            
            SetInformationTab(_selectedRooster);
            SetGenesTab(_selectedRooster);
            
            roosterInformationTab.Show();
            roosterGenesTab.Hide();
            roosterSkillsTab.Hide();
        }

        public void SelectRooster(Rooster rooster)
        {
            if (rooster == null)
            {
                Debug.LogWarning("Attempted to select a null rooster.");
                return;
            }

            _selectedRooster = rooster; 
            OnRoosterSelected?.Invoke(_selectedRooster);
        }

        private void SetRoosterList(Rooster[] roosters)
        {
            if (roosters == null || roosters.Length == 0)
            {
                Debug.LogWarning("No roosters to display in BreederUI.");
                return;
            }

            foreach (Transform child in roostersSlotsParent)
            {
                Destroy(child.gameObject);
            }

            foreach (var rooster in roosters)
            {
                if (rooster == null) continue;

                var slot = Instantiate(slotPrefab, roostersSlotsParent);
                slot.SetItem(null, null);
            }
        }

        private void SetInformationTab(Rooster rooster)
        {
            if (rooster == null)
            {
                Debug.LogWarning("Attempted to set information tab with a null rooster.");
                return;
            } 
            breederInformationUI.SetRooster(rooster);  
        }
        
        private void SetGenesTab(Rooster rooster)
        {
            if(rooster == null)
            {
                Debug.LogWarning("Attempted to set genes tab with a null rooster.");
                return;
            }
            
            if (rooster.Genes.Any())
            {
                breederGenesUI.SetGenes(rooster.Genes);
            }
            else
            {
                Debug.LogWarning("Rooster has no genes to display.");
                breederGenesUI.SetGenes(null);
                
            }
        }
        
        private void RoosterChanged(Rooster obj)
        {
            if (obj == null)
            {
                Debug.LogWarning("RoosterChanged called with a null rooster.");
                return;
            }

            SetInformationTab(obj);
        }
        
    }
}