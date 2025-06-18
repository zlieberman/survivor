using UnityEngine;
using Survivor.Shared;

namespace Survivor.Items
{
    [System.Serializable]
    public class CoconutItem : IItem
    {
        public string ItemName => "coconut";
        public float Weight => 0.5f;
        public string Description => "A fresh coconut from the palm trees.";
    }

    public class Coconut : MonoBehaviour
    {
        [SerializeField] private float interactionRange = 1f;
        [SerializeField] private LayerMask playerLayer;
        [SerializeField] private GameObject interactionPrompt;
        [SerializeField] private GameObject fullInventoryPrompt;

        private bool isPlayerInRange = false;
        private IInventory playerInventory;
        private CoconutItem coconutItem = new CoconutItem();

        private void Start()
        {
            // Initialize the player layer here instead
            playerLayer = 1 << LayerMask.NameToLayer("Player");
            
            Debug.Log($"[Coconut] Initialized at position {transform.position} on layer {LayerMask.LayerToName(gameObject.layer)}");
            Debug.Log($"[Coconut] Looking for player on layer {LayerMask.LayerToName(Mathf.RoundToInt(Mathf.Log(playerLayer.value, 2)))}");
            
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
            else
            {
                Debug.LogWarning("[Coconut] Interaction prompt is not assigned!");
            }

            if (fullInventoryPrompt != null)
            {
                fullInventoryPrompt.SetActive(false);
            }
            else
            {
                Debug.LogWarning("[Coconut] Full inventory prompt is not assigned!");
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
                Debug.Log($"[Coconut] Found {colliders.Length} colliders in range. First collider: {colliders[0].gameObject.name} on layer {LayerMask.LayerToName(colliders[0].gameObject.layer)}");
            }

            if (isPlayerInRange && !wasInRange)
            {
                Debug.Log("[Coconut] Player entered interaction range");
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
                        Debug.LogError("[Coconut] Player does not implement IInventory!");
                    }
                    else
                    {
                        Debug.Log("[Coconut] Found player inventory");
                    }
                }
            }
            else if (!isPlayerInRange && wasInRange)
            {
                Debug.Log("[Coconut] Player left interaction range");
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
            if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("[Coconut] E key pressed while player in range");
                CollectCoconut();
            }
        }

        private void CollectCoconut()
        {
            if (playerInventory != null)
            {
                Debug.Log("[Coconut] Attempting to add coconut to inventory");
                bool success = playerInventory.AddItem(coconutItem.ItemName);
                
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
                Debug.LogError("[Coconut] Cannot collect coconut - player inventory reference is null!");
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