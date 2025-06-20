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

        private void OnDestroy()
        {
            // Unregister when destroyed
            if (Instance == this)
            {
                GameTimeService.UnregisterTimeProvider();
            }
        }
    }
} 