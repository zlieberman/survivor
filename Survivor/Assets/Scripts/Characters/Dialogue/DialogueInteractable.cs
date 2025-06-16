using UnityEngine;
using Survivor.Shared;
using Survivor.Shared.Interfaces;

namespace Survivor.Characters.Dialogue
{
    public class DialogueInteractable : MonoBehaviour, IDialogueInteractable
    {
        [Header("Dialogue Settings")]
        [SerializeField] private string displayName = "NPC";
        [SerializeField] private string dialogueId = "default";
        [SerializeField] private string tribeName = "DefaultTribe";
        [SerializeField] private Survivor.Shared.CharacterStats stats = new Survivor.Shared.CharacterStats();

        private IDialogueSystem dialogueSystem;
        private bool isPlayerInRange = false;

        public string TribeName => tribeName;
        public Survivor.Shared.CharacterStats Stats => stats;

        private void Start()
        {
            // Find the DialogueManager through the interface
            dialogueSystem = FindObjectOfType<DialogueManager>() as IDialogueSystem;
            if (dialogueSystem == null)
            {
                Debug.LogError($"[DialogueInteractable] Could not find DialogueManager implementing IDialogueSystem for {displayName}");
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                isPlayerInRange = true;
                if (dialogueSystem != null)
                {
                    dialogueSystem.SetInteractable(this, true);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                isPlayerInRange = false;
                if (dialogueSystem != null)
                {
                    dialogueSystem.SetInteractable(this, false);
                }
            }
        }

        public string GetDisplayName()
        {
            return displayName;
        }

        public string GetDialogueId()
        {
            return dialogueId;
        }

        public void OnDialogueStart()
        {
            Debug.Log($"[DialogueInteractable] Dialogue started with {displayName}");
            // Add any NPC-specific behavior when dialogue starts
            // For example, you might want to:
            // - Play an animation
            // - Change the NPC's state
            // - Trigger other game events
        }

        public void OnDialogueEnd()
        {
            Debug.Log($"[DialogueInteractable] Dialogue ended with {displayName}");
            // Add any NPC-specific behavior when dialogue ends
            // For example, you might want to:
            // - Return to idle animation
            // - Reset the NPC's state
            // - Trigger other game events
        }
    }
} 