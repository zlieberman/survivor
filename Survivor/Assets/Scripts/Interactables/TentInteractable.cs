using UnityEngine;
using Survivor.Shared;
using Survivor.Characters;

namespace Survivor.Interactables
{
    public class TentInteractable : BaseInteractable
    {
        [Header("Tent Settings")]
        public float interactionRadius = 3f;
        
        [Header("Rest Settings")]
        [SerializeField] private float energyRestored = 10f; // Energy restored per rest
        [SerializeField] private float timeAdvanced = 2f; // Hours advanced per rest
        
        [SerializeField] private GameObject restPanelPrefab;
        // private Survivor.UI.RestPanelUI restPanelInstance;
        
        private CharacterStats playerStats;
        private SphereCollider triggerCollider;

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
            Debug.Log("[TentInteractable] Interact called");
            if (!CanInteract() || !isPlayerInRange)
            {
                Debug.Log("[TentInteractable] Cannot interact - on cooldown, not in range, or other condition");
                return;
            }
            
            if (playerStats == null)
            {
                Debug.LogError("[TentInteractable] Player stats is null!");
                return;
            }

            // Restore energy using reflection to access CharacterStats
            RestoreEnergy(energyRestored);
            
            // Advance game time using reflection to avoid circular dependencies
            AdvanceGameTime(timeAdvanced);
            
            // Start cooldown
            StartCooldown();
            Debug.Log("[TentInteractable] Started cooldown");
            
            // Call base interact to trigger events
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

        private void AdvanceGameTime(float hours)
        {
            Debug.Log($"[TentInteractable] Attempting to advance game time by {hours} hours");
            
            // Use reflection to find and call TimeManager
            var timeManagerType = System.Type.GetType("Survivor.Core.TimeManager, Survivor.Core");
            if (timeManagerType != null)
            {
                var instanceProperty = timeManagerType.GetProperty("Instance");
                if (instanceProperty != null)
                {
                    var timeManager = instanceProperty.GetValue(null);
                    if (timeManager != null)
                    {
                        // TimeManager doesn't have an AdvanceTime method, so we need to manually advance time
                        // by modifying the ElapsedRealTime property
                        var elapsedTimeProperty = timeManagerType.GetProperty("ElapsedRealTime");
                        if (elapsedTimeProperty != null)
                        {
                            float currentElapsedTime = (float)elapsedTimeProperty.GetValue(timeManager);
                            float realTimePerGameHour = (float)timeManagerType.GetProperty("RealTimePerGameHour").GetValue(timeManager);
                            
                            // Calculate how much real time to advance
                            float timeToAdvance = hours * realTimePerGameHour;
                            float newElapsedTime = currentElapsedTime + timeToAdvance;
                            
                            // Set the new elapsed time
                            elapsedTimeProperty.SetValue(timeManager, newElapsedTime);
                            Debug.Log($"[TentInteractable] Successfully advanced game time by {hours} hours (real time: {timeToAdvance}s)");
                        }
                        else
                        {
                            Debug.LogError("[TentInteractable] ElapsedRealTime property not found on TimeManager");
                        }
                    }
                    else
                    {
                        Debug.LogError("[TentInteractable] TimeManager.Instance is null");
                    }
                }
                else
                {
                    Debug.LogError("[TentInteractable] TimeManager.Instance property not found");
                }
            }
            else
            {
                Debug.LogError("[TentInteractable] TimeManager type not found");
            }
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
            Debug.Log($"  - Energy Restored: {energyRestored}");
            Debug.Log($"  - Time Advanced: {timeAdvanced} hours");
            Debug.Log($"  - Cooldown Time: {cooldownTime} seconds");
            Debug.Log($"  - Interaction Radius: {interactionRadius}");
            Debug.Log($"  - Player In Range: {isPlayerInRange}");
            Debug.Log($"  - Can Interact: {CanInteract()}");
        }

        // Add this to override the default interaction key
        private void Update()
        {
            if (isPlayerInRange && !isOnCooldown && Input.GetKeyDown(KeyCode.R))
            {
                ShowRestPanel();
            }
        }

        private void ShowRestPanel()
        {
            // if (restPanelInstance == null)
            // {
            //     // Try to find an existing one in the scene
            //     // restPanelInstance = FindObjectOfType<Survivor.UI.RestPanelUI>();
            //     if (restPanelInstance == null && restPanelPrefab != null)
            //     {
            //         var canvas = FindObjectOfType<UnityEngine.Canvas>();
            //         var panelObj = Instantiate(restPanelPrefab, canvas.transform);
            //         restPanelInstance = panelObj.GetComponent<Survivor.UI.RestPanelUI>();
            //     }
            // }
            // if (restPanelInstance != null)
            // {
            //     restPanelInstance.Initialize(
            //         energyRestored / timeAdvanced, // energy per hour
            //         12f, // max hours, or make this a field if needed
            //         OnRestConfirmed,
            //         () => restPanelInstance.Hide()
            //     );
            //     restPanelInstance.Show();
            // }
            // else
            // {
            //     Debug.LogError("[TentInteractable] RestPanelUI prefab is not assigned or failed to instantiate.");
            // }
        }

        private void OnRestConfirmed(float hours)
        {
            RestoreEnergy(hours * (energyRestored / timeAdvanced));
            AdvanceGameTime(hours);
            StartCooldown();
            // Update the UI if this is the player
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
            // if (restPanelInstance != null) restPanelInstance.Hide();
            base.Interact();
        }
    }
} 