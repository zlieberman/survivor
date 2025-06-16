using UnityEngine;
using Survivor.Shared;

namespace Survivor.Characters
{
    public class PlayerInteractionManager : MonoBehaviour
    {
        [Header("Interaction Settings")]
        public float interactionRange = 3f;
        public LayerMask interactableLayer;
        public KeyCode interactKey = KeyCode.E;

        private IInteractable currentInteractable;
        private PlayerController playerController;

        private void Start()
        {
            playerController = GetComponent<PlayerController>();
        }

        private void Update()
        {
            if (playerController == null) return;

            // Check for interactable objects in range
            Collider[] interactables = Physics.OverlapSphere(transform.position, interactionRange, interactableLayer);
            IInteractable closestInteractable = null;
            float closestDistance = float.MaxValue;

            foreach (var collider in interactables)
            {
                IInteractable interactable = collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    float distance = Vector3.Distance(transform.position, collider.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestInteractable = interactable;
                    }
                }
            }

            // Update current interactable
            if (closestInteractable != currentInteractable)
            {
                if (currentInteractable != null)
                {
                    currentInteractable.OnInteractionExit();
                }

                currentInteractable = closestInteractable;

                if (currentInteractable != null)
                {
                    currentInteractable.OnInteractionEnter();
                }
            }

            // Handle interaction input
            if (Input.GetKeyDown(interactKey) && currentInteractable != null)
            {
                currentInteractable.Interact();
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Draw interaction range in editor
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
    }
} 