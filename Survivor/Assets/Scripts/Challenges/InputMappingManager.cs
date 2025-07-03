using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;
using System.Collections;

namespace Survivor.Challenges
{
    /// <summary>
    /// Simple input mapping manager that disables PlayerInput when checkpoints are triggered
    /// and directly handles keyboard input based on the current mapping
    /// </summary>
    public class InputMappingManager : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;
        
        // Singleton
        public static InputMappingManager Instance { get; private set; }
        
        // Components
        private PlayerInput _playerInput;
        private StarterAssetsInputs _starterInputs;
        
        // Current state
        private InputMapping _currentMapping;
        private InputMapping _defaultMapping;
        private string _currentCheckpointName = "None";
        private bool _isCheckpointActive = false;
        
        // Input state
        private Vector2 _moveInput;
        private bool _jumpInput;
        private bool _sprintInput;
        private bool _crouchInput;
        private bool _crawlMovementInput;
        
        private void Awake()
        {
            // Singleton setup
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeManager();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void InitializeManager()
        {
            // Find components
            _playerInput = FindObjectOfType<PlayerInput>();
            _starterInputs = FindObjectOfType<StarterAssetsInputs>();
            
            if (_playerInput == null)
            {
                Debug.LogError("[InputMappingManager] No PlayerInput found! Cannot manage input.");
                return;
            }
            
            if (_starterInputs == null)
            {
                Debug.LogError("[InputMappingManager] No StarterAssetsInputs found! Cannot manage input.");
                return;
            }
            
            // Create default mapping (all inputs allowed)
            _defaultMapping = InputMapping.CreateDefault();
            _currentMapping = _defaultMapping;
            
            Debug.Log("[InputMappingManager] Initialized successfully");
            Debug.Log($"[InputMappingManager] Found PlayerInput: {_playerInput.gameObject.name}");
            Debug.Log($"[InputMappingManager] Found StarterAssetsInputs: {_starterInputs.gameObject.name}");
        }
        
        private void Update()
        {
            if (_isCheckpointActive && _starterInputs != null)
            {
                // Handle direct keyboard input when checkpoint is active
                HandleDirectKeyboardInput();
                
                // Apply filtered input to StarterAssetsInputs
                ApplyFilteredInput();
            }
        }
        
        private void OnGUI()
        {
            // Debug info - only call GUI functions in OnGUI
            if (showDebugInfo && _currentMapping != null)
            {
                DrawDebugGUI();
            }
        }
        
        private void HandleDirectKeyboardInput()
        {
            // Get raw keyboard input
            _moveInput = Vector2.zero;
            
            // Check movement keys
            if (Input.GetKey(KeyCode.W)) _moveInput.y += 1f;
            if (Input.GetKey(KeyCode.S)) _moveInput.y -= 1f;
            if (Input.GetKey(KeyCode.A)) _moveInput.x -= 1f;
            if (Input.GetKey(KeyCode.D)) _moveInput.x += 1f;
            
            // Check other inputs
            _jumpInput = Input.GetKey(KeyCode.Space);
            _sprintInput = Input.GetKey(KeyCode.LeftShift);
            _crouchInput = Input.GetKey(KeyCode.C);
            _crawlMovementInput = Input.GetKey(KeyCode.Space); // Space is also crawl movement
        }
        
        private void ApplyFilteredInput()
        {
            // Filter movement input based on current mapping
            Vector2 filteredMove = Vector2.zero;
            
            if (_currentMapping.allowForward && _moveInput.y > 0) filteredMove.y = _moveInput.y;
            if (_currentMapping.allowBackward && _moveInput.y < 0) filteredMove.y = _moveInput.y;
            if (_currentMapping.allowLeft && _moveInput.x < 0) filteredMove.x = _moveInput.x;
            if (_currentMapping.allowRight && _moveInput.x > 0) filteredMove.x = _moveInput.x;
            
            // Apply movement
            _starterInputs.move = filteredMove;
            
            // Apply sprint
            _starterInputs.sprint = _currentMapping.allowSprint && _sprintInput;
            
            // Apply crawl (C key)
            _starterInputs.crawl = _currentMapping.allowCrouch && _crouchInput;
            
            // Handle space bar - can be used for jumping OR crawl movement
            bool canJump = _currentMapping.allowJump && _jumpInput;
            bool canCrawlMove = _currentMapping.allowCrawlMovement && _crawlMovementInput;
            
            // Check if player is actually crawling by finding the ThirdPersonController
            bool isPlayerCrawling = false;
            var thirdPersonController = _starterInputs.GetComponent<ThirdPersonController>();
            if (thirdPersonController != null)
            {
                try
                {
                    isPlayerCrawling = thirdPersonController.IsCrawling;
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[InputMappingManager] Could not check crawling state: {ex.Message}");
                }
            }
            
            // If player is crawling and can use space for crawl movement, prioritize that
            if (isPlayerCrawling && canCrawlMove)
            {
                // Player is crawling and can use space for crawl movement
                _starterInputs.jump = canCrawlMove; // Use space bar for crawl movement
                
                // Debug logging for crawling
                if (_jumpInput && showDebugInfo)
                {
                    Debug.Log($"[InputMappingManager] Space bar used for crawl movement (player is crawling)");
                }
            }
            else
            {
                // Player is not crawling, use space bar for jumping
                _starterInputs.jump = canJump;
                
                // Debug logging for jumping
                if (_jumpInput && canJump && showDebugInfo)
                {
                    Debug.Log($"[InputMappingManager] Space bar used for jumping (player not crawling)");
                }
            }
        }
        
        /// <summary>
        /// Sets a new input mapping when a checkpoint is triggered
        /// </summary>
        public void SetInputMapping(InputMapping newMapping)
        {
            if (newMapping == null) return;
            
            var oldMapping = _currentMapping;
            
            // Check if crawling is being disabled and player is currently crawling
            if (_starterInputs != null && oldMapping != null)
            {
                var thirdPersonController = _starterInputs.GetComponent<ThirdPersonController>();
                if (thirdPersonController != null)
                {
                    // If the new mapping disables crawling but player is currently crawling
                    bool crawlingWasAllowed = oldMapping.allowCrouch;
                    bool crawlingNowDisabled = !newMapping.allowCrouch;
                    bool playerIsCrawling = thirdPersonController.IsCrawling;
                    
                    if (crawlingWasAllowed && crawlingNowDisabled && playerIsCrawling)
                    {
                        Debug.Log("[InputMappingManager] Crawling disabled while player is crawling - attempting to stand up...");
                        
                        // Try to force the player to stand up by calling the same logic as the C key
                        ForceStandUp(thirdPersonController);
                    }
                }
            }
            
            _currentMapping = newMapping;
            _currentCheckpointName = newMapping.checkpointName;
            
            // Disable PlayerInput and enable direct input handling
            if (_playerInput != null)
            {
                _playerInput.enabled = false;
                _isCheckpointActive = true;
            }
            
            Debug.Log($"[InputMappingManager] *** CHECKPOINT TRIGGERED ***");
            Debug.Log($"[InputMappingManager] Changed from '{oldMapping?.checkpointName ?? "None"}' to '{newMapping.checkpointName}'");
            Debug.Log($"[InputMappingManager] New mapping: Jump={newMapping.allowJump}, Sprint={newMapping.allowSprint}, Crouch={newMapping.allowCrouch}, F={newMapping.allowForward}, B={newMapping.allowBackward}, L={newMapping.allowLeft}, R={newMapping.allowRight}");
        }
        
        /// <summary>
        /// Forces the player to stand up from crawling position if possible
        /// </summary>
        private void ForceStandUp(ThirdPersonController controller)
        {
            bool success = controller.ForceStandUp();
            
            if (success)
            {
                Debug.Log("[InputMappingManager] Successfully forced player to stand up - crawling disabled by checkpoint");
            }
            else
            {
                Debug.LogWarning("[InputMappingManager] Cannot force stand up - not enough room above player. Player remains crawling despite checkpoint disabling crawling.");
            }
        }
        
        /// <summary>
        /// Resets to default input mapping
        /// </summary>
        public void ResetToDefault()
        {
            _currentMapping = _defaultMapping;
            _currentCheckpointName = "Default";
            
            // Re-enable PlayerInput and disable direct input handling
            if (_playerInput != null)
            {
                _playerInput.enabled = true;
                _isCheckpointActive = false;
            }
            
            Debug.Log("[InputMappingManager] Reset to default input mapping");
        }
        
        /// <summary>
        /// Gets the current input mapping
        /// </summary>
        public InputMapping GetCurrentMapping()
        {
            return _currentMapping;
        }
        
        /// <summary>
        /// Checks if default mapping is active
        /// </summary>
        public bool IsDefaultMapping()
        {
            return _currentMapping == _defaultMapping;
        }
        
        private void DrawDebugGUI()
        {
            GUI.color = Color.white;
            GUI.backgroundColor = Color.black;
            
            GUILayout.BeginArea(new Rect(10, 10, 350, 250));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("<b>Input Mapping Debug</b>", new GUIStyle(GUI.skin.label) { richText = true });
            GUILayout.Space(5);
            
            GUILayout.Label($"Current: {_currentCheckpointName}");
            GUILayout.Label($"Mode: {(_isCheckpointActive ? "Checkpoint" : "Normal")}");
            
            if (_currentMapping != null)
            {
                GUILayout.Space(5);
                GUILayout.Label("<b>Permissions:</b>", new GUIStyle(GUI.skin.label) { richText = true });
                GUILayout.Label($"Movement: F={_currentMapping.allowForward} B={_currentMapping.allowBackward} L={_currentMapping.allowLeft} R={_currentMapping.allowRight}");
                GUILayout.Label($"Actions: Jump={_currentMapping.allowJump} Sprint={_currentMapping.allowSprint} Crouch={_currentMapping.allowCrouch}");
                GUILayout.Label($"Advanced: CrawlMove={_currentMapping.allowCrawlMovement} Swimming={_currentMapping.allowSwimming}");
                
                if (_isCheckpointActive)
                {
                    GUILayout.Space(5);
                    GUILayout.Label("<b>Input State:</b>", new GUIStyle(GUI.skin.label) { richText = true });
                    GUILayout.Label($"Raw Input: ({_moveInput.x:F1}, {_moveInput.y:F1})");
                    GUILayout.Label($"Filtered: ({_starterInputs?.move.x:F1}, {_starterInputs?.move.y:F1})");
                    
                    // Show crawling state
                    if (_starterInputs != null)
                    {
                        var thirdPersonController = _starterInputs.GetComponent<ThirdPersonController>();
                        if (thirdPersonController != null)
                        {
                            try
                            {
                                bool isPlayerCrawling = thirdPersonController.IsCrawling;
                                GUILayout.Label($"C Key: {_crouchInput} | Crawling: {isPlayerCrawling}");
                                GUILayout.Label($"Space: {_jumpInput} | Jump Out: {_starterInputs.jump}");
                                
                                if (isPlayerCrawling)
                                {
                                    GUI.color = Color.yellow;
                                    GUILayout.Label($"<b>CRAWLING MODE - Space = Crawl Movement</b>", new GUIStyle(GUI.skin.label) { richText = true });
                                    GUI.color = Color.white;
                                }
                                else
                                {
                                    GUI.color = Color.cyan;
                                    GUILayout.Label($"<b>NORMAL MODE - Space = Jump</b>", new GUIStyle(GUI.skin.label) { richText = true });
                                    GUI.color = Color.white;
                                }
                            }
                            catch
                            {
                                GUILayout.Label("Crawling state: Error");
                            }
                        }
                    }
                }
            }
            
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        
        [ContextMenu("Reset To Default")]
        public void DebugResetToDefault()
        {
            ResetToDefault();
        }
        
        [ContextMenu("Test Forward Only")]
        public void DebugTestForwardOnly()
        {
            var testMapping = InputMapping.CreateForwardOnly();
            testMapping.checkpointName = "Debug Forward Only";
            SetInputMapping(testMapping);
        }
        
        /// <summary>
        /// Emergency initialization method - creates InputMappingManager if it doesn't exist
        /// Call this from any script that needs the manager
        /// </summary>
        public static void EnsureExists()
        {
            if (Instance != null) return;
            
            Debug.Log("[InputMappingManager] Creating emergency InputMappingManager...");
            GameObject managerGO = new GameObject("InputMappingManager");
            managerGO.AddComponent<InputMappingManager>();
        }
    }
} 