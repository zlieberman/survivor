using UnityEngine;

namespace Survivor.Challenges
{
    /// <summary>
    /// Simple setup script that ensures InputMappingManager exists when checkpoints are used
    /// </summary>
    public class InputMappingCheckpointSetup : MonoBehaviour
    {
        [Header("Auto Setup")]
        [SerializeField] private bool setupOnStart = true;
        [SerializeField] private bool showDebugInfo = true;
        
        private void Start()
        {
            Debug.Log("[InputMappingCheckpointSetup] Starting setup...");
            if (setupOnStart)
            {
                SetupInputMappingManager();
            }
            else
            {
                Debug.Log("[InputMappingCheckpointSetup] setupOnStart is false, skipping automatic setup");
            }
        }
        
        /// <summary>
        /// Creates InputMappingManager if it doesn't exist
        /// </summary>
        [ContextMenu("Setup InputMappingManager")]
        public void SetupInputMappingManager()
        {
            // Check if InputMappingManager already exists
            if (InputMappingManager.Instance != null)
            {
                if (showDebugInfo)
                {
                    Debug.Log("[InputMappingCheckpointSetup] InputMappingManager already exists");
                }
                return;
            }
            
            // Create InputMappingManager
            GameObject inputManagerGO = new GameObject("InputMappingManager");
            var inputManager = inputManagerGO.AddComponent<InputMappingManager>();
            
            if (showDebugInfo)
            {
                Debug.Log("[InputMappingCheckpointSetup] Created InputMappingManager");
            }
            
            // Verify it was created successfully
            if (InputMappingManager.Instance != null)
            {
                Debug.Log("[InputMappingCheckpointSetup] ✓ InputMappingManager setup complete and verified!");
            }
            else
            {
                Debug.LogError("[InputMappingCheckpointSetup] ✗ InputMappingManager creation failed!");
            }
        }
    }
} 