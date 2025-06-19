using UnityEngine;
using Survivor.Shared;
using System.Collections;

namespace Survivor.Core
{
    public class EnhancedSunManager : MonoBehaviour
    {
        [Header("Sun Settings")]
        [SerializeField] private Light sunLight;
        [SerializeField] private Transform sunTransform;
        [SerializeField] private Material skyboxMaterial;
        [SerializeField] private Light moonLight; // Optional moon light for night
        
        [Header("Time Settings")]
        [SerializeField] private int sunriseHour = 5; // 5 AM
        [SerializeField] private int sunsetHour = 21; // 9 PM
        [SerializeField] private float sunRiseDuration = 2f; // Hours for sunrise transition
        [SerializeField] private float sunSetDuration = 2f; // Hours for sunset transition
        
        [Header("Lighting Settings")]
        [SerializeField] private Color dayLightColor = new Color(1f, 0.95f, 0.8f);
        [SerializeField] private Color nightLightColor = new Color(0.1f, 0.1f, 0.3f);
        [SerializeField] private Color moonLightColor = new Color(0.7f, 0.7f, 1f);
        [SerializeField] private float dayIntensity = 1f;
        [SerializeField] private float nightIntensity = 0.1f;
        [SerializeField] private float moonIntensity = 0.3f;
        [SerializeField] private float dayTemperature = 5500f; // Kelvin
        [SerializeField] private float nightTemperature = 2000f; // Kelvin
        [SerializeField] private float moonTemperature = 4000f; // Kelvin
        
        [Header("Skybox Settings")]
        [SerializeField] private Color daySkyColor = new Color(0.5f, 0.7f, 1f);
        [SerializeField] private Color nightSkyColor = new Color(0.05f, 0.05f, 0.1f);
        [SerializeField] private Color dayHorizonColor = new Color(0.8f, 0.9f, 1f);
        [SerializeField] private Color nightHorizonColor = new Color(0.1f, 0.1f, 0.2f);
        [SerializeField] private Color sunriseColor = new Color(1f, 0.6f, 0.3f);
        [SerializeField] private Color sunsetColor = new Color(1f, 0.4f, 0.2f);
        
        [Header("Sun Movement")]
        [SerializeField] private float sunStartAngle = -90f; // Sunrise position
        [SerializeField] private float sunEndAngle = 90f; // Sunset position
        [SerializeField] private float sunHeight = 100f; // Height of sun above ground
        [SerializeField] private float moonStartAngle = 90f; // Moonrise position
        [SerializeField] private float moonEndAngle = -90f; // Moonset position
        
        [Header("Weather Effects")]
        [SerializeField] private bool enableWeatherEffects = true;
        [SerializeField] private float cloudCover = 0f; // 0 = clear, 1 = overcast
        [SerializeField] private float fogDensity = 0f; // 0 = clear, 1 = foggy
        [SerializeField] private Color overcastColor = new Color(0.6f, 0.6f, 0.7f);
        
        [Header("Performance")]
        [SerializeField] private bool updateEveryFrame = false; // Set to true for smooth transitions
        [SerializeField] private float updateInterval = 1f; // Seconds between updates if not every frame
        
        private bool isInitialized = false;
        private int lastUpdateHour = -1;
        private int lastUpdateMinute = -1; // Track minutes for more frequent updates
        private float lastUpdateTime = 0f;
        private float currentCloudCover = 0f;
        private float currentFogDensity = 0f;
        
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
                    Debug.LogError("[EnhancedSunManager] No Light component found in scene!");
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
                    Debug.LogWarning("[EnhancedSunManager] No skybox material found - skybox effects will be disabled");
                }
            }
            
            // Initialize weather effects
            currentCloudCover = cloudCover;
            currentFogDensity = fogDensity;
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            GameTimeService.OnTimeProviderRegistered -= OnTimeProviderRegistered;
            GameTimeService.OnTimeProviderUnregistered -= OnTimeProviderUnregistered;
        }
        
        private void OnTimeProviderRegistered(ITimeProvider provider)
        {
            Debug.Log("[EnhancedSunManager] Time provider registered, initializing enhanced sun system");
            isInitialized = true;
            lastUpdateHour = -1; // Force initial update
            lastUpdateMinute = -1; // Force initial update
            UpdateSunPosition();
        }
        
        private void OnTimeProviderUnregistered()
        {
            Debug.Log("[EnhancedSunManager] Time provider unregistered");
            isInitialized = false;
        }
        
        private void Update()
        {
            if (!isInitialized) return;
            
            // Update based on settings
            if (updateEveryFrame)
            {
                UpdateSunPosition();
            }
            else if (Time.time - lastUpdateTime >= updateInterval)
            {
                UpdateSunPosition();
                lastUpdateTime = Time.time;
            }
        }
        
        private void UpdateSunPosition()
        {
            var (hour, minute, _) = GameTimeService.GetCurrentGameTime();
            
            // Only update if hour has changed (for performance) or if updating every frame
            if (!updateEveryFrame && hour == lastUpdateHour && minute == lastUpdateMinute) return;
            
            lastUpdateHour = hour;
            lastUpdateMinute = minute;
            
            // Calculate sun position and lighting
            float sunProgress = CalculateSunProgress(hour, minute);
            UpdateSunTransform(sunProgress);
            UpdateMoonTransform(sunProgress);
            UpdateLighting(sunProgress);
            UpdateSkybox(sunProgress);
            UpdateWeatherEffects(sunProgress);
            
            Debug.Log($"[EnhancedSunManager] Updated sun position for {hour:D2}:{minute:D2} - Progress: {sunProgress:F2}");
            if (sunTransform != null)
            {
                Debug.Log($"[EnhancedSunManager] Sun position: {sunTransform.position}, Rotation: {sunTransform.rotation.eulerAngles}");
            }
            if (moonLight != null)
            {
                Debug.Log($"[EnhancedSunManager] Moon position: {moonLight.transform.position}, Rotation: {moonLight.transform.rotation.eulerAngles}");
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
            else if (timeInHours >= sunsetHour)
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
        
        private void UpdateMoonTransform(float sunProgress)
        {
            if (moonLight == null) return;
            
            // Moon follows opposite cycle to sun
            float moonProgress = 1f - sunProgress;
            
            // Calculate moon position based on time of day
            // Moon rises when sun sets and sets when sun rises
            float moonAngle;
            
            if (moonProgress <= 0f)
            {
                // Moon below horizon
                moonAngle = -90f;
            }
            else if (moonProgress >= 1f)
            {
                // Moon below horizon
                moonAngle = -90f;
            }
            else
            {
                // Moon follows an arc opposite to the sun
                float normalizedProgress = moonProgress * 2f - 1f; // Convert 0-1 to -1 to 1
                moonAngle = Mathf.Sin(normalizedProgress * Mathf.PI) * 90f;
            }
            
            // Position the moon in 3D space
            float radians = moonAngle * Mathf.Deg2Rad;
            
            float x = Mathf.Sin(radians) * sunHeight; // East-west position
            float y = Mathf.Cos(radians) * sunHeight; // Height above horizon
            
            // Ensure the moon is always above the horizon during the night
            if (moonProgress > 0f && moonProgress < 1f)
            {
                y = Mathf.Max(y, 10f); // Minimum height during night
            }
            
            moonLight.transform.position = new Vector3(x, y, 0f);
            
            // Make the moon look down at the world
            moonLight.transform.LookAt(Vector3.zero);
            
            // Rotate the moon to face the correct direction
            if (moonProgress > 0f && moonProgress < 1f)
            {
                float rotationAngle = Mathf.Lerp(-45f, 45f, moonProgress);
                moonLight.transform.rotation = Quaternion.Euler(rotationAngle, 0f, 0f);
            }
        }
        
        private void UpdateLighting(float progress)
        {
            if (sunLight == null) return;
            
            // Interpolate between night and day lighting
            float dayNightBlend = Mathf.SmoothStep(0f, 1f, progress);
            
            // Update sun light
            sunLight.color = Color.Lerp(nightLightColor, dayLightColor, dayNightBlend);
            sunLight.intensity = Mathf.Lerp(nightIntensity, dayIntensity, dayNightBlend);
            
            // Update light temperature if using URP/HDRP
            if (sunLight.useColorTemperature)
            {
                sunLight.colorTemperature = Mathf.Lerp(nightTemperature, dayTemperature, dayNightBlend);
            }
            
            // Update moon light
            if (moonLight != null)
            {
                moonLight.color = moonLightColor;
                moonLight.intensity = moonIntensity * (1f - dayNightBlend);
                if (moonLight.useColorTemperature)
                {
                    moonLight.colorTemperature = moonTemperature;
                }
            }
            
            // Update ambient lighting
            Color ambientColor = Color.Lerp(nightLightColor * 0.1f, dayLightColor * 0.3f, dayNightBlend);
            RenderSettings.ambientLight = ambientColor;
        }
        
        private void UpdateSkybox(float progress)
        {
            if (skyboxMaterial == null) return;
            
            // Interpolate skybox colors with sunrise/sunset effects
            float dayNightBlend = Mathf.SmoothStep(0f, 1f, progress);
            
            Color skyColor, horizonColor;
            
            // Special colors for sunrise/sunset
            if (progress < 0.1f) // Early morning
            {
                float sunriseBlend = progress / 0.1f;
                skyColor = Color.Lerp(nightSkyColor, sunriseColor, sunriseBlend);
                horizonColor = Color.Lerp(nightHorizonColor, sunriseColor, sunriseBlend);
            }
            else if (progress > 0.9f) // Late evening
            {
                float sunsetBlend = (progress - 0.9f) / 0.1f;
                skyColor = Color.Lerp(sunsetColor, nightSkyColor, sunsetBlend);
                horizonColor = Color.Lerp(sunsetColor, nightHorizonColor, sunsetBlend);
            }
            else // Daytime
            {
                skyColor = Color.Lerp(nightSkyColor, daySkyColor, dayNightBlend);
                horizonColor = Color.Lerp(nightHorizonColor, dayHorizonColor, dayNightBlend);
            }
            
            // Apply weather effects
            if (enableWeatherEffects)
            {
                skyColor = Color.Lerp(skyColor, overcastColor, currentCloudCover);
                horizonColor = Color.Lerp(horizonColor, overcastColor, currentCloudCover);
            }
            
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
        
        private void UpdateWeatherEffects(float progress)
        {
            if (!enableWeatherEffects) return;
            
            // Update fog
            RenderSettings.fogDensity = currentFogDensity;
            
            // Update cloud cover effect on lighting
            if (sunLight != null)
            {
                float cloudEffect = 1f - (currentCloudCover * 0.5f);
                sunLight.intensity *= cloudEffect;
            }
        }
        
        // Public methods for external access
        public bool IsDay()
        {
            if (!isInitialized) return true;
            
            var (hour, minute, _) = GameTimeService.GetCurrentGameTime();
            float timeInHours = hour + (minute / 60f);
            
            // Day is between sunrise and sunset (inclusive of sunrise, exclusive of sunset)
            return timeInHours >= sunriseHour && timeInHours < sunsetHour;
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
        
        // Weather control methods
        public void SetCloudCover(float cover)
        {
            cloudCover = Mathf.Clamp01(cover);
            currentCloudCover = cloudCover;
        }
        
        public void SetFogDensity(float density)
        {
            fogDensity = Mathf.Clamp01(density);
            currentFogDensity = fogDensity;
        }
        
        public void SetWeather(float cloudCover, float fogDensity)
        {
            SetCloudCover(cloudCover);
            SetFogDensity(fogDensity);
        }
        
        // Debug method to force update and test time calculation
        [ContextMenu("Force Update")]
        public void ForceUpdate()
        {
            Debug.Log("[EnhancedSunManager] Force update called");
            if (!isInitialized)
            {
                Debug.LogWarning("[EnhancedSunManager] Not initialized - cannot force update");
                return;
            }
            
            lastUpdateHour = -1; // Force update by resetting last update hour
            lastUpdateMinute = -1; // Force update by resetting last update minute
            UpdateSunPosition();
        }
        
        [ContextMenu("Show Current Status")]
        public void ShowCurrentStatus()
        {
            Debug.Log("=== EnhancedSunManager Status ===");
            Debug.Log($"[EnhancedSunManager] Initialized: {isInitialized}");
            Debug.Log($"[EnhancedSunManager] Last update hour: {lastUpdateHour}, Last update minute: {lastUpdateMinute}");
            
            if (GameTimeService.HasTimeProvider)
            {
                var (hour, minute, day) = GameTimeService.GetCurrentGameTime();
                Debug.Log($"[EnhancedSunManager] Current game time: {hour:D2}:{minute:D2} (Day {day})");
                
                float progress = CalculateSunProgress(hour, minute);
                Debug.Log($"[EnhancedSunManager] Sun progress: {progress:F2}");
                Debug.Log($"[EnhancedSunManager] Is day: {IsDay()}, Is night: {IsNight()}");
            }
            else
            {
                Debug.LogWarning("[EnhancedSunManager] No time provider available!");
            }
            
            if (sunTransform != null)
            {
                Debug.Log($"[EnhancedSunManager] Sun transform: {sunTransform.name}, Position: {sunTransform.position}");
            }
            else
            {
                Debug.LogWarning("[EnhancedSunManager] Sun transform is null!");
            }
            
            if (sunLight != null)
            {
                Debug.Log($"[EnhancedSunManager] Sun light: {sunLight.name}, Intensity: {sunLight.intensity}, Color: {sunLight.color}");
            }
            else
            {
                Debug.LogWarning("[EnhancedSunManager] Sun light is null!");
            }
            
            if (moonLight != null)
            {
                Debug.Log($"[EnhancedSunManager] Moon light: {moonLight.name}, Intensity: {moonLight.intensity}, Color: {moonLight.color}");
            }
            else
            {
                Debug.Log("[EnhancedSunManager] Moon light is null (optional)");
            }
            
            Debug.Log("=== End Status ===");
        }
        
        [ContextMenu("Test Time Calculation")]
        public void TestTimeCalculation()
        {
            Debug.Log("=== EnhancedSunManager Time Calculation Test ===");
            
            if (!isInitialized)
            {
                Debug.LogWarning("[EnhancedSunManager] Not initialized - cannot test time calculation");
                return;
            }
            
            var (hour, minute, day) = GameTimeService.GetCurrentGameTime();
            float timeInHours = hour + (minute / 60f);
            
            Debug.Log($"[EnhancedSunManager] Current game time: {hour:D2}:{minute:D2} ({timeInHours:F2}h)");
            Debug.Log($"[EnhancedSunManager] Sunrise: {sunriseHour}, Sunset: {sunsetHour}");
            Debug.Log($"[EnhancedSunManager] Is day time? {timeInHours >= sunriseHour && timeInHours < sunsetHour}");
            
            float progress = CalculateSunProgress(hour, minute);
            Debug.Log($"[EnhancedSunManager] Sun progress: {progress:F2}");
            
            if (progress <= 0f)
            {
                Debug.Log("[EnhancedSunManager] Status: NIGHT (before sunrise)");
            }
            else if (progress >= 1f)
            {
                Debug.Log("[EnhancedSunManager] Status: NIGHT (after sunset)");
            }
            else
            {
                Debug.Log($"[EnhancedSunManager] Status: DAY (progress: {progress:F2})");
            }
            
            Debug.Log("=== End Test ===");
        }
        
        [ContextMenu("Force Day Lighting")]
        public void ForceDayLighting()
        {
            Debug.Log("[EnhancedSunManager] Force day lighting called");
            if (sunLight != null)
            {
                sunLight.color = dayLightColor;
                sunLight.intensity = dayIntensity;
                if (sunLight.useColorTemperature)
                {
                    sunLight.colorTemperature = dayTemperature;
                }
                RenderSettings.ambientLight = dayLightColor * 0.3f;
                Debug.Log($"[EnhancedSunManager] Forced day lighting - Color: {dayLightColor}, Intensity: {dayIntensity}");
            }
            else
            {
                Debug.LogWarning("[EnhancedSunManager] sunLight is null - cannot force day lighting");
            }
        }
        
        // Editor helper methods
        [ContextMenu("Test Day Lighting")]
        public void TestDayLighting()
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
        public void TestNightLighting()
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
        
        [ContextMenu("Test Clear Weather")]
        private void TestClearWeather()
        {
            SetWeather(0f, 0f);
        }
        
        [ContextMenu("Test Overcast Weather")]
        private void TestOvercastWeather()
        {
            SetWeather(0.8f, 0.2f);
        }
    }
} 