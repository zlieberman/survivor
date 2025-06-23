using UnityEngine;
using Survivor.Shared;
using Survivor.Characters;
using Survivor.UI;
using UnityEngine.Events;
using System.Collections;

namespace Survivor.Interactables
{
    public class TentInteractable : BaseInteractable
    {
        [Header("Tent Settings")]
        public float interactionRadius = 3f;
        
        [Header("Rest Settings")]
        [SerializeField] private float energyRestored = 2f; // Energy restored per rest (legacy, kept for compatibility)
        [SerializeField] private float timeAdvanced = 2f; // Hours advanced per rest (legacy, kept for compatibility)
        
        [Header("Exponential Energy Settings")]
        [SerializeField] private float exponentialPower = 1.5f; // Power for exponential energy calculation (hours^power)
        
        [SerializeField] private GameObject restPanelPrefab;
        private Survivor.UI.RestPanelUI restPanelInstance;
        
        private CharacterStats playerStats;
        private SphereCollider triggerCollider;
        private bool isAdvancingTime = false; // Flag to prevent multiple time advancements
        private bool isRestInProgress = false; // Flag to prevent multiple rest confirmations

        protected override void Start()
        {
            Debug.Log("[TentInteractable] Initializing tent...");
            base.Start();
            interactionPrompt = "Press R to rest in tent";
            cooldownPrompt = "Cannot rest yet";
            cooldownTime = 5f; // Cooldown between rest sessions

            // Set up trigger collider
            triggerCollider = gameObject.AddComponent<SphereCollider>();
            triggerCollider.radius = interactionRadius;
            triggerCollider.isTrigger = true;
            
            Debug.Log("[TentInteractable] Tent initialized - using event-driven time advancement system");
            
            // Check TimeManager subscription after a short delay
            StartCoroutine(CheckSubscriptionAfterDelay());
        }

        private IEnumerator CheckSubscriptionAfterDelay()
        {
            // Wait a few frames to ensure TimeManager has had time to subscribe
            yield return new WaitForSeconds(0.5f);
            
            Debug.Log("[TentInteractable] Checking TimeManager subscription status after initialization...");
            CheckTimeManagerSubscription();
            
            // If still not subscribed, try to trigger TimeManager subscription
            if (onTimeAdvanced.GetPersistentEventCount() == 0)
            {
                Debug.LogWarning("[TentInteractable] Still not subscribed after delay - trying manual event connection");
                ManualEventConnection();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Debug.Log("[TentInteractable] Player entered interaction range");
                var player = other.GetComponent<Character>();
                if (player != null)
                {
                    playerStats = player.Stats;
                    Debug.Log("[TentInteractable] Found player stats component");
                    isPlayerInRange = true;
                    OnInteractionEnter();
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Debug.Log("[TentInteractable] Player exited interaction range");
                isPlayerInRange = false;
                playerStats = null;
                OnInteractionExit();
            }
        }

        public override void Interact()
        {
            Debug.Log("[TentInteractable] Interact called - showing rest panel instead of auto-resting");
            if (!CanInteract() || !isPlayerInRange)
            {
                Debug.Log("[TentInteractable] Cannot interact - on cooldown, not in range, or other condition");
                return;
            }
            
            // Instead of automatically resting, show the rest panel
            ShowRestPanel();
            
            // Call base interact to trigger events (but don't start cooldown yet)
            base.Interact();
        }

        private void RestoreEnergy(float energyAmount)
        {
            Debug.Log($"[TentInteractable] Attempting to restore {energyAmount} energy");
            
            // Find the Character component that contains CharacterStats
            var characterType = System.Type.GetType("Survivor.Characters.Character, Survivor.Characters");
            if (characterType != null)
            {
                // Find the player character
                var playerCharacter = FindObjectOfType(characterType);
                if (playerCharacter != null)
                {
                    // Get the Stats property using reflection
                    var statsProperty = characterType.GetProperty("Stats");
                    if (statsProperty != null)
                    {
                        var stats = statsProperty.GetValue(playerCharacter);
                        if (stats != null)
                        {
                            // Get the energy property from CharacterStats
                            var energyProperty = stats.GetType().GetField("energy");
                            if (energyProperty != null)
                            {
                                float oldEnergy = (float)energyProperty.GetValue(stats);
                                float newEnergy = Mathf.Min(100f, oldEnergy + energyAmount);
                                energyProperty.SetValue(stats, newEnergy);
                                Debug.Log($"[TentInteractable] Restored energy from {oldEnergy} to {newEnergy}");
                            }
                            else
                            {
                                Debug.LogError("[TentInteractable] Energy property not found on CharacterStats");
                            }
                        }
                        else
                        {
                            Debug.LogError("[TentInteractable] Stats is null");
                        }
                    }
                    else
                    {
                        Debug.LogError("[TentInteractable] Stats property not found on Character");
                    }
                }
                else
                {
                    Debug.LogError("[TentInteractable] Player character not found");
                }
            }
            else
            {
                Debug.LogError("[TentInteractable] Character type not found");
            }
        }

        /// <summary>
        /// Calculates energy restoration using exponential formula: energy = hours^exponentialPower
        /// This provides better scaling for longer rest periods
        /// </summary>
        /// <param name="hours">Number of hours rested</param>
        /// <returns>Amount of energy to restore</returns>
        private float CalculateExponentialEnergy(float hours)
        {
            float energy = Mathf.Pow(hours, exponentialPower);
            Debug.Log($"[TentInteractable] Exponential energy calculation: {hours} hours = {energy} energy (power: {exponentialPower})");
            return energy;
        }

        private void OnDrawGizmosSelected()
        {
            // Draw interaction radius
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, interactionRadius);
        }

        [ContextMenu("Show Tent Status")]
        public void ShowStatus()
        {
            Debug.Log($"[TentInteractable] Tent Status:");
            Debug.Log($"  - Exponential Power: {exponentialPower}");
            Debug.Log($"  - Energy for 1 hour: {CalculateExponentialEnergy(1f)}");
            Debug.Log($"  - Energy for 2 hours: {CalculateExponentialEnergy(2f)}");
            Debug.Log($"  - Energy for 12 hours: {CalculateExponentialEnergy(12f)}");
            Debug.Log($"  - Legacy Energy Restored: {energyRestored}");
            Debug.Log($"  - Legacy Time Advanced: {timeAdvanced} hours");
            Debug.Log($"  - Cooldown Time: {cooldownTime} seconds");
            Debug.Log($"  - Interaction Radius: {interactionRadius}");
            Debug.Log($"  - Player In Range: {isPlayerInRange}");
            Debug.Log($"  - Can Interact: {CanInteract()}");
        }
        
        [ContextMenu("Test Exponential Energy")]
        public void TestExponentialEnergy()
        {
            Debug.Log("[TentInteractable] Testing Exponential Energy Calculation:");
            Debug.Log($"  - 1 hour: {CalculateExponentialEnergy(1f)} energy");
            Debug.Log($"  - 2 hours: {CalculateExponentialEnergy(2f)} energy");
            Debug.Log($"  - 4 hours: {CalculateExponentialEnergy(4f)} energy");
            Debug.Log($"  - 8 hours: {CalculateExponentialEnergy(8f)} energy");
            Debug.Log($"  - 12 hours: {CalculateExponentialEnergy(12f)} energy");
        }
        
        [ContextMenu("Test Time Advancement")]
        public void TestTimeAdvancement()
        {
            Debug.Log("[TentInteractable] Testing Time Advancement:");
            Debug.Log($"[TentInteractable] onTimeAdvanced event has {onTimeAdvanced.GetPersistentEventCount()} listeners");
            
            // Test with 2 hours
            float testHours = 2f;
            Debug.Log($"[TentInteractable] Testing advancement of {testHours} hours");
            
            // Direct call to TimeManager using reflection
            try
            {
                var timeManagerType = System.Type.GetType("Survivor.Core.TimeManager, Assembly-CSharp");
                if (timeManagerType == null)
                {
                    timeManagerType = System.Type.GetType("Survivor.Core.TimeManager");
                }
                
                if (timeManagerType != null)
                {
                    var instanceProperty = timeManagerType.GetProperty("Instance");
                    if (instanceProperty != null)
                    {
                        var timeManagerInstance = instanceProperty.GetValue(null);
                        if (timeManagerInstance != null)
                        {
                            var advanceTimeMethod = timeManagerType.GetMethod("AdvanceTime");
                            if (advanceTimeMethod != null)
                            {
                                Debug.Log($"[TentInteractable] Test: Direct call to TimeManager.Instance.AdvanceTime({testHours})");
                                advanceTimeMethod.Invoke(timeManagerInstance, new object[] { testHours });
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[TentInteractable] Test time advancement failed: {e.Message}");
            }
            
            Debug.Log("[TentInteractable] Time advancement test completed - check TimeManager logs");
        }

        [ContextMenu("Test Event System")]
        public void TestEventSystem()
        {
            Debug.Log("[TentInteractable] Testing Event System:");
            Debug.Log($"[TentInteractable] onTimeAdvanced event has {onTimeAdvanced.GetPersistentEventCount()} listeners");
            
            if (onTimeAdvanced.GetPersistentEventCount() > 0)
            {
                Debug.Log("[TentInteractable] Event system is properly connected - triggering test event");
                TriggerTimeAdvancement(1f); // Test with 1 hour
            }
            else
            {
                Debug.LogWarning("[TentInteractable] No event listeners found - TimeManager should subscribe to this event automatically");
                Debug.LogWarning("[TentInteractable] Check if TimeManager is finding and subscribing to interactables properly");
            }
        }

        [ContextMenu("Test Small Time Advancement")]
        public void TestSmallTimeAdvancement()
        {
            Debug.Log("[TentInteractable] Testing Small Time Advancement (0.1 hours):");
            Debug.Log($"[TentInteractable] onTimeAdvanced event has {onTimeAdvanced.GetPersistentEventCount()} listeners");
            
            if (onTimeAdvanced.GetPersistentEventCount() > 0)
            {
                Debug.Log("[TentInteractable] Triggering small time advancement via event system");
                TriggerTimeAdvancement(0.1f); // Test with 0.1 hours (6 minutes)
            }
            else
            {
                Debug.LogError("[TentInteractable] No event listeners found - cannot test time advancement");
            }
        }

        [ContextMenu("Request TimeManager Subscription")]
        public void RequestTimeManagerSubscription()
        {
            Debug.Log("[TentInteractable] Requesting TimeManager to subscribe to new interactables...");
            
            // Instead of using reflection, we'll use a simpler approach
            // The TimeManager should automatically find and subscribe to us
            // Let's just trigger a manual subscription check
            
            // Find all BaseInteractable objects and trigger their events to see if TimeManager responds
            var allInteractables = FindObjectsOfType<BaseInteractable>();
            Debug.Log($"[TentInteractable] Found {allInteractables.Length} interactables in scene");
            
            // Check if any of them have TimeManager listeners
            foreach (var interactable in allInteractables)
            {
                if (interactable.onTimeAdvanced != null)
                {
                    int listenerCount = interactable.onTimeAdvanced.GetPersistentEventCount();
                    Debug.Log($"[TentInteractable] {interactable.name} has {listenerCount} listeners");
                    
                    if (listenerCount > 0)
                    {
                        // List the listeners
                        for (int i = 0; i < listenerCount; i++)
                        {
                            var target = interactable.onTimeAdvanced.GetPersistentTarget(i);
                            var methodName = interactable.onTimeAdvanced.GetPersistentMethodName(i);
                            Debug.Log($"[TentInteractable] Listener {i}: {target?.GetType().Name}.{methodName}");
                        }
                    }
                }
            }
            
            // Check our own subscription status
            Debug.Log($"[TentInteractable] Our onTimeAdvanced event has {onTimeAdvanced.GetPersistentEventCount()} listeners");
            
            if (onTimeAdvanced.GetPersistentEventCount() == 0)
            {
                Debug.LogWarning("[TentInteractable] No TimeManager subscription found - this might indicate a timing issue");
                Debug.LogWarning("[TentInteractable] Try using the TimeManager's context menu 'Manual Subscribe to New Interactables'");
            }
        }

        [ContextMenu("Check TimeManager Subscription")]
        public void CheckTimeManagerSubscription()
        {
            Debug.Log("[TentInteractable] Checking TimeManager subscription status:");
            Debug.Log($"[TentInteractable] onTimeAdvanced event has {onTimeAdvanced.GetPersistentEventCount()} listeners");
            
            if (onTimeAdvanced.GetPersistentEventCount() == 0)
            {
                Debug.LogWarning("[TentInteractable] No TimeManager subscription found - requesting subscription");
                RequestTimeManagerSubscription();
            }
            else
            {
                Debug.Log("[TentInteractable] TimeManager subscription found - system should work properly");
                
                // List all listeners for debugging
                for (int i = 0; i < onTimeAdvanced.GetPersistentEventCount(); i++)
                {
                    var target = onTimeAdvanced.GetPersistentTarget(i);
                    var methodName = onTimeAdvanced.GetPersistentMethodName(i);
                    Debug.Log($"[TentInteractable] Listener {i}: {target?.GetType().Name}.{methodName}");
                }
            }
        }

        private void TriggerTimeManagerSubscription()
        {
            Debug.Log("[TentInteractable] Triggering TimeManager subscription via event system...");
            
            // Try to trigger a small time advancement to see if TimeManager responds
            // This will help us determine if the subscription is working
            Debug.Log("[TentInteractable] Attempting to trigger time advancement to test connection...");
            
            // Check if we have any listeners
            if (onTimeAdvanced.GetPersistentEventCount() > 0)
            {
                Debug.Log("[TentInteractable] Found listeners - testing with small time advancement");
                TriggerTimeAdvancement(0.01f); // Very small test
            }
            else
            {
                Debug.LogError("[TentInteractable] No listeners found - TimeManager subscription failed");
                Debug.LogError("[TentInteractable] Please use TimeManager's 'Manual Subscribe to New Interactables' context menu");
            }
        }

        // Override Update to prevent base class from handling E key for tent
        protected override void Update()
        {
            // Handle cooldown timer (from base class)
            if (isOnCooldown)
            {
                cooldownTimer -= Time.deltaTime;
                if (cooldownTimer <= 0f)
                {
                    isOnCooldown = false;
                }
            }
            
            // Handle R key for tent interaction
            if (isPlayerInRange && !isOnCooldown && InputBlocker.GetKeyDown(KeyCode.R))
            {
                Debug.Log("[TentInteractable] R key pressed, showing rest panel");
                ShowRestPanel();
            }
            
            // Debug logging for troubleshooting
            if (InputBlocker.GetKeyDown(KeyCode.R))
            {
                Debug.Log($"[TentInteractable] R key pressed - isPlayerInRange: {isPlayerInRange}, isOnCooldown: {isOnCooldown}");
            }
        }

        public void ShowRestPanel()
        {
            Debug.Log("[TentInteractable] ShowRestPanel called");
            
            if (restPanelInstance == null)
            {
                // Method 1: Direct GameObject.Find approach
                GameObject restPanelObj = GameObject.Find("RestPanel");
                if (restPanelObj != null)
                {
                    Debug.Log($"[TentInteractable] Found RestPanel GameObject: {restPanelObj.name}");
                    restPanelInstance = restPanelObj.GetComponent<RestPanelUI>();
                    
                    if (restPanelInstance == null)
                    {
                        Debug.LogError("[TentInteractable] RestPanel GameObject found but RestPanelUI component is missing!");
                        return;
                    }
                    
                    Debug.Log("[TentInteractable] Successfully found RestPanelUI component");
                }
                else
                {
                    // Method 2: Fallback - search all GameObjects for RestPanelUI
                    Debug.Log("[TentInteractable] RestPanel not found by name, searching all GameObjects...");
                    var allGameObjects = FindObjectsOfType<GameObject>();
                    
                    foreach (var obj in allGameObjects)
                    {
                        var component = obj.GetComponent<RestPanelUI>();
                        if (component != null)
                        {
                            restPanelInstance = component;
                            Debug.Log($"[TentInteractable] Found RestPanelUI on GameObject: {obj.name}");
                            break;
                        }
                    }
                    
                    if (restPanelInstance == null)
                    {
                        Debug.LogError("[TentInteractable] RestPanelUI not found anywhere in scene!");
                        return;
                    }
                }
            }
            
            if (restPanelInstance != null)
            {
                Debug.Log("[TentInteractable] Initializing RestPanelUI");
                // Calculate average energy per hour for display (using 1 hour as baseline for exponential)
                float energyPerHour = CalculateExponentialEnergy(1f);
                restPanelInstance.Initialize(
                    energyPerHour, // energy per hour (exponential baseline)
                    12f, // max hours, or make this a field if needed
                    OnRestConfirmed,
                    () => restPanelInstance.Hide()
                );
                
                // Reset processing state to ensure buttons are enabled
                restPanelInstance.ResetProcessingState();
                
                restPanelInstance.Show();
                Debug.Log("[TentInteractable] RestPanelUI shown successfully");
            }
            else
            {
                Debug.LogError("[TentInteractable] RestPanelUI is null after finding it.");
            }
        }

        private void OnRestConfirmed(float hours)
        {
            // Prevent multiple rest confirmations
            if (isRestInProgress)
            {
                Debug.Log("[TentInteractable] Rest already in progress, ignoring duplicate call");
                return;
            }
            
            isRestInProgress = true;
            Debug.Log($"[TentInteractable] OnRestConfirmed called with {hours} hours");
            
            // Check if we have event listeners
            Debug.Log($"[TentInteractable] onTimeAdvanced event has {onTimeAdvanced.GetPersistentEventCount()} listeners");
            
            // 1. Calculate and restore energy (exponential formula)
            float energyToRestore = CalculateExponentialEnergy(hours);
            RestoreEnergy(energyToRestore);
            
            // 2. Start cooldown
            StartCooldown();
            
            // 3. Update the UI if this is the player
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                var character = player.GetComponent<Survivor.Characters.Character>();
                if (character != null && character.IsPlayer)
                {
                    var statusBar = FindObjectOfType<Survivor.UI.PlayerStatusBar>();
                    if (statusBar != null)
                        statusBar.SetStatus(character.Stats);
                }
            }
            
            // 4. Hide the rest panel
            if (restPanelInstance != null) restPanelInstance.Hide();
            
            // 5. TRIGGER TIME ADVANCEMENT VIA CLEAN EVENT SYSTEM
            // Note: TimeManager will detect that this call is from TentInteractable and skip player stats updates
            // (hunger, thirst, energy changes) while still advancing time and updating other systems
            Debug.Log($"[TentInteractable] Triggering time advancement of {hours} hours via event system");
            
            // Use direct method call if available, otherwise use event system
            if (timeManagerReference != null && timeManagerMethod != null)
            {
                Debug.Log("[TentInteractable] Using direct TimeManager method call");
                TriggerTimeAdvancementDirect(hours);
            }
            else
            {
                Debug.Log("[TentInteractable] Using event system");
                TriggerTimeAdvancement(hours); // This calls onTimeAdvanced.Invoke(hours)
            }
            
            Debug.Log($"[TentInteractable] Time advancement event triggered - TimeManager will handle the rest");
            
            // Reset the flag after a short delay to allow for any UI cleanup
            StartCoroutine(ResetRestInProgressFlag());
        }

        private IEnumerator ResetRestInProgressFlag()
        {
            yield return new WaitForSeconds(0.5f); // Wait half a second
            isRestInProgress = false;
            Debug.Log("[TentInteractable] Rest in progress flag reset");
        }

        [ContextMenu("Force TimeManager Subscription")]
        public void ForceTimeManagerSubscription()
        {
            Debug.Log("[TentInteractable] Force TimeManager Subscription called");
            
            // First check current status
            Debug.Log($"[TentInteractable] Current listener count: {onTimeAdvanced.GetPersistentEventCount()}");
            
            if (onTimeAdvanced.GetPersistentEventCount() == 0)
            {
                Debug.LogWarning("[TentInteractable] No listeners found - attempting to trigger subscription");
                
                // Try to trigger the periodic check by calling the TimeManager's method via reflection
                try
                {
                    // Find TimeManager in scene using a more reliable approach
                    var allMonoBehaviours = FindObjectsOfType<MonoBehaviour>();
                    MonoBehaviour timeManager = null;
                    
                    foreach (var mb in allMonoBehaviours)
                    {
                        if (mb.GetType().Name == "TimeManager")
                        {
                            timeManager = mb;
                            break;
                        }
                    }
                    
                    if (timeManager != null)
                    {
                        Debug.Log($"[TentInteractable] Found TimeManager: {timeManager.name}");
                        
                        var subscribeMethod = timeManager.GetType().GetMethod("SubscribeToNewInteractables");
                        if (subscribeMethod != null)
                        {
                            Debug.Log("[TentInteractable] Calling TimeManager.SubscribeToNewInteractables() via reflection");
                            subscribeMethod.Invoke(timeManager, null);
                            
                            // Check if it worked
                            Debug.Log($"[TentInteractable] After force subscription - listener count: {onTimeAdvanced.GetPersistentEventCount()}");
                            
                            if (onTimeAdvanced.GetPersistentEventCount() > 0)
                            {
                                Debug.Log("[TentInteractable] Success! Testing with small time advancement...");
                                TriggerTimeAdvancement(0.01f);
                            }
                            else
                            {
                                Debug.LogError("[TentInteractable] Force subscription failed - still no listeners");
                                
                                // Try the specific TentInteractable subscription method
                                var tentSubscribeMethod = timeManager.GetType().GetMethod("SubscribeToTentInteractable");
                                if (tentSubscribeMethod != null)
                                {
                                    Debug.Log("[TentInteractable] Trying specific TentInteractable subscription method...");
                                    tentSubscribeMethod.Invoke(timeManager, null);
                                    
                                    Debug.Log($"[TentInteractable] After specific subscription - listener count: {onTimeAdvanced.GetPersistentEventCount()}");
                                }
                            }
                        }
                        else
                        {
                            Debug.LogError("[TentInteractable] SubscribeToNewInteractables method not found on TimeManager");
                        }
                    }
                    else
                    {
                        Debug.LogError("[TentInteractable] TimeManager not found in scene");
                        
                        // List all MonoBehaviour types for debugging
                        Debug.Log("[TentInteractable] Available MonoBehaviour types in scene:");
                        foreach (var mb in allMonoBehaviours)
                        {
                            if (mb.GetType().Name.Contains("Manager") || mb.GetType().Name.Contains("Time"))
                            {
                                Debug.Log($"  - {mb.name}: {mb.GetType().Name}");
                            }
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[TentInteractable] Force subscription failed: {e.Message}");
                    Debug.LogError($"[TentInteractable] Stack trace: {e.StackTrace}");
                }
            }
            else
            {
                Debug.Log("[TentInteractable] Already has listeners - testing connection...");
                TriggerTimeAdvancement(0.01f);
            }
        }

        [ContextMenu("Debug TimeManager Status")]
        public void DebugTimeManagerStatus()
        {
            Debug.Log("[TentInteractable] === DEBUGGING TIMEMANAGER STATUS ===");
            
            // Check our own status
            Debug.Log($"[TentInteractable] Our listener count: {onTimeAdvanced.GetPersistentEventCount()}");
            Debug.Log($"[TentInteractable] Our onTimeAdvanced event is null: {onTimeAdvanced == null}");
            
            // Find TimeManager and check its status
            var allMonoBehaviours = FindObjectsOfType<MonoBehaviour>();
            MonoBehaviour timeManager = null;
            
            foreach (var mb in allMonoBehaviours)
            {
                if (mb.GetType().Name == "TimeManager")
                {
                    timeManager = mb;
                    break;
                }
            }
            
            if (timeManager != null)
            {
                Debug.Log($"[TentInteractable] Found TimeManager: {timeManager.name}");
                
                // Check if TimeManager has the subscription methods
                var subscribeMethod = timeManager.GetType().GetMethod("SubscribeToNewInteractables");
                var tentSubscribeMethod = timeManager.GetType().GetMethod("SubscribeToTentInteractable");
                
                Debug.Log($"[TentInteractable] TimeManager has SubscribeToNewInteractables: {subscribeMethod != null}");
                Debug.Log($"[TentInteractable] TimeManager has SubscribeToTentInteractable: {tentSubscribeMethod != null}");
                
                // Try to call the subscription method directly
                if (subscribeMethod != null)
                {
                    Debug.Log("[TentInteractable] Calling TimeManager.SubscribeToNewInteractables()...");
                    subscribeMethod.Invoke(timeManager, null);
                    
                    Debug.Log($"[TentInteractable] After calling SubscribeToNewInteractables - listener count: {onTimeAdvanced.GetPersistentEventCount()}");
                }
                
                // Also try the specific TentInteractable method
                if (tentSubscribeMethod != null)
                {
                    Debug.Log("[TentInteractable] Calling TimeManager.SubscribeToTentInteractable()...");
                    tentSubscribeMethod.Invoke(timeManager, null);
                    
                    Debug.Log($"[TentInteractable] After calling SubscribeToTentInteractable - listener count: {onTimeAdvanced.GetPersistentEventCount()}");
                }
            }
            else
            {
                Debug.LogError("[TentInteractable] TimeManager not found in scene!");
                
                // List all MonoBehaviour types for debugging
                Debug.Log("[TentInteractable] Available MonoBehaviour types in scene:");
                foreach (var mb in allMonoBehaviours)
                {
                    Debug.Log($"  - {mb.name}: {mb.GetType().Name}");
                }
            }
            
            Debug.Log("[TentInteractable] === END DEBUGGING ===");
        }

        [ContextMenu("Direct TimeManager Subscription")]
        public void DirectTimeManagerSubscription()
        {
            Debug.Log("[TentInteractable] Direct TimeManager Subscription called");
            
            // Find TimeManager
            var allMonoBehaviours = FindObjectsOfType<MonoBehaviour>();
            MonoBehaviour timeManager = null;
            
            foreach (var mb in allMonoBehaviours)
            {
                if (mb.GetType().Name == "TimeManager")
                {
                    timeManager = mb;
                    break;
                }
            }
            
            if (timeManager != null)
            {
                Debug.Log($"[TentInteractable] Found TimeManager: {timeManager.name}");
                
                // Call the specific subscription method
                var subscribeMethod = timeManager.GetType().GetMethod("SubscribeToSpecificInteractable");
                if (subscribeMethod != null)
                {
                    Debug.Log("[TentInteractable] Calling TimeManager.SubscribeToSpecificInteractable(this)...");
                    subscribeMethod.Invoke(timeManager, new object[] { this });
                    
                    Debug.Log($"[TentInteractable] After direct subscription - listener count: {onTimeAdvanced.GetPersistentEventCount()}");
                    
                    if (onTimeAdvanced.GetPersistentEventCount() > 0)
                    {
                        Debug.Log("[TentInteractable] Direct subscription successful! Testing...");
                        TriggerTimeAdvancement(0.01f);
                    }
                    else
                    {
                        Debug.LogError("[TentInteractable] Direct subscription failed - still no listeners");
                    }
                }
                else
                {
                    Debug.LogError("[TentInteractable] SubscribeToSpecificInteractable method not found on TimeManager");
                }
            }
            else
            {
                Debug.LogError("[TentInteractable] TimeManager not found in scene");
            }
        }

        [ContextMenu("Manual Event Connection")]
        public void ManualEventConnection()
        {
            Debug.Log("[TentInteractable] Manual Event Connection called");
            
            // Check current status
            Debug.Log($"[TentInteractable] Current listener count: {onTimeAdvanced.GetPersistentEventCount()}");
            
            if (onTimeAdvanced.GetPersistentEventCount() > 0)
            {
                Debug.Log("[TentInteractable] Already has listeners - testing connection...");
                TriggerTimeAdvancement(0.01f);
                return;
            }
            
            // Find TimeManager and store reference for direct calls
            var allMonoBehaviours = FindObjectsOfType<MonoBehaviour>();
            MonoBehaviour timeManager = null;
            
            foreach (var mb in allMonoBehaviours)
            {
                if (mb.GetType().Name == "TimeManager")
                {
                    timeManager = mb;
                    break;
                }
            }
            
            if (timeManager != null)
            {
                Debug.Log($"[TentInteractable] Found TimeManager: {timeManager.name}");
                
                // Store the TimeManager reference for direct calls
                timeManagerReference = timeManager;
                
                // Get the OnInteractableTimeAdvanced method from TimeManager
                var onInteractableTimeAdvancedMethod = timeManager.GetType().GetMethod("OnInteractableTimeAdvanced", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (onInteractableTimeAdvancedMethod != null)
                {
                    Debug.Log("[TentInteractable] Found OnInteractableTimeAdvanced method - storing for direct calls");
                    
                    // Store the method for direct invocation
                    timeManagerMethod = onInteractableTimeAdvancedMethod;
                    
                    Debug.Log("[TentInteractable] Manual connection setup completed - will use direct method calls");
                    
                    // Test the connection
                    Debug.Log("[TentInteractable] Testing direct method call...");
                    TriggerTimeAdvancementDirect(0.01f);
                }
                else
                {
                    Debug.LogError("[TentInteractable] OnInteractableTimeAdvanced method not found on TimeManager");
                }
            }
            else
            {
                Debug.LogError("[TentInteractable] TimeManager not found in scene");
            }
        }
        
        // Fields to store TimeManager reference and method for direct calls
        private MonoBehaviour timeManagerReference = null;
        private System.Reflection.MethodInfo timeManagerMethod = null;
        
        // Method to trigger time advancement with direct TimeManager call
        private void TriggerTimeAdvancementDirect(float hours)
        {
            Debug.Log($"[TentInteractable] Triggering time advancement of {hours} hours via direct method call");
            
            if (timeManagerReference != null && timeManagerMethod != null)
            {
                try
                {
                    timeManagerMethod.Invoke(timeManagerReference, new object[] { hours });
                    Debug.Log("[TentInteractable] Direct method call successful");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[TentInteractable] Direct method call failed: {e.Message}");
                }
            }
            else
            {
                Debug.LogError("[TentInteractable] TimeManager reference or method not available for direct call");
            }
        }

        [ContextMenu("Test Direct TimeManager Call")]
        public void TestDirectTimeManagerCall()
        {
            Debug.Log("[TentInteractable] Test Direct TimeManager Call called");
            
            if (timeManagerReference != null && timeManagerMethod != null)
            {
                Debug.Log("[TentInteractable] Direct TimeManager call is available - testing...");
                TriggerTimeAdvancementDirect(0.01f);
            }
            else
            {
                Debug.LogWarning("[TentInteractable] Direct TimeManager call not available - setting up...");
                ManualEventConnection();
            }
        }
    }
} 