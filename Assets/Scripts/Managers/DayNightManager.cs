using Mirror;
using UnityEngine;

namespace Managers
{
    public class DayNightManager : NetworkBehaviour
    {
        private static readonly int _tex1 = Shader.PropertyToID("_Texture1");
        private static readonly int _tex2 = Shader.PropertyToID("_Texture2");
        private static readonly int _blend = Shader.PropertyToID("_Blend");

        [Header("Cycle Settings")]
        [Tooltip("Length of a full 0→1 day cycle, in seconds")]
        [SerializeField] private float dayLength = 10;

        [SyncVar(hook = nameof(OnTimeOfDayChanged))]
        private float _timeOfDay; 
 
        public int CurrentHour
        {
            get
            { 
                var totalHours = _timeOfDay * 24f;
                return Mathf.FloorToInt(totalHours) % 24;
            }
        }
 
        public int CurrentMinute
        {
            get
            { 
                var totalMinutes = _timeOfDay * 24f * 60f;
                return Mathf.FloorToInt(totalMinutes) % 60;
            }
        } 

        [Header("Skybox Settings")]
        [SerializeField] private Material skyboxMaterial;
        [SerializeField] private Cubemap dayCubemap;
        [SerializeField] private Cubemap nightCubemap;

        [Header("Sun Settings")]
        [SerializeField] private Light sun;
        [SerializeField] private AnimationCurve sunIntensityCurve = AnimationCurve.Linear(0f, 0f, 1f, 0f);

        [Header("Ambient & Fog")]
        [SerializeField] private Gradient ambientColor;
        [SerializeField] private Gradient fogColor;

        public override void OnStartServer()
        {
            base.OnStartServer(); 
            _timeOfDay = 0f;
        }

        public override void OnStartClient()
        {
            base.OnStartClient(); 
            InitSkybox(); 
            UpdateEnvironment(_timeOfDay);
        }

        [ServerCallback]
        private void Update()
        { 
            _timeOfDay = (_timeOfDay + Time.deltaTime / dayLength) % 1f;
        }

        private void OnTimeOfDayChanged(float oldT, float newT)
        { 
            UpdateEnvironment(newT);
        }

        private void InitSkybox()
        {
            if (!skyboxMaterial) return; 
            skyboxMaterial.SetTexture(_tex1, nightCubemap);
            skyboxMaterial.SetTexture(_tex2, dayCubemap);
            RenderSettings.skybox = skyboxMaterial;
        }

        private void UpdateEnvironment(float t)
        { 
            if (skyboxMaterial)
                skyboxMaterial.SetFloat(_blend, t);
 
            if (sun)
            {
                sun.transform.rotation = Quaternion.Euler((t * 360f) - 90f, 170f, 0f);
                sun.intensity = sunIntensityCurve.Evaluate(t);
            }
 
            RenderSettings.ambientSkyColor = ambientColor.Evaluate(t);
            RenderSettings.fogColor        = fogColor.Evaluate(t);
 
            DynamicGI.UpdateEnvironment();
        }
    }
}
