using UnityEngine;
using StarterAssets;

namespace Survivor.Shared
{
    /// <summary>
    /// Global input blocker that can disable keyboard input for movement and interactions
    /// when the player is typing in UI elements like chat input fields.
    /// </summary>
    public class InputBlocker : MonoBehaviour
    {
        private static InputBlocker instance;
        public static InputBlocker Instance => instance;

        [Header("Input Components")]
        [SerializeField] private StarterAssetsInputs starterAssetsInputs;
        [SerializeField] private ThirdPersonController thirdPersonController;
        
        private bool wasInputEnabled = true;
        private bool wasThirdPersonControllerEnabled = true;
        private bool isInputBlocked = false;

        // Store references to other input-handling scripts
        private MonoBehaviour[] inputHandlingScripts;
        private bool[] wasScriptEnabled;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Try to find input components if not assigned
            RefreshInputComponents();
            
            // Ensure arrays are initialized
            EnsureArraysInitialized();
        }

        /// <summary>
        /// Ensure the input handling arrays are properly initialized
        /// </summary>
        private void EnsureArraysInitialized()
        {
            if (inputHandlingScripts == null)
            {
                inputHandlingScripts = new MonoBehaviour[0];
            }
            if (wasScriptEnabled == null)
            {
                wasScriptEnabled = new bool[0];
            }
        }

        /// <summary>
        /// Refresh input components - useful if they're created after this script
        /// </summary>
        public void RefreshInputComponents()
        {
            if (starterAssetsInputs == null || thirdPersonController == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player == null)
                {
                    player = GameObject.Find("Player");
                }
                
                if (player != null)
                {
                    if (starterAssetsInputs == null)
                    {
                        starterAssetsInputs = player.GetComponent<StarterAssetsInputs>();
                    }
                    if (thirdPersonController == null)
                    {
                        thirdPersonController = player.GetComponent<ThirdPersonController>();
                    }
                    
                    if (starterAssetsInputs != null || thirdPersonController != null)
                    {
                        Debug.Log("[InputBlocker] Found input components on player");
                    }
                }
                else
                {
                    Debug.LogWarning("[InputBlocker] Player not found - input blocking may not work");
                }
            }
        }

        /// <summary>
        /// Find all scripts that handle keyboard input
        /// </summary>
        private void FindInputHandlingScripts()
        {
            // Find scripts that commonly handle keyboard input
            var allScripts = FindObjectsOfType<MonoBehaviour>();
            var inputScripts = new System.Collections.Generic.List<MonoBehaviour>();

            foreach (var script in allScripts)
            {
                if (script != null && IsInputHandlingScript(script))
                {
                    inputScripts.Add(script);
                    Debug.Log($"[InputBlocker] Found input handling script: {script.GetType().Name}");
                }
            }

            inputHandlingScripts = inputScripts.ToArray();
            wasScriptEnabled = new bool[inputHandlingScripts.Length];
            
            Debug.Log($"[InputBlocker] Found {inputHandlingScripts.Length} input handling scripts");
        }

        /// <summary>
        /// Check if a script is likely to handle keyboard input
        /// </summary>
        private bool IsInputHandlingScript(MonoBehaviour script)
        {
            if (script == null) return false;

            string scriptName = script.GetType().Name.ToLower();
            
            // Common input handling script names
            string[] inputScriptNames = {
                "tribeinfomenucontroller",
                "inventorypanel", 
                "playerinteractionmanager",
                "playerinteractionhandler",
                "characterinteractionmanager",
                "interactionmanager",
                "dialogueinteractable",
                "baseinteractable",
                "firewood",
                "coconut",
                "banana",
                "campfireinteractable"
            };

            foreach (string name in inputScriptNames)
            {
                if (scriptName.Contains(name))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Block all keyboard input for movement and interactions
        /// </summary>
        public void BlockInput()
        {
            if (isInputBlocked) return;

            isInputBlocked = true;
            
            // Find input handling scripts
            FindInputHandlingScripts();
            
            // Disable Starter Assets components
            if (starterAssetsInputs != null)
            {
                wasInputEnabled = starterAssetsInputs.enabled;
                starterAssetsInputs.enabled = false;
            }
            else
            {
                Debug.LogWarning("[InputBlocker] StarterAssetsInputs not found - cannot block movement input");
            }
            
            if (thirdPersonController != null)
            {
                wasThirdPersonControllerEnabled = thirdPersonController.enabled;
                thirdPersonController.enabled = false;
            }
            else
            {
                Debug.LogWarning("[InputBlocker] ThirdPersonController not found - cannot block movement input");
            }

            // Disable other input handling scripts (with null checks)
            if (inputHandlingScripts != null && wasScriptEnabled != null)
            {
                for (int i = 0; i < inputHandlingScripts.Length && i < wasScriptEnabled.Length; i++)
                {
                    if (inputHandlingScripts[i] != null)
                    {
                        wasScriptEnabled[i] = inputHandlingScripts[i].enabled;
                        inputHandlingScripts[i].enabled = false;
                        Debug.Log($"[InputBlocker] Disabled input script: {inputHandlingScripts[i].GetType().Name}");
                    }
                }
            }

            Debug.Log($"[InputBlocker] Keyboard input blocked - disabled {(inputHandlingScripts != null ? inputHandlingScripts.Length : 0)} input scripts");
        }

        /// <summary>
        /// Unblock keyboard input and restore previous state
        /// </summary>
        public void UnblockInput()
        {
            if (!isInputBlocked) return;

            isInputBlocked = false;
            
            // Re-enable Starter Assets components
            if (starterAssetsInputs != null && wasInputEnabled)
            {
                starterAssetsInputs.enabled = true;
            }
            
            if (thirdPersonController != null && wasThirdPersonControllerEnabled)
            {
                thirdPersonController.enabled = true;
            }

            // Re-enable other input handling scripts (with null checks)
            if (inputHandlingScripts != null && wasScriptEnabled != null)
            {
                for (int i = 0; i < inputHandlingScripts.Length && i < wasScriptEnabled.Length; i++)
                {
                    if (inputHandlingScripts[i] != null && wasScriptEnabled[i])
                    {
                        inputHandlingScripts[i].enabled = true;
                        Debug.Log($"[InputBlocker] Re-enabled input script: {inputHandlingScripts[i].GetType().Name}");
                    }
                }
            }

            Debug.Log("[InputBlocker] Keyboard input unblocked");
        }

        /// <summary>
        /// Check if input is currently blocked
        /// </summary>
        public bool IsInputBlocked => isInputBlocked;

        /// <summary>
        /// Force enable input components (useful for cleanup)
        /// </summary>
        public void ForceEnableInput()
        {
            try
            {
                isInputBlocked = false;
                
                if (starterAssetsInputs != null)
                {
                    starterAssetsInputs.enabled = true;
                }
                
                if (thirdPersonController != null)
                {
                    thirdPersonController.enabled = true;
                }

                // Force enable all input scripts (with comprehensive null checks)
                if (inputHandlingScripts != null && inputHandlingScripts.Length > 0)
                {
                    for (int i = 0; i < inputHandlingScripts.Length; i++)
                    {
                        if (inputHandlingScripts[i] != null)
                        {
                            inputHandlingScripts[i].enabled = true;
                        }
                    }
                }
                
                Debug.Log("[InputBlocker] Force enable input completed successfully");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[InputBlocker] Error in ForceEnableInput: {e.Message}");
            }
        }

        private void OnDestroy()
        {
            try
            {
                // Make sure to re-enable input when destroyed
                if (isInputBlocked)
                {
                    ForceEnableInput();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[InputBlocker] Error in OnDestroy: {e.Message}");
            }
        }
    }
} 