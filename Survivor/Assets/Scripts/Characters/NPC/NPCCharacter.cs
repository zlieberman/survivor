using UnityEngine;
using Survivor.Shared;
using Survivor.Characters.Dialogue;
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

        protected override void Awake()
        {
            base.Awake();
            // Get character controller
            controller = GetComponent<UnityEngine.CharacterController>();
            // Set NPC-specific properties
            isPlayer = false;
            Debug.Log($"[NPCCharacter] Initialized NPC: {characterName}");
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

            // Notify dialogue manager of state change
            if (wasInRange != isPlayerInRange && dialogueManager != null)
            {
                dialogueManager.SetInteractable(isPlayerInRange ? this : null, isPlayerInRange);
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