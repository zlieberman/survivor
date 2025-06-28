using UnityEngine;
using StarterAssets;
using Survivor.Characters;
using System.Collections.Generic;

namespace Survivor.Characters.Core
{
    [System.Serializable]
    public class StatModifier
    {
        [Header("Stat Configuration")]
        [Tooltip("The stat to monitor (0-100)")]
        public string statName;
        
        [Tooltip("The ThirdPersonController field to modify")]
        public string controllerField;
        
        [Header("Value Calculation")]
        [Tooltip("Minimum output value")]
        public float minValue = 0f;
        
        [Tooltip("Maximum output value")]
        public float maxValue = 10f;
        
        [Tooltip("Multiplier applied to the calculated value")]
        public float multiplier = 1f;
        
        [Tooltip("Use normal distribution curve (true) or linear mapping (false)")]
        public bool useNormalDistribution = true;
        
        [Tooltip("Center point for normal distribution (0-1)")]
        [Range(0f, 1f)]
        public float distributionCenter = 0.5f;
        
        [Tooltip("Spread of normal distribution")]
        public float distributionSpread = 2f;
        
        [Tooltip("Offset added to final value")]
        public float offset = 0f;
    }

    public class CharacterStatModifier : MonoBehaviour
    {
        [Header("Stat Modifiers")]
        [SerializeField] private List<StatModifier> statModifiers = new List<StatModifier>();
        
        [Header("Default Modifiers")]
        [SerializeField] private bool useDefaultModifiers = true;

        private ThirdPersonController controller;
        private Character character;
        private Dictionary<string, float> lastStatValues = new Dictionary<string, float>();
        
        // Store the calculated values persistently to prevent them from being reset
        private Dictionary<string, float> calculatedValues = new Dictionary<string, float>();
        
        // Track if we've initialized the values to prevent overwriting during startup
        private bool valuesInitialized = false;

        private void Start()
        {
            // Get the ThirdPersonController component
            controller = GetComponent<ThirdPersonController>();
            if (controller == null)
            {
                Debug.LogError("[CharacterStatModifier] ThirdPersonController not found!");
                return;
            }

            // Get the Character component
            character = GetComponent<Character>();
            if (character == null)
            {
                Debug.LogError("[CharacterStatModifier] Character component not found!");
                return;
            }

            Debug.Log($"[CharacterStatModifier] Initialized for {character.CharacterName} - Speed: {character.Stats.speed}, Strength: {character.Stats.strength}");

            // Initialize last stat values
            InitializeLastStatValues();

            // Update modifiers immediately
            UpdateAllModifiers();
            
            // Mark values as initialized
            valuesInitialized = true;
        }

        private void Update()
        {
            // Only check for changes if values have been initialized
            if (!valuesInitialized || character == null || character.Stats == null) return;

            bool hasChanges = false;
            foreach (var modifier in statModifiers)
            {
                float currentStatValue = GetStatValue(modifier.statName);
                string statKey = modifier.statName.ToLower();
                
                if (!lastStatValues.ContainsKey(statKey) || 
                    Mathf.Abs(lastStatValues[statKey] - currentStatValue) > 0.01f) // Use small threshold for float comparison
                {
                    lastStatValues[statKey] = currentStatValue;
                    hasChanges = true;
                    Debug.Log($"[CharacterStatModifier] Stat '{modifier.statName}' changed to {currentStatValue} for {character.CharacterName}");
                }
            }
            
            if (hasChanges)
            {
                Debug.Log($"[CharacterStatModifier] Detected changes, updating all modifiers for {character.CharacterName}");
                UpdateAllModifiers();
            }
            
            // Periodically check if other systems have reset our values (every 30 frames = ~0.5 seconds at 60fps)
            if (Time.frameCount % 30 == 0)
            {
                CheckAndReapplyValues();
            }
        }

        private void InitializeLastStatValues()
        {
            if (character?.Stats == null) return;
            
            foreach (var modifier in statModifiers)
            {
                lastStatValues[modifier.statName] = GetStatValue(modifier.statName);
            }
        }

        private float GetStatValue(string statName)
        {
            if (character?.Stats == null) return 0f;

            switch (statName.ToLower())
            {
                case "speed": return character.Stats.speed;
                case "strength": return character.Stats.strength;
                case "stamina": return character.Stats.stamina;
                case "agility": return character.Stats.agility;
                case "swimming": return character.Stats.swimming;
                default: return 0f;
            }
        }

        private void UpdateAllModifiers()
        {
            if (controller == null || character?.Stats == null) return;

            foreach (var modifier in statModifiers)
            {
                UpdateModifier(modifier);
            }
        }

        private void UpdateModifier(StatModifier modifier)
        {
            // 1) Read the raw stat (0–100) and normalize to [0,1]
            float statValue      = GetStatValue(modifier.statName);
            float normalizedStat = Mathf.Clamp01(statValue / 100f);
            Debug.Log($"[CharacterStatModifier] '{modifier.statName}' = {statValue:F1}, normalized = {normalizedStat:F3}");

            // 2) Linear mapping: t==0→minValue, t==0.5→midpoint, t==1→maxValue
            float baseValue = Mathf.Lerp(
                modifier.minValue, 
                modifier.maxValue, 
                normalizedStat
            );
            Debug.Log($"[CharacterStatModifier] Linear Lerp({modifier.minValue}→{modifier.maxValue}, t={normalizedStat:F3}) = {baseValue:F3}");

            // 3) Apply multiplier and offset
            float mult       = modifier.multiplier != 0f ? modifier.multiplier : 1f;
            float off        = modifier.offset;
            float finalValue = baseValue * mult + off;
            Debug.Log($"[CharacterStatModifier] After ×{mult} +{off} = {finalValue:F3}");

            // 4) Store the calculated value persistently
            string fieldKey = modifier.controllerField.ToLower();
            calculatedValues[fieldKey] = finalValue;

            // 5) Push the value into the controller
            ApplyToController(modifier.controllerField, finalValue);
            Debug.Log($"[CharacterStatModifier] → {modifier.controllerField} = {finalValue:F3}");
        }

        private void ApplyToController(string fieldName, float value)
        {
            if (controller == null) return;

            // Normalize field name for comparison
            string normalizedFieldName = fieldName.Replace(" ", "").ToLower();
            
            Debug.Log($"[CharacterStatModifier] Applying {fieldName} = {value:F2} to {character?.CharacterName ?? "Unknown"}");
            
            // Apply to the appropriate field
            switch (normalizedFieldName)
            {
                case "movespeed":
                    controller.MoveSpeed = value;
                    Debug.Log($"[CharacterStatModifier] ✓ Set MoveSpeed to {value:F2} (was {controller.MoveSpeed:F2})");
                    break;
                case "sprintspeed":
                    controller.SprintSpeed = value;
                    Debug.Log($"[CharacterStatModifier] ✓ Set SprintSpeed to {value:F2} (was {controller.SprintSpeed:F2})");
                    break;
                case "swimspeed":
                    controller.SwimSpeed = value;
                    Debug.Log($"[CharacterStatModifier] ✓ Set SwimSpeed to {value:F2}");
                    break;
                case "crawlspeed":
                    controller.CrawlSpeed = value;
                    Debug.Log($"[CharacterStatModifier] ✓ Set CrawlSpeed to {value:F2}");
                    break;
                case "maxcrawlspeed":
                    controller.MaxCrawlSpeed = value;
                    Debug.Log($"[CharacterStatModifier] ✓ Set MaxCrawlSpeed to {value:F2}");
                    break;
                case "jumpheight":
                    controller.JumpHeight = value;
                    Debug.Log($"[CharacterStatModifier] ✓ Set JumpHeight to {value:F2}");
                    break;
                default:
                    Debug.LogWarning($"[CharacterStatModifier] Unknown field: {fieldName}");
                    break;
            }
        }

        // Public method to refresh all modifiers
        public void RefreshAllModifiers()
        {
            UpdateAllModifiers();
        }

        // Public method to get the calculated value for a field
        public float GetCalculatedValue(string fieldName)
        {
            string fieldKey = fieldName.ToLower();
            return calculatedValues.ContainsKey(fieldKey) ? calculatedValues[fieldKey] : 0f;
        }

        // Public method to force reapply all calculated values (useful when other systems might have reset them)
        public void ReapplyAllValues()
        {
            if (!valuesInitialized) return;
            
            Debug.Log($"[CharacterStatModifier] Reapplying all calculated values for {character?.CharacterName ?? "Unknown"}");
            
            foreach (var kvp in calculatedValues)
            {
                string fieldName = kvp.Key;
                float value = kvp.Value;
                
                // Convert back to proper field name format
                string properFieldName = "";
                switch (fieldName)
                {
                    case "movespeed": properFieldName = "MoveSpeed"; break;
                    case "sprintspeed": properFieldName = "SprintSpeed"; break;
                    case "swimspeed": properFieldName = "SwimSpeed"; break;
                    case "crawlspeed": properFieldName = "CrawlSpeed"; break;
                    case "maxcrawlspeed": properFieldName = "MaxCrawlSpeed"; break;
                    case "jumpheight": properFieldName = "JumpHeight"; break;
                    default: properFieldName = fieldName; break;
                }
                
                ApplyToController(properFieldName, value);
            }
        }

        // Method to check if controller values match our calculated values and reapply if needed
        public void CheckAndReapplyValues()
        {
            if (!valuesInitialized || controller == null) return;
            
            bool needsReapply = false;
            
            foreach (var kvp in calculatedValues)
            {
                string fieldName = kvp.Key;
                float expectedValue = kvp.Value;
                float actualValue = 0f;
                
                // Get the actual controller value
                switch (fieldName)
                {
                    case "movespeed": actualValue = controller.MoveSpeed; break;
                    case "sprintspeed": actualValue = controller.SprintSpeed; break;
                    case "swimspeed": actualValue = controller.SwimSpeed; break;
                    case "crawlspeed": actualValue = controller.CrawlSpeed; break;
                    case "maxcrawlspeed": actualValue = controller.MaxCrawlSpeed; break;
                    case "jumpheight": actualValue = controller.JumpHeight; break;
                }
                
                // Check if the values are significantly different
                if (Mathf.Abs(actualValue - expectedValue) > 0.01f)
                {
                    needsReapply = true;
                    Debug.Log($"[CharacterStatModifier] Value mismatch detected: {fieldName} expected {expectedValue:F2}, actual {actualValue:F2}");
                }
            }
            
            if (needsReapply)
            {
                Debug.Log($"[CharacterStatModifier] Reapplying values due to mismatch for {character?.CharacterName ?? "Unknown"}");
                ReapplyAllValues();
            }
        }

        // Test method to verify the system is working
        [ContextMenu("Test All Modifiers")]
        public void TestAllModifiers()
        {
            Debug.Log($"[CharacterStatModifier] Manual test triggered for {character?.CharacterName ?? "Unknown"}");
            UpdateAllModifiers();
        }

        // Context menu to manually reapply all values
        [ContextMenu("Reapply All Values")]
        public void ManualReapplyValues()
        {
            Debug.Log($"[CharacterStatModifier] Manual reapply triggered for {character?.CharacterName ?? "Unknown"}");
            ReapplyAllValues();
        }

        // Context menu to check and reapply values if needed
        [ContextMenu("Check and Reapply Values")]
        public void ManualCheckAndReapply()
        {
            Debug.Log($"[CharacterStatModifier] Manual check and reapply triggered for {character?.CharacterName ?? "Unknown"}");
            CheckAndReapplyValues();
        }

        // Debug method to show current values
        [ContextMenu("Show Current Values")]
        public void ShowCurrentValues()
        {
            if (character?.Stats == null)
            {
                Debug.Log("[CharacterStatModifier] Character or Stats is null!");
                return;
            }

            Debug.Log($"[CharacterStatModifier] Current stats for {character.CharacterName}:");
            Debug.Log($"  Speed: {character.Stats.speed}");
            Debug.Log($"  Strength: {character.Stats.strength}");
            Debug.Log($"  Stamina: {character.Stats.stamina}");
            Debug.Log($"  Agility: {character.Stats.agility}");
            Debug.Log($"  Swimming: {character.Stats.swimming}");

            if (controller != null)
            {
                Debug.Log($"[CharacterStatModifier] Current controller values:");
                Debug.Log($"  MoveSpeed: {controller.MoveSpeed}");
                Debug.Log($"  SprintSpeed: {controller.SprintSpeed}");
                Debug.Log($"  SwimSpeed: {controller.SwimSpeed}");
                Debug.Log($"  JumpHeight: {controller.JumpHeight}");
                Debug.Log($"  CrawlSpeed: {controller.CrawlSpeed}");
                Debug.Log($"  MaxCrawlSpeed: {controller.MaxCrawlSpeed}");
            }

            Debug.Log($"[CharacterStatModifier] Stored calculated values:");
            foreach (var kvp in calculatedValues)
            {
                Debug.Log($"  {kvp.Key}: {kvp.Value:F2}");
            }

            Debug.Log($"[CharacterStatModifier] Number of stat modifiers: {statModifiers.Count}");
            foreach (var modifier in statModifiers)
            {
                Debug.Log($"  Modifier: {modifier.statName} -> {modifier.controllerField} (Min: {modifier.minValue}, Max: {modifier.maxValue})");
            }
        }

        // Test method to show expected values for different speed stats
        [ContextMenu("Test Speed Values")]
        public void TestSpeedValues()
        {
            Debug.Log("[CharacterStatModifier] Testing speed values for different stats:");
            
            foreach (var modifier in statModifiers)
            {
                if (modifier.statName.ToLower() == "speed")
                {
                    Debug.Log($"\nTesting {modifier.controllerField} (Min: {modifier.minValue}, Max: {modifier.maxValue}):");
                    
                    // Test different speed values
                    float[] testSpeeds = { 0f, 25f, 50f, 75f, 100f };
                    foreach (float testSpeed in testSpeeds)
                    {
                        float normalizedStat = testSpeed / 100f;
                        float result = Mathf.Lerp(modifier.minValue, modifier.maxValue, normalizedStat);
                        result = result * modifier.multiplier + modifier.offset;
                        
                        Debug.Log($"  Speed {testSpeed}: Normalized={normalizedStat:F2}, Result={result:F2}");
                    }
                }
            }
        }

        // Static method to automatically add this component to any character with ThirdPersonController
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoSetupCharacterStatModifier()
        {
            // Find all Character components in the scene
            Character[] characters = FindObjectsOfType<Character>();
            
            foreach (Character charComponent in characters)
            {
                // Check if this character already has a CharacterStatModifier
                if (charComponent.GetComponent<CharacterStatModifier>() == null)
                {
                    // Check if it has a ThirdPersonController
                    if (charComponent.GetComponent<ThirdPersonController>() != null)
                    {
                        // Add the CharacterStatModifier component
                        charComponent.gameObject.AddComponent<CharacterStatModifier>();
                        Debug.Log($"[CharacterStatModifier] Auto-added CharacterStatModifier to {charComponent.gameObject.name}");
                    }
                }
            }
        }
    }
} 