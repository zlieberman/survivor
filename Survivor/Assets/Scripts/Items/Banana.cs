using Survivor.Shared;
using UnityEngine;

namespace Survivor.Items
{
    [System.Serializable]
    public class BananaItem : IItem
    {
        public string ItemName => "banana";
        public float Weight => 0.3f;
        public string Description => "A ripe yellow banana.";
    }

    public class Banana : MonoBehaviour
    {
        [SerializeField] private float interactionRange = 1f;
        [SerializeField] private LayerMask playerLayer;
        [SerializeField] private GameObject interactionPrompt;
        [SerializeField] private GameObject fullInventoryPrompt;

        private bool isPlayerInRange = false;
        private IInventory playerInventory;
        private BananaItem bananaItem = new BananaItem();

        private void Start()
        {
            playerLayer = 1 << LayerMask.NameToLayer("Player");
            
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }

            if (fullInventoryPrompt != null)
            {
                fullInventoryPrompt.SetActive(false);
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

            if (isPlayerInRange && !wasInRange)
            {
                if (interactionPrompt != null)
                {
                    interactionPrompt.SetActive(true);
                }

                if (playerInventory == null)
                {
                    playerInventory = colliders[0].GetComponent<IInventory>();
                }
            }
            else if (!isPlayerInRange && wasInRange)
            {
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
                CollectBanana();
            }
        }

        private void CollectBanana()
        {
            if (playerInventory != null)
            {
                bool success = playerInventory.AddItem(bananaItem.ItemName);
                
                if (success)
                {
                    Destroy(gameObject);
                }
                else
                {
                    if (fullInventoryPrompt != null)
                    {
                        fullInventoryPrompt.SetActive(true);
                        Invoke(nameof(HideFullInventoryPrompt), 2f);
                    }
                }
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