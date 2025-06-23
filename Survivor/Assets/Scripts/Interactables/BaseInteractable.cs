using UnityEngine;
using UnityEngine.Events;
using Survivor.Shared;

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
        public UnityEvent<float> onTimeAdvanced; // Event triggered when this interactable advances time

        protected bool isOnCooldown = false;
        protected float cooldownTimer = 0f;
        protected bool isPlayerInRange = false;

        // Public property to access isPlayerInRange
        public bool IsPlayerInRange => isPlayerInRange;

        // Public property to access CanInteract state for testing
        public bool CanInteractNow => CanInteract();

        protected virtual void Start()
        {
            // Initialize events if they're null
            if (onTimeAdvanced == null)
            {
                onTimeAdvanced = new UnityEvent<float>();
            }
            if (onInteractionStart == null)
            {
                onInteractionStart = new UnityEvent();
            }
            if (onInteractionEnd == null)
            {
                onInteractionEnd = new UnityEvent();
            }
            if (onInteractionEnter == null)
            {
                onInteractionEnter = new UnityEvent();
            }
            if (onInteractionExit == null)
            {
                onInteractionExit = new UnityEvent();
            }
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
            if (isPlayerInRange && InputBlocker.GetKeyDown(KeyCode.E))
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
        
        // Helper method for derived classes to trigger time advancement
        // Made public to allow TimeManager to test subscription connections
        public void TriggerTimeAdvancement(float hours)
        {
            Debug.Log($"[BaseInteractable] Triggering time advancement of {hours} hours from {gameObject.name}");
            onTimeAdvanced?.Invoke(hours);
        }
        
        [ContextMenu("Check Event Initialization")]
        public void CheckEventInitialization()
        {
            Debug.Log($"[BaseInteractable] Event initialization check for {gameObject.name}:");
            Debug.Log($"  - onTimeAdvanced: {(onTimeAdvanced != null ? "Initialized" : "NULL")}");
            Debug.Log($"  - onInteractionStart: {(onInteractionStart != null ? "Initialized" : "NULL")}");
            Debug.Log($"  - onInteractionEnd: {(onInteractionEnd != null ? "Initialized" : "NULL")}");
            Debug.Log($"  - onInteractionEnter: {(onInteractionEnter != null ? "Initialized" : "NULL")}");
            Debug.Log($"  - onInteractionExit: {(onInteractionExit != null ? "Initialized" : "NULL")}");
        }
    }
} 