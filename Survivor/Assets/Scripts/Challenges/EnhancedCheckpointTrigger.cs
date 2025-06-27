using UnityEngine;
using Survivor.Characters;
using Survivor.Shared;
using StarterAssets;

namespace Survivor.Challenges
{
    /// <summary>
    /// Enhanced trigger component for race challenge checkpoints with segment support
    /// </summary>
    public class EnhancedCheckpointTrigger : MonoBehaviour
    {
        [Header("Checkpoint Settings")]
        [SerializeField] private int checkpointIndex = 0;
        [SerializeField] private string tribeId = "tribe_1"; // Which tribe this checkpoint is for
        [SerializeField] private bool isFinishLine = false;
        [SerializeField] private float triggerRadius = 2f;
        
        [Header("Visual Feedback")]
        [SerializeField] private GameObject checkpointVisual;
        [SerializeField] private Material activeMaterial;
        [SerializeField] private Material completedMaterial;
        
        [Header("Audio")]
        [SerializeField] private AudioClip checkpointSound;
        [SerializeField] private AudioClip finishLineSound;
        
        private bool isTriggered = false;
        private EnhancedRaceChallengeController raceController;
        private Renderer checkpointRenderer;
        private AudioSource audioSource;
        
        private void Start()
        {
            // Find the race controller
            raceController = FindObjectOfType<EnhancedRaceChallengeController>();
            if (raceController == null)
            {
                Debug.LogWarning("[EnhancedCheckpointTrigger] No EnhancedRaceChallengeController found in scene!");
            }
            
            // Set up visual feedback
            if (checkpointVisual != null)
            {
                checkpointRenderer = checkpointVisual.GetComponent<Renderer>();
                if (checkpointRenderer != null && activeMaterial != null)
                {
                    checkpointRenderer.material = activeMaterial;
                }
            }
            
            // Set up trigger collider if not already present
            var collider = GetComponent<Collider>();
            if (collider == null)
            {
                var sphereCollider = gameObject.AddComponent<SphereCollider>();
                sphereCollider.radius = triggerRadius;
                sphereCollider.isTrigger = true;
            }
            
            // Get or add audio source
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            // Auto-determine tribe ID based on position if not set
            if (string.IsNullOrEmpty(tribeId))
            {
                DetermineTribeId();
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
        
        private void TriggerCheckpoint(string participantId)
        {
            if (isTriggered) return;
            
            isTriggered = true;
            
            if (isFinishLine)
            {
                // This is a finish line
                if (raceController != null)
                {
                    raceController.OnFinishLineCrossed(tribeId, participantId);
                }
                Debug.Log($"[EnhancedCheckpointTrigger] Finish line crossed by {participantId} for tribe {tribeId}");
            }
            else
            {
                // This is a regular checkpoint
                if (raceController != null)
                {
                    raceController.OnCheckpointReached(tribeId, checkpointIndex);
                }
                Debug.Log($"[EnhancedCheckpointTrigger] Checkpoint {checkpointIndex} reached by {participantId} for tribe {tribeId}");
            }
            
            // Update visual feedback
            UpdateVisualFeedback();
            
            // Play sound effect
            PlayCheckpointSound();
        }
        
        private void UpdateVisualFeedback()
        {
            if (checkpointRenderer != null && completedMaterial != null)
            {
                checkpointRenderer.material = completedMaterial;
            }
            
            // Could also add particle effects or other visual feedback here
        }
        
        private void PlayCheckpointSound()
        {
            if (audioSource != null)
            {
                AudioClip clipToPlay = isFinishLine ? finishLineSound : checkpointSound;
                if (clipToPlay != null)
                {
                    audioSource.PlayOneShot(clipToPlay);
                }
            }
        }
        
        private void DetermineTribeId()
        {
            // Determine which tribe this checkpoint belongs to based on position
            // Assuming Tribe A is on the left (negative X) and Tribe B is on the right (positive X)
            if (transform.position.x < 0)
            {
                tribeId = "tribe_1"; // or "TribeA"
            }
            else
            {
                tribeId = "tribe_2"; // or "TribeB"
            }
        }
        
        // Public methods for external triggering
        public void ResetCheckpoint()
        {
            isTriggered = false;
            
            if (checkpointRenderer != null && activeMaterial != null)
            {
                checkpointRenderer.material = activeMaterial;
            }
        }
        
        public bool IsTriggered()
        {
            return isTriggered;
        }
        
        public int GetCheckpointIndex()
        {
            return checkpointIndex;
        }
        
        public bool IsFinishLine()
        {
            return isFinishLine;
        }
        
        public string GetTribeId()
        {
            return tribeId;
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
        
        private void OnDrawGizmosSelected()
        {
            // Draw trigger radius
            Gizmos.color = isTriggered ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, triggerRadius);
            
            // Draw checkpoint index and tribe
            #if UNITY_EDITOR
            string label = isFinishLine ? "FINISH" : $"CP {checkpointIndex}";
            if (!string.IsNullOrEmpty(tribeId))
            {
                label += $" ({tribeId})";
            }
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, label);
            #endif
        }
    }
} 