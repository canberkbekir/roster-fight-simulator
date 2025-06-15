using System;
using Managers;
using TMPro;
using UnityEngine;

namespace UI.DayNight
{
    public class TimeOfDayAlert : BaseUI
    {
        [Header("References")] 
        [SerializeField] private TextMeshProUGUI timeOfDayText;
        
        private DayNightManager _dayNightManager;
        private GameManager _gameManager;

        protected override void Awake()
        {
            _dayNightManager = GameManager.Instance.DayNightManager;
            _gameManager = GameManager.Instance;
        }

        private void Start()
        {
            if (_dayNightManager == null)
            {
                Debug.LogError("DayNightManager is not assigned in TimeOfDayAlert.");
                return;
            }

            _dayNightManager.OnTimeOfDayChanged += HandleTimeOfDayChanged;
            
        }

        private void HandleTimeOfDayChanged(TimeOfDay obj)
        {
            if (timeOfDayText == null)
            {
                Debug.LogError("TextMeshProUGUI component is not assigned in the inspector.");
                return;
            }
            
            Show(false);

            timeOfDayText.text = obj switch
            {
                TimeOfDay.Morning => "Morning",
                TimeOfDay.Afternoon => "Afternoon",
                TimeOfDay.Evening => "Evening",
                TimeOfDay.Night => "Night",
                _ => throw new ArgumentOutOfRangeException(nameof(obj), obj, null)
            };
            
            Hide(true);
        }
    }
}
