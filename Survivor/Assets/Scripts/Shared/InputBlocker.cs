using UnityEngine;

namespace Survivor.Shared
{
    /// <summary>
    /// Simple input blocker that disables movement components and blocks all input when dialogue is active
    /// </summary>
    public class InputBlocker : MonoBehaviour
    {
        private static InputBlocker instance;
        public static InputBlocker Instance => instance;

        private MonoBehaviour starterAssetsInputs;
        private MonoBehaviour thirdPersonController;
        private bool wasStarterAssetsEnabled = true;
        private bool wasThirdPersonEnabled = true;
        private bool isBlocking = false;

        // Keys that should always be allowed even when input is blocked
        private readonly KeyCode[] allowedKeys = {
            KeyCode.Escape,    // Close dialogue
            KeyCode.Tab,       // UI navigation
            KeyCode.Return,    // Send message
            KeyCode.KeypadEnter // Numpad enter
        };

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
            // Find movement components
            FindMovementComponents();
        }

        private void FindMovementComponents()
        {
            Debug.Log("[InputBlocker] Finding movement components...");
            
            // Find player GameObject
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                player = GameObject.Find("Player");
            }

            if (player != null)
            {
                Debug.Log($"[InputBlocker] Found player: {player.name}");
                
                // List all components on player for debugging
                var allComponents = player.GetComponents<MonoBehaviour>();
                Debug.Log($"[InputBlocker] Player has {allComponents.Length} MonoBehaviour components:");
                foreach (var comp in allComponents)
                {
                    Debug.Log($"[InputBlocker] - {comp.GetType().Name}: enabled = {comp.enabled}");
                }

                // Find StarterAssetsInputs component by type name
                foreach (var comp in allComponents)
                {
                    if (comp.GetType().Name == "StarterAssetsInputs")
                    {
                        starterAssetsInputs = comp;
                        wasStarterAssetsEnabled = comp.enabled;
                        Debug.Log($"[InputBlocker] Found StarterAssetsInputs: enabled = {comp.enabled}");
                        break;
                    }
                }

                // Find ThirdPersonController component by type name
                foreach (var comp in allComponents)
                {
                    if (comp.GetType().Name == "ThirdPersonController")
                    {
                        thirdPersonController = comp;
                        wasThirdPersonEnabled = comp.enabled;
                        Debug.Log($"[InputBlocker] Found ThirdPersonController: enabled = {comp.enabled}");
                        break;
                    }
                }

                if (starterAssetsInputs == null)
                {
                    Debug.LogWarning("[InputBlocker] StarterAssetsInputs component not found on player");
                }
                if (thirdPersonController == null)
                {
                    Debug.LogWarning("[InputBlocker] ThirdPersonController component not found on player");
                }
            }
            else
            {
                Debug.LogWarning("[InputBlocker] Player GameObject not found");
            }
        }

        private void Update()
        {
            if (starterAssetsInputs != null)
            {
                if (isBlocking && starterAssetsInputs.enabled)
                {
                    // Should block input, disable StarterAssetsInputs
                    wasStarterAssetsEnabled = starterAssetsInputs.enabled;
                    starterAssetsInputs.enabled = false;
                    Debug.Log("[InputBlocker] DISABLED StarterAssetsInputs - input blocking active");
                }
                else if (!isBlocking && !starterAssetsInputs.enabled && wasStarterAssetsEnabled)
                {
                    // Should not block input, re-enable StarterAssetsInputs
                    starterAssetsInputs.enabled = true;
                    Debug.Log("[InputBlocker] RE-ENABLED StarterAssetsInputs - input blocking ended");
                }
            }

            if (thirdPersonController != null)
            {
                if (isBlocking && thirdPersonController.enabled)
                {
                    // Should block input, disable ThirdPersonController
                    wasThirdPersonEnabled = thirdPersonController.enabled;
                    thirdPersonController.enabled = false;
                    Debug.Log("[InputBlocker] DISABLED ThirdPersonController - input blocking active");
                }
                else if (!isBlocking && !thirdPersonController.enabled && wasThirdPersonEnabled)
                {
                    // Should not block input, re-enable ThirdPersonController
                    thirdPersonController.enabled = true;
                    Debug.Log("[InputBlocker] RE-ENABLED ThirdPersonController - input blocking ended");
                }
            }
            else
            {
                // Try to find components again if they're null
                if (Time.frameCount % 60 == 0) // Try every 60 frames
                {
                    FindMovementComponents();
                }
            }
        }

        /// <summary>
        /// Check if a key is allowed even when input is blocked
        /// </summary>
        private bool IsKeyAllowed(KeyCode key)
        {
            foreach (var allowedKey in allowedKeys)
            {
                if (key == allowedKey)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Check if input is currently blocked
        /// </summary>
        public static bool IsInputBlocked => instance != null && instance.isBlocking;

        /// <summary>
        /// Start blocking input
        /// </summary>
        public void BlockInput()
        {
            isBlocking = true;
            Debug.Log("[InputBlocker] Input blocking enabled");
        }

        /// <summary>
        /// Stop blocking input
        /// </summary>
        public void UnblockInput()
        {
            isBlocking = false;
            Debug.Log("[InputBlocker] Input blocking disabled");
        }

        /// <summary>
        /// Force enable all movement components (useful for cleanup)
        /// </summary>
        public void ForceEnableMovement()
        {
            if (starterAssetsInputs != null)
            {
                starterAssetsInputs.enabled = true;
            }
            if (thirdPersonController != null)
            {
                thirdPersonController.enabled = true;
            }
        }

        // Static input methods that other scripts should use instead of Input.GetKeyDown()
        
        /// <summary>
        /// Get key down state - blocks all keys except allowed ones when input is blocked
        /// </summary>
        public static bool GetKeyDown(KeyCode key)
        {
            if (instance != null && instance.isBlocking)
            {
                return instance.IsKeyAllowed(key) && Input.GetKeyDown(key);
            }
            return Input.GetKeyDown(key);
        }

        /// <summary>
        /// Get key state - blocks all keys except allowed ones when input is blocked
        /// </summary>
        public static bool GetKey(KeyCode key)
        {
            if (instance != null && instance.isBlocking)
            {
                return instance.IsKeyAllowed(key);
            }
            return Input.GetKey(key);
        }

        /// <summary>
        /// Get key up state - blocks all keys except allowed ones when input is blocked
        /// </summary>
        public static bool GetKeyUp(KeyCode key)
        {
            if (instance != null && instance.isBlocking)
            {
                return instance.IsKeyAllowed(key) && Input.GetKeyUp(key);
            }
            return Input.GetKeyUp(key);
        }

        /// <summary>
        /// Get axis value - returns 0 when input is blocked
        /// </summary>
        public static float GetAxis(string axisName)
        {
            if (instance != null && instance.isBlocking)
            {
                return 0f;
            }
            return Input.GetAxis(axisName);
        }

        /// <summary>
        /// Get axis raw value - returns 0 when input is blocked
        /// </summary>
        public static float GetAxisRaw(string axisName)
        {
            if (instance != null && instance.isBlocking)
            {
                return 0f;
            }
            return Input.GetAxisRaw(axisName);
        }

        private void OnDestroy()
        {
            // Make sure to re-enable movement components when destroyed
            ForceEnableMovement();
        }
    }
} 