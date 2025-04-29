using UnityEngine;
using Mirror;

namespace Managers
{
    public class GameManager : NetworkBehaviour
    { 
        public static GameManager Instance { get; private set; }
        
        [Header("Managers")]
        [SerializeField] private RoosterSpawnerManager roosterSpawnerManager;
        [SerializeField] private ContainerManager containerManager;
        public RoosterSpawnerManager RoosterSpawnerManager => roosterSpawnerManager;
        public ContainerManager ContainerManager => containerManager;
        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                // Only one GameManager allowed
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } 
 
        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
 
       
    }
}