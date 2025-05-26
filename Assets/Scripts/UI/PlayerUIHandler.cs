using Mirror;
using UI.Breeder;
using UnityEngine;

namespace UI
{
    public class PlayerUIHandler : MonoBehaviour
    {
        [SerializeField] private InventoryUI inventoryUI;
        [SerializeField] private InteractUI interactUI;
        [SerializeField] private RoosterInspectorUI roosterInspectorUI;
        [SerializeField] private BreederUI breederUI;
        
        public InventoryUI InventoryUI => inventoryUI;
        
        public InteractUI InteractUI => interactUI;
        
        public RoosterInspectorUI RoosterInspectorUI => roosterInspectorUI;
        
        public BreederUI BreederUI => breederUI;
        
        public static PlayerUIHandler Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            { 
                Destroy(gameObject);
                return;
            }
            
            Instance = this; 
            
            if (inventoryUI == null)
            {
                Debug.LogError("InventoryUI component is not assigned in the inspector.");
            }
            if (interactUI == null)
            {
                Debug.LogError("InteractUI component is not assigned in the inspector.");
            }
            if (roosterInspectorUI == null)
            {
                Debug.LogError("RoosterInspectorUI component is not assigned in the inspector.");
            }
            
            if (breederUI == null)
            {
                Debug.LogError("BreederUI component is not assigned in the inspector.");
            }
        }
        
    }
}
