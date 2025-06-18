using UnityEngine;
using Survivor.Shared;
using Survivor.Characters.Dialogue;
using Survivor.Characters.UI;
using StarterAssets;

namespace Survivor.Characters
{
    public class PlayerInteractionManager : MonoBehaviour
    {
        [Header("Interaction Settings")]
        public float interactionRange = 3f;
        public LayerMask interactableLayer;
        public LayerMask npcLayer;
        public KeyCode interactKey = KeyCode.E;

        [Header("References")]
        private PlayerController playerController;
        private StarterAssetsInputs starterAssetsInputs;
        private ThirdPersonController thirdPersonController;
        private DialogueManager dialogueManager;

        private IInteractable currentInteractable;
        private IDialogueInteractable currentDialogueInteractable;
        private bool inDialogue;

        private void Start()
        {
            // Get required components
            playerController = GetComponent<PlayerController>();
            starterAssetsInputs = GetComponent<StarterAssetsInputs>();
            thirdPersonController = GetComponent<ThirdPersonController>();

            // Get dialogue manager
            dialogueManager = FindObjectOfType<DialogueManager>();
            if (dialogueManager != null)
            {
                dialogueManager.OnDialogueStateChanged += OnDialogueStateChanged;
            }
        }

        private void Update()
        {
            if (playerController == null || inDialogue) return;

            // Check for general interactables
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

            // Check for NPCs
            Collider[] npcs = Physics.OverlapSphere(transform.position, interactionRange, npcLayer);
            IDialogueInteractable closestNPC = null;
            float closestNPCDistance = float.MaxValue;

            foreach (var collider in npcs)
            {
                IDialogueInteractable npc = collider.GetComponent<IDialogueInteractable>();
                if (npc != null)
                {
                    float distance = Vector3.Distance(transform.position, collider.transform.position);
                    if (distance < closestNPCDistance)
                    {
                        closestNPCDistance = distance;
                        closestNPC = npc;
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

            // Update current dialogue interactable
            if (closestNPC != currentDialogueInteractable)
            {
                currentDialogueInteractable = closestNPC;
                if (currentDialogueInteractable != null)
                {
                    CharacterUIManager.Instance.ShowNPCInfo(currentDialogueInteractable.GetDisplayName());
                }
                else
                {
                    CharacterUIManager.Instance.HideNPCInfo();
                }
            }

            // Handle interaction input
            if (Input.GetKeyDown(interactKey))
            {
                if (currentInteractable != null)
                {
                    currentInteractable.Interact();
                }
                else if (currentDialogueInteractable != null)
                {
                    StartDialogue(currentDialogueInteractable);
                }
            }
        }

        private void StartDialogue(IDialogueInteractable interactable)
        {
            if (dialogueManager == null) return;

            inDialogue = true;
            interactable.OnDialogueStart();
            
            // Disable movement while in dialogue
            if (thirdPersonController != null)
            {
                thirdPersonController.enabled = false;
            }
            if (starterAssetsInputs != null)
            {
                starterAssetsInputs.enabled = false;
            }

            // Show cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void OnDialogueStateChanged(bool inDialogue)
        {
            this.inDialogue = inDialogue;
            
            if (!inDialogue)
            {
                // Re-enable movement
                if (thirdPersonController != null)
                {
                    thirdPersonController.enabled = true;
                }
                if (starterAssetsInputs != null)
                {
                    starterAssetsInputs.enabled = true;
                }

                // Hide cursor
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                // Clear current dialogue interactable
                if (currentDialogueInteractable != null)
                {
                    currentDialogueInteractable.OnDialogueEnd();
                    currentDialogueInteractable = null;
                }
                CharacterUIManager.Instance.HideNPCInfo();
            }
        }

        private void OnDestroy()
        {
            if (dialogueManager != null)
            {
                dialogueManager.OnDialogueStateChanged -= OnDialogueStateChanged;
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