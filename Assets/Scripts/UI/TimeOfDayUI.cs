using System;
using Managers;
using UnityEngine;

namespace UI
{
    public class TimeOfDayUI : MonoBehaviour
    {
        [SerializeField] private TMPro.TextMeshProUGUI timeText;
         
        private DayNightManager _dayNightManager;
        private void Start()
        {
            _dayNightManager = GameManager.Instance.DayNightManager;
            if (_dayNightManager != null) return;
            Debug.LogError("DayNightManager not found in GameManager.");
            return; 
        }

        private void Update()
        {
            if (!_dayNightManager) return;

            SetTimeOfDayText();
        }

        private void SetTimeOfDayText()
        { 
            var hour = _dayNightManager.CurrentHour;
            var minute = _dayNightManager.CurrentMinute;
 
            var formattedTime = $"{hour:D2}:{minute:D2}";
 
            timeText.text = formattedTime; 
        }
    }
}
