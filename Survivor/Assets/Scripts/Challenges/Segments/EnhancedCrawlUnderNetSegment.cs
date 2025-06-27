using UnityEngine;
using Survivor.Characters;
using StarterAssets;

namespace Survivor.Challenges.Segments
{
    /// <summary>
    /// Enhanced challenge segment where players automatically go prone to crawl under a net
    /// </summary>
    public class EnhancedCrawlUnderNetSegment : MonoBehaviour
    {
        [Header("Net Configuration")]
        [SerializeField] private float netHeight = 1.5f;
        [SerializeField] private float netLength = 8f;
        [SerializeField] private float triggerDistance = 2f;
        [SerializeField] private LayerMask playerLayer = 1;
        
        [Header("Crawling Configuration")]
        [SerializeField] private float crawlSpeed = 1.5f;
        [SerializeField] private float proneHeight = 0.8f;
        [SerializeField] private float standUpDelay = 0.5f;
        
        [Header("Visual Feedback")]
        [SerializeField] private Material activeMaterial;
        [SerializeField] private Material completedMaterial;
        [SerializeField] private GameObject netVisual;
        
        [Header("Audio")]
        [SerializeField] private AudioClip netCrossedSound;
        [SerializeField] private AudioClip goProneSound;
        [SerializeField] private AudioClip standUpSound;
        
        private bool isCompleted = false;
        private bool isPlayerCrawling = false;
        private EnhancedRaceChallengeController raceController;
        private Renderer netRenderer;
        private AudioSource audioSource;
        private string currentTribeId;
        private ThirdPersonController currentPlayer;
        private CharacterController characterController;
        private Animator playerAnimator;
        private StarterAssetsInputs playerInput;
        
        // Crawling state
        private bool isProne = false;
        private float originalMoveSpeed;
        private float originalSprintSpeed;
        private float originalCharacterHeight;
        private Vector3 originalCharacterCenter;
        private float standUpTimer = 0f;
        
        // Animation IDs
        private int animIDSleep; // Using Sleep parameter for prone state
        
        private void Start()
        {
            // Find the race controller
            raceController = FindObjectOfType<EnhancedRaceChallengeController>();
            if (raceController == null)
            {
                Debug.LogWarning("[EnhancedCrawlUnderNetSegment] No EnhancedRaceChallengeController found!");
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
            
            // Get animation ID
            animIDSleep = Animator.StringToHash("Sleep");
        }
        
        private void Update()
        {
            if (isCompleted) return;
            
            // Check if any player is near the net
            CheckForPlayerNearNet();
            
            // Handle crawling state
            if (isPlayerCrawling && currentPlayer != null)
            {
                HandleCrawlingState();
            }
            
            // Handle stand up timer
            if (standUpTimer > 0f)
            {
                standUpTimer -= Time.deltaTime;
                if (standUpTimer <= 0f)
                {
                    StandPlayerUp();
                }
            }
        }
        
        private void CheckForPlayerNearNet()
        {
            // Find all players in the area
            Collider[] playersInRange = Physics.OverlapSphere(transform.position, triggerDistance, playerLayer);
            
            foreach (var playerCollider in playersInRange)
            {
                var player = playerCollider.GetComponent<ThirdPersonController>();
                if (player != null)
                {
                    // Check if player is approaching the net from the front
                    if (IsPlayerApproachingNet(player.transform.position))
                    {
                        if (!isPlayerCrawling)
                        {
                            // Player just approached the net - start crawling sequence
                            StartCrawlingSequence(player);
                        }
                        return;
                    }
                }
            }
            
            // No player near net
            if (isPlayerCrawling && !isProne)
            {
                isPlayerCrawling = false;
                currentPlayer = null;
            }
        }
        
        private bool IsPlayerApproachingNet(Vector3 playerPosition)
        {
            // Check if player is in front of the net (approaching from the start side)
            Vector3 localPos = transform.InverseTransformPoint(playerPosition);
            
            // Player is approaching if they're in front of the net and within trigger distance
            bool inFrontOfNet = localPos.z < -1f; // In front of the net
            bool withinTriggerDistance = Mathf.Abs(localPos.x) <= triggerDistance;
            bool atNetHeight = Mathf.Abs(localPos.y) <= 1f; // Near the net height
            
            return inFrontOfNet && withinTriggerDistance && atNetHeight;
        }
        
        private void StartCrawlingSequence(ThirdPersonController player)
        {
            isPlayerCrawling = true;
            currentPlayer = player;
            
            // Get player components
            characterController = player.GetComponent<CharacterController>();
            playerAnimator = player.GetComponent<Animator>();
            playerInput = player.GetComponent<StarterAssetsInputs>();
            
            if (characterController != null && playerAnimator != null)
            {
                // Store original values
                originalMoveSpeed = player.MoveSpeed;
                originalSprintSpeed = player.SprintSpeed;
                originalCharacterHeight = characterController.height;
                originalCharacterCenter = characterController.center;
                
                // Go prone
                GoProne();
                
                Debug.Log($"[EnhancedCrawlUnderNetSegment] Started crawling sequence for {player.name}");
            }
        }
        
        private void GoProne()
        {
            if (isProne) return;
            
            isProne = true;
            
            // Set crawling speed
            if (currentPlayer != null)
            {
                currentPlayer.MoveSpeed = crawlSpeed;
                currentPlayer.SprintSpeed = crawlSpeed;
            }
            
            // Adjust character controller for prone position
            if (characterController != null)
            {
                characterController.height = proneHeight;
                characterController.center = new Vector3(0, proneHeight / 2f, 0);
            }
            
            // Set animation to prone state (using Sleep parameter)
            if (playerAnimator != null)
            {
                playerAnimator.SetBool(animIDSleep, true);
            }
            
            // Play go prone sound
            if (audioSource != null && goProneSound != null)
            {
                audioSource.PlayOneShot(goProneSound);
            }
            
            Debug.Log($"[EnhancedCrawlUnderNetSegment] Player went prone");
        }
        
        private void HandleCrawlingState()
        {
            if (!isProne) return;
            
            // Check if player has reached the other side of the net
            if (HasPlayerReachedOtherSide(currentPlayer.transform.position))
            {
                // Start stand up timer
                standUpTimer = standUpDelay;
                
                // Complete the segment
                CompleteSegment();
            }
        }
        
        private bool HasPlayerReachedOtherSide(Vector3 playerPosition)
        {
            // Check if player has passed through the net
            Vector3 localPos = transform.InverseTransformPoint(playerPosition);
            
            // Player has reached the other side if they're past the net
            return localPos.z > netLength / 2f + 1f; // Past the net with some buffer
        }
        
        private void StandPlayerUp()
        {
            if (!isProne) return;
            
            isProne = false;
            
            // Restore original speed
            if (currentPlayer != null)
            {
                currentPlayer.MoveSpeed = originalMoveSpeed;
                currentPlayer.SprintSpeed = originalSprintSpeed;
            }
            
            // Restore character controller
            if (characterController != null)
            {
                characterController.height = originalCharacterHeight;
                characterController.center = originalCharacterCenter;
            }
            
            // Set animation back to normal
            if (playerAnimator != null)
            {
                playerAnimator.SetBool(animIDSleep, false);
            }
            
            // Play stand up sound
            if (audioSource != null && standUpSound != null)
            {
                audioSource.PlayOneShot(standUpSound);
            }
            
            // Reset crawling state
            isPlayerCrawling = false;
            currentPlayer = null;
            
            Debug.Log($"[EnhancedCrawlUnderNetSegment] Player stood up");
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
            
            // Play completion sound
            if (audioSource != null && netCrossedSound != null)
            {
                audioSource.PlayOneShot(netCrossedSound);
            }
            
            // Notify the race controller
            if (raceController != null)
            {
                int checkpointIndex = GetCheckpointIndex();
                raceController.OnChallengeSegmentCompleted(currentTribeId, checkpointIndex);
            }
            
            Debug.Log($"[EnhancedCrawlUnderNetSegment] Net crawling segment completed for tribe {currentTribeId}");
        }
        
        private void DetermineTribeId()
        {
            // Determine which tribe this segment belongs to based on position
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
            isPlayerCrawling = false;
            isProne = false;
            standUpTimer = 0f;
            
            // Stand up any current player
            if (currentPlayer != null && isProne)
            {
                StandPlayerUp();
            }
            
            if (netRenderer != null && activeMaterial != null)
            {
                netRenderer.material = activeMaterial;
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            // Draw the net area
            Gizmos.color = isCompleted ? Color.green : Color.yellow;
            Gizmos.DrawWireCube(transform.position, new Vector3(triggerDistance * 2, netHeight, netLength));
            
            // Draw trigger distance
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, triggerDistance);
            
            // Draw net boundaries
            Gizmos.color = Color.red;
            Vector3 netStart = transform.position - transform.forward * (netLength / 2f);
            Vector3 netEnd = transform.position + transform.forward * (netLength / 2f);
            Gizmos.DrawWireSphere(netStart, 0.5f);
            Gizmos.DrawWireSphere(netEnd, 0.5f);
        }
    }
} 