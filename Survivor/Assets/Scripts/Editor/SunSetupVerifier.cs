using UnityEngine;
using Survivor.Core;
using Survivor.Shared;

namespace Survivor.Editor
{
    public class SunSetupVerifier : MonoBehaviour
    {
        [Header("Verification Settings")]
        [SerializeField] private bool runOnStart = true;
        [SerializeField] private bool showDetailedInfo = true;
        
        private void Start()
        {
            if (runOnStart)
            {
                VerifySunSetup();
            }
        }
        
        [ContextMenu("Verify Sun Setup")]
        public void VerifySunSetup()
        {
            Debug.Log("=== SUN SETUP VERIFICATION ===");
            
            // Check for SunManager scripts
            var sunManagers = FindObjectsOfType<SunManager>();
            var enhancedSunManagers = FindObjectsOfType<EnhancedSunManager>();
            
            Debug.Log($"Found {sunManagers.Length} SunManager(s) and {enhancedSunManagers.Length} EnhancedSunManager(s)");
            
            // Check each SunManager
            foreach (var sunManager in sunManagers)
            {
                VerifySunManager(sunManager, "SunManager");
            }
            
            // Check each EnhancedSunManager
            foreach (var enhancedSunManager in enhancedSunManagers)
            {
                VerifySunManager(enhancedSunManager, "EnhancedSunManager");
            }
            
            // Check TimeManager
            VerifyTimeManager();
            
            // Check for lights
            VerifyLights();
            
            // Check skybox
            VerifySkybox();
            
            Debug.Log("=== VERIFICATION COMPLETE ===");
        }
        
        private void VerifySunManager(MonoBehaviour sunManager, string managerType)
        {
            Debug.Log($"--- {managerType} Verification ---");
            
            if (sunManager == null)
            {
                Debug.LogError($"{managerType} is null!");
                return;
            }
            
            Debug.Log($"{managerType} GameObject: {sunManager.name}");
            Debug.Log($"{managerType} Enabled: {sunManager.enabled}");
            Debug.Log($"{managerType} Active: {sunManager.gameObject.activeInHierarchy}");
            
            // Check if it's the correct type
            if (sunManager is SunManager regularSunManager)
            {
                Debug.Log($"{managerType} is correct type: SunManager");
            }
            else if (sunManager is EnhancedSunManager enhancedSunManager)
            {
                Debug.Log($"{managerType} is correct type: EnhancedSunManager");
            }
            else
            {
                Debug.LogError($"{managerType} is not the expected type! Actual type: {sunManager.GetType().Name}");
            }
        }
        
        private void VerifyTimeManager()
        {
            Debug.Log("--- TimeManager Verification ---");
            
            if (GameTimeService.HasTimeProvider)
            {
                Debug.Log("✅ TimeProvider is registered");
                var (hour, minute, day) = GameTimeService.GetCurrentGameTime();
                Debug.Log($"Current game time: {hour:D2}:{minute:D2} (Day {day})");
                
                // Check if it's a TimeManager to get starting time
                if (GameTimeService.TimeProvider is TimeManager timeManager)
                {
                    Debug.Log($"Game starts at: {timeManager.StartHour:D2}:{timeManager.StartMinute:D2}");
                    Debug.Log($"Real time per game hour: {timeManager.RealTimePerGameHour}s");
                }
            }
            else
            {
                Debug.LogError("❌ No TimeProvider registered!");
            }
            
            // Look for TimeManager in scene
            var timeManagers = FindObjectsOfType<MonoBehaviour>();
            bool foundTimeManager = false;
            foreach (var mb in timeManagers)
            {
                if (mb.GetType().Name == "TimeManager")
                {
                    Debug.Log($"Found TimeManager: {mb.name} (Enabled: {mb.enabled})");
                    foundTimeManager = true;
                }
            }
            
            if (!foundTimeManager)
            {
                Debug.LogWarning("No TimeManager found in scene!");
            }
        }
        
        private void VerifyLights()
        {
            Debug.Log("--- Lights Verification ---");
            
            var lights = FindObjectsOfType<Light>();
            Debug.Log($"Found {lights.Length} lights in scene:");
            
            foreach (var light in lights)
            {
                Debug.Log($"- {light.name}: Type={light.type}, Intensity={light.intensity}, Enabled={light.enabled}");
                
                if (light.type == LightType.Directional)
                {
                    Debug.Log($"  Directional light found: {light.name}");
                }
            }
            
            if (lights.Length == 0)
            {
                Debug.LogError("No lights found in scene!");
            }
        }
        
        private void VerifySkybox()
        {
            Debug.Log("--- Skybox Verification ---");
            
            var skybox = RenderSettings.skybox;
            if (skybox != null)
            {
                Debug.Log($"Skybox: {skybox.name}");
                Debug.Log($"Shader: {skybox.shader.name}");
            }
            else
            {
                Debug.LogWarning("No skybox assigned to RenderSettings!");
            }
        }
        
        [ContextMenu("Force Sun Update")]
        public void ForceSunUpdate()
        {
            Debug.Log("Forcing sun update...");
            
            var sunManagers = FindObjectsOfType<SunManager>();
            var enhancedSunManagers = FindObjectsOfType<EnhancedSunManager>();
            
            foreach (var sunManager in sunManagers)
            {
                if (sunManager.enabled)
                {
                    sunManager.ForceUpdate();
                }
            }
            
            foreach (var enhancedSunManager in enhancedSunManagers)
            {
                if (enhancedSunManager.enabled)
                {
                    enhancedSunManager.ForceUpdate();
                }
            }
        }
    }
} 