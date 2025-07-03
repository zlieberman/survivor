using UnityEngine;
using StarterAssets;

namespace Survivor.Challenges
{
    /// <summary>
    /// Checkpoint component that changes player input mapping when triggered
    /// Can be attached to any GameObject in the scene
    /// </summary>
    public class InputMappingCheckpoint : MonoBehaviour
    {
        [Header("Checkpoint Configuration")]
        [SerializeField] private InputMapping inputMapping;
        [SerializeField] private float triggerRadius = 2f;
        [SerializeField] private bool isTriggered = false;
        [SerializeField] private float triggerDelay = 0.1f;
        
        [Header("Visual Feedback")]
        [SerializeField] private Material activeMaterial;
        [SerializeField] private Material triggeredMaterial;
        
        [Header("Audio")]
        [SerializeField] private AudioClip triggerSound;
        
        private Renderer _checkpointRenderer;
        private AudioSource _audioSource;
        private float _lastTriggerTime;
        
        private void Start()
        {
            // Set up trigger collider if not already present
            SetupTriggerCollider();
            
            // Set up visual feedback
            SetupVisualFeedback();
            
            // Set up audio
            SetupAudio();
            
            // Validate input mapping
            if (inputMapping == null)
            {
                Debug.LogWarning($"[InputMappingCheckpoint] No input mapping configured for {gameObject.name}! Using default.");
                inputMapping = InputMapping.CreateDefault();
            }
        }
        
        private void SetupTriggerCollider()
        {
            var collider = GetComponent<Collider>();
            if (collider == null)
            {
                var sphereCollider = gameObject.AddComponent<SphereCollider>();
                sphereCollider.radius = triggerRadius;
                sphereCollider.isTrigger = true;
            }
            else if (collider is SphereCollider sphereCollider)
            {
                sphereCollider.radius = triggerRadius;
                sphereCollider.isTrigger = true;
            }
        }
        
        private void SetupVisualFeedback()
        {
            // Try to find a renderer on this object or its children
            _checkpointRenderer = GetComponent<Renderer>();
            if (_checkpointRenderer == null)
            {
                _checkpointRenderer = GetComponentInChildren<Renderer>();
            }
            
            // Apply active material if available
            if (_checkpointRenderer != null && activeMaterial != null)
            {
                _checkpointRenderer.material = activeMaterial;
            }
        }
        
        private void SetupAudio()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null && triggerSound != null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (isTriggered) return;
            
            // Check if it's a player
            var player = other.GetComponent<ThirdPersonController>();
            if (player != null)
            {
                TriggerCheckpoint();
                return;
            }
            
            // Also check for StarterAssetsInputs component
            var playerInputs = other.GetComponent<StarterAssetsInputs>();
            if (playerInputs != null)
            {
                TriggerCheckpoint();
                return;
            }
        }
        
        private void TriggerCheckpoint()
        {
            if (isTriggered || Time.time - _lastTriggerTime < triggerDelay) return;
            
            _lastTriggerTime = Time.time;
            isTriggered = true;
            
            // Apply the input mapping
            if (InputMappingManager.Instance != null)
            {
                InputMappingManager.Instance.SetInputMapping(inputMapping);
            }
            else
            {
                // Try to create the manager automatically
                Debug.LogWarning("[InputMappingCheckpoint] InputMappingManager not found! Creating one automatically...");
                InputMappingManager.EnsureExists();
                
                // Try again after creation
                if (InputMappingManager.Instance != null)
                {
                    InputMappingManager.Instance.SetInputMapping(inputMapping);
                    Debug.Log("[InputMappingCheckpoint] Successfully created InputMappingManager and applied mapping!");
                }
                else
                {
                    Debug.LogError("[InputMappingCheckpoint] Failed to create InputMappingManager! System may not work properly.");
                    return;
                }
            }
            
            // Update visual feedback
            UpdateVisualFeedback();
            
            // Play sound effect
            PlayTriggerSound();
            
            Debug.Log($"[InputMappingCheckpoint] Triggered: {inputMapping.checkpointName} - {inputMapping.description}");
        }
        
        private void UpdateVisualFeedback()
        {
            if (_checkpointRenderer != null && triggeredMaterial != null)
            {
                _checkpointRenderer.material = triggeredMaterial;
            }
        }
        
        private void PlayTriggerSound()
        {
            if (_audioSource != null && triggerSound != null)
            {
                _audioSource.PlayOneShot(triggerSound);
            }
        }
        
        /// <summary>
        /// Resets the checkpoint so it can be triggered again
        /// </summary>
        public void ResetCheckpoint()
        {
            isTriggered = false;
            
            if (_checkpointRenderer != null && activeMaterial != null)
            {
                _checkpointRenderer.material = activeMaterial;
            }
        }
        
        /// <summary>
        /// Sets a new input mapping for this checkpoint
        /// </summary>
        public void SetInputMapping(InputMapping newMapping)
        {
            inputMapping = newMapping;
        }
        
        /// <summary>
        /// Checks if the checkpoint has been triggered
        /// </summary>
        public bool IsTriggered()
        {
            return isTriggered;
        }
        
        /// <summary>
        /// Gets the current input mapping
        /// </summary>
        public InputMapping GetInputMapping()
        {
            return inputMapping;
        }
        
        /// <summary>
        /// Gets the current input mapping for migration purposes
        /// </summary>
        public InputMapping GetInputMappingForMigration()
        {
            return inputMapping;
        }
        
        /// <summary>
        /// Sets the input mapping for migration purposes
        /// </summary>
        public void SetInputMappingForMigration(InputMapping newMapping)
        {
            inputMapping = newMapping;
        }
        
        // Debug methods
        [ContextMenu("Test Trigger")]
        public void TestTrigger()
        {
            TriggerCheckpoint();
        }
        
        [ContextMenu("Reset Checkpoint")]
        public void TestReset()
        {
            ResetCheckpoint();
        }
        
        [ContextMenu("Set Forward Only")]
        public void SetForwardOnly()
        {
            inputMapping = InputMapping.CreateForwardOnly();
            inputMapping.checkpointName = "Forward Only";
        }
        
        [ContextMenu("Set Disabled")]
        public void SetDisabled()
        {
            inputMapping = InputMapping.CreateDisabled();
            inputMapping.checkpointName = "Disabled";
        }
        
        [ContextMenu("Set Jump Only")]
        public void SetJumpOnly()
        {
            inputMapping = InputMapping.CreateJumpOnly();
            inputMapping.checkpointName = "Jump Only";
        }
        
        [ContextMenu("Set Default")]
        public void SetDefault()
        {
            inputMapping = InputMapping.CreateDefault();
            inputMapping.checkpointName = "Default";
        }
        
        private void OnDrawGizmosSelected()
        {
            // Draw trigger radius
            Gizmos.color = isTriggered ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, triggerRadius);
            
            // Draw checkpoint info
            #if UNITY_EDITOR
            string label = inputMapping != null ? inputMapping.checkpointName : "No Mapping";
            if (isTriggered)
            {
                label += " (Triggered)";
            }
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, label);
            #endif
        }
        
        private void OnValidate()
        {
            // Ensure trigger radius is positive
            if (triggerRadius <= 0)
            {
                triggerRadius = 2f;
            }
            
            // Ensure trigger delay is positive
            if (triggerDelay < 0)
            {
                triggerDelay = 0.1f;
            }
        }
    }
} 