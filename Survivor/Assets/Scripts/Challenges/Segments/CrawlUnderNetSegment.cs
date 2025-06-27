using UnityEngine;
using Survivor.Characters;
using StarterAssets;

namespace Survivor.Challenges.Segments
{
    /// <summary>
    /// Challenge segment where players must crawl under a net
    /// </summary>
    public class CrawlUnderNetSegment : MonoBehaviour
    {
        [Header("Net Configuration")]
        [SerializeField] private float netHeight = 1.5f;
        [SerializeField] private float triggerDistance = 2f;
        [SerializeField] private LayerMask playerLayer = 1;
        
        [Header("Visual Feedback")]
        [SerializeField] private Material activeMaterial;
        [SerializeField] private Material completedMaterial;
        [SerializeField] private GameObject netVisual;
        
        [Header("Audio")]
        [SerializeField] private AudioClip netCrossedSound;
        
        private bool isCompleted = false;
        private EnhancedRaceChallengeController raceController;
        private Renderer netRenderer;
        private AudioSource audioSource;
        private string currentTribeId;
        
        private void Start()
        {
            // Find the race controller
            raceController = FindObjectOfType<EnhancedRaceChallengeController>();
            if (raceController == null)
            {
                Debug.LogWarning("[CrawlUnderNetSegment] No EnhancedRaceChallengeController found!");
            }
            
            // Set up visual feedback
            if (netVisual != null)
            {
                netRenderer = netVisual.GetComponent<Renderer>();
                if (netRenderer != null && activeMaterial != null)
                {
                    netRenderer.material = activeMaterial;
                }
            }
            
            // Get or add audio source
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            // Determine which tribe this segment belongs to based on position
            DetermineTribeId();
        }
        
        private void Update()
        {
            if (isCompleted) return;
            
            // Check if any player is under the net
            CheckForPlayerUnderNet();
        }
        
        private void CheckForPlayerUnderNet()
        {
            // Find all players in the area
            Collider[] playersInRange = Physics.OverlapSphere(transform.position, triggerDistance, playerLayer);
            
            foreach (var playerCollider in playersInRange)
            {
                var player = playerCollider.GetComponent<ThirdPersonController>();
                if (player != null)
                {
                    // Check if player is below the net height
                    float playerHeight = player.transform.position.y;
                    float netBottomHeight = transform.position.y;
                    
                    if (playerHeight < netBottomHeight + netHeight)
                    {
                        // Player is under the net - check if they're actually crawling
                        if (IsPlayerCrawling(player))
                        {
                            CompleteSegment();
                            break;
                        }
                    }
                }
            }
        }
        
        private bool IsPlayerCrawling(ThirdPersonController player)
        {
            // This is a simplified check - in a real implementation, you'd check the player's animation state
            // For now, we'll assume if they're under the net and moving slowly, they're crawling
            var input = player.GetComponent<StarterAssetsInputs>();
            if (input != null)
            {
                // Check if player is moving slowly (crawling speed)
                Vector2 moveInput = input.move;
                float inputMagnitude = moveInput.magnitude;
                
                // If player is moving slowly under the net, consider it crawling
                return inputMagnitude > 0.1f && inputMagnitude < 0.5f;
            }
            
            return false;
        }
        
        private void CompleteSegment()
        {
            if (isCompleted) return;
            
            isCompleted = true;
            
            // Update visual feedback
            if (netRenderer != null && completedMaterial != null)
            {
                netRenderer.material = completedMaterial;
            }
            
            // Play sound effect
            if (audioSource != null && netCrossedSound != null)
            {
                audioSource.PlayOneShot(netCrossedSound);
            }
            
            // Notify the race controller
            if (raceController != null)
            {
                // Find the checkpoint index for this segment
                int checkpointIndex = GetCheckpointIndex();
                raceController.OnChallengeSegmentCompleted(currentTribeId, checkpointIndex);
            }
            
            Debug.Log($"[CrawlUnderNetSegment] Net crawling segment completed for tribe {currentTribeId}");
        }
        
        private void DetermineTribeId()
        {
            // Determine which tribe this segment belongs to based on position
            // Assuming Tribe A is on the left (negative X) and Tribe B is on the right (positive X)
            if (transform.position.x < 0)
            {
                currentTribeId = "tribe_1"; // or "TribeA"
            }
            else
            {
                currentTribeId = "tribe_2"; // or "TribeB"
            }
        }
        
        private int GetCheckpointIndex()
        {
            // This should return the checkpoint index where this segment is located
            // You can either set this in the inspector or determine it based on position
            // For now, we'll assume it's checkpoint 1 (the first segment)
            return 1;
        }
        
        // Debug methods
        [ContextMenu("Test Complete Segment")]
        public void TestCompleteSegment()
        {
            CompleteSegment();
        }
        
        [ContextMenu("Reset Segment")]
        public void ResetSegment()
        {
            isCompleted = false;
            
            if (netRenderer != null && activeMaterial != null)
            {
                netRenderer.material = activeMaterial;
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            // Draw the net area
            Gizmos.color = isCompleted ? Color.green : Color.yellow;
            Gizmos.DrawWireCube(transform.position, new Vector3(triggerDistance * 2, netHeight, triggerDistance * 2));
            
            // Draw trigger distance
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, triggerDistance);
        }
    }
} 