using UnityEngine;
using Survivor.Shared;
using System.Collections;

namespace Survivor.Core
{
    public class SunManager : MonoBehaviour
    {
        [Header("Sun Settings")]
        [SerializeField] private Light sunLight;
        [SerializeField] private Transform sunTransform;
        [SerializeField] private Material skyboxMaterial;
        
        [Header("Time Settings")]
        [SerializeField] private int sunriseHour = 5; // 5 AM
        [SerializeField] private int sunsetHour = 21; // 9 PM
        [SerializeField] private float sunRiseDuration = 2f; // Hours for sunrise transition
        [SerializeField] private float sunSetDuration = 2f; // Hours for sunset transition
        
        [Header("Lighting Settings")]
        [SerializeField] private Color dayLightColor = new Color(1f, 0.95f, 0.8f);
        [SerializeField] private Color nightLightColor = new Color(0.1f, 0.1f, 0.3f);
        [SerializeField] private float dayIntensity = 1f;
        [SerializeField] private float nightIntensity = 0.1f;
        [SerializeField] private float dayTemperature = 5500f; // Kelvin
        [SerializeField] private float nightTemperature = 2000f; // Kelvin
        
        [Header("Skybox Settings")]
        [SerializeField] private Color daySkyColor = new Color(0.5f, 0.7f, 1f);
        [SerializeField] private Color nightSkyColor = new Color(0.05f, 0.05f, 0.1f);
        [SerializeField] private Color dayHorizonColor = new Color(0.8f, 0.9f, 1f);
        [SerializeField] private Color nightHorizonColor = new Color(0.1f, 0.1f, 0.2f);
        
        [Header("Sun Movement")]
        [SerializeField] private float sunStartAngle = -90f; // Sunrise position
        [SerializeField] private float sunEndAngle = 90f; // Sunset position
        [SerializeField] private float sunHeight = 100f; // Height of sun above ground
        
        private bool isInitialized = false;
        private int lastUpdateHour = -1;
        
        private void Start()
        {
            // Subscribe to time provider events
            GameTimeService.OnTimeProviderRegistered += OnTimeProviderRegistered;
            GameTimeService.OnTimeProviderUnregistered += OnTimeProviderUnregistered;
            
            // Check if time provider is already available
            if (GameTimeService.HasTimeProvider)
            {
                OnTimeProviderRegistered(GameTimeService.TimeProvider);
            }
            
            // Auto-find components if not assigned
            if (sunLight == null)
            {
                sunLight = FindObjectOfType<Light>();
                if (sunLight == null)
                {
                    Debug.LogError("[SunManager] No Light component found in scene!");
                }
            }
            
            if (sunTransform == null && sunLight != null)
            {
                sunTransform = sunLight.transform;
            }
            
            if (skyboxMaterial == null)
            {
                skyboxMaterial = RenderSettings.skybox;
                if (skyboxMaterial == null)
                {
                    Debug.LogWarning("[SunManager] No skybox material found - skybox effects will be disabled");
                }
            }
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            GameTimeService.OnTimeProviderRegistered -= OnTimeProviderRegistered;
            GameTimeService.OnTimeProviderUnregistered -= OnTimeProviderUnregistered;
        }
        
        private void OnTimeProviderRegistered(ITimeProvider provider)
        {
            Debug.Log("[SunManager] Time provider registered, initializing sun system");
            isInitialized = true;
            UpdateSunPosition();
        }
        
        private void OnTimeProviderUnregistered()
        {
            Debug.Log("[SunManager] Time provider unregistered");
            isInitialized = false;
        }
        
        private void Update()
        {
            if (isInitialized)
            {
                UpdateSunPosition();
            }
        }
        
        private void UpdateSunPosition()
        {
            var (hour, minute, _) = GameTimeService.GetCurrentGameTime();
            
            // Only update if hour has changed (for performance)
            if (hour == lastUpdateHour) return;
            
            lastUpdateHour = hour;
            
            // Calculate sun position and lighting
            float sunProgress = CalculateSunProgress(hour, minute);
            UpdateSunTransform(sunProgress);
            UpdateLighting(sunProgress);
            UpdateSkybox(sunProgress);
            
            Debug.Log($"[SunManager] Updated sun position for {hour:D2}:{minute:D2} - Progress: {sunProgress:F2}");
            if (sunTransform != null)
            {
                Debug.Log($"[SunManager] Sun position: {sunTransform.position}, Rotation: {sunTransform.rotation.eulerAngles}");
            }
        }
        
        private float CalculateSunProgress(int hour, int minute)
        {
            float timeInHours = hour + (minute / 60f);
            
            // Calculate progress through the day (0 = sunrise, 1 = sunset)
            if (timeInHours < sunriseHour)
            {
                // Before sunrise - night
                return 0f;
            }
            else if (timeInHours > sunsetHour)
            {
                // After sunset - night
                return 1f;
            }
            else
            {
                // During day - calculate progress from sunrise to sunset
                float dayDuration = sunsetHour - sunriseHour;
                float timeSinceSunrise = timeInHours - sunriseHour;
                return timeSinceSunrise / dayDuration;
            }
        }
        
        private void UpdateSunTransform(float progress)
        {
            if (sunTransform == null) return;
            
            // Calculate sun position based on time of day
            // We want the sun to follow a realistic arc across the sky
            // 0 = sunrise (5 AM), 0.5 = noon, 1 = sunset (9 PM)
            
            // Convert progress to an angle that represents the sun's position in the sky
            // Start at -90 degrees (below horizon) and go to 90 degrees (above horizon)
            float sunAngle;
            
            if (progress <= 0f)
            {
                // Before sunrise - sun below horizon
                sunAngle = -90f;
            }
            else if (progress >= 1f)
            {
                // After sunset - sun below horizon
                sunAngle = -90f;
            }
            else
            {
                // During the day - sun follows an arc
                // Use a smooth curve that peaks at noon (progress = 0.5)
                float normalizedProgress = progress * 2f - 1f; // Convert 0-1 to -1 to 1
                sunAngle = Mathf.Sin(normalizedProgress * Mathf.PI) * 90f;
            }
            
            // Position the sun in 3D space
            // X-axis represents east-west movement
            // Y-axis represents height above horizon
            // Z-axis is depth (we'll keep it at 0 for simplicity)
            
            float radians = sunAngle * Mathf.Deg2Rad;
            
            // Calculate position based on the sun's arc
            // At sunrise (progress = 0): sun should be low on the horizon, moving east to west
            // At noon (progress = 0.5): sun should be high in the sky
            // At sunset (progress = 1): sun should be low on the horizon
            
            float x = Mathf.Sin(radians) * sunHeight; // East-west position
            float y = Mathf.Cos(radians) * sunHeight; // Height above horizon
            
            // Ensure the sun is always above the horizon during the day
            if (progress > 0f && progress < 1f)
            {
                y = Mathf.Max(y, 10f); // Minimum height during day
            }
            
            sunTransform.position = new Vector3(x, y, 0f);
            
            // Make the sun look down at the world
            sunTransform.LookAt(Vector3.zero);
            
            // Rotate the sun to face the correct direction based on time
            // At sunrise, sun should face west (negative X)
            // At sunset, sun should face east (positive X)
            if (progress > 0f && progress < 1f)
            {
                float rotationAngle = Mathf.Lerp(-45f, 45f, progress);
                sunTransform.rotation = Quaternion.Euler(rotationAngle, 0f, 0f);
            }
        }
        
        private void UpdateLighting(float progress)
        {
            if (sunLight == null) return;
            
            // Interpolate between night and day lighting
            float dayNightBlend = Mathf.SmoothStep(0f, 1f, progress);
            
            // Update light color and intensity
            sunLight.color = Color.Lerp(nightLightColor, dayLightColor, dayNightBlend);
            sunLight.intensity = Mathf.Lerp(nightIntensity, dayIntensity, dayNightBlend);
            
            // Update light temperature if using URP/HDRP
            if (sunLight.useColorTemperature)
            {
                sunLight.colorTemperature = Mathf.Lerp(nightTemperature, dayTemperature, dayNightBlend);
            }
            
            // Update ambient lighting
            RenderSettings.ambientLight = Color.Lerp(nightLightColor * 0.1f, dayLightColor * 0.3f, dayNightBlend);
        }
        
        private void UpdateSkybox(float progress)
        {
            if (skyboxMaterial == null) return;
            
            // Interpolate skybox colors
            float dayNightBlend = Mathf.SmoothStep(0f, 1f, progress);
            
            Color skyColor = Color.Lerp(nightSkyColor, daySkyColor, dayNightBlend);
            Color horizonColor = Color.Lerp(nightHorizonColor, dayHorizonColor, dayNightBlend);
            
            // Update skybox material properties
            if (skyboxMaterial.HasProperty("_SkyTint"))
            {
                skyboxMaterial.SetColor("_SkyTint", skyColor);
            }
            
            if (skyboxMaterial.HasProperty("_GroundColor"))
            {
                skyboxMaterial.SetColor("_GroundColor", horizonColor);
            }
            
            if (skyboxMaterial.HasProperty("_SkyColor"))
            {
                skyboxMaterial.SetColor("_SkyColor", skyColor);
            }
            
            if (skyboxMaterial.HasProperty("_HorizonColor"))
            {
                skyboxMaterial.SetColor("_HorizonColor", horizonColor);
            }
        }
        
        // Public methods for external access
        public bool IsDay()
        {
            if (!isInitialized) return true;
            
            var (hour, _, _) = GameTimeService.GetCurrentGameTime();
            return hour >= sunriseHour && hour <= sunsetHour;
        }
        
        public bool IsNight()
        {
            return !IsDay();
        }
        
        public float GetDayProgress()
        {
            if (!isInitialized) return 0.5f;
            
            var (hour, minute, _) = GameTimeService.GetCurrentGameTime();
            return CalculateSunProgress(hour, minute);
        }
        
        public (int hour, int minute) GetSunriseTime()
        {
            return (sunriseHour, 0);
        }
        
        public (int hour, int minute) GetSunsetTime()
        {
            return (sunsetHour, 0);
        }
        
        // Editor helper method
        [ContextMenu("Test Day Lighting")]
        private void TestDayLighting()
        {
            if (sunLight != null)
            {
                sunLight.color = dayLightColor;
                sunLight.intensity = dayIntensity;
                if (sunLight.useColorTemperature)
                {
                    sunLight.colorTemperature = dayTemperature;
                }
            }
        }
        
        [ContextMenu("Test Night Lighting")]
        private void TestNightLighting()
        {
            if (sunLight != null)
            {
                sunLight.color = nightLightColor;
                sunLight.intensity = nightIntensity;
                if (sunLight.useColorTemperature)
                {
                    sunLight.colorTemperature = nightTemperature;
                }
            }
        }
    }
} 