using UnityEngine;
using Survivor.Shared;

namespace Survivor.Core.Interaction
{
    public class CharacterInteractionManager : MonoBehaviour
    {
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        
        private IDialogueInteractable currentInteractable;
        private bool isInRange = false;
        private DialogueManager dialogueManager;

        private void Start()
        {
            dialogueManager = FindObjectOfType<DialogueManager>();
            if (dialogueManager == null)
            {
                Debug.LogError("[CharacterInteractionManager] No DialogueManager found in scene!");
            }
            else
            {
                Debug.Log("[CharacterInteractionManager] Found DialogueManager");
            }
        }

        private void Update()
        {
            // Debug log when E is pressed
            if (InputBlocker.GetKeyDown(interactKey))
            {
                Debug.Log("[CharacterInteractionManager] Interaction key pressed");
            }

            if (isInRange && InputBlocker.GetKeyDown(interactKey) && currentInteractable != null)
            {
                Debug.Log($"[CharacterInteractionManager] Attempting to start dialogue with {currentInteractable.GetDisplayName()}");
                if (dialogueManager != null)
                {
                    dialogueManager.StartDialogue(currentInteractable.GetDisplayName(), "Starting conversation...");
                }
                else
                {
                    Debug.LogError("[CharacterInteractionManager] Cannot start dialogue: DialogueManager is null");
                }
            }
        }

        public void SetInteractable(IDialogueInteractable interactable, bool inRange)
        {
            if (currentInteractable != interactable || isInRange != inRange)
            {
                Debug.Log($"[CharacterInteractionManager] Setting interactable: {(interactable != null ? interactable.GetDisplayName() : "null")}, inRange: {inRange}");
                currentInteractable = inRange ? interactable : null;
                isInRange = inRange;
            }
        }
    }
} 