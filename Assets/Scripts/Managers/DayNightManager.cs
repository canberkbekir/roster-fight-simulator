using System;
using Configs.DayNightManager;
using Mirror;
using UnityEngine; 

namespace Managers
{
    public enum TimeOfDay
    {
        Morning,
        Afternoon,
        Evening,
        Night
    }

    public class DayNightManager : NetworkBehaviour
    {
        private static readonly int _tex1   = Shader.PropertyToID("_Texture1");
        private static readonly int _tex2   = Shader.PropertyToID("_Texture2");
        private static readonly int _blend  = Shader.PropertyToID("_Blend");

        [Header("Cycle Settings")]
        [Tooltip("Length of a full 0→1 day cycle, in seconds")]
        [SerializeField] private float dayCycleSeconds = 10f;

        [Tooltip("Defines start hours for each TimeOfDay segment")]
        [SerializeField] private TimeOfDayHours timeOfDaySchedule;

        // 0→1 normalized time, synced from server to clients
        [SyncVar(hook = nameof(OnNormalizedTimeChanged))]
        private float _normalizedTime;

        /// <summary>
        /// Normalized time of day [0..1)
        /// </summary>
        public float NormalizedTime => _normalizedTime;

        /// <summary>
        /// The current hour (0–23) based on normalized time.
        /// </summary>
        public int HourOfDay
        {
            get
            {
                var totalHours = _normalizedTime * 24f;
                return Mathf.FloorToInt(totalHours) % 24;
            }
        }

        /// <summary>
        /// The current minute (0–59) based on normalized time.
        /// </summary>
        public int MinuteOfHour
        {
            get
            {
                var totalMinutes = _normalizedTime * 24f * 60f;
                return Mathf.FloorToInt(totalMinutes) % 60;
            }
        }

        /// <summary>
        /// The current TimeOfDay segment (Morning/Afternoon/Evening/Night).
        /// </summary>
        public TimeOfDay CurrentSegment
        {
            get
            {
                var h = HourOfDay;
                if (h >= timeOfDaySchedule.StartHourForMorning &&
                    h <  timeOfDaySchedule.StartHourForAfternoon)
                    return TimeOfDay.Morning;

                if (h >= timeOfDaySchedule.StartHourForAfternoon &&
                    h <  timeOfDaySchedule.StartHourForEvening)
                    return TimeOfDay.Afternoon;

                if (h >= timeOfDaySchedule.StartHourForEvening &&
                    h <  timeOfDaySchedule.StartHourForNight)
                    return TimeOfDay.Evening;

                return TimeOfDay.Night;
            }
        }

        /// <summary>
        /// Fired whenever the TimeOfDay *segment* actually changes.
        /// </summary>
        public event Action<TimeOfDay> OnTimeOfDayChanged;
        
        [Header("Skybox Settings")]
        [SerializeField] private Material skyboxMaterial;
        [SerializeField] private Cubemap  dayCubemap;
        [SerializeField] private Cubemap  nightCubemap;

        [Header("Sun Settings")]
        [SerializeField] private Light            sun;
        [SerializeField] private AnimationCurve   sunIntensityCurve = AnimationCurve.Linear(0f, 0f, 1f, 0f);

        [Header("Ambient & Fog")]
        [SerializeField] private Gradient ambientColor;
        [SerializeField] private Gradient fogColor;

        public override void OnStartServer()
        {
            base.OnStartServer(); 
            var startHour = timeOfDaySchedule.StartHourOfDay;
            _normalizedTime = (startHour % 24) / 24f; 
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            InitializeSkybox();
            UpdateEnvironment(_normalizedTime);
            OnTimeOfDayChanged?.Invoke(GetSegmentAtNormalizedTime(_normalizedTime));
        }

        [ServerCallback]
        private void Update()
        {
            _normalizedTime = (_normalizedTime + Time.deltaTime / dayCycleSeconds) % 1f;
        }

        private void OnNormalizedTimeChanged(float oldValue, float newValue)
        { 
            UpdateEnvironment(newValue);  
            TryRaiseSegmentChangeEvent(oldValue, newValue);
        }

        private void TryRaiseSegmentChangeEvent(float oldNorm, float newNorm)
        {
            var oldSegment = GetSegmentAtNormalizedTime(oldNorm);
            var newSegment = GetSegmentAtNormalizedTime(newNorm);

            if (oldSegment != newSegment)
                OnTimeOfDayChanged?.Invoke(newSegment);
        }

        private TimeOfDay GetSegmentAtNormalizedTime(float t)
        {
            var hour = Mathf.FloorToInt(t * 24f) % 24;

            if (hour >= timeOfDaySchedule.StartHourForMorning &&
                hour <  timeOfDaySchedule.StartHourForAfternoon)
                return TimeOfDay.Morning;

            if (hour >= timeOfDaySchedule.StartHourForAfternoon &&
                hour <  timeOfDaySchedule.StartHourForEvening)
                return TimeOfDay.Afternoon;

            if (hour >= timeOfDaySchedule.StartHourForEvening &&
                hour <  timeOfDaySchedule.StartHourForNight)
                return TimeOfDay.Evening;

            return TimeOfDay.Night;
        }

        private void InitializeSkybox()
        {
            if (skyboxMaterial == null) return;

            skyboxMaterial.SetTexture(_tex1, nightCubemap);
            skyboxMaterial.SetTexture(_tex2, dayCubemap);
            RenderSettings.skybox = skyboxMaterial;
        }

        private void UpdateEnvironment(float t)
        {
            if (skyboxMaterial != null)
                skyboxMaterial.SetFloat(_blend, t);

            if (sun != null)
            {
                sun.transform.rotation = Quaternion.Euler((t * 360f) - 90f, 170f, 0f);
                sun.intensity          = sunIntensityCurve.Evaluate(t);
            }

            RenderSettings.ambientSkyColor = ambientColor.Evaluate(t);
            RenderSettings.fogColor        = fogColor.Evaluate(t);
            DynamicGI.UpdateEnvironment();
        }
    }
}
