using UnityEngine;
using Survivor.Characters;
using StarterAssets;

namespace Survivor.Environment
{
    /// <summary>
    /// Helper script to set up swimming mechanics in any scene.
    /// Add this to a GameObject in your scene to automatically set up the swimming system.
    /// </summary>
    public class SwimmingSystemSetup : MonoBehaviour
    {
        [Header("Setup Options")]
        public bool autoSetupOnStart = true;
        public bool addToPlayerIfMissing = true;
        public bool addToNPCsIfMissing = true;

        [Header("Component References")]
        public GameObject playerObject;
        public GameObject[] npcObjects;

        private void Start()
        {
            if (autoSetupOnStart)
            {
                SetupSwimmingSystem();
            }
        }

        public void SetupSwimmingSystem()
        {
            Debug.Log("[SwimmingSystemSetup] Setting up swimming system...");

            // Setup player
            if (addToPlayerIfMissing)
            {
                SetupCharacterSwimming(playerObject, "Player");
            }

            // Setup NPCs
            if (addToNPCsIfMissing && npcObjects != null)
            {
                foreach (var npc in npcObjects)
                {
                    if (npc != null)
                    {
                        SetupCharacterSwimming(npc, "NPC");
                    }
                }
            }

            // Auto-find characters if not assigned
            if (playerObject == null)
            {
                var player = FindObjectOfType<StarterAssets.ThirdPersonController>();
                if (player == null)
                {
                    player = FindObjectOfType<ThirdPersonController>();
                }
                if (player != null)
                {
                    SetupCharacterSwimming(player.gameObject, "Auto-found Player");
                }
            }

            VerifySetup();
        }

        private void SetupCharacterSwimming(GameObject character, string characterType)
        {
            if (character == null)
            {
                Debug.LogWarning($"[SwimmingSystemSetup] {characterType} object is null, skipping setup");
                return;
            }

            // Check if CharacterSwimming component already exists
            var existingSwimming = character.GetComponent<CharacterSwimming>();
            if (existingSwimming != null)
            {
                Debug.Log($"[SwimmingSystemSetup] {characterType} already has CharacterSwimming component");
                return;
            }

            // Add CharacterSwimming component
            var swimming = character.AddComponent<CharacterSwimming>();
            Debug.Log($"[SwimmingSystemSetup] Added CharacterSwimming to {characterType}: {character.name}");

            // Configure default settings based on character type
            if (characterType.Contains("Player"))
            {
                // Player settings - more responsive
                swimming.swimGravity = -2f;
                swimming.swimDepthThreshold = 0.9f;
                swimming.showDebug = true; // Show debug for player
            }
            else
            {
                // NPC settings - more conservative
                swimming.swimGravity = -1.5f;
                swimming.swimDepthThreshold = 1.1f;
                swimming.showDebug = false; // Hide debug for NPCs
            }

            // Note: Swimming speed is now controlled by CharacterStatModifier component
            // Make sure the character has a CharacterStatModifier with a swimming stat modifier configured
            var statModifier = character.GetComponent<Characters.Core.CharacterStatModifier>();
            if (statModifier == null)
            {
                Debug.LogWarning($"[SwimmingSystemSetup] {characterType} should have a CharacterStatModifier component to control swimming speed based on stats");
            }
        }

        private void VerifySetup()
        {
            bool setupComplete = true;

            // Check if player has required components
            if (playerObject != null)
            {
                var swimming = playerObject.GetComponent<CharacterSwimming>();
                var controller = playerObject.GetComponent<UnityEngine.CharacterController>();
                var statModifier = playerObject.GetComponent<Characters.Core.CharacterStatModifier>();
                
                // Check for both possible ThirdPersonController types
                var thirdPersonController = playerObject.GetComponent<StarterAssets.ThirdPersonController>();
                if (thirdPersonController == null)
                {
                    thirdPersonController = playerObject.GetComponent<ThirdPersonController>();
                }

                if (swimming == null)
                {
                    Debug.LogError("[SwimmingSystemSetup] Player missing CharacterSwimming component!");
                    setupComplete = false;
                }
                if (controller == null)
                {
                    Debug.LogError("[SwimmingSystemSetup] Player missing CharacterController component!");
                    setupComplete = false;
                }
                if (statModifier == null)
                {
                    Debug.LogWarning("[SwimmingSystemSetup] Player missing CharacterStatModifier component - swimming speed will use default values");
                }
                if (thirdPersonController == null)
                {
                    Debug.LogWarning("[SwimmingSystemSetup] Player missing ThirdPersonController component - swimming will still work but movement speed adjustments may not function");
                }
            }

            // Check for water layer setup
            if (LayerMask.NameToLayer("Water") == -1)
            {
                Debug.LogWarning("[SwimmingSystemSetup] No 'Water' layer found! Please create a Water layer in Edit > Project Settings > Tags and Layers");
            }

            if (setupComplete)
            {
                Debug.Log("[SwimmingSystemSetup] Swimming system setup complete! Characters will automatically detect water and swim when needed.");
                Debug.Log("[SwimmingSystemSetup] Remember to: 1) Create a 'Water' layer, 2) Add colliders to water objects, 3) Set water objects to the Water layer");
                Debug.Log("[SwimmingSystemSetup] Note: Swimming speed is now controlled by CharacterStatModifier component - configure swimming stat modifiers for optimal control");
            }
            else
            {
                Debug.LogError("[SwimmingSystemSetup] Swimming system setup incomplete. Check the errors above.");
            }
        }

        // Public method to manually trigger setup
        [ContextMenu("Setup Swimming System")]
        public void ManualSetup()
        {
            SetupSwimmingSystem();
        }

        // Public method to add swimming to a specific character
        public void AddSwimmingToCharacter(GameObject character)
        {
            if (character != null)
            {
                SetupCharacterSwimming(character, "Manual");
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (playerObject != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(playerObject.transform.position, 1f);
            }
        }
    }
} 