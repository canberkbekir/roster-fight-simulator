using System;
using Sirenix.OdinInspector;
using UnityEngine; 

namespace Configs.DayNightManager
{
    [CreateAssetMenu(fileName = "TimeOfDayHours", menuName = "Configs/DayNightManager/TimeOfDayHours")]
    public class TimeOfDayHours : ScriptableObject
    {
        [Header("Settings")]
        [SerializeField] private int startHourOfDay = 6;
        
        [Space]
        [Header("Start hours for each segment (0–23)")]
        [Range(0, 23)] [SerializeField] private int startHourForMorning   = 6;
        [Range(0, 23)] [SerializeField] private int startHourForAfternoon = 12;
        [Range(0, 23)] [SerializeField] private int startHourForEvening   = 14;
        [Range(0, 23)] [SerializeField] private int startHourForNight     = 20;
        
        public int StartHourForMorning => startHourForMorning;
        public int StartHourForAfternoon => startHourForAfternoon;
        public int StartHourForEvening => startHourForEvening;
        public int StartHourForNight => startHourForNight;
        public int StartHourOfDay => startHourOfDay;

        private void OnValidate()
        {
            if (startHourForAfternoon <= startHourForMorning)
            {
                Debug.LogWarning($"StartHourForAfternoon ({startHourForAfternoon}) must be > StartHourForMorning ({startHourForMorning}). Adjusting.");
                startHourForAfternoon = Mathf.Min(startHourForMorning + 1, 23);
            }

            if (startHourForEvening <= startHourForAfternoon)
            {
                Debug.LogWarning($"StartHourForEvening ({startHourForEvening}) must be > StartHourForAfternoon ({startHourForAfternoon}). Adjusting.");
                startHourForEvening = Mathf.Min(startHourForAfternoon + 1, 23);
            }

            if (startHourForNight <= startHourForEvening)
            {
                Debug.LogWarning($"StartHourForNight ({startHourForNight}) must be > StartHourForEvening ({startHourForEvening}). Adjusting.");
                startHourForNight = Mathf.Min(startHourForEvening + 1, 23);
            }
            
        }
    }
}
