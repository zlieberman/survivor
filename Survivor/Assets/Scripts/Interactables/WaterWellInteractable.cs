using UnityEngine;
using Survivor.Shared;

namespace Survivor.Interactables
{
    public class WaterWellInteractable : BaseInteractable
    {
        [Header("Water Well Settings")]
        public float thirstReduction = 2f;
        public float interactionRadius = 4f;

        private ICharacterStats playerStats;
        private SphereCollider triggerCollider;

        protected override void Start()
        {
            Debug.Log("[WaterWellInteractable] Initializing water well...");
            base.Start();
            interactionPrompt = "Press E to drink from the well";
            cooldownPrompt = "Cannot drink yet";
            cooldownTime = 5f;

            // Set up trigger collider
            triggerCollider = gameObject.AddComponent<SphereCollider>();
            triggerCollider.radius = interactionRadius;
            triggerCollider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Debug.Log("[WaterWellInteractable] Player entered interaction range");
                playerStats = other.GetComponent<ICharacterStats>();
                if (playerStats != null)
                {
                    Debug.Log("[WaterWellInteractable] Found player stats component");
                    isPlayerInRange = true;
                    OnInteractionEnter();
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Debug.Log("[WaterWellInteractable] Player exited interaction range");
                isPlayerInRange = false;
                playerStats = null;
                OnInteractionExit();
            }
        }

        public override void Interact()
        {
            Debug.Log("[WaterWellInteractable] Interact called");
            if (!CanInteract() || !isPlayerInRange)
            {
                Debug.Log("[WaterWellInteractable] Cannot interact - on cooldown, not in range, or other condition");
                return;
            }
            
            if (playerStats == null)
            {
                Debug.LogError("[WaterWellInteractable] Player stats is null!");
                return;
            }

            // Reduce thirst
            float oldThirst = playerStats.thirst;
            playerStats.thirst = Mathf.Max(0f, playerStats.thirst - thirstReduction);
            Debug.Log($"[WaterWellInteractable] Reduced thirst from {oldThirst} to {playerStats.thirst}");
            
            // Start cooldown
            StartCooldown();
            Debug.Log("[WaterWellInteractable] Started cooldown");
            
            // Call base interact to trigger events
            base.Interact();
        }

        private void OnDrawGizmosSelected()
        {
            // Draw interaction radius
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRadius);
        }
    }
} 