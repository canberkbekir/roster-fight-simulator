using System;
using Inputs;
using UnityEngine;
using Mirror;
using Services;
using UnityEngine.Serialization;

namespace Managers
{
    public class GameManager : NetworkBehaviour
    { 
        public static GameManager Instance { get; private set; }
        
        [FormerlySerializedAs("chickenSpawnerManager")]
        [FormerlySerializedAs("roosterSpawnerManager")]
        [Header("Managers")]
        [SerializeField] private ChickenSpawnerService chickenSpawnerService;
        [FormerlySerializedAs("containerManager")] [SerializeField] private ContainerService containerService;
        [FormerlySerializedAs("eggManager")] [SerializeField] private EggService eggService;
        [FormerlySerializedAs("breedingManager")] [SerializeField] private BreedingService breedingService; 
        [SerializeField] private DayNightManager dayNightManager;
        
        [Space]
        [Header("References")]
        [SerializeField] private InputReader inputReader;
        
        [Space]
        [Header("Settings for Debug")] 
        [SerializeField]private float[] gameTimeRate; 
        
        // Properties for Managers
        public ChickenSpawnerService ChickenSpawnerService => chickenSpawnerService;
        public ContainerService ContainerService => containerService;
        public EggService EggService => eggService;
        public BreedingService BreedingService => breedingService; 
        public DayNightManager DayNightManager => dayNightManager;
        
        // Property for References
        public InputReader InputReader => inputReader; 
        
        public static event Action OnGamePaused;
        public static event Action OnGameResumed;

        [SyncVar(hook = nameof(HandlePauseChanged))]
        private bool _gamePaused = false; 
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
        
        [Server]
        public void PauseGame()
        {
            _gamePaused = true;
        }

        [Server]
        public void ResumeGame()
        {
            _gamePaused = false;
        }

        private void HandlePauseChanged(bool oldVal, bool newVal)
        {
            if (newVal) OnGamePaused?.Invoke();
            else        OnGameResumed?.Invoke();
        }
        

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
 
       
    }
}