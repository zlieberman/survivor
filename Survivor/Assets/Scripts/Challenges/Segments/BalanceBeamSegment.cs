using UnityEngine;
using Survivor.Characters;
using StarterAssets;

namespace Survivor.Challenges.Segments
{
    /// <summary>
    /// Challenge segment where players must walk across a balance beam
    /// </summary>
    public class BalanceBeamSegment : MonoBehaviour
    {
        [Header("Beam Configuration")]
        [SerializeField] private float beamLength = 10f;
        [SerializeField] private float beamWidth = 0.5f;
        [SerializeField] private float fallHeight = 2f;
        [SerializeField] private LayerMask playerLayer = 1;
        
        [Header("Visual Feedback")]
        [SerializeField] private Material activeMaterial;
        [SerializeField] private Material completedMaterial;
        [SerializeField] private GameObject beamVisual;
        
        [Header("Audio")]
        [SerializeField] private AudioClip beamCompletedSound;
        [SerializeField] private AudioClip fallSound;
        
        private bool isCompleted = false;
        private bool isPlayerOnBeam = false;
        private Vector3 beamStart;
        private Vector3 beamEnd;
        private EnhancedRaceChallengeController raceController;
        private Renderer beamRenderer;
        private AudioSource audioSource;
        private string currentTribeId;
        private ThirdPersonController currentPlayer;
        
        private void Start()
        {
            // Find the race controller
            raceController = FindObjectOfType<EnhancedRaceChallengeController>();
            if (raceController == null)
            {
                Debug.LogWarning("[BalanceBeamSegment] No EnhancedRaceChallengeController found!");
            }
            
            // Set up visual feedback
            if (beamVisual != null)
            {
                beamRenderer = beamVisual.GetComponent<Renderer>();
                if (beamRenderer != null && activeMaterial != null)
                {
                    beamRenderer.material = activeMaterial;
                }
            }
            
            // Get or add audio source
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            // Calculate beam start and end points
            CalculateBeamPoints();
            
            // Determine which tribe this segment belongs to based on position
            DetermineTribeId();
        }
        
        private void Update()
        {
            if (isCompleted) return;
            
            // Check if any player is on the beam
            CheckForPlayerOnBeam();
            
            // If a player is on the beam, check if they fall off
            if (isPlayerOnBeam && currentPlayer != null)
            {
                CheckForPlayerFall();
            }
        }
        
        private void CheckForPlayerOnBeam()
        {
            // Find all players in the area
            Collider[] playersInRange = Physics.OverlapSphere(transform.position, beamLength, playerLayer);
            
            foreach (var playerCollider in playersInRange)
            {
                var player = playerCollider.GetComponent<ThirdPersonController>();
                if (player != null)
                {
                    // Check if player is on the beam
                    if (IsPlayerOnBeam(player.transform.position))
                    {
                        if (!isPlayerOnBeam)
                        {
                            // Player just got on the beam
                            isPlayerOnBeam = true;
                            currentPlayer = player;
                            Debug.Log($"[BalanceBeamSegment] Player {player.name} started balance beam challenge");
                        }
                        return;
                    }
                }
            }
            
            // No player on beam
            if (isPlayerOnBeam)
            {
                isPlayerOnBeam = false;
                currentPlayer = null;
            }
        }
        
        private void CheckForPlayerFall()
        {
            if (currentPlayer == null) return;
            
            // Check if player has fallen below the beam
            float playerHeight = currentPlayer.transform.position.y;
            float beamHeight = transform.position.y;
            
            if (playerHeight < beamHeight - fallHeight)
            {
                // Player fell off the beam
                PlayerFellOff();
                return;
            }
            
            // Check if player has reached the end of the beam
            if (HasPlayerReachedEnd(currentPlayer.transform.position))
            {
                CompleteSegment();
            }
        }
        
        private bool IsPlayerOnBeam(Vector3 playerPosition)
        {
            // Check if player is within the beam area
            Vector3 localPos = transform.InverseTransformPoint(playerPosition);
            
            // Check if player is within beam width and length
            bool withinWidth = Mathf.Abs(localPos.x) <= beamWidth / 2f;
            bool withinLength = Mathf.Abs(localPos.z) <= beamLength / 2f;
            bool onBeamHeight = Mathf.Abs(localPos.y) <= 0.5f; // Within 0.5 units of beam height
            
            return withinWidth && withinLength && onBeamHeight;
        }
        
        private bool HasPlayerReachedEnd(Vector3 playerPosition)
        {
            // Check if player has reached the end of the beam
            Vector3 localPos = transform.InverseTransformPoint(playerPosition);
            
            // Player has reached the end if they're near the end of the beam
            return localPos.z >= beamLength / 2f - 1f; // Within 1 unit of the end
        }
        
        private void PlayerFellOff()
        {
            Debug.Log($"[BalanceBeamSegment] Player {currentPlayer.name} fell off the beam");
            
            // Play fall sound
            if (audioSource != null && fallSound != null)
            {
                audioSource.PlayOneShot(fallSound);
            }
            
            // Reset beam state
            isPlayerOnBeam = false;
            currentPlayer = null;
            
            // You could add a penalty here, like forcing the player to restart the segment
        }
        
        private void CompleteSegment()
        {
            if (isCompleted) return;
            
            isCompleted = true;
            
            // Update visual feedback
            if (beamRenderer != null && completedMaterial != null)
            {
                beamRenderer.material = completedMaterial;
            }
            
            // Play completion sound
            if (audioSource != null && beamCompletedSound != null)
            {
                audioSource.PlayOneShot(beamCompletedSound);
            }
            
            // Notify the race controller
            if (raceController != null)
            {
                int checkpointIndex = GetCheckpointIndex();
                raceController.OnChallengeSegmentCompleted(currentTribeId, checkpointIndex);
            }
            
            Debug.Log($"[BalanceBeamSegment] Balance beam segment completed for tribe {currentTribeId}");
        }
        
        private void CalculateBeamPoints()
        {
            // Calculate the start and end points of the beam
            beamStart = transform.position - transform.forward * (beamLength / 2f);
            beamEnd = transform.position + transform.forward * (beamLength / 2f);
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
            // For now, we'll assume it's checkpoint 2 (the second segment)
            return 2;
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
            isPlayerOnBeam = false;
            currentPlayer = null;
            
            if (beamRenderer != null && activeMaterial != null)
            {
                beamRenderer.material = activeMaterial;
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            // Draw the beam
            Gizmos.color = isCompleted ? Color.green : Color.yellow;
            Gizmos.DrawWireCube(transform.position, new Vector3(beamWidth, 0.2f, beamLength));
            
            // Draw the fall area
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position - Vector3.up * fallHeight, new Vector3(beamWidth * 2, 0.1f, beamLength));
            
            // Draw start and end points
            if (Application.isPlaying)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(beamStart, 0.5f);
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(beamEnd, 0.5f);
            }
        }
    }
} 