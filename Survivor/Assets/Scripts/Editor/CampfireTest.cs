using UnityEngine;
using Survivor.Interactables;

namespace Survivor.Editor
{
    public class CampfireTest : MonoBehaviour
    {
        [Header("Test Settings")]
        public GameObject testLitCampfirePrefab;  // Assign this in the inspector for testing
        
        private CampfireInteractable campfireInteractable;
        
        private void Start()
        {
            // Find the campfire interactable in the scene
            campfireInteractable = FindObjectOfType<CampfireInteractable>();
            
            if (campfireInteractable == null)
            {
                Debug.LogWarning("[CampfireTest] No CampfireInteractable found in the scene!");
                return;
            }
            
            // Assign the test lit campfire prefab if provided
            if (testLitCampfirePrefab != null)
            {
                campfireInteractable.litCampfirePrefab = testLitCampfirePrefab;
                Debug.Log("[CampfireTest] Assigned test lit campfire prefab to campfire interactable");
            }
            else
            {
                Debug.LogWarning("[CampfireTest] No test lit campfire prefab assigned! The campfire won't be able to light.");
            }
            
            Debug.Log("[CampfireTest] Campfire test initialized successfully");
        }
        
        private void Update()
        {
            // Test interaction with key press (for debugging)
            if (Input.GetKeyDown(KeyCode.T) && campfireInteractable != null)
            {
                Debug.Log("[CampfireTest] Testing campfire interaction...");
                if (campfireInteractable.IsPlayerInRange)
                {
                    campfireInteractable.Interact();
                }
                else
                {
                    Debug.Log("[CampfireTest] Player not in range of campfire");
                }
            }
        }
        
        [ContextMenu("Test Campfire Interaction")]
        public void TestCampfireInteraction()
        {
            if (campfireInteractable != null)
            {
                Debug.Log("[CampfireTest] Testing campfire interaction via context menu...");
                campfireInteractable.Interact();
            }
            else
            {
                Debug.LogError("[CampfireTest] No campfire interactable found!");
            }
        }
    }
} 