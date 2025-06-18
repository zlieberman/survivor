using UnityEngine;
using Survivor.Shared;
using System.Collections;
using TMPro;

namespace Survivor.Interactables
{
    public class CampfireInteractable : BaseInteractable, IGameHourListener
    {
        [Header("Campfire Settings")]
        public GameObject litCampfirePrefab;  // The lit campfire prefab to spawn
        public float interactionRadius = 3f;
        public Vector3 litFireOffset = Vector3.zero;  // Offset for the lit fire position

        [Header("Wood Management")]
        [SerializeField] private int maxWoodCapacity = 20;
        [SerializeField] private int woodRequiredToLight = 4;
        [SerializeField] private int woodBurnedPerHour = 1;

        [Header("UI Display")]
        [SerializeField] private TextMeshPro woodCountText;
        [SerializeField] private Vector3 textOffset = new Vector3(0, 2f, 0); // Offset above campfire
        [SerializeField] private bool showWoodCount = true;

        private bool isLit = false;
        private GameObject currentCampfire;
        private GameObject litCampfireInstance;
        private SphereCollider triggerCollider;
        private int currentWoodAmount = 0;
        private IInventory playerInventory;

        protected override void Start()
        {
            Debug.Log("[CampfireInteractable] Initializing campfire...");
            base.Start();
            interactionPrompt = "Press E to add wood to campfire";
            cooldownPrompt = "Cannot interact yet";
            cooldownTime = 1f;  // Short cooldown for interaction

            // Store reference to the current campfire (this object)
            currentCampfire = gameObject;

            // Set up trigger collider
            triggerCollider = gameObject.AddComponent<SphereCollider>();
            triggerCollider.radius = interactionRadius;
            triggerCollider.isTrigger = true;
            
            // Find player inventory (without direct namespace dependency)
            FindPlayerInventory();

            // Create wood count text display
            CreateWoodCountDisplay();

            UpdateInteractionPrompt();
            UpdateWoodCountDisplay();
            
            // Register with InteractableManager for game hour updates
            Debug.Log($"[CampfireInteractable] Attempting to register with InteractableManager...");
            Debug.Log($"[CampfireInteractable] InteractableManager.Instance is null: {InteractableManager.Instance == null}");
            
            if (InteractableManager.Instance != null)
            {
                InteractableManager.Instance.RegisterGameHourListener(this);
                Debug.Log($"[CampfireInteractable] Successfully registered with InteractableManager for game hour updates");
                Debug.Log($"[CampfireInteractable] Total registered listeners: {InteractableManager.Instance.GetListenerCount()}");
            }
            else
            {
                Debug.LogError("[CampfireInteractable] InteractableManager.Instance is null - campfire won't receive hour updates!");
                Debug.LogError("[CampfireInteractable] This means the campfire wood count will NOT decrease over time!");
            }
            
            Debug.Log($"[CampfireInteractable] Campfire '{gameObject.name}' initialized successfully. OnGameHourPassed method is available: {GetType().GetMethod("OnGameHourPassed") != null}");
        }

        private void OnDestroy()
        {
            // Unregister from InteractableManager when destroyed
            if (InteractableManager.Instance != null)
            {
                InteractableManager.Instance.UnregisterGameHourListener(this);
                Debug.Log($"[CampfireInteractable] Unregistered from InteractableManager");
            }
        }

        private void CreateWoodCountDisplay()
        {
            if (!showWoodCount) return;

            // Create a GameObject for the text
            GameObject textObject = new GameObject("WoodCountText");
            textObject.transform.SetParent(transform);
            textObject.transform.localPosition = textOffset;

            // Add TextMeshPro component
            woodCountText = textObject.AddComponent<TextMeshPro>();
            woodCountText.text = $"{currentWoodAmount}/{maxWoodCapacity}";
            woodCountText.fontSize = 3f;
            woodCountText.color = Color.white;
            woodCountText.alignment = TextAlignmentOptions.Center;
            woodCountText.fontStyle = FontStyles.Bold;

            // Add outline for better visibility
            woodCountText.outlineWidth = 0.2f;
            woodCountText.outlineColor = Color.black;

            // Make text face the camera
            woodCountText.transform.rotation = Quaternion.identity;

            Debug.Log("[CampfireInteractable] Created wood count text display");
        }

        private void UpdateWoodCountDisplay()
        {
            Debug.Log($"[CampfireInteractable] UpdateWoodCountDisplay called - showWoodCount: {showWoodCount}, woodCountText null: {woodCountText == null}");
            
            if (woodCountText != null && showWoodCount)
            {
                string newText = $"{currentWoodAmount}/{maxWoodCapacity}";
                Debug.Log($"[CampfireInteractable] Setting text to: '{newText}' (current wood: {currentWoodAmount}, max: {maxWoodCapacity})");
                Debug.Log($"[CampfireInteractable] Text position: {woodCountText.transform.position}, enabled: {woodCountText.enabled}");
                
                woodCountText.text = newText;
                
                // Change color based on wood amount
                Color newColor;
                if (currentWoodAmount >= woodRequiredToLight)
                {
                    newColor = Color.green; // Can light
                    Debug.Log("[CampfireInteractable] Setting text color to GREEN (can light)");
                }
                else if (currentWoodAmount > 0)
                {
                    newColor = Color.yellow; // Has some wood
                    Debug.Log("[CampfireInteractable] Setting text color to YELLOW (has some wood)");
                }
                else
                {
                    newColor = Color.red; // No wood
                    Debug.Log("[CampfireInteractable] Setting text color to RED (no wood)");
                }
                
                woodCountText.color = newColor;
                Debug.Log($"[CampfireInteractable] Text updated successfully - Text: '{woodCountText.text}', Color: {woodCountText.color}, Position: {woodCountText.transform.position}");
            }
            else
            {
                if (woodCountText == null)
                {
                    Debug.LogWarning("[CampfireInteractable] Cannot update display - woodCountText is null!");
                }
                if (!showWoodCount)
                {
                    Debug.Log("[CampfireInteractable] Cannot update display - showWoodCount is false");
                }
            }
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
                        Debug.Log("[CampfireInteractable] Found player inventory via interface");
                        break;
                    }
                }
            }
            
            if (playerInventory == null)
            {
                Debug.LogWarning("[CampfireInteractable] Player inventory not found - will try to find later");
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Debug.Log("[CampfireInteractable] Player entered interaction range");
                isPlayerInRange = true;
                
                // Try to find player inventory if not already found
                if (playerInventory == null)
                {
                    var inventory = other.GetComponent<IInventory>();
                    if (inventory != null)
                    {
                        playerInventory = inventory;
                        Debug.Log("[CampfireInteractable] Found player inventory on trigger enter");
                    }
                }
                
                OnInteractionEnter();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Debug.Log("[CampfireInteractable] Player exited interaction range");
                isPlayerInRange = false;
                OnInteractionExit();
            }
        }

        public override void Interact()
        {
            Debug.Log("[CampfireInteractable] Interact called");
            if (!CanInteract() || !isPlayerInRange)
            {
                Debug.Log("[CampfireInteractable] Cannot interact - on cooldown or not in range");
                return;
            }

            // Try to find player inventory if not already found
            if (playerInventory == null)
            {
                FindPlayerInventory();
                if (playerInventory == null)
                {
                    Debug.LogError("[CampfireInteractable] Cannot interact - Player inventory not found!");
                    return;
                }
            }

            // Check if player has wood
            int playerWoodCount = playerInventory.GetItemCount("firewood");
            if (playerWoodCount <= 0)
            {
                Debug.Log("[CampfireInteractable] Player has no firewood to add");
                interactionPrompt = "No firewood to add";
                return;
            }

            // Transfer all player wood to campfire
            TransferPlayerWoodToCampfire();
            
            // Start cooldown
            StartCooldown();
            Debug.Log("[CampfireInteractable] Started cooldown");
            
            // Call base interact to trigger events
            base.Interact();
        }

        private void TransferPlayerWoodToCampfire()
        {
            if (playerInventory == null) return;

            int playerWoodCount = playerInventory.GetItemCount("firewood");
            int spaceAvailable = maxWoodCapacity - currentWoodAmount;
            int woodToTransfer = Mathf.Min(playerWoodCount, spaceAvailable);

            if (woodToTransfer <= 0)
            {
                Debug.Log("[CampfireInteractable] Campfire is full of wood");
                interactionPrompt = "Campfire is full";
                return;
            }

            // Remove wood from player inventory
            bool removed = playerInventory.RemoveItem("firewood", woodToTransfer);
            if (!removed)
            {
                Debug.LogError("[CampfireInteractable] Failed to remove wood from player inventory!");
                return;
            }

            // Add wood to campfire
            currentWoodAmount += woodToTransfer;
            Debug.Log($"[CampfireInteractable] Transferred {woodToTransfer} wood from player. Campfire now has {currentWoodAmount}/{maxWoodCapacity} wood");

            // Check if we can light the campfire
            if (!isLit && currentWoodAmount >= woodRequiredToLight)
            {
                Debug.Log($"[CampfireInteractable] Campfire has {currentWoodAmount} wood - can be lit!");
                LightCampfire();
            }

            UpdateInteractionPrompt();
            UpdateWoodCountDisplay();
        }

        private void LightCampfire()
        {
            if (litCampfirePrefab == null)
            {
                Debug.LogError("[CampfireInteractable] Lit campfire prefab is not assigned!");
                return;
            }

            Debug.Log($"[CampfireInteractable] Lighting campfire with {currentWoodAmount} wood");
            
            // Store the original position and rotation
            Vector3 originalPosition = transform.position;
            Quaternion originalRotation = transform.rotation;
            Vector3 originalScale = transform.localScale;
            
            // Hide the unlit campfire first
            if (currentCampfire != null)
            {
                // Hide the visual mesh instead of deactivating the GameObject
                // This keeps the component active for hour updates and interactions
                Renderer[] renderers = currentCampfire.GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in renderers)
                {
                    // Don't hide the wood count text renderer
                    if (renderer.gameObject != woodCountText?.gameObject)
                    {
                        renderer.enabled = false;
                    }
                }
                
                // Also hide any mesh filters to completely hide the unlit campfire
                MeshRenderer[] meshRenderers = currentCampfire.GetComponentsInChildren<MeshRenderer>();
                foreach (MeshRenderer meshRenderer in meshRenderers)
                {
                    // Don't hide the wood count text mesh renderer
                    if (meshRenderer.gameObject != woodCountText?.gameObject)
                    {
                        meshRenderer.enabled = false;
                    }
                }
            }

            // Spawn the lit campfire at the same position as the current campfire
            Vector3 litPosition = originalPosition + litFireOffset;
            litCampfireInstance = Instantiate(litCampfirePrefab, litPosition, originalRotation);
            litCampfireInstance.transform.parent = transform.parent;  // Keep same parent hierarchy
            
            // Match the scale of the original campfire
            litCampfireInstance.transform.localScale = originalScale;
            
            // Move wood count text to follow the lit campfire
            if (woodCountText != null)
            {
                woodCountText.transform.SetParent(litCampfireInstance.transform);
                woodCountText.transform.localPosition = textOffset;
                Debug.Log($"[CampfireInteractable] Moved wood count text to lit campfire at position: {woodCountText.transform.position}");
                Debug.Log($"[CampfireInteractable] Text local position: {woodCountText.transform.localPosition}, Text offset: {textOffset}");
                
                // Make sure the text is visible and update it
                woodCountText.enabled = true;
                UpdateWoodCountDisplay();
            }
            else
            {
                Debug.LogWarning("[CampfireInteractable] woodCountText is null when trying to move to lit campfire");
            }
            
            // Ensure the fire effects are active
            ParticleSystem[] fireParticles = litCampfireInstance.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in fireParticles)
            {
                ps.Play();
            }
            
            // Ensure any lights are active
            Light[] lights = litCampfireInstance.GetComponentsInChildren<Light>();
            foreach (Light light in lights)
            {
                light.enabled = true;
            }
            
            Debug.Log($"[CampfireInteractable] Found {fireParticles.Length} particle systems and {lights.Length} lights in lit campfire");

            isLit = true;
            Debug.Log("[CampfireInteractable] Campfire lit successfully!");
            
            UpdateInteractionPrompt();
            UpdateWoodCountDisplay();
        }

        private void ExtinguishCampfire()
        {
            Debug.Log("[CampfireInteractable] Extinguishing campfire - no wood remaining");
            
            // Move wood count text back to the unlit campfire
            if (woodCountText != null)
            {
                woodCountText.transform.SetParent(transform);
                woodCountText.transform.localPosition = textOffset;
                Debug.Log("[CampfireInteractable] Moved wood count text back to unlit campfire");
            }
            
            // Destroy the lit campfire instance
            if (litCampfireInstance != null)
            {
                Destroy(litCampfireInstance);
                litCampfireInstance = null;
            }

            // Show the unlit campfire again
            if (currentCampfire != null)
            {
                // Show the visual mesh again
                Renderer[] renderers = currentCampfire.GetComponentsInChildren<Renderer>();
                foreach (Renderer renderer in renderers)
                {
                    renderer.enabled = true;
                }
                
                // Also show any mesh filters
                MeshRenderer[] meshRenderers = currentCampfire.GetComponentsInChildren<MeshRenderer>();
                foreach (MeshRenderer meshRenderer in meshRenderers)
                {
                    meshRenderer.enabled = true;
                }
            }

            isLit = false;
            Debug.Log("[CampfireInteractable] Campfire extinguished");
            
            UpdateInteractionPrompt();
            UpdateWoodCountDisplay();
        }

        // This method is called directly by TimeManager every game hour
        public void OnGameHourPassed()
        {
            Debug.Log($"[CampfireInteractable] ===== OnGameHourPassed CALLED on {gameObject.name} =====");
            Debug.Log($"[CampfireInteractable] WOOD LEVEL: {currentWoodAmount}/{maxWoodCapacity} (IsLit: {isLit})");
            Debug.Log($"[CampfireInteractable] Current state - IsLit: {isLit}, Wood: {currentWoodAmount}/{maxWoodCapacity}");
            
            if (!isLit)
            {
                Debug.Log("[CampfireInteractable] Campfire is not lit - no wood will be burned");
                return;
            }

            Debug.Log($"[CampfireInteractable] Campfire is lit - burning {woodBurnedPerHour} wood");
            Debug.Log($"[CampfireInteractable] Wood before burning: {currentWoodAmount}");
            
            currentWoodAmount -= woodBurnedPerHour;
            
            Debug.Log($"[CampfireInteractable] Wood after burning: {currentWoodAmount}");
            
            if (currentWoodAmount <= 0)
            {
                currentWoodAmount = 0;
                Debug.Log("[CampfireInteractable] Wood reached 0 - extinguishing campfire");
                ExtinguishCampfire();
            }
            else
            {
                Debug.Log($"[CampfireInteractable] Campfire now has {currentWoodAmount} wood remaining - updating display");
                UpdateWoodCountDisplay();
                Debug.Log($"[CampfireInteractable] Display updated - text should show: {currentWoodAmount}/{maxWoodCapacity}");
            }
            
            Debug.Log($"[CampfireInteractable] ===== OnGameHourPassed COMPLETED on {gameObject.name} =====");
        }

        private void UpdateInteractionPrompt()
        {
            if (isLit)
            {
                interactionPrompt = $"Campfire is lit ({currentWoodAmount}/{maxWoodCapacity} wood)";
            }
            else if (currentWoodAmount >= woodRequiredToLight)
            {
                interactionPrompt = $"Press E to light campfire ({currentWoodAmount}/{maxWoodCapacity} wood)";
            }
            else
            {
                interactionPrompt = $"Press E to add wood ({currentWoodAmount}/{maxWoodCapacity} wood, need {woodRequiredToLight} to light)";
            }
        }

        public bool IsLit => isLit;
        public int CurrentWoodAmount => currentWoodAmount;
        public int MaxWoodCapacity => maxWoodCapacity;

        // Debug method to manually test hour passing
        [ContextMenu("Test Game Hour Passed")]
        public void TestGameHourPassed()
        {
            Debug.Log($"[CampfireInteractable] ===== MANUAL TEST: OnGameHourPassed on {gameObject.name} =====");
            OnGameHourPassed();
            Debug.Log($"[CampfireInteractable] ===== MANUAL TEST COMPLETE =====");
        }

        // Debug method to show current status
        [ContextMenu("Show Campfire Status")]
        public void ShowStatus()
        {
            Debug.Log($"[CampfireInteractable] === CAMPFIRE STATUS ===");
            Debug.Log($"[CampfireInteractable] Name: {gameObject.name}");
            Debug.Log($"[CampfireInteractable] Is Lit: {isLit}");
            Debug.Log($"[CampfireInteractable] Wood: {currentWoodAmount}/{maxWoodCapacity}");
            Debug.Log($"[CampfireInteractable] Wood Required to Light: {woodRequiredToLight}");
            Debug.Log($"[CampfireInteractable] Wood Burned Per Hour: {woodBurnedPerHour}");
            Debug.Log($"[CampfireInteractable] InteractableManager Instance: {(InteractableManager.Instance != null ? "EXISTS" : "NULL")}");
            if (InteractableManager.Instance != null)
            {
                Debug.Log($"[CampfireInteractable] Total Registered Listeners: {InteractableManager.Instance.GetListenerCount()}");
            }
            Debug.Log($"[CampfireInteractable] =========================");
        }

        private void OnDrawGizmosSelected()
        {
            // Draw interaction radius
            Gizmos.color = new Color(1f, 0.5f, 0f); // Orange color
            Gizmos.DrawWireSphere(transform.position, interactionRadius);
            
            // Draw text position
            if (showWoodCount)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(transform.position + textOffset, 0.2f);
            }
        }
    }
} 