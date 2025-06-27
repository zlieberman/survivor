using UnityEngine;
using Survivor.Characters;
using StarterAssets;

namespace Survivor.Challenges.Segments
{
    /// <summary>
    /// Debug version of the crawl segment to help troubleshoot issues
    /// </summary>
    public class DebugCrawlSegment : MonoBehaviour
    {
        [Header("Debug Settings")]
        [SerializeField] private bool showDebugInfo = true;
        [SerializeField] private LayerMask playerLayer = 1;
        [SerializeField] private float triggerDistance = 3f;
        
        private void Update()
        {
            if (!showDebugInfo) return;
            
            // Find all players in the area
            Collider[] playersInRange = Physics.OverlapSphere(transform.position, triggerDistance, playerLayer);
            
            Debug.Log($"[DebugCrawlSegment] Found {playersInRange.Length} objects in range");
            
            foreach (var playerCollider in playersInRange)
            {
                var player = playerCollider.GetComponent<ThirdPersonController>();
                if (player != null)
                {
                    Debug.Log($"[DebugCrawlSegment] Found player: {player.name}");
                    
                    // Check player position relative to net
                    Vector3 localPos = transform.InverseTransformPoint(player.transform.position);
                    Debug.Log($"[DebugCrawlSegment] Player local position: {localPos}");
                    
                    // Check if player is approaching
                    bool inFrontOfNet = localPos.z < -1f;
                    bool withinTriggerDistance = Mathf.Abs(localPos.x) <= triggerDistance;
                    bool atNetHeight = Mathf.Abs(localPos.y) <= 1f;
                    
                    Debug.Log($"[DebugCrawlSegment] In front of net: {inFrontOfNet}");
                    Debug.Log($"[DebugCrawlSegment] Within trigger distance: {withinTriggerDistance}");
                    Debug.Log($"[DebugCrawlSegment] At net height: {atNetHeight}");
                    
                    if (inFrontOfNet && withinTriggerDistance && atNetHeight)
                    {
                        Debug.Log($"[DebugCrawlSegment] PLAYER SHOULD GO PRONE NOW!");
                    }
                }
                else
                {
                    Debug.Log($"[DebugCrawlSegment] Object {playerCollider.name} has no ThirdPersonController");
                }
            }
        }
        
        private void OnDrawGizmos()
        {
            if (!showDebugInfo) return;
            
            // Draw trigger sphere
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, triggerDistance);
            
            // Draw net orientation
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, transform.forward * 5f);
            
            // Draw net boundaries
            Gizmos.color = Color.blue;
            Vector3 netStart = transform.position - transform.forward * 4f;
            Vector3 netEnd = transform.position + transform.forward * 4f;
            Gizmos.DrawWireSphere(netStart, 0.5f);
            Gizmos.DrawWireSphere(netEnd, 0.5f);
        }
    }
} 