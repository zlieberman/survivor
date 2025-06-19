using UnityEngine;
using Survivor.Shared;

namespace Survivor.Core
{
    public class SimpleSunManager : MonoBehaviour
    {
        [Header("Time Settings")]
        [SerializeField] private float realTimePerGameHour = 120f; // 2 minutes = 1 hour
        [SerializeField] private int startHour = 8; // Game starts at 8:00 AM
        [SerializeField] private int startMinute = 0;
        
        [Header("Sun Cycle")]
        [SerializeField] private int sunriseHour = 5; // 5 AM
        [SerializeField] private int sunsetHour = 20; // 8 PM
        
        [Header("Sun Settings")]
        [SerializeField] private Material skyboxMaterial;
        [SerializeField] private Light directionalLight;
        
        [Header("Lighting Colors")]
        [SerializeField] private Color nightColor = new Color(0.05f, 0.05f, 0.15f);
        [SerializeField] private Color sunriseColor = new Color(1f, 0.6f, 0.3f);
        [SerializeField] private Color dayColor = new Color(1f, 0.95f, 0.8f);
        [SerializeField] private Color sunsetColor = new Color(1f, 0.4f, 0.2f);
        [SerializeField] private Color deepNightColor = new Color(0.02f, 0.02f, 0.08f);
        
        [Header("Lighting Intensities")]
        [SerializeField] private float nightIntensity = 0.02f;
        [SerializeField] private float sunriseIntensity = 0.6f;
        [SerializeField] private float dayIntensity = 1f;
        [SerializeField] private float sunsetIntensity = 0.4f;
        [SerializeField] private float deepNightIntensity = 0.005f;
        
        [Header("Skybox Colors")]
        [SerializeField] private Color daySkyColor = new Color(0.5f, 0.7f, 1f);
        [SerializeField] private Color sunriseSkyColor = new Color(1f, 0.6f, 0.3f);
        [SerializeField] private Color sunsetSkyColor = new Color(1f, 0.4f, 0.2f);
        [SerializeField] private Color nightSkyColor = new Color(0.05f, 0.05f, 0.15f);
        [SerializeField] private Color deepNightSkyColor = new Color(0.02f, 0.02f, 0.08f);
        
        [Header("Horizon Colors")]
        [SerializeField] private Color dayHorizonColor = new Color(0.8f, 0.9f, 1f);
        [SerializeField] private Color sunriseHorizonColor = new Color(1f, 0.7f, 0.4f);
        [SerializeField] private Color sunsetHorizonColor = new Color(1f, 0.5f, 0.3f);
        [SerializeField] private Color nightHorizonColor = new Color(0.1f, 0.1f, 0.2f);
        [SerializeField] private Color deepNightHorizonColor = new Color(0.05f, 0.05f, 0.1f);
        
        private float elapsedTime = 0f;
        private bool isInitialized = false;
        
        private void Start()
        {
            Debug.Log("[SimpleSunManager] Start() called");
            
            // Subscribe to time provider events
            GameTimeService.OnTimeProviderRegistered += OnTimeProviderRegistered;
            GameTimeService.OnTimeProviderUnregistered += OnTimeProviderUnregistered;
            
            // Check if time provider is already available
            if (GameTimeService.HasTimeProvider)
            {
                Debug.Log("[SimpleSunManager] Time provider already available, initializing immediately");
                OnTimeProviderRegistered(GameTimeService.TimeProvider);
            }
            else
            {
                Debug.Log("[SimpleSunManager] No time provider available yet, waiting for registration");
            }
            
            // Auto-find components if not assigned
            if (skyboxMaterial == null)
            {
                skyboxMaterial = RenderSettings.skybox;
                if (skyboxMaterial == null)
                {
                    Debug.LogError("[SimpleSunManager] No skybox material found!");
                }
                else
                {
                    Debug.Log($"[SimpleSunManager] Found skybox material: {skyboxMaterial.name}");
                }
            }
            
            if (directionalLight == null)
            {
                directionalLight = FindObjectOfType<Light>();
                if (directionalLight == null)
                {
                    Debug.LogError("[SimpleSunManager] No Light component found in scene!");
                }
                else
                {
                    Debug.Log($"[SimpleSunManager] Found Light component: {directionalLight.name}");
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
            Debug.Log("[SimpleSunManager] Time provider registered, initializing simple sun system");
            
            // Get the real time per game hour from the time provider
            if (provider is TimeManager timeManager)
            {
                realTimePerGameHour = timeManager.RealTimePerGameHour;
                startHour = timeManager.StartHour;
                startMinute = timeManager.StartMinute;
                Debug.Log($"[SimpleSunManager] Got time settings from TimeManager - RealTimePerGameHour: {realTimePerGameHour}s, Start: {startHour:D2}:{startMinute:D2}");
            }
            
            InitializeSunSystem();
            isInitialized = true;
        }
        
        private void OnTimeProviderUnregistered()
        {
            Debug.Log("[SimpleSunManager] Time provider unregistered");
            isInitialized = false;
        }
        
        private void InitializeSunSystem()
        {
            // Calculate starting sun position based on start time and sunrise/sunset times
            float startTimeInHours = startHour + (startMinute / 60f);
            
            // Calculate day progress for start time
            float dayProgress = 0f;
            bool isDay = false;
            
            if (startTimeInHours < sunriseHour)
            {
                // Before sunrise - night
                dayProgress = 0f;
                isDay = false;
            }
            else if (startTimeInHours >= sunsetHour)
            {
                // After sunset - night
                dayProgress = 1f;
                isDay = false;
            }
            else
            {
                // During day - calculate progress from sunrise to sunset
                float dayDuration = sunsetHour - sunriseHour;
                float timeSinceSunrise = startTimeInHours - sunriseHour;
                dayProgress = timeSinceSunrise / dayDuration;
                isDay = true;
            }
            
            // Calculate initial sun position
            Vector3 sunPosition;
            if (isDay)
            {
                // Convert day progress to angle
                float dayAngle = dayProgress * 180f;
                float radians = dayAngle * Mathf.Deg2Rad;
                
                float x = Mathf.Cos(radians);
                float y = Mathf.Sin(radians);
                float sunDistance = 100f;
                sunPosition = new Vector3(x * sunDistance, Mathf.Max(y * sunDistance, 10f), 0f);
            }
            else
            {
                // Night time - sun below horizon
                sunPosition = new Vector3(0f, -50f, 0f);
            }
            
            if (directionalLight != null)
            {
                directionalLight.transform.position = sunPosition;
                directionalLight.transform.LookAt(Vector3.zero);
                directionalLight.transform.rotation = Quaternion.Euler(dayProgress * 180f, 0f, 0f);
                Debug.Log($"[SimpleSunManager] Set initial sun position to {sunPosition} for {startHour:D2}:{startMinute:D2} (day progress: {dayProgress:F2})");
            }
            
            // Set initial skybox sun direction
            if (skyboxMaterial != null && skyboxMaterial.HasProperty("_SunDirection"))
            {
                Vector3 sunDirection = sunPosition.normalized;
                skyboxMaterial.SetVector("_SunDirection", sunDirection);
                Debug.Log($"[SimpleSunManager] Set initial skybox sun direction to {sunDirection}");
            }
        }
        
        private void Update()
        {
            if (isInitialized)
            {
                elapsedTime += Time.deltaTime;
                UpdateSunPosition();
            }
            else
            {
                // Debug: Check if we should be initialized but aren't
                if (GameTimeService.HasTimeProvider && !isInitialized)
                {
                    Debug.LogWarning("[SimpleSunManager] Time provider available but not initialized - forcing initialization");
                    OnTimeProviderRegistered(GameTimeService.TimeProvider);
                }
            }
        }
        
        private void UpdateSunPosition()
        {
            // Calculate current game time
            float totalGameHours = elapsedTime / realTimePerGameHour;
            float currentTimeInHours = totalGameHours + startHour + (startMinute / 60f);
            
            // Handle 24-hour cycle - wrap time around midnight
            currentTimeInHours = currentTimeInHours % 24f;
            
            // Debug: Log time and sun position every 10 seconds
            if (Time.frameCount % 600 == 0) // Every 10 seconds at 60fps
            {
                int hour = Mathf.FloorToInt(currentTimeInHours);
                int minute = Mathf.FloorToInt((currentTimeInHours - hour) * 60f);
                Debug.Log($"[SimpleSunManager] Game time: {hour:D2}:{minute:D2}, Elapsed: {elapsedTime:F1}s, Total hours: {totalGameHours:F2}");
            }
            
            // Calculate sun position based on actual sunrise/sunset times
            // Sun rises at sunriseHour (5 AM) and sets at sunsetHour (8 PM)
            
            // Calculate progress through the day cycle (0 = sunrise, 1 = sunset)
            float dayProgress = 0f;
            bool isDay = false;
            
            if (currentTimeInHours < sunriseHour)
            {
                // Before sunrise - night
                dayProgress = 0f;
                isDay = false;
            }
            else if (currentTimeInHours >= sunsetHour)
            {
                // After sunset - night
                dayProgress = 1f;
                isDay = false;
            }
            else
            {
                // During day - calculate progress from sunrise to sunset
                float dayDuration = sunsetHour - sunriseHour;
                float timeSinceSunrise = currentTimeInHours - sunriseHour;
                dayProgress = timeSinceSunrise / dayDuration;
                isDay = true;
            }
            
            // Debug: Log day progress
            if (Time.frameCount % 600 == 0)
            {
                Debug.Log($"[SimpleSunManager] Day progress: {dayProgress:F2}, Is day: {isDay}, Sunrise: {sunriseHour}, Sunset: {sunsetHour}");
            }
            
            // Calculate sun position in 3D space
            // During day: sun moves from east to west in an arc
            // During night: sun is below horizon
            
            Vector3 sunPosition;
            if (isDay)
            {
                // Convert day progress (0-1) to angle (0-180 degrees for day arc)
                float dayAngle = dayProgress * 180f; // 0° = sunrise (east), 90° = noon (overhead), 180° = sunset (west)
                float radians = dayAngle * Mathf.Deg2Rad;
                
                // X: East to West movement
                // At 0° (sunrise): X = +1 (east)
                // At 90° (noon): X = 0 (overhead)
                // At 180° (sunset): X = -1 (west)
                float x = Mathf.Cos(radians); // Use cosine so sunrise is +1 (east)
                
                // Y: Height arc that peaks at noon
                // Use sine for height (peaks at noon, lowest at sunrise/sunset)
                float y = Mathf.Sin(radians);
                
                // Scale the position
                float sunDistance = 100f;
                sunPosition = new Vector3(x * sunDistance, Mathf.Max(y * sunDistance, 10f), 0f);
            }
            else
            {
                // Night time - sun below horizon
                // Calculate night progress to position sun for next sunrise
                float nightProgress = 0f;
                
                if (currentTimeInHours < sunriseHour)
                {
                    // Before sunrise - sun is rising from the east
                    nightProgress = currentTimeInHours / sunriseHour;
                }
                else
                {
                    // After sunset - sun is setting to the west
                    nightProgress = (currentTimeInHours - sunsetHour) / (24f - sunsetHour);
                }
                
                // Position sun below horizon, moving from west to east during night
                float nightAngle = nightProgress * 180f; // 0° = west, 180° = east
                float radians = nightAngle * Mathf.Deg2Rad;
                
                float x = Mathf.Cos(radians);
                float sunDistance = 100f;
                sunPosition = new Vector3(x * sunDistance, -50f, 0f);
            }
            
            // Debug: Log sun position and angle
            if (Time.frameCount % 600 == 0)
            {
                float sunHeight = sunPosition.y;
                float sunDistance = sunPosition.magnitude;
                float sunAngleFromHorizon = Mathf.Asin(sunHeight / sunDistance) * Mathf.Rad2Deg;
                Debug.Log($"[SimpleSunManager] Sun position: {sunPosition}, Angle from horizon: {sunAngleFromHorizon:F1}°");
            }
            
            // Update directional light position and rotation
            if (directionalLight != null)
            {
                directionalLight.transform.position = sunPosition;
                directionalLight.transform.LookAt(Vector3.zero);
                
                // Calculate rotation based on day progress
                float sunRotation = dayProgress * 180f; // 0° = sunrise, 180° = sunset
                directionalLight.transform.rotation = Quaternion.Euler(sunRotation, 0f, 0f);
            }
            
            // Update skybox sun direction if available
            if (skyboxMaterial != null && skyboxMaterial.HasProperty("_SunDirection"))
            {
                Vector3 sunDirection = sunPosition.normalized;
                skyboxMaterial.SetVector("_SunDirection", sunDirection);
            }
            
            // Update lighting based on time of day
            UpdateLighting(currentTimeInHours);
        }
        
        private void UpdateLighting(float timeInHours)
        {
            if (directionalLight == null) return;
            
            // Calculate lighting based on sun's actual position/angle
            // Colors should change based on how close the sun is to the horizon
            
            // Get the current sun position to determine angle
            Vector3 sunPosition = directionalLight.transform.position;
            float sunHeight = sunPosition.y;
            float sunDistance = sunPosition.magnitude;
            
            // Calculate sun angle from horizon (0 = horizon, 90 = overhead)
            float sunAngleFromHorizon = Mathf.Asin(sunHeight / sunDistance) * Mathf.Rad2Deg;
            
            Color lightColor;
            float lightIntensity;
            Color ambientColor;
            Color skyColor;
            Color horizonColor;
            
            if (sunAngleFromHorizon > 45f)
            {
                // Sun high in sky - full daylight
                lightColor = dayColor;
                lightIntensity = dayIntensity;
                ambientColor = dayColor * 0.3f;
                skyColor = daySkyColor;
                horizonColor = dayHorizonColor;
            }
            else if (sunAngleFromHorizon > 15f)
            {
                // Sun moderately high - transitioning to sunset/sunrise
                float transitionProgress = (sunAngleFromHorizon - 15f) / (45f - 15f);
                lightColor = Color.Lerp(sunsetColor, dayColor, transitionProgress);
                lightIntensity = Mathf.Lerp(sunsetIntensity, dayIntensity, transitionProgress);
                ambientColor = Color.Lerp(sunsetColor * 0.15f, dayColor * 0.3f, transitionProgress);
                skyColor = Color.Lerp(sunsetSkyColor, daySkyColor, transitionProgress);
                horizonColor = Color.Lerp(sunsetHorizonColor, dayHorizonColor, transitionProgress);
            }
            else if (sunAngleFromHorizon > 0f)
            {
                // Sun near horizon - warm sunset/sunrise colors
                float horizonProgress = sunAngleFromHorizon / 15f;
                lightColor = Color.Lerp(sunriseColor, sunsetColor, horizonProgress);
                lightIntensity = Mathf.Lerp(sunriseIntensity, sunsetIntensity, horizonProgress);
                ambientColor = Color.Lerp(sunriseColor * 0.2f, sunsetColor * 0.1f, horizonProgress);
                skyColor = Color.Lerp(sunriseSkyColor, sunsetSkyColor, horizonProgress);
                horizonColor = Color.Lerp(sunriseHorizonColor, sunsetHorizonColor, horizonProgress);
            }
            else if (sunAngleFromHorizon > -10f)
            {
                // Sun just below horizon - early night
                float nightProgress = (sunAngleFromHorizon + 10f) / 10f;
                lightColor = Color.Lerp(nightColor, sunriseColor, nightProgress);
                lightIntensity = Mathf.Lerp(nightIntensity, sunriseIntensity, nightProgress);
                ambientColor = Color.Lerp(nightColor * 0.05f, sunriseColor * 0.2f, nightProgress);
                skyColor = Color.Lerp(nightSkyColor, sunriseSkyColor, nightProgress);
                horizonColor = Color.Lerp(nightHorizonColor, sunriseHorizonColor, nightProgress);
            }
            else
            {
                // Sun well below horizon - deep night
                float deepNightProgress = Mathf.Clamp01((sunAngleFromHorizon + 30f) / -20f);
                lightColor = Color.Lerp(deepNightColor, nightColor, deepNightProgress);
                lightIntensity = Mathf.Lerp(deepNightIntensity, nightIntensity, deepNightProgress);
                ambientColor = Color.Lerp(deepNightColor * 0.02f, nightColor * 0.05f, deepNightProgress);
                skyColor = Color.Lerp(deepNightSkyColor, nightSkyColor, deepNightProgress);
                horizonColor = Color.Lerp(deepNightHorizonColor, nightHorizonColor, deepNightProgress);
            }
            
            // Apply the lighting
            directionalLight.color = lightColor;
            directionalLight.intensity = lightIntensity;
            RenderSettings.ambientLight = ambientColor;
            
            // Update global lighting settings for night
            UpdateGlobalLightingSettings(sunAngleFromHorizon);
            
            // Update skybox colors
            UpdateSkyboxColors(skyColor, horizonColor);
        }
        
        private void UpdateGlobalLightingSettings(float sunAngleFromHorizon)
        {
            // Make night much darker by adjusting global lighting settings
            if (sunAngleFromHorizon < 0f)
            {
                // Night time - reduce global lighting
                float nightDarkness = Mathf.Clamp01(-sunAngleFromHorizon / 30f); // 0 = just below horizon, 1 = deep night
                
                // Reduce ambient intensity
                RenderSettings.ambientIntensity = Mathf.Lerp(0.3f, 0.1f, nightDarkness);
                
                // Reduce reflection intensity
                RenderSettings.reflectionIntensity = Mathf.Lerp(0.5f, 0.1f, nightDarkness);
                
                // Reduce fog density if fog is enabled
                if (RenderSettings.fog)
                {
                    RenderSettings.fogDensity = Mathf.Lerp(0.01f, 0.05f, nightDarkness);
                }
            }
            else
            {
                // Day time - normal lighting
                RenderSettings.ambientIntensity = 1f;
                RenderSettings.reflectionIntensity = 1f;
                if (RenderSettings.fog)
                {
                    RenderSettings.fogDensity = 0.01f;
                }
            }
        }
        
        private void UpdateSkyboxColors(Color skyColor, Color horizonColor)
        {
            if (skyboxMaterial == null) return;
            
            // Debug: Log available properties
            if (Time.frameCount % 300 == 0) // Log every 5 seconds at 60fps
            {
                Debug.Log($"[SimpleSunManager] Skybox material: {skyboxMaterial.name}");
                Debug.Log($"[SimpleSunManager] Skybox shader: {skyboxMaterial.shader.name}");
                Debug.Log($"[SimpleSunManager] Testing skybox color changes...");
            }
            
            // Procedural Skybox uses different property names
            // Try the standard Procedural Skybox properties
            if (skyboxMaterial.HasProperty("_SkyTint"))
            {
                skyboxMaterial.SetColor("_SkyTint", skyColor);
            }
            
            if (skyboxMaterial.HasProperty("_GroundColor"))
            {
                skyboxMaterial.SetColor("_GroundColor", horizonColor);
            }
            
            if (skyboxMaterial.HasProperty("_SunSize"))
            {
                // Adjust sun size based on time of day
                float sunSize = 0.04f; // Default size
                skyboxMaterial.SetFloat("_SunSize", sunSize);
            }
            
            if (skyboxMaterial.HasProperty("_AtmosphereThickness"))
            {
                // Adjust atmosphere thickness for different times
                float atmosphereThickness = 1f; // Default thickness
                skyboxMaterial.SetFloat("_AtmosphereThickness", atmosphereThickness);
            }
            
            // Also try setting the main color property
            if (skyboxMaterial.HasProperty("_Color"))
            {
                skyboxMaterial.SetColor("_Color", skyColor);
            }
            
            // Force the material to update
            skyboxMaterial.SetFloat("_Exposure", 1f);
        }
        
        // Public methods for external access
        public bool IsDay()
        {
            if (!isInitialized) return true;
            
            float totalGameHours = elapsedTime / realTimePerGameHour;
            float currentTimeInHours = totalGameHours + startHour + (startMinute / 60f);
            
            return currentTimeInHours >= sunriseHour && currentTimeInHours < sunsetHour;
        }
        
        public bool IsNight()
        {
            return !IsDay();
        }
        
        public float GetDayProgress()
        {
            if (!isInitialized) return 0.5f;
            
            float totalGameHours = elapsedTime / realTimePerGameHour;
            float currentTimeInHours = totalGameHours + startHour + (startMinute / 60f);
            
            // Calculate progress through the day (0 = sunrise, 1 = sunset)
            if (currentTimeInHours < sunriseHour)
            {
                return 0f; // Before sunrise
            }
            else if (currentTimeInHours >= sunsetHour)
            {
                return 1f; // After sunset
            }
            else
            {
                // During day - calculate progress from sunrise to sunset
                float dayDuration = sunsetHour - sunriseHour;
                float timeSinceSunrise = currentTimeInHours - sunriseHour;
                return timeSinceSunrise / dayDuration;
            }
        }
        
        // Debug methods
        [ContextMenu("Show Current Status")]
        public void ShowCurrentStatus()
        {
            Debug.Log("=== SimpleSunManager Status ===");
            Debug.Log($"[SimpleSunManager] Initialized: {isInitialized}");
            Debug.Log($"[SimpleSunManager] Elapsed time: {elapsedTime:F1}s");
            
            if (isInitialized)
            {
                float totalGameHours = elapsedTime / realTimePerGameHour;
                float currentTimeInHours = totalGameHours + startHour + (startMinute / 60f);
                int hour = Mathf.FloorToInt(currentTimeInHours);
                int minute = Mathf.FloorToInt((currentTimeInHours - hour) * 60f);
                
                Debug.Log($"[SimpleSunManager] Current game time: {hour:D2}:{minute:D2}");
                Debug.Log($"[SimpleSunManager] Is day: {IsDay()}, Is night: {IsNight()}");
                Debug.Log($"[SimpleSunManager] Day progress: {GetDayProgress():F2}");
                
                if (directionalLight != null)
                {
                    Debug.Log($"[SimpleSunManager] Sun rotation: {directionalLight.transform.rotation.eulerAngles}");
                }
            }
            
            Debug.Log("=== End Status ===");
        }
        
        [ContextMenu("Test Day")]
        public void TestDay()
        {
            if (directionalLight != null)
            {
                directionalLight.color = dayColor;
                directionalLight.intensity = dayIntensity;
                RenderSettings.ambientLight = dayColor * 0.3f;
            }
            UpdateSkyboxColors(daySkyColor, dayHorizonColor);
        }
        
        [ContextMenu("Test Night")]
        public void TestNight()
        {
            if (directionalLight != null)
            {
                directionalLight.color = nightColor;
                directionalLight.intensity = nightIntensity;
                RenderSettings.ambientLight = nightColor * 0.1f;
            }
            UpdateSkyboxColors(nightSkyColor, nightHorizonColor);
        }
        
        [ContextMenu("Test Sunrise")]
        public void TestSunrise()
        {
            if (directionalLight != null)
            {
                directionalLight.color = sunriseColor;
                directionalLight.intensity = sunriseIntensity;
                RenderSettings.ambientLight = sunriseColor * 0.3f;
            }
            UpdateSkyboxColors(sunriseSkyColor, sunriseHorizonColor);
        }
        
        [ContextMenu("Test Sunset")]
        public void TestSunset()
        {
            if (directionalLight != null)
            {
                directionalLight.color = sunsetColor;
                directionalLight.intensity = sunsetIntensity;
                RenderSettings.ambientLight = sunsetColor * 0.2f;
            }
            UpdateSkyboxColors(sunsetSkyColor, sunsetHorizonColor);
        }
        
        [ContextMenu("Test Deep Night")]
        public void TestDeepNight()
        {
            if (directionalLight != null)
            {
                directionalLight.color = deepNightColor;
                directionalLight.intensity = deepNightIntensity;
                RenderSettings.ambientLight = deepNightColor * 0.05f;
            }
            UpdateSkyboxColors(deepNightSkyColor, deepNightHorizonColor);
        }
        
        [ContextMenu("Debug Skybox Properties")]
        public void DebugSkyboxProperties()
        {
            if (skyboxMaterial == null)
            {
                Debug.LogError("[SimpleSunManager] No skybox material assigned!");
                return;
            }
            
            Debug.Log("=== Skybox Debug Info ===");
            Debug.Log($"Material: {skyboxMaterial.name}");
            Debug.Log($"Shader: {skyboxMaterial.shader.name}");
            
            // Test common Procedural Skybox properties
            Debug.Log("Testing common Procedural Skybox properties:");
            
            // Try to set a bright red color to test if it works
            if (skyboxMaterial.HasProperty("_SkyTint"))
            {
                skyboxMaterial.SetColor("_SkyTint", Color.red);
                Debug.Log("Set _SkyTint to red - check if skybox changes!");
            }
            else
            {
                Debug.Log("_SkyTint property not found");
            }
            
            if (skyboxMaterial.HasProperty("_GroundColor"))
            {
                skyboxMaterial.SetColor("_GroundColor", Color.blue);
                Debug.Log("Set _GroundColor to blue - check if horizon changes!");
            }
            else
            {
                Debug.Log("_GroundColor property not found");
            }
            
            if (skyboxMaterial.HasProperty("_SunColor"))
            {
                skyboxMaterial.SetColor("_SunColor", Color.yellow);
                Debug.Log("Set _SunColor to yellow - check if sun changes!");
            }
            else
            {
                Debug.Log("_SunColor property not found");
            }
            
            Debug.Log("=== End Debug Info ===");
        }
        
        [ContextMenu("Test Time Progression")]
        public void TestTimeProgression()
        {
            Debug.Log("=== Time Progression Test ===");
            Debug.Log($"Current elapsed time: {elapsedTime:F1}s");
            Debug.Log($"Real time per game hour: {realTimePerGameHour}s");
            Debug.Log($"Start time: {startHour:D2}:{startMinute:D2}");
            Debug.Log($"Sunrise: {sunriseHour}, Sunset: {sunsetHour}");
            
            // Test different times including 24-hour cycle
            float[] testTimes = { 0f, 1f, 2f, 3f, 4f, 5f, 6f, 8f, 10f, 12f, 14f, 16f, 18f, 20f, 22f, 24f, 25f, 26f, 27f, 28f, 29f, 30f };
            
            foreach (float testHour in testTimes)
            {
                float testTimeInHours = testHour % 24f; // Wrap around 24 hours
                
                bool testIsDay = testTimeInHours >= sunriseHour && testTimeInHours < sunsetHour;
                float testDayProgress = 0f;
                
                if (testTimeInHours < sunriseHour)
                {
                    testDayProgress = 0f;
                }
                else if (testTimeInHours >= sunsetHour)
                {
                    testDayProgress = 1f;
                }
                else
                {
                    float dayDuration = sunsetHour - sunriseHour;
                    float timeSinceSunrise = testTimeInHours - sunriseHour;
                    testDayProgress = timeSinceSunrise / dayDuration;
                }
                
                Debug.Log($"Time {testHour:D2}:00 (wrapped: {testTimeInHours:F1}) - Is day: {testIsDay}, Progress: {testDayProgress:F2}");
            }
            
            Debug.Log("=== End Time Test ===");
        }
        
        [ContextMenu("Advance 1 Hour")]
        public void AdvanceOneHour()
        {
            elapsedTime += realTimePerGameHour;
            Debug.Log($"[SimpleSunManager] Advanced 1 hour. New elapsed time: {elapsedTime:F1}s");
        }
        
        [ContextMenu("Advance 6 Hours")]
        public void AdvanceSixHours()
        {
            elapsedTime += realTimePerGameHour * 6f;
            Debug.Log($"[SimpleSunManager] Advanced 6 hours. New elapsed time: {elapsedTime:F1}s");
        }
    }
} 