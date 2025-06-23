using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Survivor.Shared;

namespace Survivor.Interactables
{
    public class InteractionManager : MonoBehaviour
    {
        [Header("Interaction Settings")]
        public float interactionRange = 3f;
        public LayerMask interactableLayer;
        public KeyCode interactionKey = KeyCode.E;

        [Header("UI References")]
        public GameObject interactionPrompt;
        public TextMeshProUGUI interactionText;

        private BaseInteractable currentInteractable;
        private bool isInRange;

        private void Start()
        {
            // Set up UI if not already set
            if (interactionPrompt == null || interactionText == null)
            {
                InteractionUISetup.SetupInteractionUI(this);
            }
        }

        private void Update()
        {
            CheckForInteractables();
            HandleInteractionInput();
        }

        private void CheckForInteractables()
        {
            // Find all interactables in range
            Collider[] colliders = Physics.OverlapSphere(transform.position, interactionRange, interactableLayer);
            
            BaseInteractable closestInteractable = null;
            float closestDistance = float.MaxValue;

            foreach (Collider collider in colliders)
            {
                BaseInteractable interactable = collider.GetComponent<BaseInteractable>();
                if (interactable != null && interactable.IsPlayerInRange)
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
            if (currentInteractable != closestInteractable)
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

            // Update UI
            UpdateInteractionUI();
        }

        private void HandleInteractionInput()
        {
            if (InputBlocker.GetKeyDown(interactionKey) && currentInteractable != null && currentInteractable.IsPlayerInRange)
            {
                currentInteractable.Interact();
            }
        }

        private void UpdateInteractionUI()
        {
            if (currentInteractable != null && currentInteractable.IsPlayerInRange)
            {
                interactionPrompt.SetActive(true);
                interactionText.text = currentInteractable.GetInteractionPrompt();
            }
            else
            {
                interactionPrompt.SetActive(false);
            }
        }
    }
} 