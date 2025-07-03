using UnityEngine;
using Survivor.Characters;
using Survivor.Shared;
using StarterAssets;

namespace Survivor.Challenges
{
    /// <summary>
    /// Enhanced checkpoint that combines race challenge functionality with input mapping changes
    /// Extends the existing checkpoint system with input control features
    /// </summary>
    public class EnhancedInputMappingCheckpoint : MonoBehaviour
    {
        [Header("Race Checkpoint Settings")]
        [SerializeField] private int checkpointIndex = 0;
        [SerializeField] private string tribeId = "tribe_1";
        [SerializeField] private bool isFinishLine = false;
        [SerializeField] private float triggerRadius = 2f;
        
        [Header("Input Mapping Settings")]
        [SerializeField] private InputMapping inputMapping;
        [SerializeField] private bool changeInputMapping = true;
        [SerializeField] private bool resetToDefaultOnExit = false;
        
        [Header("Visual Feedback")]
        [SerializeField] private GameObject checkpointVisual;
        [SerializeField] private Material activeMaterial;
        [SerializeField] private Material completedMaterial;
        [SerializeField] private Material triggeredMaterial;
        
        [Header("Audio")]
        [SerializeField] private AudioClip checkpointSound;
        [SerializeField] private AudioClip finishLineSound;
        [SerializeField] private AudioClip inputChangeSound;
        
        private bool isTriggered = false;
        private RaceChallengeController raceController;
        private EnhancedRaceChallengeController enhancedRaceController;
        private Renderer checkpointRenderer;
        private AudioSource audioSource;
        private InputMapping originalMapping;
        
        private void Start()
        {
            // Find race controllers
            FindRaceControllers();
            
            // Set up visual feedback
            SetupVisualFeedback();
            
            // Set up trigger collider
            SetupTriggerCollider();
            
            // Set up audio
            SetupAudio();
            
            // Store original mapping
            if (InputMappingManager.Instance != null)
            {
                originalMapping = InputMappingManager.Instance.GetCurrentMapping();
            }
            
            // Validate input mapping
            if (inputMapping == null && changeInputMapping)
            {
                Debug.LogWarning($"[EnhancedInputMappingCheckpoint] No input mapping configured for {gameObject.name}! Using default.");
                inputMapping = InputMapping.CreateDefault();
            }
        }
        
        private void FindRaceControllers()
        {
            raceController = FindObjectOfType<RaceChallengeController>();
            enhancedRaceController = FindObjectOfType<EnhancedRaceChallengeController>();
            
            if (raceController == null && enhancedRaceController == null)
            {
                Debug.LogWarning("[EnhancedInputMappingCheckpoint] No race controller found in scene!");
            }
        }
        
        private void SetupVisualFeedback()
        {
            if (checkpointVisual != null)
            {
                checkpointRenderer = checkpointVisual.GetComponent<Renderer>();
            }
            else
            {
                checkpointRenderer = GetComponent<Renderer>();
            }
            
            if (checkpointRenderer != null && activeMaterial != null)
            {
                checkpointRenderer.material = activeMaterial;
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
        
        private void SetupAudio()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (isTriggered) return;
            
            // Check if it's a player
            var player = other.GetComponent<ThirdPersonController>();
            if (player != null)
            {
                TriggerCheckpoint("player_main");
                return;
            }
            
            // Check if it's an NPC
            var npc = other.GetComponent<NPCCharacter>();
            if (npc != null)
            {
                TriggerCheckpoint($"npc_{npc.GetInstanceID()}");
                return;
            }
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (!resetToDefaultOnExit) return;
            
            // Check if it's a player
            var player = other.GetComponent<ThirdPersonController>();
            if (player != null)
            {
                ResetToDefaultInput();
                return;
            }
            
            // Check if it's an NPC
            var npc = other.GetComponent<NPCCharacter>();
            if (npc != null)
            {
                ResetToDefaultInput();
                return;
            }
        }
        
        private void TriggerCheckpoint(string participantId)
        {
            if (isTriggered) return;
            
            isTriggered = true;
            
            // Handle race checkpoint logic
            HandleRaceCheckpoint(participantId);
            
            // Handle input mapping change
            if (changeInputMapping && inputMapping != null)
            {
                ChangeInputMapping();
            }
            
            // Update visual feedback
            UpdateVisualFeedback();
            
            // Play sound effects
            PlayCheckpointSound();
        }
        
        private void HandleRaceCheckpoint(string participantId)
        {
            if (isFinishLine)
            {
                // This is a finish line
                if (enhancedRaceController != null)
                {
                    enhancedRaceController.OnFinishLineCrossed(tribeId, participantId);
                }
                else if (raceController != null)
                {
                    raceController.OnFinishLineCrossed(tribeId, participantId);
                }
                Debug.Log($"[EnhancedInputMappingCheckpoint] Finish line crossed by {participantId} for tribe {tribeId}");
            }
            else
            {
                // This is a regular checkpoint
                if (enhancedRaceController != null)
                {
                    enhancedRaceController.OnCheckpointReached(tribeId, checkpointIndex);
                }
                else if (raceController != null)
                {
                    raceController.OnCheckpointReached(tribeId, checkpointIndex);
                }
                Debug.Log($"[EnhancedInputMappingCheckpoint] Checkpoint {checkpointIndex} reached by {participantId} for tribe {tribeId}");
            }
        }
        
        private void ChangeInputMapping()
        {
            if (InputMappingManager.Instance != null)
            {
                InputMappingManager.Instance.SetInputMapping(inputMapping);
                Debug.Log($"[EnhancedInputMappingCheckpoint] Applied input mapping: {inputMapping.checkpointName}");
            }
            else
            {
                Debug.LogError("[EnhancedInputMappingCheckpoint] InputMappingManager not found in scene!");
            }
        }
        
        private void ResetToDefaultInput()
        {
            if (InputMappingManager.Instance != null)
            {
                InputMappingManager.Instance.ResetToDefault();
                Debug.Log("[EnhancedInputMappingCheckpoint] Reset to default input mapping");
            }
        }
        
        private void UpdateVisualFeedback()
        {
            if (checkpointRenderer != null)
            {
                if (triggeredMaterial != null)
                {
                    checkpointRenderer.material = triggeredMaterial;
                }
                else if (completedMaterial != null)
                {
                    checkpointRenderer.material = completedMaterial;
                }
            }
        }
        
        private void PlayCheckpointSound()
        {
            if (audioSource != null)
            {
                AudioClip clipToPlay = null;
                
                if (isFinishLine)
                {
                    clipToPlay = finishLineSound;
                }
                else if (changeInputMapping && inputChangeSound != null)
                {
                    clipToPlay = inputChangeSound;
                }
                else
                {
                    clipToPlay = checkpointSound;
                }
                
                if (clipToPlay != null)
                {
                    audioSource.PlayOneShot(clipToPlay);
                }
            }
        }
        
        /// <summary>
        /// Resets the checkpoint so it can be triggered again
        /// </summary>
        public void ResetCheckpoint()
        {
            isTriggered = false;
            
            if (checkpointRenderer != null && activeMaterial != null)
            {
                checkpointRenderer.material = activeMaterial;
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
        /// Enables or disables input mapping changes
        /// </summary>
        public void SetInputMappingEnabled(bool enabled)
        {
            changeInputMapping = enabled;
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
        
        // Debug methods
        [ContextMenu("Test Trigger")]
        public void TestTrigger()
        {
            TriggerCheckpoint("test_participant");
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
            string label = isFinishLine ? "FINISH" : $"CP {checkpointIndex}";
            if (changeInputMapping && inputMapping != null)
            {
                label += $" ({inputMapping.checkpointName})";
            }
            if (isTriggered)
            {
                label += " [Triggered]";
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
        }
    }
} 