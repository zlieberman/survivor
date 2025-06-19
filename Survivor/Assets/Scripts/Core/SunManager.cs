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
        [SerializeField] private Color sunsetOrangeColor = new Color(1f, 0.6f, 0.3f);
        [SerializeField] private Color sunsetRedColor = new Color(1f, 0.4f, 0.2f);
        [SerializeField] private Color sunsetPurpleColor = new Color(0.6f, 0.2f, 0.4f);
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
        
        [Header("Smooth Movement")]
        [SerializeField] private float realTimePerGameHour = 120f; // 2 minutes = 1 hour (should match TimeManager)
        [SerializeField] private bool updateEveryFrame = true; // Set to false for performance
        
        private bool isInitialized = false;
        private float elapsedTime = 0f;
        private int startHour = 10; // Game starts at 10:00 AM
        private int startMinute = 0;
        
        // Precalculated values for smooth movement
        private float startAngle;
        private float angleChangePerSecond;
        private float fullDayDuration; // Total time for full day cycle in seconds
        
        private void Start()
        {
            Debug.Log("[SunManager] Start() called");
            
            // Subscribe to time provider events
            GameTimeService.OnTimeProviderRegistered += OnTimeProviderRegistered;
            GameTimeService.OnTimeProviderUnregistered += OnTimeProviderUnregistered;
            
            // Check if time provider is already available
            if (GameTimeService.HasTimeProvider)
            {
                Debug.Log("[SunManager] Time provider already available, initializing immediately");
                OnTimeProviderRegistered(GameTimeService.TimeProvider);
            }
            else
            {
                Debug.Log("[SunManager] No time provider available yet, waiting for registration");
            }
            
            // Auto-find components if not assigned
            if (sunLight == null)
            {
                sunLight = FindObjectOfType<Light>();
                if (sunLight == null)
                {
                    Debug.LogError("[SunManager] No Light component found in scene!");
                }
                else
                {
                    Debug.Log($"[SunManager] Found Light component: {sunLight.name}");
                }
            }
            
            if (sunTransform == null && sunLight != null)
            {
                sunTransform = sunLight.transform;
                Debug.Log($"[SunManager] Assigned sun transform: {sunTransform.name}");
            }
            
            if (skyboxMaterial == null)
            {
                skyboxMaterial = RenderSettings.skybox;
                if (skyboxMaterial == null)
                {
                    Debug.LogWarning("[SunManager] No skybox material found - skybox effects will be disabled");
                }
                else
                {
                    Debug.Log($"[SunManager] Found skybox material: {skyboxMaterial.name}");
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
            
            // Get the real time per game hour from the time provider
            if (provider is TimeManager timeManager)
            {
                realTimePerGameHour = timeManager.RealTimePerGameHour;
                startHour = timeManager.StartHour;
                startMinute = timeManager.StartMinute;
                Debug.Log($"[SunManager] Got time settings from TimeManager - RealTimePerGameHour: {realTimePerGameHour}s, Start: {startHour:D2}:{startMinute:D2}");
            }
            
            InitializeSmoothMovement();
            isInitialized = true;
        }
        
        private void OnTimeProviderUnregistered()
        {
            Debug.Log("[SunManager] Time provider unregistered");
            isInitialized = false;
        }
        
        private void InitializeSmoothMovement()
        {
            // Calculate the starting angle based on the game's start time
            float startTimeInHours = startHour + (startMinute / 60f);
            
            // Calculate how many hours into the day cycle we are
            // Day cycle: 0h = sunrise (5 AM), 16h = sunset (9 PM), 24h = next sunrise
            float hoursIntoDayCycle = (startTimeInHours - sunriseHour + 24f) % 24f;
            
            // Convert hours into day cycle to angle (0-360 degrees)
            // 0h = 0° (sunrise), 8h = 180° (noon), 16h = 360° (sunset), 24h = 0° (next sunrise)
            startAngle = (hoursIntoDayCycle / 24f) * 360f;
            
            // Calculate the full day cycle duration (24 hours in real time)
            fullDayDuration = 24f * realTimePerGameHour;
            
            // Calculate angle change per second for smooth movement
            // Full cycle: 360° total rotation over 24 hours
            angleChangePerSecond = 360f / fullDayDuration;
            
            Debug.Log($"[SunManager] Smooth movement initialized:");
            Debug.Log($"[SunManager] Start time: {startHour:D2}:{startMinute:D2} ({startTimeInHours:F2}h)");
            Debug.Log($"[SunManager] Hours into day cycle: {hoursIntoDayCycle:F2}h");
            Debug.Log($"[SunManager] Start angle: {startAngle:F1}°");
            Debug.Log($"[SunManager] Full day duration: {fullDayDuration:F1}s");
            Debug.Log($"[SunManager] Angle change per second: {angleChangePerSecond:F3}°/s");
            
            // Set initial position
            UpdateSunPosition();
        }
        
        private void Update()
        {
            if (isInitialized)
            {
                elapsedTime += Time.deltaTime;
                
                if (updateEveryFrame)
                {
                    UpdateSunPosition();
                }
            }
            else
            {
                // Debug: Check if we should be initialized but aren't
                if (GameTimeService.HasTimeProvider && !isInitialized)
                {
                    Debug.LogWarning("[SunManager] Time provider available but not initialized - forcing initialization");
                    OnTimeProviderRegistered(GameTimeService.TimeProvider);
                }
            }
        }
        
        private void UpdateSunPosition()
        {
            // Calculate current angle based on elapsed time
            float currentAngle = startAngle + (elapsedTime * angleChangePerSecond);
            
            // Keep angle within 0-360 range
            currentAngle = currentAngle % 360f;
            if (currentAngle < 0f) currentAngle += 360f;
            
            // Convert angle to progress for lighting calculations
            float progress = CalculateProgressFromAngleForLighting(currentAngle);
            
            UpdateSunTransform(currentAngle, progress);
            UpdateLighting(progress);
            UpdateSkybox(progress);
        }
        
        private float CalculateProgressFromTime(float timeInHours)
        {
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
        
        private float CalculateAngleFromProgress(float progress)
        {
            // Convert progress (0-1) to angle (0-360)
            // Progress 0 (sunrise): angle = 0°
            // Progress 0.5 (noon): angle = 180°
            // Progress 1 (sunset): angle = 360°
            return progress * 360f;
        }
        
        private float CalculateProgressFromAngle(float angle)
        {
            // Convert angle (0-360) back to progress (0-1)
            // This is used for lighting calculations
            return angle / 360f;
        }
        
        private float CalculateProgressFromAngleForLighting(float angle)
        {
            // Convert angle (0-360) to progress (0-1) for lighting
            // This accounts for the fact that lighting should be based on day/night cycle
            // not just the sun's position in the sky
            
            // Normalize angle to 0-360
            angle = angle % 360f;
            if (angle < 0f) angle += 360f;
            
            // Convert to hours into day cycle (0-24)
            float hoursIntoDayCycle = (angle / 360f) * 24f;
            
            // Convert to time of day
            float timeOfDay = (hoursIntoDayCycle + sunriseHour) % 24f;
            
            // Calculate progress for lighting (0 = sunrise, 1 = sunset)
            return CalculateProgressFromTime(timeOfDay);
        }
        
        private void UpdateSunTransform(float angle, float progress)
        {
            if (sunTransform == null) 
            {
                Debug.LogWarning("[SunManager] UpdateSunTransform: sunTransform is null!");
                return;
            }
            
            // Convert angle to radians for positioning
            float radians = angle * Mathf.Deg2Rad;
            
            // Calculate position based on the sun's arc
            // Sun rises in the east (positive X) and sets in the west (negative X)
            // Height follows a realistic arc that peaks at noon
            
            // X position: East to West movement
            // At 0° (sunrise): X = +sunHeight (east)
            // At 180° (noon): X = 0 (overhead)
            // At 360° (sunset): X = -sunHeight (west)
            float x = Mathf.Cos(radians) * sunHeight;
            
            // Y position: Height arc that peaks at noon
            // Use a smooth curve that rises from horizon to peak and back down
            float heightProgress = Mathf.Sin(radians); // -1 to 1
            float y = Mathf.Max(heightProgress * sunHeight, 0f); // Never below horizon during day
            
            // Ensure the sun is always above the horizon during the day
            if (progress > 0f && progress < 1f)
            {
                y = Mathf.Max(y, 10f); // Minimum height during day
            }
            
            Vector3 newPosition = new Vector3(x, y, 0f);
            sunTransform.position = newPosition;
            
            // Make the sun look down at the world
            sunTransform.LookAt(Vector3.zero);
            
            // Rotate the sun to face the correct direction based on time
            if (progress > 0f && progress < 1f)
            {
                float rotationAngle = Mathf.Lerp(-45f, 45f, progress);
                sunTransform.rotation = Quaternion.Euler(rotationAngle, 0f, 0f);
            }
        }
        
        private void UpdateLighting(float progress)
        {
            if (sunLight == null) 
            {
                Debug.LogWarning("[SunManager] UpdateLighting: sunLight is null!");
                return;
            }
            
            // Calculate smooth transitions for different phases of the day
            Color newColor;
            float newIntensity;
            float newTemperature;
            
            if (progress <= 0f)
            {
                // Night time - before sunrise
                newColor = nightLightColor;
                newIntensity = nightIntensity;
                newTemperature = nightTemperature;
            }
            else if (progress <= 0.1f)
            {
                // Early morning - sunrise transition
                float sunriseBlend = progress / 0.1f;
                newColor = Color.Lerp(nightLightColor, dayLightColor, sunriseBlend);
                newIntensity = Mathf.Lerp(nightIntensity, dayIntensity, sunriseBlend);
                newTemperature = Mathf.Lerp(nightTemperature, dayTemperature, sunriseBlend);
            }
            else if (progress <= 0.8f)
            {
                // Day time - normal daylight
                newColor = dayLightColor;
                newIntensity = dayIntensity;
                newTemperature = dayTemperature;
            }
            else if (progress <= 0.9f)
            {
                // Late afternoon - sunset orange transition
                float sunsetBlend = (progress - 0.8f) / 0.1f;
                newColor = Color.Lerp(dayLightColor, sunsetOrangeColor, sunsetBlend);
                newIntensity = Mathf.Lerp(dayIntensity, dayIntensity * 0.8f, sunsetBlend);
                newTemperature = Mathf.Lerp(dayTemperature, 4000f, sunsetBlend);
            }
            else if (progress <= 0.95f)
            {
                // Sunset - orange to red transition
                float redBlend = (progress - 0.9f) / 0.05f;
                newColor = Color.Lerp(sunsetOrangeColor, sunsetRedColor, redBlend);
                newIntensity = Mathf.Lerp(dayIntensity * 0.8f, dayIntensity * 0.6f, redBlend);
                newTemperature = Mathf.Lerp(4000f, 3000f, redBlend);
            }
            else if (progress <= 1f)
            {
                // Dusk - red to purple to night transition
                float duskBlend = (progress - 0.95f) / 0.05f;
                Color purpleColor = Color.Lerp(sunsetRedColor, sunsetPurpleColor, duskBlend);
                newColor = Color.Lerp(purpleColor, nightLightColor, duskBlend);
                newIntensity = Mathf.Lerp(dayIntensity * 0.6f, nightIntensity, duskBlend);
                newTemperature = Mathf.Lerp(3000f, nightTemperature, duskBlend);
            }
            else
            {
                // Night time - after sunset
                newColor = nightLightColor;
                newIntensity = nightIntensity;
                newTemperature = nightTemperature;
            }
            
            sunLight.color = newColor;
            sunLight.intensity = newIntensity;
            
            // Update light temperature if using URP/HDRP
            if (sunLight.useColorTemperature)
            {
                sunLight.colorTemperature = newTemperature;
            }
            
            // Update ambient lighting with smooth transitions
            Color ambientColor;
            if (progress <= 0f)
            {
                ambientColor = nightLightColor * 0.1f;
            }
            else if (progress <= 0.1f)
            {
                float sunriseBlend = progress / 0.1f;
                ambientColor = Color.Lerp(nightLightColor * 0.1f, dayLightColor * 0.3f, sunriseBlend);
            }
            else if (progress <= 0.8f)
            {
                ambientColor = dayLightColor * 0.3f;
            }
            else if (progress <= 0.9f)
            {
                float sunsetBlend = (progress - 0.8f) / 0.1f;
                ambientColor = Color.Lerp(dayLightColor * 0.3f, sunsetOrangeColor * 0.2f, sunsetBlend);
            }
            else if (progress <= 0.95f)
            {
                float redBlend = (progress - 0.9f) / 0.05f;
                ambientColor = Color.Lerp(sunsetOrangeColor * 0.2f, sunsetRedColor * 0.15f, redBlend);
            }
            else if (progress <= 1f)
            {
                float duskBlend = (progress - 0.95f) / 0.05f;
                Color purpleAmbient = Color.Lerp(sunsetRedColor * 0.15f, sunsetPurpleColor * 0.1f, duskBlend);
                ambientColor = Color.Lerp(purpleAmbient, nightLightColor * 0.1f, duskBlend);
            }
            else
            {
                ambientColor = nightLightColor * 0.1f;
            }
            
            RenderSettings.ambientLight = ambientColor;
        }
        
        private void UpdateSkybox(float progress)
        {
            if (skyboxMaterial == null) return;
            
            // Calculate sky colors with smooth transitions matching the lighting
            Color skyColor, horizonColor;
            
            if (progress <= 0f)
            {
                // Night time - before sunrise
                skyColor = nightSkyColor;
                horizonColor = nightHorizonColor;
            }
            else if (progress <= 0.1f)
            {
                // Early morning - sunrise transition
                float sunriseBlend = progress / 0.1f;
                skyColor = Color.Lerp(nightSkyColor, daySkyColor, sunriseBlend);
                horizonColor = Color.Lerp(nightHorizonColor, dayHorizonColor, sunriseBlend);
            }
            else if (progress <= 0.8f)
            {
                // Day time - normal daylight
                skyColor = daySkyColor;
                horizonColor = dayHorizonColor;
            }
            else if (progress <= 0.9f)
            {
                // Late afternoon - sunset orange transition
                float sunsetBlend = (progress - 0.8f) / 0.1f;
                Color sunsetSky = Color.Lerp(daySkyColor, sunsetOrangeColor, sunsetBlend);
                Color sunsetHorizon = Color.Lerp(dayHorizonColor, sunsetOrangeColor, sunsetBlend);
                skyColor = sunsetSky;
                horizonColor = sunsetHorizon;
            }
            else if (progress <= 0.95f)
            {
                // Sunset - orange to red transition
                float redBlend = (progress - 0.9f) / 0.05f;
                Color redSky = Color.Lerp(sunsetOrangeColor, sunsetRedColor, redBlend);
                Color redHorizon = Color.Lerp(sunsetOrangeColor, sunsetRedColor, redBlend);
                skyColor = redSky;
                horizonColor = redHorizon;
            }
            else if (progress <= 1f)
            {
                // Dusk - red to purple to night transition
                float duskBlend = (progress - 0.95f) / 0.05f;
                Color purpleSky = Color.Lerp(sunsetRedColor, sunsetPurpleColor, duskBlend);
                Color purpleHorizon = Color.Lerp(sunsetRedColor, sunsetPurpleColor, duskBlend);
                skyColor = Color.Lerp(purpleSky, nightSkyColor, duskBlend);
                horizonColor = Color.Lerp(purpleHorizon, nightHorizonColor, duskBlend);
            }
            else
            {
                // Night time - after sunset
                skyColor = nightSkyColor;
                horizonColor = nightHorizonColor;
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
        
        // Public methods for external access
        public bool IsDay()
        {
            if (!isInitialized) return true;
            
            var (hour, minute, _) = GetCurrentGameTimeFromElapsed();
            float timeInHours = hour + (minute / 60f);
            
            // Day is between sunrise and sunset (inclusive of sunrise, exclusive of sunset)
            bool isDay = timeInHours >= sunriseHour && timeInHours < sunsetHour;
            return isDay;
        }
        
        public bool IsNight()
        {
            return !IsDay();
        }
        
        public float GetDayProgress()
        {
            if (!isInitialized) return 0.5f;
            
            var (hour, minute, _) = GetCurrentGameTimeFromElapsed();
            return CalculateProgressFromTime(hour + (minute / 60f));
        }
        
        public (int hour, int minute) GetSunriseTime()
        {
            return (sunriseHour, 0);
        }
        
        public (int hour, int minute) GetSunsetTime()
        {
            return (sunsetHour, 0);
        }
        
        // Helper method to calculate current game time from elapsed time
        private (int hour, int minute, int day) GetCurrentGameTimeFromElapsed()
        {
            float totalGameHours = elapsedTime / realTimePerGameHour;
            float totalTimeInHours = totalGameHours + startHour + (startMinute / 60f);
            
            int totalHours = Mathf.FloorToInt(totalTimeInHours);
            int hour = totalHours % 24;
            int minute = Mathf.FloorToInt((totalTimeInHours - totalHours) * 60f);
            int day = (totalHours / 24) + 1;
            
            return (hour, minute, day);
        }
        
        // Debug method to force update and test time calculation
        [ContextMenu("Force Update")]
        public void ForceUpdate()
        {
            Debug.Log("[SunManager] Force update called");
            if (!isInitialized)
            {
                Debug.LogWarning("[SunManager] Not initialized - cannot force update");
                return;
            }
            
            elapsedTime = 0f; // Force update by resetting elapsed time
            UpdateSunPosition();
        }
        
        [ContextMenu("Show Current Status")]
        public void ShowCurrentStatus()
        {
            Debug.Log("=== SunManager Status ===");
            Debug.Log($"[SunManager] Initialized: {isInitialized}");
            Debug.Log($"[SunManager] Elapsed time: {elapsedTime:F1}s");
            
            if (isInitialized)
            {
                var (hour, minute, day) = GetCurrentGameTimeFromElapsed();
                Debug.Log($"[SunManager] Calculated game time: {hour:D2}:{minute:D2} (Day {day})");
                
                float progress = CalculateProgressFromTime(hour + (minute / 60f));
                Debug.Log($"[SunManager] Sun progress: {progress:F2}");
                Debug.Log($"[SunManager] Is day: {IsDay()}, Is night: {IsNight()}");
                
                float currentAngle = startAngle + (elapsedTime * angleChangePerSecond);
                currentAngle = currentAngle % 360f;
                if (currentAngle < 0f) currentAngle += 360f;
                Debug.Log($"[SunManager] Current angle: {currentAngle:F1}°");
            }
            else
            {
                Debug.LogWarning("[SunManager] Not initialized!");
            }
            
            if (sunTransform != null)
            {
                Debug.Log($"[SunManager] Sun transform: {sunTransform.name}, Position: {sunTransform.position}");
            }
            else
            {
                Debug.LogWarning("[SunManager] Sun transform is null!");
            }
            
            if (sunLight != null)
            {
                Debug.Log($"[SunManager] Sun light: {sunLight.name}, Intensity: {sunLight.intensity}, Color: {sunLight.color}");
            }
            else
            {
                Debug.LogWarning("[SunManager] Sun light is null!");
            }
            
            Debug.Log("=== End Status ===");
        }
        
        [ContextMenu("Test Time Calculation")]
        public void TestTimeCalculation()
        {
            Debug.Log("=== SunManager Time Calculation Test ===");
            
            if (!isInitialized)
            {
                Debug.LogWarning("[SunManager] Not initialized - cannot test time calculation");
                return;
            }
            
            var (hour, minute, day) = GetCurrentGameTimeFromElapsed();
            float timeInHours = hour + (minute / 60f);
            
            Debug.Log($"[SunManager] Elapsed time: {elapsedTime:F1}s");
            Debug.Log($"[SunManager] Calculated game time: {hour:D2}:{minute:D2} ({timeInHours:F2}h)");
            Debug.Log($"[SunManager] Sunrise: {sunriseHour}, Sunset: {sunsetHour}");
            Debug.Log($"[SunManager] Is day time? {timeInHours >= sunriseHour && timeInHours < sunsetHour}");
            
            float progress = CalculateProgressFromTime(timeInHours);
            Debug.Log($"[SunManager] Sun progress: {progress:F2}");
            
            float currentAngle = startAngle + (elapsedTime * angleChangePerSecond);
            currentAngle = currentAngle % 360f;
            if (currentAngle < 0f) currentAngle += 360f;
            Debug.Log($"[SunManager] Current angle: {currentAngle:F1}°");
            
            // Calculate hours into day cycle
            float hoursIntoDayCycle = (currentAngle / 360f) * 24f;
            float timeOfDay = (hoursIntoDayCycle + sunriseHour) % 24f;
            Debug.Log($"[SunManager] Hours into day cycle: {hoursIntoDayCycle:F2}h");
            Debug.Log($"[SunManager] Time of day from angle: {timeOfDay:F2}h");
            
            if (progress <= 0f)
            {
                Debug.Log("[SunManager] Status: NIGHT (before sunrise)");
            }
            else if (progress >= 1f)
            {
                Debug.Log("[SunManager] Status: NIGHT (after sunset)");
            }
            else
            {
                Debug.Log($"[SunManager] Status: DAY (progress: {progress:F2})");
            }
            
            Debug.Log("=== End Test ===");
        }
        
        [ContextMenu("Test Sun Positioning")]
        public void TestSunPositioning()
        {
            Debug.Log("=== Sun Positioning Test ===");
            
            if (!isInitialized)
            {
                Debug.LogWarning("[SunManager] Not initialized - cannot test sun positioning");
                return;
            }
            
            var (hour, minute, day) = GetCurrentGameTimeFromElapsed();
            float timeInHours = hour + (minute / 60f);
            
            Debug.Log($"[SunManager] Elapsed time: {elapsedTime:F1}s");
            Debug.Log($"[SunManager] Calculated game time: {hour:D2}:{minute:D2} ({timeInHours:F2}h)");
            Debug.Log($"[SunManager] Sunrise: {sunriseHour}, Sunset: {sunsetHour}");
            
            float progress = CalculateProgressFromTime(timeInHours);
            Debug.Log($"[SunManager] Sun progress: {progress:F2}");
            
            // Calculate what the sun angle should be
            float currentAngle = startAngle + (elapsedTime * angleChangePerSecond);
            currentAngle = currentAngle % 360f;
            if (currentAngle < 0f) currentAngle += 360f;
            
            Debug.Log($"[SunManager] Current angle: {currentAngle:F1}°");
            
            // Calculate hours into day cycle and time of day
            float hoursIntoDayCycle = (currentAngle / 360f) * 24f;
            float timeOfDay = (hoursIntoDayCycle + sunriseHour) % 24f;
            Debug.Log($"[SunManager] Hours into day cycle: {hoursIntoDayCycle:F2}h");
            Debug.Log($"[SunManager] Time of day from angle: {timeOfDay:F2}h");
            
            // Calculate expected position with new east-west system
            float radians = currentAngle * Mathf.Deg2Rad;
            float x = Mathf.Cos(radians) * sunHeight; // East to West
            float heightProgress = Mathf.Sin(radians);
            float y = Mathf.Max(heightProgress * sunHeight, 0f);
            
            if (progress > 0f && progress < 1f)
            {
                y = Mathf.Max(y, 10f);
            }
            
            Vector3 expectedPosition = new Vector3(x, y, 0f);
            Debug.Log($"[SunManager] Expected sun position: {expectedPosition}");
            Debug.Log($"[SunManager] X position: {x:F1} ({(x > 0 ? "East" : x < 0 ? "West" : "Overhead")})");
            Debug.Log($"[SunManager] Y height: {y:F1}");
            
            if (sunTransform != null)
            {
                Debug.Log($"[SunManager] Actual sun position: {sunTransform.position}");
                Debug.Log($"[SunManager] Position difference: {sunTransform.position - expectedPosition}");
            }
            
            Debug.Log("=== End Test ===");
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
        
        [ContextMenu("Force Day Lighting")]
        public void ForceDayLighting()
        {
            Debug.Log("[SunManager] Force day lighting called");
            if (sunLight != null)
            {
                sunLight.color = dayLightColor;
                sunLight.intensity = dayIntensity;
                if (sunLight.useColorTemperature)
                {
                    sunLight.colorTemperature = dayTemperature;
                }
                RenderSettings.ambientLight = dayLightColor * 0.3f;
                Debug.Log($"[SunManager] Forced day lighting - Color: {dayLightColor}, Intensity: {dayIntensity}");
            }
            else
            {
                Debug.LogWarning("[SunManager] sunLight is null - cannot force day lighting");
            }
        }
    }
} 