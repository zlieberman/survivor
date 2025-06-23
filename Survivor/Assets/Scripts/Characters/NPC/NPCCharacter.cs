using UnityEngine;
using Survivor.Shared;
using Survivor.Characters;
using Survivor.Characters.UI;

namespace Survivor.Characters
{
    [RequireComponent(typeof(UnityEngine.CharacterController))]
    public class NPCCharacter : Character
    {
        [Header("Interaction")]
        [SerializeField] private float interactionRadius = 3f;
        [SerializeField] private LayerMask playerLayer;

        private UnityEngine.CharacterController controller;
        private bool hasGeneratedStats = false;
        private CharacterModelManager modelManager;
        private NPCBehavior npcBehavior; // Reference to NPCBehavior for movement control
        private bool isInDialogue = false; // Simple flag to track dialogue state

        // Properties for DialogueManager
        public bool IsInteractable => isPlayerInRange;
        public bool IsInDialogue => isInDialogue; // Public property to check dialogue state
        public float DistanceToPlayer
        {
            get
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    return Vector3.Distance(transform.position, player.transform.position);
                }
                return float.MaxValue;
            }
        }

        protected override void Awake()
        {
            Debug.Log("[NPCCharacter] Awake called");
            base.Awake();
            
            // Get character controller
            controller = GetComponent<UnityEngine.CharacterController>();
            
            // Get NPCBehavior component for movement control
            npcBehavior = GetComponent<NPCBehavior>();
            if (npcBehavior == null)
            {
                Debug.LogWarning($"[NPCCharacter] No NPCBehavior component found on {gameObject.name}");
            }
            
            // Set NPC-specific properties
            isPlayer = false;

            // Ensure NPCGenerator exists
            if (NPCGenerator.Instance == null)
            {
                Debug.LogError("[NPCCharacter] NPCGenerator.Instance is null!");
                return;
            }

            // Assign gender first
            gender = NPCGenerator.Instance.AssignGender(tribeName);
            Debug.Log($"[NPCCharacter] Assigned gender: {gender}");

            // Generate random name and stats
            if (string.IsNullOrEmpty(characterName))
            {
                Debug.Log("[NPCCharacter] Generating random name");
                characterName = NPCGenerator.Instance.GenerateRandomName(gender);
                Debug.Log($"[NPCCharacter] Generated name: {characterName}");
            }
            
            if (!hasGeneratedStats)
            {
                Debug.Log("[NPCCharacter] Generating random stats");
                stats = NPCGenerator.Instance.GenerateRandomStats();
                hasGeneratedStats = true;
                Debug.Log($"[NPCCharacter] Generated stats - Speed: {stats.speed}, Strength: {stats.strength}, etc.");
            }

            // Update name tag if it exists
            if (nameTag != null)
            {
                nameTag.SetName(characterName);
            }

            // Find or create CharacterModelManager
            modelManager = FindObjectOfType<CharacterModelManager>();
            if (modelManager == null)
            {
                GameObject managerObj = new GameObject("CharacterModelManager");
                modelManager = managerObj.AddComponent<CharacterModelManager>();
            }

            // Assign character model based on gender
            modelManager.AssignCharacterModel(this);

            Debug.Log($"[NPCCharacter] Initialized NPC: {characterName}");
        }

        public override void Initialize(string name, string tribe, bool isPlayerCharacter, int id)
        {
            Debug.Log("[NPCCharacter] Initialize called");
            base.Initialize(name, tribe, false, id); // Force isPlayer to false for NPCs

            // Assign gender if not already set
            if (NPCGenerator.Instance != null && string.IsNullOrEmpty(characterName))
            {
                gender = NPCGenerator.Instance.AssignGender(tribeName);
                Debug.Log($"[NPCCharacter] Assigned gender in Initialize: {gender}");
                
                // Generate name based on gender
                characterName = NPCGenerator.Instance.GenerateRandomName(gender);
                Debug.Log($"[NPCCharacter] Generated name in Initialize: {characterName}");
            }

            // Re-generate stats if they haven't been generated yet
            if (!hasGeneratedStats && NPCGenerator.Instance != null)
            {
                Debug.Log("[NPCCharacter] Generating stats in Initialize");
                stats = NPCGenerator.Instance.GenerateRandomStats();
                hasGeneratedStats = true;
                Debug.Log($"[NPCCharacter] Generated stats in Initialize - Speed: {stats.speed}, Strength: {stats.strength}, etc.");
            }
        }

        // Override dialogue methods to control NPC movement
        public override void OnDialogueStart()
        {
            Debug.Log($"[NPCCharacter] Dialogue started with {characterName}, setting isInDialogue = true");
            isInDialogue = true;
            
            // Call base implementation
            base.OnDialogueStart();
        }

        public override void OnDialogueEnd()
        {
            Debug.Log($"[NPCCharacter] Dialogue ended with {characterName}, setting isInDialogue = false");
            isInDialogue = false;
            
            // Call base implementation
            base.OnDialogueEnd();
        }

        private void Update()
        {
            // Check for player in range
            Collider[] colliders = Physics.OverlapSphere(transform.position, interactionRadius, playerLayer);
            bool wasInRange = isPlayerInRange;
            isPlayerInRange = colliders.Length > 0;

            // Log when player enters/exits range
            if (wasInRange != isPlayerInRange)
            {
                Debug.Log($"[NPCCharacter] Player {(isPlayerInRange ? "entered" : "exited")} interaction range of {characterName}");
            }
        }

        private void OnDrawGizmosSelected()
        {
            // Draw interaction radius
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRadius);
        }
    }
} 