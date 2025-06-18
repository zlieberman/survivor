using UnityEngine;
using UnityEngine.Events;

namespace Survivor.Interactables
{
    public abstract class BaseInteractable : MonoBehaviour
    {
        [Header("Interaction Settings")]
        public string interactionPrompt = "Press E to interact";
        public string cooldownPrompt = "Cannot interact yet";
        public float cooldownTime = 0f;

        [Header("Events")]
        public UnityEvent onInteractionStart;
        public UnityEvent onInteractionEnd;
        public UnityEvent onInteractionEnter;
        public UnityEvent onInteractionExit;

        protected bool isOnCooldown = false;
        protected float cooldownTimer = 0f;
        protected bool isPlayerInRange = false;

        // Public property to access isPlayerInRange
        public bool IsPlayerInRange => isPlayerInRange;

        protected virtual void Start()
        {
            // Base initialization
        }

        protected virtual void Update()
        {
            if (isOnCooldown)
            {
                cooldownTimer -= Time.deltaTime;
                if (cooldownTimer <= 0f)
                {
                    isOnCooldown = false;
                }
            }

            // Handle interaction input when in range
            if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
            {
                Interact();
            }
        }

        public virtual void Interact()
        {
            if (isOnCooldown) return;

            onInteractionStart?.Invoke();
            
            if (cooldownTime > 0)
            {
                StartCooldown();
            }
        }

        public virtual void OnInteractionEnter()
        {
            if (!isOnCooldown)
            {
                Debug.Log(interactionPrompt);
            }
            else
            {
                Debug.Log($"{cooldownPrompt} ({cooldownTimer:F1}s)");
            }
            onInteractionEnter?.Invoke();
        }

        public virtual void OnInteractionExit()
        {
            Debug.Log("Left interaction range");
            onInteractionExit?.Invoke();
        }

        protected virtual void StartCooldown()
        {
            isOnCooldown = true;
            cooldownTimer = cooldownTime;
        }

        protected virtual bool CanInteract()
        {
            return !isOnCooldown;
        }

        public virtual string GetInteractionPrompt()
        {
            return isOnCooldown ? cooldownPrompt : interactionPrompt;
        }
    }
} 