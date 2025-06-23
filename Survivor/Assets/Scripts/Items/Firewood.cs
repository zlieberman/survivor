using UnityEngine;
using Survivor.Shared;

namespace Survivor.Items
{
    [System.Serializable]
    public class FirewoodItem : IItem
    {
        public string ItemName => "firewood";
        public float Weight => 1.0f;
        public string Description => "Dry wood suitable for building fires.";
    }

    public class Firewood : MonoBehaviour
    {
        [SerializeField] private float interactionRange = 1f;
        [SerializeField] private LayerMask playerLayer;
        [SerializeField] private GameObject interactionPrompt;
        [SerializeField] private GameObject fullInventoryPrompt;

        private bool isPlayerInRange = false;
        private IInventory playerInventory;
        private FirewoodItem firewoodItem = new FirewoodItem();

        private void Start()
        {
            // Initialize the player layer here instead
            playerLayer = 1 << LayerMask.NameToLayer("Player");
            
            Debug.Log($"[Firewood] Initialized at position {transform.position} on layer {LayerMask.LayerToName(gameObject.layer)}");
            Debug.Log($"[Firewood] Looking for player on layer {LayerMask.LayerToName(Mathf.RoundToInt(Mathf.Log(playerLayer.value, 2)))}");
            
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
            else
            {
                Debug.LogWarning("[Firewood] Interaction prompt is not assigned!");
            }

            if (fullInventoryPrompt != null)
            {
                fullInventoryPrompt.SetActive(false);
            }
            else
            {
                Debug.LogWarning("[Firewood] Full inventory prompt is not assigned!");
            }
        }

        private void Update()
        {
            CheckForPlayer();
            HandleInteraction();
        }

        private void CheckForPlayer()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, interactionRange, playerLayer);
            bool wasInRange = isPlayerInRange;
            isPlayerInRange = colliders.Length > 0;

            // Debug every frame to see if we're detecting anything
            if (colliders.Length > 0)
            {
                Debug.Log($"[Firewood] Found {colliders.Length} colliders in range. First collider: {colliders[0].gameObject.name} on layer {LayerMask.LayerToName(colliders[0].gameObject.layer)}");
            }

            if (isPlayerInRange && !wasInRange)
            {
                Debug.Log("[Firewood] Player entered interaction range");
                // Player just entered range
                if (interactionPrompt != null)
                {
                    interactionPrompt.SetActive(true);
                }

                // Get player inventory reference
                if (playerInventory == null)
                {
                    playerInventory = colliders[0].GetComponent<IInventory>();
                    if (playerInventory == null)
                    {
                        Debug.LogError("[Firewood] Player does not implement IInventory!");
                    }
                    else
                    {
                        Debug.Log("[Firewood] Found player inventory");
                    }
                }
            }
            else if (!isPlayerInRange && wasInRange)
            {
                Debug.Log("[Firewood] Player left interaction range");
                // Player just left range
                if (interactionPrompt != null)
                {
                    interactionPrompt.SetActive(false);
                }
                if (fullInventoryPrompt != null)
                {
                    fullInventoryPrompt.SetActive(false);
                }
            }
        }

        private void HandleInteraction()
        {
            if (isPlayerInRange && InputBlocker.GetKeyDown(KeyCode.E))
            {
                CollectFirewood();
            }
        }

        private void CollectFirewood()
        {
            if (playerInventory != null)
            {
                Debug.Log("[Firewood] Attempting to add firewood to inventory");
                bool success = playerInventory.AddItem(firewoodItem.ItemName);
                
                if (success)
                {
                    Destroy(gameObject);
                }
                else
                {
                    // Show full inventory prompt
                    if (fullInventoryPrompt != null)
                    {
                        fullInventoryPrompt.SetActive(true);
                        // Hide after 2 seconds
                        Invoke(nameof(HideFullInventoryPrompt), 2f);
                    }
                }
            }
            else
            {
                Debug.LogError("[Firewood] Cannot collect firewood - player inventory reference is null!");
            }
        }

        private void HideFullInventoryPrompt()
        {
            if (fullInventoryPrompt != null)
            {
                fullInventoryPrompt.SetActive(false);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
    }
} 