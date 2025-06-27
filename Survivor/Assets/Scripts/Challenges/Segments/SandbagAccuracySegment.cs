using System.Collections.Generic;
using UnityEngine;
using Survivor.Characters;
using StarterAssets;

namespace Survivor.Challenges.Segments
{
    /// <summary>
    /// Challenge segment where players must throw sandbags at targets
    /// </summary>
    public class SandbagAccuracySegment : MonoBehaviour
    {
        [Header("Target Configuration")]
        [SerializeField] private GameObject[] targets;
        [SerializeField] private float targetHitRadius = 1f;
        [SerializeField] private LayerMask sandbagLayer = 1;
        
        [Header("Sandbag Configuration")]
        [SerializeField] private GameObject sandbagPrefab;
        [SerializeField] private Transform[] sandbagSpawnPoints;
        [SerializeField] private int maxSandbags = 10;
        [SerializeField] private float throwForce = 10f;
        
        [Header("Visual Feedback")]
        [SerializeField] private Material activeMaterial;
        [SerializeField] private Material hitMaterial;
        [SerializeField] private Material completedMaterial;
        
        [Header("Audio")]
        [SerializeField] private AudioClip targetHitSound;
        [SerializeField] private AudioClip segmentCompletedSound;
        
        private bool isCompleted = false;
        private List<bool> targetHitStatus = new List<bool>();
        private List<GameObject> activeSandbags = new List<GameObject>();
        private EnhancedRaceChallengeController raceController;
        private AudioSource audioSource;
        private string currentTribeId;
        private int sandbagsThrown = 0;
        
        private void Start()
        {
            // Find the race controller
            raceController = FindObjectOfType<EnhancedRaceChallengeController>();
            if (raceController == null)
            {
                Debug.LogWarning("[SandbagAccuracySegment] No EnhancedRaceChallengeController found!");
            }
            
            // Get or add audio source
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            // Initialize target status
            InitializeTargets();
            
            // Determine which tribe this segment belongs to based on position
            DetermineTribeId();
            
            // Spawn initial sandbags
            SpawnSandbags();
        }
        
        private void Update()
        {
            if (isCompleted) return;
            
            // Check for target hits
            CheckForTargetHits();
            
            // Check if all targets are hit
            if (AreAllTargetsHit())
            {
                CompleteSegment();
            }
        }
        
        private void InitializeTargets()
        {
            targetHitStatus.Clear();
            for (int i = 0; i < targets.Length; i++)
            {
                targetHitStatus.Add(false);
                
                // Set initial material
                var renderer = targets[i].GetComponent<Renderer>();
                if (renderer != null && activeMaterial != null)
                {
                    renderer.material = activeMaterial;
                }
            }
        }
        
        private void SpawnSandbags()
        {
            // Clear existing sandbags
            foreach (var sandbag in activeSandbags)
            {
                if (sandbag != null)
                {
                    Destroy(sandbag);
                }
            }
            activeSandbags.Clear();
            
            // Spawn new sandbags
            for (int i = 0; i < maxSandbags && i < sandbagSpawnPoints.Length; i++)
            {
                if (sandbagPrefab != null && sandbagSpawnPoints[i] != null)
                {
                    var sandbag = Instantiate(sandbagPrefab, sandbagSpawnPoints[i].position, sandbagSpawnPoints[i].rotation);
                    activeSandbags.Add(sandbag);
                    
                    // Add physics and make it throwable
                    var rb = sandbag.GetComponent<Rigidbody>();
                    if (rb == null)
                    {
                        rb = sandbag.AddComponent<Rigidbody>();
                    }
                    
                    // Add a script to handle throwing
                    var throwable = sandbag.GetComponent<ThrowableSandbag>();
                    if (throwable == null)
                    {
                        throwable = sandbag.AddComponent<ThrowableSandbag>();
                    }
                    throwable.Initialize(this, throwForce);
                }
            }
        }
        
        private void CheckForTargetHits()
        {
            // Check each target for nearby sandbags
            for (int i = 0; i < targets.Length; i++)
            {
                if (targetHitStatus[i]) continue; // Already hit
                
                if (IsTargetHit(targets[i]))
                {
                    HitTarget(i);
                }
            }
        }
        
        private bool IsTargetHit(GameObject target)
        {
            // Check for sandbags near the target
            Collider[] nearbySandbags = Physics.OverlapSphere(target.transform.position, targetHitRadius, sandbagLayer);
            
            foreach (var sandbagCollider in nearbySandbags)
            {
                // Check if the sandbag is moving slowly (has landed)
                var rb = sandbagCollider.GetComponent<Rigidbody>();
                if (rb != null && rb.velocity.magnitude < 0.5f)
                {
                    return true;
                }
            }
            
            return false;
        }
        
        private void HitTarget(int targetIndex)
        {
            if (targetHitStatus[targetIndex]) return;
            
            targetHitStatus[targetIndex] = true;
            
            // Update visual feedback
            var renderer = targets[targetIndex].GetComponent<Renderer>();
            if (renderer != null && hitMaterial != null)
            {
                renderer.material = hitMaterial;
            }
            
            // Play hit sound
            if (audioSource != null && targetHitSound != null)
            {
                audioSource.PlayOneShot(targetHitSound);
            }
            
            Debug.Log($"[SandbagAccuracySegment] Target {targetIndex} hit!");
        }
        
        private bool AreAllTargetsHit()
        {
            foreach (bool hit in targetHitStatus)
            {
                if (!hit) return false;
            }
            return true;
        }
        
        private void CompleteSegment()
        {
            if (isCompleted) return;
            
            isCompleted = true;
            
            // Update all targets to completed material
            for (int i = 0; i < targets.Length; i++)
            {
                var renderer = targets[i].GetComponent<Renderer>();
                if (renderer != null && completedMaterial != null)
                {
                    renderer.material = completedMaterial;
                }
            }
            
            // Play completion sound
            if (audioSource != null && segmentCompletedSound != null)
            {
                audioSource.PlayOneShot(segmentCompletedSound);
            }
            
            // Notify the race controller
            if (raceController != null)
            {
                int checkpointIndex = GetCheckpointIndex();
                raceController.OnChallengeSegmentCompleted(currentTribeId, checkpointIndex);
            }
            
            Debug.Log($"[SandbagAccuracySegment] Sandbag accuracy segment completed for tribe {currentTribeId}");
        }
        
        public void OnSandbagThrown()
        {
            sandbagsThrown++;
            
            // If we're running low on sandbags, spawn more
            if (activeSandbags.Count < maxSandbags / 2)
            {
                SpawnSandbags();
            }
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
            // For now, we'll assume it's checkpoint 3 (the final segment)
            return 3;
        }
        
        // Debug methods
        [ContextMenu("Test Complete Segment")]
        public void TestCompleteSegment()
        {
            // Mark all targets as hit
            for (int i = 0; i < targetHitStatus.Count; i++)
            {
                targetHitStatus[i] = true;
            }
            CompleteSegment();
        }
        
        [ContextMenu("Reset Segment")]
        public void ResetSegment()
        {
            isCompleted = false;
            sandbagsThrown = 0;
            InitializeTargets();
            SpawnSandbags();
        }
        
        private void OnDrawGizmosSelected()
        {
            // Draw target hit areas
            if (targets != null)
            {
                for (int i = 0; i < targets.Length; i++)
                {
                    if (targets[i] != null)
                    {
                        Gizmos.color = targetHitStatus != null && i < targetHitStatus.Count && targetHitStatus[i] ? Color.green : Color.red;
                        Gizmos.DrawWireSphere(targets[i].transform.position, targetHitRadius);
                    }
                }
            }
            
            // Draw sandbag spawn points
            if (sandbagSpawnPoints != null)
            {
                Gizmos.color = Color.blue;
                foreach (var spawnPoint in sandbagSpawnPoints)
                {
                    if (spawnPoint != null)
                    {
                        Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Component for throwable sandbags
    /// </summary>
    public class ThrowableSandbag : MonoBehaviour
    {
        private SandbagAccuracySegment segment;
        private float throwForce;
        private bool isThrown = false;
        
        public void Initialize(SandbagAccuracySegment segment, float throwForce)
        {
            this.segment = segment;
            this.throwForce = throwForce;
        }
        
        private void OnMouseDown()
        {
            if (isThrown) return;
            
            // Get mouse position in world space
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 10f; // Distance from camera
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            
            // Calculate throw direction
            Vector3 throwDirection = (worldPos - transform.position).normalized;
            
            // Apply force
            var rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
                isThrown = true;
                
                // Notify the segment
                if (segment != null)
                {
                    segment.OnSandbagThrown();
                }
            }
        }
    }
} 