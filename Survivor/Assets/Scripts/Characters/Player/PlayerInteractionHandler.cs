using UnityEngine;
using UnityEngine.InputSystem;
using Survivor.Characters.Dialogue;
using Survivor.Characters.UI;
using StarterAssets;
using Survivor.Shared;

namespace Survivor.Characters
{
    public class PlayerInteractionHandler : MonoBehaviour
    {
        [Header("Interaction Settings")]
        public float interactionRange = 3f;
        public LayerMask npcLayer;

        private IDialogueInteractable selectedNPC;
        private bool inDialogue;
        private DialogueManager dialogueManager;
        private StarterAssets.StarterAssetsInputs starterAssetsInputs;
        private StarterAssets.ThirdPersonController thirdPersonController;

        private void Start()
        {
            // Get the Starter Assets components
            starterAssetsInputs = GetComponent<StarterAssets.StarterAssetsInputs>();
            thirdPersonController = GetComponent<StarterAssets.ThirdPersonController>();

            // Get dialogue manager
            dialogueManager = FindObjectOfType<DialogueManager>();
            if (dialogueManager != null)
            {
                dialogueManager.OnDialogueLine += OnDialogueReceived;
            }
        }

        private void Update()
        {
            if (!inDialogue)
            {
                HandleInteraction();
            }
        }

        private void HandleInteraction()
        {
            // Check for nearby NPCs
            Collider[] nearbyNPCs = Physics.OverlapSphere(transform.position, interactionRange, npcLayer);
            IDialogueInteractable closestNPC = null;
            float closestDistance = float.MaxValue;

            foreach (var collider in nearbyNPCs)
            {
                IDialogueInteractable interactable = collider.GetComponent<IDialogueInteractable>();
                if (interactable != null)
                {
                    float distance = Vector3.Distance(transform.position, collider.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestNPC = interactable;
                    }
                }
            }

            // Update selected NPC
            if (closestNPC != selectedNPC)
            {
                selectedNPC = closestNPC;
                if (selectedNPC != null)
                {
                    CharacterUIManager.Instance.ShowNPCInfo(selectedNPC.GetDisplayName());
                }
                else
                {
                    CharacterUIManager.Instance.HideNPCInfo();
                }
            }

            // Handle interaction input
            if (Input.GetKeyDown(KeyCode.E) && selectedNPC != null)
            {
                StartDialogue(selectedNPC);
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

        private void OnDialogueReceived(string response)
        {
            // Handle dialogue response
            Debug.Log($"Received dialogue: {response}");
        }

        public void CloseDialogue()
        {
            if (!inDialogue) return;

            inDialogue = false;
            if (selectedNPC != null)
            {
                selectedNPC.OnDialogueEnd();
            }
            selectedNPC = null;
            CharacterUIManager.Instance.HideNPCInfo();

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
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (dialogueManager != null)
            {
                dialogueManager.OnDialogueLine -= OnDialogueReceived;
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