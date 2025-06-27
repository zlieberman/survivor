using UnityEngine;
using Survivor.Characters;
using Survivor.Shared;
using StarterAssets;

namespace Survivor.Challenges
{
    /// <summary>
    /// Trigger component for race challenge checkpoints
    /// </summary>
    public class CheckpointTrigger : MonoBehaviour
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
        private RaceChallengeController raceController;
        private Renderer checkpointRenderer;
        private AudioSource audioSource;
        
        private void Start()
        {
            // Find the race controller
            raceController = FindObjectOfType<RaceChallengeController>();
            if (raceController == null)
            {
                Debug.LogWarning("[CheckpointTrigger] No RaceChallengeController found in scene!");
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
                Debug.Log($"[CheckpointTrigger] Finish line crossed by {participantId}");
            }
            else
            {
                // This is a regular checkpoint
                if (raceController != null)
                {
                    raceController.OnCheckpointReached(tribeId, checkpointIndex);
                }
                Debug.Log($"[CheckpointTrigger] Checkpoint {checkpointIndex} reached by {participantId}");
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
            
            // Draw checkpoint index
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, 
                isFinishLine ? "FINISH" : $"CP {checkpointIndex}");
            #endif
        }
    }
} 