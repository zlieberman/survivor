using UnityEngine;
using Survivor.Shared;

namespace Survivor.Interactables
{
    public class CampfireInteractable : BaseInteractable
    {
        [Header("Campfire Settings")]
        public GameObject litCampfirePrefab;  // The lit campfire prefab to spawn
        public float interactionRadius = 3f;
        public Vector3 litFireOffset = Vector3.zero;  // Offset for the lit fire position

        private bool isLit = false;
        private GameObject currentCampfire;
        private GameObject litCampfireInstance;
        private SphereCollider triggerCollider;

        protected override void Start()
        {
            Debug.Log("[CampfireInteractable] Initializing campfire...");
            base.Start();
            interactionPrompt = "Press E to light the campfire";
            cooldownPrompt = "Cannot light yet";
            cooldownTime = 2f;  // Short cooldown for lighting

            // Store reference to the current campfire (this object)
            currentCampfire = gameObject;

            // Set up trigger collider
            triggerCollider = gameObject.AddComponent<SphereCollider>();
            triggerCollider.radius = interactionRadius;
            triggerCollider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Debug.Log("[CampfireInteractable] Player entered interaction range");
                isPlayerInRange = true;
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
                Debug.Log("[CampfireInteractable] Cannot interact - on cooldown, not in range, or already lit");
                return;
            }

            if (isLit)
            {
                Debug.Log("[CampfireInteractable] Campfire is already lit!");
                return;
            }

            // Light the campfire
            LightCampfire();
            
            // Start cooldown
            StartCooldown();
            Debug.Log("[CampfireInteractable] Started cooldown");
            
            // Call base interact to trigger events
            base.Interact();
        }

        private void LightCampfire()
        {
            if (litCampfirePrefab == null)
            {
                Debug.LogError("[CampfireInteractable] Lit campfire prefab is not assigned!");
                return;
            }

            Debug.Log($"[CampfireInteractable] Using lit campfire prefab: {litCampfirePrefab.name}");
            
            // Store the original position and rotation
            Vector3 originalPosition = transform.position;
            Quaternion originalRotation = transform.rotation;
            Vector3 originalScale = transform.localScale;
            
            // Hide the unlit campfire first
            if (currentCampfire != null)
            {
                currentCampfire.SetActive(false);
            }

            // Spawn the lit campfire at the same position as the current campfire
            Vector3 litPosition = originalPosition + litFireOffset;
            litCampfireInstance = Instantiate(litCampfirePrefab, litPosition, originalRotation);
            litCampfireInstance.transform.parent = transform.parent;  // Keep same parent hierarchy
            
            // Match the scale of the original campfire
            litCampfireInstance.transform.localScale = originalScale;
            
            // Debug: Check what components are in the prefab
            Debug.Log($"[CampfireInteractable] Lit campfire instance name: {litCampfireInstance.name}");
            Debug.Log($"[CampfireInteractable] Lit campfire active: {litCampfireInstance.activeInHierarchy}");
            
            // Check all children
            Transform[] allChildren = litCampfireInstance.GetComponentsInChildren<Transform>();
            Debug.Log($"[CampfireInteractable] Found {allChildren.Length} child transforms:");
            foreach (Transform child in allChildren)
            {
                Debug.Log($"[CampfireInteractable] - {child.name} (active: {child.gameObject.activeInHierarchy})");
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
            
            Debug.Log($"[CampfireInteractable] Original campfire scale: {originalScale}, Lit campfire scale: {litCampfireInstance.transform.localScale}");
            Debug.Log($"[CampfireInteractable] Found {fireParticles.Length} particle systems and {lights.Length} lights in lit campfire");

            isLit = true;
            interactionPrompt = "Campfire is lit";  // Update prompt since it's now lit
            
            Debug.Log("[CampfireInteractable] Campfire lit successfully!");
        }

        public bool IsLit => isLit;

        private void OnDrawGizmosSelected()
        {
            // Draw interaction radius
            Gizmos.color = new Color(1f, 0.5f, 0f); // Orange color
            Gizmos.DrawWireSphere(transform.position, interactionRadius);
        }
    }
} 