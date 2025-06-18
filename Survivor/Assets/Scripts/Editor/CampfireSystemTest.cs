using UnityEngine;
using Survivor.Interactables;
using Survivor.Shared;

namespace Survivor.Editor
{
    public class CampfireSystemTest : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private KeyCode testHourKey = KeyCode.T;
        [SerializeField] private KeyCode addWoodKey = KeyCode.W;
        [SerializeField] private KeyCode lightFireKey = KeyCode.L;
        [SerializeField] private KeyCode showStatusKey = KeyCode.S;
        
        private CampfireInteractable campfireInteractable;
        private IInventory playerInventory;
        private MonoBehaviour timeManagerInstance;
        
        private void Start()
        {
            Debug.Log("[CampfireSystemTest] Initializing campfire system test...");
            
            // Find the campfire interactable in the scene
            campfireInteractable = FindObjectOfType<CampfireInteractable>();
            
            if (campfireInteractable == null)
            {
                Debug.LogError("[CampfireSystemTest] No CampfireInteractable found in the scene!");
                return;
            }
            
            // Find the player inventory
            FindPlayerInventory();
            
            // Find TimeManager using reflection
            FindTimeManager();
            
            // Check if InteractableManager exists
            if (InteractableManager.Instance == null)
            {
                Debug.LogError("[CampfireSystemTest] InteractableManager.Instance is null! This will prevent campfire wood from burning.");
            }
            else
            {
                Debug.Log($"[CampfireSystemTest] InteractableManager found with {InteractableManager.Instance.GetListenerCount()} registered listeners");
            }
            
            // Check if TimeManager exists
            if (timeManagerInstance == null)
            {
                Debug.LogError("[CampfireSystemTest] TimeManager.Instance is null! This will prevent game hours from passing.");
            }
            else
            {
                Debug.Log("[CampfireSystemTest] TimeManager found and active");
            }
            
            Debug.Log("[CampfireSystemTest] Test controls:");
            Debug.Log("[CampfireSystemTest] T - Test game hour passed");
            Debug.Log("[CampfireSystemTest] W - Add 5 wood to player");
            Debug.Log("[CampfireSystemTest] L - Light campfire (if enough wood)");
            Debug.Log("[CampfireSystemTest] S - Show status");
        }
        
        private void FindPlayerInventory()
        {
            // Find all objects with IInventory interface
            var allMonoBehaviours = FindObjectsOfType<MonoBehaviour>();
            foreach (var mb in allMonoBehaviours)
            {
                if (mb is IInventory inventory)
                {
                    // Check if this is the player (has "Player" tag)
                    if (mb.CompareTag("Player"))
                    {
                        playerInventory = inventory;
                        Debug.Log("[CampfireSystemTest] Found player inventory");
                        break;
                    }
                }
            }
            
            if (playerInventory == null)
            {
                Debug.LogWarning("[CampfireSystemTest] Player inventory not found");
            }
        }
        
        private void FindTimeManager()
        {
            // Find TimeManager by type name without namespace dependency
            var allMonoBehaviours = FindObjectsOfType<MonoBehaviour>();
            foreach (var mb in allMonoBehaviours)
            {
                if (mb.GetType().Name == "TimeManager")
                {
                    timeManagerInstance = mb;
                    Debug.Log("[CampfireSystemTest] Found TimeManager via reflection");
                    break;
                }
            }
            
            if (timeManagerInstance == null)
            {
                Debug.LogWarning("[CampfireSystemTest] TimeManager not found!");
            }
        }
        
        private void Update()
        {
            if (campfireInteractable == null) return;
            
            // Test game hour passing
            if (Input.GetKeyDown(testHourKey))
            {
                Debug.Log("[CampfireSystemTest] Testing game hour passed...");
                campfireInteractable.TestGameHourPassed();
            }
            
            // Add wood to player
            if (Input.GetKeyDown(addWoodKey) && playerInventory != null)
            {
                Debug.Log("[CampfireSystemTest] Adding 5 firewood to player inventory");
                playerInventory.AddItem("firewood", 5);
            }
            
            // Light campfire
            if (Input.GetKeyDown(lightFireKey))
            {
                Debug.Log("[CampfireSystemTest] Attempting to light campfire...");
                if (campfireInteractable.CurrentWoodAmount >= 4) // Assuming 4 wood required
                {
                    // Simulate adding wood to light the campfire
                    Debug.Log("[CampfireSystemTest] Campfire has enough wood to light");
                }
                else
                {
                    Debug.Log("[CampfireSystemTest] Campfire needs more wood to light");
                }
            }
            
            // Show status
            if (Input.GetKeyDown(showStatusKey))
            {
                campfireInteractable.ShowStatus();
                
                if (playerInventory != null)
                {
                    int playerWood = playerInventory.GetItemCount("firewood");
                    Debug.Log($"[CampfireSystemTest] Player firewood: {playerWood}");
                }
            }
        }
        
        [ContextMenu("Test Complete System")]
        public void TestCompleteSystem()
        {
            Debug.Log("[CampfireSystemTest] === COMPLETE SYSTEM TEST ===");
            
            // Check InteractableManager
            if (InteractableManager.Instance == null)
            {
                Debug.LogError("[CampfireSystemTest] ❌ InteractableManager.Instance is null");
            }
            else
            {
                Debug.Log($"[CampfireSystemTest] ✅ InteractableManager found with {InteractableManager.Instance.GetListenerCount()} listeners");
            }
            
            // Check TimeManager
            if (timeManagerInstance == null)
            {
                Debug.LogError("[CampfireSystemTest] ❌ TimeManager.Instance is null");
            }
            else
            {
                Debug.Log("[CampfireSystemTest] ✅ TimeManager found");
            }
            
            // Check CampfireInteractable
            if (campfireInteractable == null)
            {
                Debug.LogError("[CampfireSystemTest] ❌ No CampfireInteractable found");
            }
            else
            {
                Debug.Log($"[CampfireSystemTest] ✅ CampfireInteractable found: {campfireInteractable.name}");
                campfireInteractable.ShowStatus();
            }
            
            // Check Player Inventory
            if (playerInventory == null)
            {
                Debug.LogError("[CampfireSystemTest] ❌ Player inventory not found");
            }
            else
            {
                Debug.Log("[CampfireSystemTest] ✅ Player inventory found");
            }
            
            Debug.Log("[CampfireSystemTest] === TEST COMPLETE ===");
        }
    }
} 