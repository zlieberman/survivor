using UnityEngine;
using Survivor.Core;
using Survivor.Shared;

namespace Survivor.Editor
{
    public class SunSystemTest : MonoBehaviour
    {
        [Header("Test Controls")]
        [SerializeField] private KeyCode testDayKey = KeyCode.D;
        [SerializeField] private KeyCode testNightKey = KeyCode.N;
        [SerializeField] private KeyCode testSunriseKey = KeyCode.R;
        [SerializeField] private KeyCode testSunsetKey = KeyCode.S;
        [SerializeField] private KeyCode toggleWeatherKey = KeyCode.W;
        [SerializeField] private KeyCode showStatusKey = KeyCode.I;
        [SerializeField] private KeyCode forceUpdateKey = KeyCode.U;
        [SerializeField] private KeyCode testTimeCalculationKey = KeyCode.T;
        [SerializeField] private KeyCode forceDayLightingKey = KeyCode.L;
        
        [Header("Weather Test")]
        [SerializeField] private float testCloudCover = 0.5f;
        [SerializeField] private float testFogDensity = 0.2f;
        
        private SunManager sunManager;
        private EnhancedSunManager enhancedSunManager;
        private bool weatherEnabled = false;
        
        private void Start()
        {
            FindSunManagers();
            Debug.Log("[SunSystemTest] Test controls:");
            Debug.Log("[SunSystemTest] D - Test day lighting");
            Debug.Log("[SunSystemTest] N - Test night lighting");
            Debug.Log("[SunSystemTest] R - Test sunrise");
            Debug.Log("[SunSystemTest] S - Test sunset");
            Debug.Log("[SunSystemTest] W - Toggle weather effects");
            Debug.Log("[SunSystemTest] I - Show status");
            Debug.Log("[SunSystemTest] U - Force update sun position");
            Debug.Log("[SunSystemTest] T - Test time calculation");
            Debug.Log("[SunSystemTest] L - Force day lighting");
        }
        
        private void FindSunManagers()
        {
            sunManager = FindObjectOfType<SunManager>();
            enhancedSunManager = FindObjectOfType<EnhancedSunManager>();
            
            if (sunManager != null)
            {
                Debug.Log("[SunSystemTest] Found SunManager");
                // Check if the script is actually working
                if (sunManager.enabled)
                {
                    Debug.Log("[SunSystemTest] SunManager is enabled and should be running");
                }
                else
                {
                    Debug.LogWarning("[SunSystemTest] SunManager is disabled!");
                }
            }
            else
            {
                Debug.LogWarning("[SunSystemTest] SunManager not found - check if script is attached to GameObject");
                
                // Look for GameObjects that might have the script but it's not working
                var allObjects = FindObjectsOfType<GameObject>();
                foreach (var obj in allObjects)
                {
                    if (obj.name.Contains("Sun") || obj.name.Contains("Light"))
                    {
                        var components = obj.GetComponents<MonoBehaviour>();
                        Debug.Log($"[SunSystemTest] Found GameObject '{obj.name}' with {components.Length} MonoBehaviour components");
                        foreach (var comp in components)
                        {
                            Debug.Log($"[SunSystemTest] - Component: {comp.GetType().Name} (Enabled: {comp.enabled})");
                        }
                    }
                }
            }
            
            if (enhancedSunManager != null)
            {
                Debug.Log("[SunSystemTest] Found EnhancedSunManager");
            }
            else
            {
                Debug.LogWarning("[SunSystemTest] EnhancedSunManager not found");
            }
        }
        
        private void Update()
        {
            // Test day lighting
            if (Input.GetKeyDown(testDayKey))
            {
                Debug.Log("[SunSystemTest] Testing day lighting...");
                if (sunManager != null)
                {
                    sunManager.TestDayLighting();
                }
                if (enhancedSunManager != null)
                {
                    enhancedSunManager.TestDayLighting();
                }
            }
            
            // Test night lighting
            if (Input.GetKeyDown(testNightKey))
            {
                Debug.Log("[SunSystemTest] Testing night lighting...");
                if (sunManager != null)
                {
                    sunManager.TestNightLighting();
                }
                if (enhancedSunManager != null)
                {
                    enhancedSunManager.TestNightLighting();
                }
            }
            
            // Test sunrise (5 AM)
            if (Input.GetKeyDown(testSunriseKey))
            {
                Debug.Log("[SunSystemTest] Testing sunrise (5 AM)...");
                TestTimeOfDay(5, 0);
            }
            
            // Test sunset (9 PM)
            if (Input.GetKeyDown(testSunsetKey))
            {
                Debug.Log("[SunSystemTest] Testing sunset (9 PM)...");
                TestTimeOfDay(21, 0);
            }
            
            // Toggle weather effects
            if (Input.GetKeyDown(toggleWeatherKey))
            {
                weatherEnabled = !weatherEnabled;
                Debug.Log($"[SunSystemTest] Weather effects: {(weatherEnabled ? "ON" : "OFF")}");
                
                if (enhancedSunManager != null)
                {
                    if (weatherEnabled)
                    {
                        enhancedSunManager.SetWeather(testCloudCover, testFogDensity);
                    }
                    else
                    {
                        enhancedSunManager.SetWeather(0f, 0f);
                    }
                }
            }
            
            // Show status
            if (Input.GetKeyDown(showStatusKey))
            {
                ShowStatus();
            }
            
            // Force update sun position
            if (Input.GetKeyDown(forceUpdateKey))
            {
                Debug.Log("[SunSystemTest] Force updating sun position...");
                if (sunManager != null)
                {
                    sunManager.ForceUpdate();
                }
                if (enhancedSunManager != null)
                {
                    enhancedSunManager.ForceUpdate();
                }
            }
            
            // Test time calculation
            if (Input.GetKeyDown(testTimeCalculationKey))
            {
                Debug.Log("[SunSystemTest] Testing time calculation...");
                if (sunManager != null)
                {
                    sunManager.TestTimeCalculation();
                }
                if (enhancedSunManager != null)
                {
                    enhancedSunManager.TestTimeCalculation();
                }
            }
            
            // Force day lighting
            if (Input.GetKeyDown(forceDayLightingKey))
            {
                Debug.Log("[SunSystemTest] Force day lighting...");
                if (sunManager != null)
                {
                    sunManager.ForceDayLighting();
                }
                if (enhancedSunManager != null)
                {
                    enhancedSunManager.ForceDayLighting();
                }
            }
        }
        
        private void TestTimeOfDay(int hour, int minute)
        {
            // This is a simulation - in a real scenario you'd need to modify the TimeManager
            Debug.Log($"[SunSystemTest] Simulating time: {hour:D2}:{minute:D2}");
            
            if (sunManager != null)
            {
                bool isDay = sunManager.IsDay();
                float progress = sunManager.GetDayProgress();
                Debug.Log($"[SunSystemTest] SunManager - IsDay: {isDay}, Progress: {progress:F2}");
            }
            
            if (enhancedSunManager != null)
            {
                bool isDay = enhancedSunManager.IsDay();
                float progress = enhancedSunManager.GetDayProgress();
                Debug.Log($"[SunSystemTest] EnhancedSunManager - IsDay: {isDay}, Progress: {progress:F2}");
            }
        }
        
        private void ShowStatus()
        {
            Debug.Log("=== Sun System Status ===");
            
            // Time status
            if (GameTimeService.HasTimeProvider)
            {
                var (hour, minute, day) = GameTimeService.GetCurrentGameTime();
                Debug.Log($"[SunSystemTest] Current Game Time: {hour:D2}:{minute:D2} (Day {day})");
                Debug.Log($"[SunSystemTest] Formatted Time: {GameTimeService.GetFormattedTime()}");
            }
            else
            {
                Debug.LogWarning("[SunSystemTest] No time provider registered!");
            }
            
            // SunManager status
            if (sunManager != null)
            {
                Debug.Log($"[SunSystemTest] SunManager - IsDay: {sunManager.IsDay()}, Progress: {sunManager.GetDayProgress():F2}");
                var (sunriseHour, sunriseMin) = sunManager.GetSunriseTime();
                var (sunsetHour, sunsetMin) = sunManager.GetSunsetTime();
                Debug.Log($"[SunSystemTest] SunManager - Sunrise: {sunriseHour:D2}:{sunriseMin:D2}, Sunset: {sunsetHour:D2}:{sunsetMin:D2}");
            }
            
            // EnhancedSunManager status
            if (enhancedSunManager != null)
            {
                Debug.Log($"[SunSystemTest] EnhancedSunManager - IsDay: {enhancedSunManager.IsDay()}, Progress: {enhancedSunManager.GetDayProgress():F2}");
                var (sunriseHour, sunriseMin) = enhancedSunManager.GetSunriseTime();
                var (sunsetHour, sunsetMin) = enhancedSunManager.GetSunsetTime();
                Debug.Log($"[SunSystemTest] EnhancedSunManager - Sunrise: {sunriseHour:D2}:{sunriseMin:D2}, Sunset: {sunsetHour:D2}:{sunsetMin:D2}");
            }
            
            // Lighting status
            var lights = FindObjectsOfType<Light>();
            Debug.Log($"[SunSystemTest] Found {lights.Length} lights in scene:");
            foreach (var light in lights)
            {
                Debug.Log($"[SunSystemTest] - {light.name}: Type={light.type}, Intensity={light.intensity}, Color={light.color}");
            }
            
            // Skybox status
            var skybox = RenderSettings.skybox;
            if (skybox != null)
            {
                Debug.Log($"[SunSystemTest] Skybox: {skybox.name}, Shader: {skybox.shader.name}");
            }
            else
            {
                Debug.LogWarning("[SunSystemTest] No skybox assigned to RenderSettings!");
            }
            
            Debug.Log("=== End Status ===");
        }
        
        // Public methods for external testing
        [ContextMenu("Test Day")]
        public void TestDay()
        {
            if (sunManager != null) sunManager.TestDayLighting();
            if (enhancedSunManager != null) enhancedSunManager.TestDayLighting();
        }
        
        [ContextMenu("Test Night")]
        public void TestNight()
        {
            if (sunManager != null) sunManager.TestNightLighting();
            if (enhancedSunManager != null) enhancedSunManager.TestNightLighting();
        }
        
        [ContextMenu("Show Status")]
        public void ShowStatusPublic()
        {
            ShowStatus();
        }
    }
} 