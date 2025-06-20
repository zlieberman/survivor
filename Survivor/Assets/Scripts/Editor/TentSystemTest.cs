using UnityEngine;
using Survivor.Interactables;

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
            
            // Create a test UI panel
            GameObject testPanel = new GameObject("TestRestPanel");
            var restPanelUI = testPanel.AddComponent<Survivor.UI.RestPanelUI>();
            
            if (restPanelUI != null)
            {
                Debug.Log("✅ RestPanelUI component created successfully");
                
                // Test initialization
                restPanelUI.Initialize(5f, 12f, 
                    (hours) => Debug.Log($"Sleep callback: {hours} hours"), 
                    () => Debug.Log("Cancel callback"));
                
                Debug.Log("✅ RestPanelUI initialized successfully");
                
                // Clean up
                DestroyImmediate(testPanel);
            }
            else
            {
                Debug.LogError("❌ Failed to create RestPanelUI component");
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
            var restPanels = FindObjectsOfType<Survivor.UI.RestPanelUI>();
            Debug.Log($"✅ Rest panels in scene: {restPanels.Length}");
            
            Debug.Log("=== STATUS COMPLETE ===");
        }
    }
} 