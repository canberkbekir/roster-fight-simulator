using System;
using UnityEngine;
using Mirror;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;

namespace Managers
{
    public class GameManager : NetworkBehaviour
    { 
        public static GameManager Instance { get; private set; }
        
        [Header("Managers")]
        [SerializeField] private RoosterSpawnerManager roosterSpawnerManager;
        [SerializeField] private ContainerManager containerManager;
        [SerializeField] private EggManager eggManager;
        [SerializeField] private BreedingManager breedingManager;
        public RoosterSpawnerManager RoosterSpawnerManager => roosterSpawnerManager;
        public ContainerManager ContainerManager => containerManager;
        public EggManager EggManager => eggManager;
        public BreedingManager BreedingManager => breedingManager;
         
        [Header("Settings for Debug")] 
        [SerializeField]private float[] gameTimeRate;

        private int _currentIndex;
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

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F2))
            {
                _currentIndex++;
                if (_currentIndex >= gameTimeRate.Length)
                    _currentIndex = 0;

                Time.timeScale = gameTimeRate[_currentIndex];
                Debug.Log($"Game time rate changed to: {gameTimeRate[_currentIndex]}");
                
            }
            else if (Input.GetKeyDown(KeyCode.F1))
            {
                _currentIndex--;
                if (_currentIndex < 0)
                    _currentIndex = gameTimeRate.Length - 1;

                Time.timeScale = gameTimeRate[_currentIndex];
                Debug.Log($"Game time rate changed to: {gameTimeRate[_currentIndex]}");
                
            }
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
 
       
    }
}