using UnityEngine;
using Survivor.Characters;
using Survivor.Shared;
using Survivor.Interactables;
using System.Collections;
using UnityEngine.Events;
using System.Linq;

namespace Survivor.Core
{
    public class TimeManager : MonoBehaviour, ITimeProvider
    {
        public static TimeManager Instance { get; private set; }

        [Header("Time Settings")]
        [SerializeField] private float realTimePerGameHour = 120f; // 2 minutes = 1 hour
        [SerializeField] private int startHour = 10; // Game starts at 10:00 AM
        [SerializeField] private int startMinute = 0; // Game starts at 10:00 AM

        // Public property to access realTimePerGameHour
        public float RealTimePerGameHour => realTimePerGameHour;
        
        // Public properties to access starting time
        public int StartHour => startHour;
        public int StartMinute => startMinute;
        
        // ITimeProvider implementation
        public float ElapsedRealTime { get; private set; } = 0f;

        [Header("Events")]
        public UnityEvent onGameHourPassed; // Event triggered every game hour
        public UnityEvent<float> onTimeAdvanced; // Event triggered when time is manually advanced

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Register this TimeManager as the time provider
            GameTimeService.RegisterTimeProvider(this);
            StartCoroutine(UpdateGameTime());
            
            // Subscribe to interactable time advancement events
            SubscribeToInteractableEvents();
            
            // Start periodic subscription check to catch interactables created after TimeManager
            StartCoroutine(PeriodicSubscriptionCheck());
            
            // Also do an immediate check after a short delay
            StartCoroutine(DelayedSubscriptionCheck());
        }

        private void Update()
        {
            // Track elapsed real time
            ElapsedRealTime += Time.deltaTime;
        }

        private IEnumerator UpdateGameTime()
        {
            Debug.Log("[TimeManager] UpdateGameTime coroutine started");
            int hourCount = 0;
            
            while (true)
            {
                yield return new WaitForSeconds(realTimePerGameHour);
                hourCount++;
                Debug.Log($"[TimeManager] ===== GAME HOUR {hourCount} PASSED =====");
                Debug.Log("[TimeManager] Game hour passed - updating all systems");
                
                try
                {
                    UpdateAllSystems();
                    Debug.Log("[TimeManager] UpdateAllSystems completed successfully");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[TimeManager] Exception in UpdateAllSystems: {e.Message}\n{e.StackTrace}");
                }
                
                Debug.Log("[TimeManager] Invoking onGameHourPassed event");
                onGameHourPassed?.Invoke();
                Debug.Log("[TimeManager] Updating player stats");
                UpdatePlayerStats();
                Debug.Log($"[TimeManager] ===== HOUR {hourCount} COMPLETE =====");
            }
        }

        private void UpdateAllSystems()
        {
            Debug.Log("[TimeManager] UpdateAllSystems called - updating registered interactables...");
            
            try
            {
                // Check if InteractableManager exists
                if (InteractableManager.Instance == null)
                {
                    Debug.LogError("[TimeManager] InteractableManager.Instance is null - no interactables will be updated");
                    Debug.LogError("[TimeManager] This means campfire wood will NOT decrease over time!");
                    return;
                }
                Debug.Log("[TimeManager] Found InteractableManager instance successfully");

                var gameHourListeners = InteractableManager.Instance.GetGameHourListeners();
                if (gameHourListeners == null)
                {
                    Debug.LogError("[TimeManager] Failed to get game hour listeners list");
                    return;
                }

                Debug.Log($"[TimeManager] Found {gameHourListeners.Count} registered game hour listeners");
                
                if (gameHourListeners.Count == 0)
                {
                    Debug.LogWarning("[TimeManager] No game hour listeners registered - this means no campfires will burn wood!");
                    Debug.LogWarning("[TimeManager] Check if InteractableManager was created and campfires registered properly");
                }
                
                int campfireCount = 0;
                
                foreach (var listener in gameHourListeners)
                {
                    var mb = listener as MonoBehaviour;
                    Debug.Log($"[TimeManager] Updating game hour listener: {mb.name} (Type: {mb.GetType().Name})");
                    
                    try
                    {
                        listener.OnGameHourPassed();
                        campfireCount++;
                        Debug.Log($"[TimeManager] Successfully updated: {mb.name}");
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"[TimeManager] Error updating {mb.name}: {e.Message}");
                    }
                }
                
                Debug.Log($"[TimeManager] Search complete - Updated {campfireCount} interactables out of {gameHourListeners.Count} total registered listeners");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[TimeManager] Exception in UpdateAllSystems: {e.Message}\n{e.StackTrace}");
            }
        }

        private void UpdatePlayerStats()
        {
            // Find the player character
            var characters = FindObjectsOfType<Character>();
            Character playerCharacter = null;
            foreach (var character in characters)
            {
                if (character.IsPlayer)
                {
                    playerCharacter = character;
                    break;
                }
            }

            if (playerCharacter == null) return;

            // Calculate stat modifiers based on stamina and grit
            float staminaModifier = 1f - (playerCharacter.Stats.stamina / 200f); // Higher stamina reduces negative effects
            float gritModifier = 1f - (playerCharacter.Stats.grit / 200f); // Higher grit reduces negative effects
            float combinedModifier = (staminaModifier + gritModifier) / 2f;

            // Update hunger (0.2-0.5 increase, reduced by stamina and grit)
            float hungerIncrease = Random.Range(0.2f, 0.5f) * combinedModifier;
            playerCharacter.Stats.hunger = Mathf.Clamp(playerCharacter.Stats.hunger + hungerIncrease, 0f, 10f);

            // Update thirst (1.5 increase, reduced by stamina and grit)
            float thirstIncrease = 1.5f * combinedModifier;
            playerCharacter.Stats.thirst = Mathf.Clamp(playerCharacter.Stats.thirst + thirstIncrease, 0f, 10f);

            // Update energy (1-5 decrease, reduced by stamina and grit)
            float energyDecrease = Random.Range(1f, 5f) * combinedModifier;
            playerCharacter.Stats.energy = Mathf.Clamp(playerCharacter.Stats.energy - energyDecrease, 0f, 100f);

            // Update the UI if this is the player
            var statusBar = FindObjectOfType<Survivor.UI.PlayerStatusBar>();
            if (playerCharacter.IsPlayer && statusBar != null)
            {
                statusBar.SetStatus(playerCharacter.Stats);
            }

            Debug.Log($"[TimeManager] Updated player stats - Hunger: +{hungerIncrease:F2}, Thirst: +{thirstIncrease:F2}, Energy: -{energyDecrease:F2}");
        }

        // Public method to manually advance time (called by tent and other systems)
        public void AdvanceTime(float hours)
        {
            float timeToAdvance = hours * realTimePerGameHour;
            ElapsedRealTime += timeToAdvance;
            
            Debug.Log($"[TimeManager] Manually advanced time by {hours} hours ({timeToAdvance}s real time)");
            Debug.Log($"[TimeManager] New elapsed time: {ElapsedRealTime:F1}s");
            
            // Trigger the time advanced event
            onTimeAdvanced?.Invoke(hours);
            
            // Also update all systems immediately
            UpdateAllSystems();
            UpdatePlayerStats();
        }

        private void OnDestroy()
        {
            // Unregister when destroyed
            if (Instance == this)
            {
                GameTimeService.UnregisterTimeProvider();
            }
        }

        private void SubscribeToInteractableEvents()
        {
            // Find all interactables in the scene and subscribe to their time advancement events
            var interactables = FindObjectsOfType<BaseInteractable>();
            Debug.Log($"[TimeManager] SubscribeToInteractableEvents called - Found {interactables.Length} interactables");
            
            if (interactables.Length == 0)
            {
                Debug.LogWarning("[TimeManager] No interactables found in scene - this might indicate a timing issue");
                Debug.LogWarning("[TimeManager] TentInteractable might not be created yet");
            }
            
            foreach (var interactable in interactables)
            {
                Debug.Log($"[TimeManager] Processing interactable: {interactable.name} (Type: {interactable.GetType().Name})");
                SubscribeToInteractable(interactable);
            }
            
            Debug.Log("[TimeManager] SubscribeToInteractableEvents completed");
        }
        
        private void SubscribeToInteractable(BaseInteractable interactable)
        {
            if (interactable == null) return;
            
            Debug.Log($"[TimeManager] Attempting to subscribe to interactable: {interactable.name} (Type: {interactable.GetType().Name})");
            
            if (interactable.onTimeAdvanced != null)
            {
                // Check if we're already subscribed to avoid duplicates
                bool alreadySubscribed = false;
                for (int i = 0; i < interactable.onTimeAdvanced.GetPersistentEventCount(); i++)
                {
                    var target = interactable.onTimeAdvanced.GetPersistentTarget(i);
                    if (target == this)
                    {
                        alreadySubscribed = true;
                        Debug.Log($"[TimeManager] Already subscribed to {interactable.name} - skipping");
                        break;
                    }
                }
                
                if (!alreadySubscribed)
                {
                    interactable.onTimeAdvanced.AddListener(OnInteractableTimeAdvanced);
                    Debug.Log($"[TimeManager] Successfully subscribed to interactable time events: {interactable.name}");
                }
                else
                {
                    Debug.Log($"[TimeManager] Already subscribed to interactable: {interactable.name}");
                }
            }
            else
            {
                Debug.LogWarning($"[TimeManager] onTimeAdvanced event is null on {interactable.name} - skipping subscription");
            }
        }
        
        // Public method to subscribe to new interactables (can be called by other systems)
        public void SubscribeToNewInteractables()
        {
            Debug.Log("[TimeManager] SubscribeToNewInteractables called - searching for new interactables...");
            var interactables = FindObjectsOfType<BaseInteractable>();
            Debug.Log($"[TimeManager] Found {interactables.Length} total interactables");
            
            foreach (var interactable in interactables)
            {
                SubscribeToInteractable(interactable);
            }
            
            Debug.Log("[TimeManager] SubscribeToNewInteractables completed");
        }
        
        [ContextMenu("Manual Subscribe to New Interactables")]
        public void ManualSubscribeToNewInteractables()
        {
            Debug.Log("[TimeManager] Manual subscription triggered via context menu");
            SubscribeToNewInteractables();
        }
        
        [ContextMenu("Subscribe to Specific Interactable")]
        public void SubscribeToSpecificInteractable()
        {
            Debug.Log("[TimeManager] Subscribe to Specific Interactable triggered");
            
            // Find the first TentInteractable in the scene
            var tentInteractable = FindObjectOfType<BaseInteractable>();
            if (tentInteractable != null)
            {
                Debug.Log($"[TimeManager] Found interactable: {tentInteractable.name} (Type: {tentInteractable.GetType().Name})");
                SubscribeToInteractable(tentInteractable);
                
                // Test the subscription
                if (tentInteractable.onTimeAdvanced != null)
                {
                    int listenerCount = tentInteractable.onTimeAdvanced.GetPersistentEventCount();
                    Debug.Log($"[TimeManager] After subscription - {tentInteractable.name} has {listenerCount} listeners");
                    
                    if (listenerCount > 0)
                    {
                        Debug.Log("[TimeManager] Subscription successful! Testing with small time advancement...");
                        // Trigger a small test
                        tentInteractable.TriggerTimeAdvancement(0.01f);
                    }
                }
            }
            else
            {
                Debug.LogError("[TimeManager] No interactables found in scene");
            }
        }
        
        [ContextMenu("Subscribe to TentInteractable")]
        public void SubscribeToTentInteractable()
        {
            Debug.Log("[TimeManager] Subscribe to TentInteractable triggered");
            
            // Find all TentInteractable objects specifically
            var tentInteractables = FindObjectsOfType<MonoBehaviour>();
            var tentInteractable = null as BaseInteractable;
            
            foreach (var mb in tentInteractables)
            {
                if (mb.GetType().Name == "TentInteractable")
                {
                    tentInteractable = mb as BaseInteractable;
                    break;
                }
            }
            
            if (tentInteractable != null)
            {
                Debug.Log($"[TimeManager] Found TentInteractable: {tentInteractable.name}");
                SubscribeToInteractable(tentInteractable);
                
                // Test the subscription
                if (tentInteractable.onTimeAdvanced != null)
                {
                    int listenerCount = tentInteractable.onTimeAdvanced.GetPersistentEventCount();
                    Debug.Log($"[TimeManager] After subscription - {tentInteractable.name} has {listenerCount} listeners");
                    
                    if (listenerCount > 0)
                    {
                        Debug.Log("[TimeManager] TentInteractable subscription successful! Testing with small time advancement...");
                        tentInteractable.TriggerTimeAdvancement(0.01f);
                    }
                    else
                    {
                        Debug.LogError("[TimeManager] TentInteractable subscription failed - still no listeners");
                    }
                }
                else
                {
                    Debug.LogError("[TimeManager] TentInteractable.onTimeAdvanced is null");
                }
            }
            else
            {
                Debug.LogError("[TimeManager] No TentInteractable found in scene");
                
                // List all MonoBehaviour types for debugging
                Debug.Log("[TimeManager] Available MonoBehaviour types in scene:");
                var allMonoBehaviours = FindObjectsOfType<MonoBehaviour>();
                foreach (var mb in allMonoBehaviours)
                {
                    if (mb.GetType().Name.Contains("Interactable") || mb.GetType().Name.Contains("Tent"))
                    {
                        Debug.Log($"  - {mb.name}: {mb.GetType().Name}");
                    }
                }
            }
        }
        
        [ContextMenu("Debug All Interactables")]
        public void DebugAllInteractables()
        {
            Debug.Log("[TimeManager] === DEBUGGING ALL INTERACTABLES ===");
            
            var allMonoBehaviours = FindObjectsOfType<MonoBehaviour>();
            var interactables = new System.Collections.Generic.List<BaseInteractable>();
            
            Debug.Log($"[TimeManager] Found {allMonoBehaviours.Length} total MonoBehaviour objects");
            
            foreach (var mb in allMonoBehaviours)
            {
                if (mb is BaseInteractable)
                {
                    var interactable = mb as BaseInteractable;
                    interactables.Add(interactable);
                    Debug.Log($"[TimeManager] Found interactable: {interactable.name} (Type: {interactable.GetType().Name})");
                    
                    if (interactable.onTimeAdvanced != null)
                    {
                        int listenerCount = interactable.onTimeAdvanced.GetPersistentEventCount();
                        Debug.Log($"[TimeManager]   - onTimeAdvanced listeners: {listenerCount}");
                        
                        for (int i = 0; i < listenerCount; i++)
                        {
                            var target = interactable.onTimeAdvanced.GetPersistentTarget(i);
                            var methodName = interactable.onTimeAdvanced.GetPersistentMethodName(i);
                            Debug.Log($"[TimeManager]     - Listener {i}: {target?.GetType().Name}.{methodName}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[TimeManager]   - onTimeAdvanced is NULL!");
                    }
                }
            }
            
            Debug.Log($"[TimeManager] Total interactables found: {interactables.Count}");
            
            // Try to subscribe to all of them
            foreach (var interactable in interactables)
            {
                Debug.Log($"[TimeManager] Attempting to subscribe to {interactable.name}...");
                SubscribeToInteractable(interactable);
            }
            
            Debug.Log("[TimeManager] === END DEBUGGING ===");
        }
        
        [ContextMenu("Manual Connect to TentInteractable")]
        public void ManualConnectToTentInteractable()
        {
            Debug.Log("[TimeManager] Manual Connect to TentInteractable triggered");
            
            // Find TentInteractable
            var allMonoBehaviours = FindObjectsOfType<MonoBehaviour>();
            BaseInteractable tentInteractable = null;
            
            foreach (var mb in allMonoBehaviours)
            {
                if (mb.GetType().Name == "TentInteractable")
                {
                    tentInteractable = mb as BaseInteractable;
                    break;
                }
            }
            
            if (tentInteractable != null)
            {
                Debug.Log($"[TimeManager] Found TentInteractable: {tentInteractable.name}");
                ManualConnectToInteractable(tentInteractable);
                
                // Test the connection
                if (tentInteractable.onTimeAdvanced != null)
                {
                    int listenerCount = tentInteractable.onTimeAdvanced.GetPersistentEventCount();
                    if (listenerCount > 0)
                    {
                        Debug.Log("[TimeManager] Manual connection successful! Testing with small time advancement...");
                        tentInteractable.TriggerTimeAdvancement(0.01f);
                    }
                }
            }
            else
            {
                Debug.LogError("[TimeManager] No TentInteractable found in scene");
            }
        }
        
        private void OnInteractableTimeAdvanced(float hours)
        {
            Debug.Log($"[TimeManager] Interactable time advanced by {hours} hours - advancing time");
            
            // Check if this time advancement is coming from a tent
            bool isFromTent = false;
            
            // Get the current stack trace to see what called this method
            var stackTrace = new System.Diagnostics.StackTrace();
            Debug.Log("[TimeManager] Checking stack trace for TentInteractable...");
            
            foreach (var frame in stackTrace.GetFrames())
            {
                var method = frame.GetMethod();
                if (method != null && method.DeclaringType != null)
                {
                    var declaringType = method.DeclaringType.Name;
                    Debug.Log($"[TimeManager] Stack frame: {declaringType}.{method.Name}");
                    if (declaringType.Contains("TentInteractable"))
                    {
                        isFromTent = true;
                        Debug.Log("[TimeManager] Detected time advancement from TentInteractable - will skip player stats update");
                        break;
                    }
                }
            }
            
            // Advance time but skip player stats update if coming from tent
            if (isFromTent)
            {
                float timeToAdvance = hours * realTimePerGameHour;
                ElapsedRealTime += timeToAdvance;
                
                Debug.Log($"[TimeManager] Tent time advancement: {hours} hours ({timeToAdvance}s real time)");
                Debug.Log($"[TimeManager] New elapsed time: {ElapsedRealTime:F1}s");
                
                // Trigger the time advanced event
                onTimeAdvanced?.Invoke(hours);
                
                // Update all systems but skip player stats
                UpdateAllSystems();
                Debug.Log("[TimeManager] Skipped player stats update (tent rest)");
            }
            else
            {
                // Normal time advancement - use the standard method
                Debug.Log("[TimeManager] Normal time advancement - updating player stats");
                AdvanceTime(hours);
            }
        }

        private IEnumerator PeriodicSubscriptionCheck()
        {
            Debug.Log("[TimeManager] Starting periodic subscription check for new interactables");
            
            while (true)
            {
                yield return new WaitForSeconds(2f); // Check every 2 seconds
                
                // Find all interactables and ensure we're subscribed to them
                var interactables = FindObjectsOfType<BaseInteractable>();
                int newSubscriptions = 0;
                
                foreach (var interactable in interactables)
                {
                    if (interactable != null && interactable.onTimeAdvanced != null)
                    {
                        // Check if we're already subscribed
                        bool alreadySubscribed = false;
                        for (int i = 0; i < interactable.onTimeAdvanced.GetPersistentEventCount(); i++)
                        {
                            var target = interactable.onTimeAdvanced.GetPersistentTarget(i);
                            if (target == this)
                            {
                                alreadySubscribed = true;
                                break;
                            }
                        }
                        
                        if (!alreadySubscribed)
                        {
                            interactable.onTimeAdvanced.AddListener(OnInteractableTimeAdvanced);
                            newSubscriptions++;
                            Debug.Log($"[TimeManager] Periodically subscribed to new interactable: {interactable.name}");
                        }
                    }
                }
                
                if (newSubscriptions > 0)
                {
                    Debug.Log($"[TimeManager] Periodic check: Added {newSubscriptions} new subscriptions");
                }
            }
        }

        private IEnumerator DelayedSubscriptionCheck()
        {
            yield return new WaitForSeconds(1f); // Wait 1 second
            
            Debug.Log("[TimeManager] Performing delayed subscription check...");
            SubscribeToNewInteractables();
        }

        // Public method to subscribe to a specific interactable by reference
        public void SubscribeToSpecificInteractable(BaseInteractable interactable)
        {
            if (interactable == null)
            {
                Debug.LogError("[TimeManager] SubscribeToSpecificInteractable called with null interactable");
                return;
            }
            
            Debug.Log($"[TimeManager] SubscribeToSpecificInteractable called for: {interactable.name}");
            SubscribeToInteractable(interactable);
            
            // Test the subscription
            if (interactable.onTimeAdvanced != null)
            {
                int listenerCount = interactable.onTimeAdvanced.GetPersistentEventCount();
                Debug.Log($"[TimeManager] After specific subscription - {interactable.name} has {listenerCount} listeners");
                
                if (listenerCount > 0)
                {
                    Debug.Log("[TimeManager] Specific subscription successful!");
                }
                else
                {
                    Debug.LogError("[TimeManager] Specific subscription failed - still no listeners");
                }
            }
            else
            {
                Debug.LogError($"[TimeManager] {interactable.name}.onTimeAdvanced is null");
            }
        }
        
        // Public method to manually connect to any interactable
        public void ManualConnectToInteractable(BaseInteractable interactable)
        {
            if (interactable == null)
            {
                Debug.LogError("[TimeManager] ManualConnectToInteractable called with null interactable");
                return;
            }
            
            Debug.Log($"[TimeManager] ManualConnectToInteractable called for: {interactable.name}");
            
            if (interactable.onTimeAdvanced != null)
            {
                // Check if we're already connected
                bool alreadyConnected = false;
                for (int i = 0; i < interactable.onTimeAdvanced.GetPersistentEventCount(); i++)
                {
                    var target = interactable.onTimeAdvanced.GetPersistentTarget(i);
                    if (target == this)
                    {
                        alreadyConnected = true;
                        Debug.Log($"[TimeManager] Already manually connected to {interactable.name}");
                        break;
                    }
                }
                
                if (!alreadyConnected)
                {
                    // Manually add our OnInteractableTimeAdvanced method to the interactable's event
                    interactable.onTimeAdvanced.AddListener(OnInteractableTimeAdvanced);
                    Debug.Log($"[TimeManager] Manually connected to {interactable.name}");
                    
                    // Test the connection
                    int listenerCount = interactable.onTimeAdvanced.GetPersistentEventCount();
                    Debug.Log($"[TimeManager] After manual connection - {interactable.name} has {listenerCount} listeners");
                    
                    if (listenerCount > 0)
                    {
                        Debug.Log("[TimeManager] Manual connection successful!");
                    }
                    else
                    {
                        Debug.LogError("[TimeManager] Manual connection failed - still no listeners");
                    }
                }
            }
            else
            {
                Debug.LogError($"[TimeManager] {interactable.name}.onTimeAdvanced is null");
            }
        }
    }
} 