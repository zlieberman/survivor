using UnityEngine;
using Survivor.Interactables;
using Survivor.Shared;

namespace Survivor.Editor
{
    public class CampfireDebugTest : MonoBehaviour
    {
        [Header("Debug Settings")]
        [SerializeField] private KeyCode addWoodKey = KeyCode.W;
        [SerializeField] private KeyCode simulateHourKey = KeyCode.H;
        [SerializeField] private KeyCode showStatusKey = KeyCode.S;
        
        private CampfireInteractable campfireInteractable;
        private IInventory playerInventory;
        private MonoBehaviour timeManagerInstance;
        
        private void Start()
        {
            // Find the campfire interactable in the scene
            campfireInteractable = FindObjectOfType<CampfireInteractable>();
            
            if (campfireInteractable == null)
            {
                Debug.LogWarning("[CampfireDebugTest] No CampfireInteractable found in the scene!");
                return;
            }
            
            // Find the player inventory (without direct namespace dependency)
            FindPlayerInventory();
            
            // Find TimeManager without namespace dependency
            FindTimeManager();
            
            Debug.Log("[CampfireDebugTest] Campfire debug test initialized successfully");
            Debug.Log("[CampfireDebugTest] Press W to add wood to player, H to simulate game hour, S to show status");
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
                        Debug.Log("[CampfireDebugTest] Found player inventory via interface");
                        break;
                    }
                }
            }
            
            if (playerInventory == null)
            {
                Debug.LogWarning("[CampfireDebugTest] Player inventory not found!");
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
                    Debug.Log("[CampfireDebugTest] Found TimeManager via reflection");
                    break;
                }
            }
            
            if (timeManagerInstance == null)
            {
                Debug.LogWarning("[CampfireDebugTest] TimeManager not found!");
            }
        }
        
        private void Update()
        {
            if (campfireInteractable == null || playerInventory == null) return;
            
            // Add wood to player inventory
            if (Input.GetKeyDown(addWoodKey))
            {
                Debug.Log("[CampfireDebugTest] Adding 5 firewood to player inventory");
                playerInventory.AddItem("firewood", 5);
            }
            
            // Simulate game hour passing
            if (Input.GetKeyDown(simulateHourKey))
            {
                Debug.Log("[CampfireDebugTest] Simulating game hour passing");
                if (campfireInteractable != null)
                {
                    // Call the OnGameHourPassed method directly on the campfire
                    var method = campfireInteractable.GetType().GetMethod("OnGameHourPassed", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (method != null)
                    {
                        method.Invoke(campfireInteractable, null);
                        Debug.Log("[CampfireDebugTest] Successfully called OnGameHourPassed on campfire");
                    }
                    else
                    {
                        Debug.LogWarning("[CampfireDebugTest] Could not find OnGameHourPassed method!");
                    }
                }
                else
                {
                    Debug.LogWarning("[CampfireDebugTest] Campfire not found!");
                }
            }
            
            // Show current status
            if (Input.GetKeyDown(showStatusKey))
            {
                ShowStatus();
            }
        }
        
        private void ShowStatus()
        {
            Debug.Log("=== CAMPFIRE DEBUG STATUS ===");
            Debug.Log($"Campfire is lit: {campfireInteractable.IsLit}");
            Debug.Log($"Campfire wood: {campfireInteractable.CurrentWoodAmount}/{campfireInteractable.MaxWoodCapacity}");
            
            if (playerInventory != null)
            {
                int playerWood = playerInventory.GetItemCount("firewood");
                Debug.Log($"Player firewood: {playerWood}");
            }
            else
            {
                Debug.LogWarning("Player inventory not found!");
            }
            
            if (timeManagerInstance != null)
            {
                Debug.Log("TimeManager found and active");
            }
            else
            {
                Debug.LogWarning("TimeManager not found!");
            }
            Debug.Log("================================");
        }
        
        [ContextMenu("Show Campfire Status")]
        public void ShowCampfireStatus()
        {
            ShowStatus();
        }
        
        [ContextMenu("Add Wood to Player")]
        public void AddWoodToPlayer()
        {
            if (playerInventory != null)
            {
                playerInventory.AddItem("firewood", 5);
                Debug.Log("[CampfireDebugTest] Added 5 firewood to player via context menu");
            }
        }
        
        [ContextMenu("Simulate Game Hour")]
        public void SimulateGameHour()
        {
            if (timeManagerInstance != null)
            {
                var eventField = timeManagerInstance.GetType().GetField("onGameHourPassed");
                if (eventField != null)
                {
                    var unityEvent = eventField.GetValue(timeManagerInstance) as UnityEngine.Events.UnityEvent;
                    unityEvent?.Invoke();
                    Debug.Log("[CampfireDebugTest] Simulated game hour via context menu");
                }
            }
        }
    }
} 