using UnityEngine;
using Survivor.Interactables;
using Survivor.UI;

namespace Survivor.Editor
{
    public class TentSystemTest : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private bool runTestsOnStart = false;
        
        private TentInteractable tentInteractable;
        private MonoBehaviour timeManagerInstance;

        private void Start()
        {
            if (runTestsOnStart)
            {
                RunTests();
            }
        }

        [ContextMenu("Run Tent System Tests")]
        public void RunTests()
        {
            Debug.Log("=== TENT SYSTEM TESTS ===");
            
            TestTimeManagerAccess();
            TestTentInteractableCreation();
            TestRestPanelUI();
            
            Debug.Log("=== TENT SYSTEM TESTS COMPLETE ===");
        }

        private void TestTimeManagerAccess()
        {
            Debug.Log("Testing TimeManager access...");
            
            // Find TimeManager using reflection
            var timeManagers = FindObjectsOfType<MonoBehaviour>();
            timeManagerInstance = null;
            
            foreach (var mb in timeManagers)
            {
                if (mb.GetType().Name == "TimeManager")
                {
                    timeManagerInstance = mb;
                    break;
                }
            }
            
            if (timeManagerInstance != null)
            {
                Debug.Log("✅ TimeManager found via reflection");
                
                // Test AdvanceTime method access
                var advanceTimeMethod = timeManagerInstance.GetType().GetMethod("AdvanceTime");
                if (advanceTimeMethod != null)
                {
                    Debug.Log("✅ AdvanceTime method found on TimeManager");
                }
                else
                {
                    Debug.LogError("❌ AdvanceTime method not found on TimeManager");
                }
            }
            else
            {
                Debug.LogError("❌ TimeManager not found in scene");
            }
        }

        private void TestTentInteractableCreation()
        {
            Debug.Log("Testing TentInteractable creation...");
            
            // Create a test tent
            GameObject testTent = new GameObject("TestTent");
            tentInteractable = testTent.AddComponent<TentInteractable>();
            
            if (tentInteractable != null)
            {
                Debug.Log("✅ TentInteractable component created successfully");
                
                // Test component properties
                Debug.Log($"✅ Interaction radius: {tentInteractable.interactionRadius}");
                
                // Clean up
                DestroyImmediate(testTent);
            }
            else
            {
                Debug.LogError("❌ Failed to create TentInteractable component");
            }
        }

        private void TestRestPanelUI()
        {
            Debug.Log("Testing RestPanelUI...");
            
            // Find the RestPanelUI in the scene
            var restPanelUI = FindObjectOfType<RestPanelUI>();
            
            if (restPanelUI != null)
            {
                Debug.Log("✅ RestPanelUI found in scene");
                Debug.Log($"✅ RestPanelUI GameObject: {restPanelUI.gameObject.name}");
                Debug.Log($"✅ RestPanelUI active: {restPanelUI.gameObject.activeInHierarchy}");
                
                // Test initialization and show
                restPanelUI.Initialize(5f, 12f, 
                    (hours) => Debug.Log($"Sleep callback: {hours} hours"), 
                    () => Debug.Log("Cancel callback"));
                
                restPanelUI.Show();
                Debug.Log("✅ RestPanelUI shown successfully");
            }
            else
            {
                Debug.LogError("❌ RestPanelUI not found in scene");
            }
        }

        [ContextMenu("Test Time Advancement")]
        public void TestTimeAdvancement()
        {
            if (timeManagerInstance == null)
            {
                Debug.LogError("TimeManager not found - run tests first");
                return;
            }
            
            Debug.Log("Testing time advancement...");
            
            var advanceTimeMethod = timeManagerInstance.GetType().GetMethod("AdvanceTime");
            if (advanceTimeMethod != null)
            {
                advanceTimeMethod.Invoke(timeManagerInstance, new object[] { 1f });
                Debug.Log("✅ Successfully advanced time by 1 hour");
            }
            else
            {
                Debug.LogError("❌ AdvanceTime method not found");
            }
        }

        [ContextMenu("Show Current Status")]
        public void ShowCurrentStatus()
        {
            Debug.Log("=== TENT SYSTEM STATUS ===");
            
            if (timeManagerInstance != null)
            {
                Debug.Log("✅ TimeManager: Found");
                
                // Get current time using reflection
                var elapsedTimeProperty = timeManagerInstance.GetType().GetProperty("ElapsedRealTime");
                if (elapsedTimeProperty != null)
                {
                    float elapsedTime = (float)elapsedTimeProperty.GetValue(timeManagerInstance);
                    Debug.Log($"✅ Elapsed time: {elapsedTime:F1}s");
                }
            }
            else
            {
                Debug.Log("❌ TimeManager: Not found");
            }
            
            // Check for tents in scene
            var tents = FindObjectsOfType<TentInteractable>();
            Debug.Log($"✅ Tents in scene: {tents.Length}");
            
            // Check for rest panels
            var restPanels = FindObjectsOfType<RestPanelUI>();
            Debug.Log($"✅ Rest panels in scene: {restPanels.Length}");
            
            Debug.Log("=== STATUS COMPLETE ===");
        }

        [ContextMenu("Create Test Tent")]
        public void CreateTestTent()
        {
            Debug.Log("Creating test tent...");
            
            // Create a simple tent GameObject
            GameObject testTent = GameObject.CreatePrimitive(PrimitiveType.Cube);
            testTent.name = "TestTent";
            testTent.transform.position = new Vector3(0, 1, 0);
            testTent.transform.localScale = new Vector3(2, 2, 3);
            
            // Add TentInteractable component
            var tentInteractable = testTent.AddComponent<TentInteractable>();
            
            // Set the layer to Interactable
            testTent.layer = LayerMask.NameToLayer("Interactable");
            
            Debug.Log("✅ Test tent created with TentInteractable component");
            Debug.Log($"✅ Tent position: {testTent.transform.position}");
            Debug.Log($"✅ Tent layer: {LayerMask.LayerToName(testTent.layer)}");
        }

        [ContextMenu("Test Tent Interaction System")]
        public void TestTentInteractionSystem()
        {
            Debug.Log("Testing tent interaction system...");
            
            // Find all tents in the scene
            var tents = FindObjectsOfType<TentInteractable>();
            
            if (tents.Length > 0)
            {
                Debug.Log($"✅ Found {tents.Length} tent(s) in scene");
                
                foreach (var tent in tents)
                {
                    Debug.Log($"✅ Tent: {tent.gameObject.name}");
                    Debug.Log($"  - Position: {tent.transform.position}");
                    Debug.Log($"  - Layer: {LayerMask.LayerToName(tent.gameObject.layer)}");
                    Debug.Log($"  - Interaction Radius: {tent.interactionRadius}");
                    Debug.Log($"  - Player In Range: {tent.IsPlayerInRange}");
                    Debug.Log($"  - Can Interact: {tent.CanInteractNow}");
                    
                    // Test the ShowRestPanel method
                    Debug.Log("Testing ShowRestPanel method...");
                    tent.ShowRestPanel();
                }
            }
            else
            {
                Debug.LogWarning("⚠️ No tents found in scene. Creating a test tent...");
                CreateTestTent();
            }
        }

        [ContextMenu("Test R Key Input")]
        public void TestRKeyInput()
        {
            Debug.Log("Testing R key input...");
            
            // Find the player
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Debug.Log($"✅ Player found: {player.name}");
                Debug.Log($"✅ Player position: {player.transform.position}");
                
                // Find tents near the player
                var tents = FindObjectsOfType<TentInteractable>();
                var nearbyTents = new System.Collections.Generic.List<TentInteractable>();
                
                foreach (var tent in tents)
                {
                    float distance = Vector3.Distance(player.transform.position, tent.transform.position);
                    Debug.Log($"Tent {tent.name} distance: {distance}");
                    
                    if (distance <= tent.interactionRadius)
                    {
                        nearbyTents.Add(tent);
                        Debug.Log($"✅ Tent {tent.name} is within interaction range");
                    }
                }
                
                if (nearbyTents.Count > 0)
                {
                    Debug.Log($"✅ Found {nearbyTents.Count} tent(s) within interaction range");
                    Debug.Log("Press R key near a tent to test interaction");
                }
                else
                {
                    Debug.LogWarning("⚠️ No tents within interaction range of player");
                }
            }
            else
            {
                Debug.LogError("❌ Player not found with 'Player' tag");
            }
        }

        [ContextMenu("Debug RestPanelUI Finding")]
        public void DebugRestPanelUIFinding()
        {
            Debug.Log("=== DEBUGGING REST PANEL UI FINDING ===");
            
            // Method 1: Find by name
            GameObject restPanelObj = GameObject.Find("RestPanel");
            if (restPanelObj != null)
            {
                Debug.Log($"✅ Found RestPanel by name: {restPanelObj.name}");
                var restPanelUI = restPanelObj.GetComponent<RestPanelUI>();
                if (restPanelUI != null)
                {
                    Debug.Log("✅ RestPanelUI component found on RestPanel GameObject");
                }
                else
                {
                    Debug.LogError("❌ RestPanelUI component missing on RestPanel GameObject");
                }
            }
            else
            {
                Debug.LogWarning("⚠️ RestPanel not found by name");
            }
            
            // Method 2: Find by component
            var allRestPanels = FindObjectsOfType<RestPanelUI>();
            Debug.Log($"✅ Found {allRestPanels.Length} RestPanelUI components in scene");
            
            foreach (var panel in allRestPanels)
            {
                Debug.Log($"  - {panel.gameObject.name} (Active: {panel.gameObject.activeInHierarchy})");
            }
            
            Debug.Log("=== DEBUG COMPLETE ===");
        }
        
        [ContextMenu("Test Time Advancement from Tent")]
        public void TestTimeAdvancementFromTent()
        {
            Debug.Log("=== TESTING TIME ADVANCEMENT FROM TENT ===");
            
            // Find a tent in the scene
            var tents = FindObjectsOfType<TentInteractable>();
            if (tents.Length == 0)
            {
                Debug.LogError("❌ No tents found in scene - create a test tent first");
                return;
            }
            
            var tent = tents[0];
            Debug.Log($"✅ Found tent: {tent.name}");
            
            // Get current time before advancement
            if (timeManagerInstance != null)
            {
                var elapsedTimeProperty = timeManagerInstance.GetType().GetProperty("ElapsedRealTime");
                if (elapsedTimeProperty != null)
                {
                    float elapsedTimeBefore = (float)elapsedTimeProperty.GetValue(timeManagerInstance);
                    Debug.Log($"✅ Current elapsed time: {elapsedTimeBefore:F1}s");
                    
                    // Simulate time advancement by calling TriggerTimeAdvancement directly
                    var triggerMethod = tent.GetType().GetMethod("TriggerTimeAdvancement", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                    if (triggerMethod != null)
                    {
                        Debug.Log("✅ TriggerTimeAdvancement method found - testing with 2 hours");
                        triggerMethod.Invoke(tent, new object[] { 2f });
                        
                        // Check if time was advanced
                        float elapsedTimeAfter = (float)elapsedTimeProperty.GetValue(timeManagerInstance);
                        Debug.Log($"✅ New elapsed time: {elapsedTimeAfter:F1}s");
                        Debug.Log($"✅ Time advanced by: {elapsedTimeAfter - elapsedTimeBefore:F1}s");
                        
                        if (elapsedTimeAfter > elapsedTimeBefore)
                        {
                            Debug.Log("✅ SUCCESS: Time advancement is working!");
                        }
                        else
                        {
                            Debug.LogError("❌ FAILED: Time was not advanced");
                        }
                    }
                    else
                    {
                        Debug.LogError("❌ TriggerTimeAdvancement method not found");
                    }
                }
            }
            else
            {
                Debug.LogError("❌ TimeManager not found");
            }
            
            Debug.Log("=== TIME ADVANCEMENT TEST COMPLETE ===");
        }
    }
} 