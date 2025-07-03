using UnityEngine;
using StarterAssets;

namespace Survivor.Characters.Player
{
    /// <summary>
    /// Test script to verify crawling collision adjustment is working properly
    /// </summary>
    public class CrawlCollisionTest : MonoBehaviour
    {
        [Header("Debug Settings")]
        [SerializeField] private bool showDebugInfo = true;
        [SerializeField] private bool showCollisionBox = true;
        
        private ThirdPersonController thirdPersonController;
        private CharacterController characterController;
        
        private void Start()
        {
            thirdPersonController = GetComponent<ThirdPersonController>();
            characterController = GetComponent<CharacterController>();
            
            if (thirdPersonController == null)
            {
                Debug.LogError("[CrawlCollisionTest] No ThirdPersonController found on this GameObject!");
            }
            
            if (characterController == null)
            {
                Debug.LogError("[CrawlCollisionTest] No CharacterController found on this GameObject!");
            }
        }
        
        private void Update()
        {
            if (!showDebugInfo) return;
            
            if (thirdPersonController != null && characterController != null)
            {
                bool isCrawling = thirdPersonController.IsCrawling;
                float currentCrawlSpeed = thirdPersonController.CurrentCrawlSpeed;
                
                Debug.Log($"[CrawlCollisionTest] Is Crawling: {isCrawling}, Crawl Speed: {currentCrawlSpeed:F2}");
                Debug.Log($"[CrawlCollisionTest] Character Controller - Height: {characterController.height:F2}, Radius: {characterController.radius:F2}, Center: {characterController.center}");
                
                // Show collision box in scene view
                if (showCollisionBox)
                {
                    DrawCollisionBox();
                }
            }
        }
        
        private void DrawCollisionBox()
        {
            if (characterController == null) return;
            
            Vector3 center = transform.position + characterController.center;
            Vector3 size = new Vector3(characterController.radius * 2, characterController.height, characterController.radius * 2);
            
            // Draw the collision box
            Color boxColor = thirdPersonController.IsCrawling ? Color.red : Color.green;
            Debug.DrawLine(center + Vector3.up * characterController.height / 2, center - Vector3.up * characterController.height / 2, boxColor);
            Debug.DrawLine(center + Vector3.right * characterController.radius, center - Vector3.right * characterController.radius, boxColor);
            Debug.DrawLine(center + Vector3.forward * characterController.radius, center - Vector3.forward * characterController.radius, boxColor);
        }
        
        private void OnDrawGizmos()
        {
            if (!showCollisionBox || characterController == null) return;
            
            Vector3 center = transform.position + characterController.center;
            Vector3 size = new Vector3(characterController.radius * 2, characterController.height, characterController.radius * 2);
            
            // Draw wireframe box
            Color boxColor = (thirdPersonController != null && thirdPersonController.IsCrawling) ? Color.red : Color.green;
            Gizmos.color = boxColor;
            Gizmos.DrawWireCube(center, size);
            
            // Draw center point
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(center, 0.1f);
        }
    }
} 