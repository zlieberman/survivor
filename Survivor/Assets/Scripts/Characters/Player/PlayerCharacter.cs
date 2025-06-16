using UnityEngine;
using Survivor.Characters.Dialogue;

namespace Survivor.Characters
{
    [DefaultExecutionOrder(-25)] // Run after TribeManager but before most other scripts
    public class PlayerCharacter : Character
    {
        private CharacterInteractionManager interactionManager;
        private StarterAssets.StarterAssetsInputs starterAssetsInputs;
        private StarterAssets.ThirdPersonController thirdPersonController;

        protected override void Awake()
        {
            Debug.Log("[PlayerCharacter] Awake called");
            base.Awake();
            isPlayer = true;
            
            // Get or add interaction manager
            interactionManager = GetComponent<CharacterInteractionManager>();
            if (interactionManager == null)
            {
                Debug.Log("[PlayerCharacter] Adding CharacterInteractionManager component");
                interactionManager = gameObject.AddComponent<CharacterInteractionManager>();
            }

            // Get Starter Assets components
            starterAssetsInputs = GetComponent<StarterAssets.StarterAssetsInputs>();
            thirdPersonController = GetComponent<StarterAssets.ThirdPersonController>();

            // Verify DialogueManager exists
            var dialogueManager = FindObjectOfType<DialogueManager>();
            if (dialogueManager == null)
            {
                Debug.LogError("[PlayerCharacter] No DialogueManager found in scene! Please add a DialogueManager GameObject.");
            }
            else
            {
                Debug.Log("[PlayerCharacter] Found DialogueManager in scene");
                dialogueManager.OnDialogueStateChanged += OnDialogueStateChanged;
            }
        }

        private void OnDialogueStateChanged(bool inDialogue)
        {
            Debug.Log($"[PlayerCharacter] Dialogue state changed: {inDialogue}");
            
            // Enable/disable player movement
            if (thirdPersonController != null)
            {
                thirdPersonController.enabled = !inDialogue;
            }
            if (starterAssetsInputs != null)
            {
                starterAssetsInputs.enabled = !inDialogue;
            }
        }

        public override void Initialize(string name, string tribe, bool isPlayerCharacter, int id)
        {
            Debug.Log($"[PlayerCharacter] Initialize called with name: {name}, tribe: {tribe}, isPlayer: {isPlayerCharacter}");
            
            // Set tribe name before base initialization
            TribeName = tribe;
            Debug.Log($"[PlayerCharacter] Set tribe name to: {TribeName}");
            
            // Call base initialization
            base.Initialize(name, tribe, true, id); // Force isPlayer to true for PlayerCharacter
            
            Debug.Log($"[PlayerCharacter] After initialization - Name: {CharacterName}, Tribe: {TribeName}, IsPlayer: {IsPlayer}");
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            var dialogueManager = FindObjectOfType<DialogueManager>();
            if (dialogueManager != null)
            {
                dialogueManager.OnDialogueStateChanged -= OnDialogueStateChanged;
            }
        }
    }
} 