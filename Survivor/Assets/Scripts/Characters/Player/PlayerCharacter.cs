using UnityEngine;
using System.Collections.Generic;
using Survivor.Shared;
using Survivor.Items;

namespace Survivor.Characters
{
    [DefaultExecutionOrder(-25)] // Run after TribeManager but before most other scripts
    public class PlayerCharacter : Character, IInventory
    {
        private StarterAssets.StarterAssetsInputs starterAssetsInputs;
        private StarterAssets.ThirdPersonController thirdPersonController;

        // Inventory management
        private Dictionary<string, int> inventory = new Dictionary<string, int>();
        
        [Header("Inventory Settings")]
        [SerializeField] private float maxWeight = 20f; // Maximum weight capacity

        // IInventory event
        public event System.Action OnInventoryChanged;

        protected override void Awake()
        {
            Debug.Log("[PlayerCharacter] Awake called");
            base.Awake();
            isPlayer = true;

            // Get Starter Assets components
            starterAssetsInputs = GetComponent<StarterAssets.StarterAssetsInputs>();
            thirdPersonController = GetComponent<StarterAssets.ThirdPersonController>();

            // Register with PlayerManager
            var playerManager = FindObjectOfType<PlayerManager>();
            if (playerManager != null)
            {
                playerManager.RegisterPlayer(this);
            }
        }

        // Inventory methods
        public bool AddItem(string itemName, int amount = 1)
        {
            Debug.Log($"[PlayerCharacter] Attempting to add {amount} {itemName}");
            
            if (!HasSpaceForItem(itemName, amount))
            {
                Debug.Log($"[PlayerCharacter] Cannot add {amount} {itemName} - not enough space");
                return false;
            }

            if (inventory.ContainsKey(itemName))
            {
                inventory[itemName] += amount;
            }
            else
            {
                inventory[itemName] = amount;
            }
            Debug.Log($"[PlayerCharacter] Added {amount} {itemName}. Total: {inventory[itemName]}");
            OnInventoryChanged?.Invoke();
            return true;
        }

        public int GetItemCount(string itemName)
        {
            int count = inventory.ContainsKey(itemName) ? inventory[itemName] : 0;
            Debug.Log($"[PlayerCharacter] Current count of {itemName}: {count}");
            return count;
        }

        public bool HasItem(string itemName)
        {
            bool hasItem = inventory.ContainsKey(itemName) && inventory[itemName] > 0;
            Debug.Log($"[PlayerCharacter] Has {itemName}: {hasItem}");
            return hasItem;
        }

        public bool RemoveItem(string itemName, int amount = 1)
        {
            Debug.Log($"[PlayerCharacter] Attempting to remove {amount} {itemName}");
            if (!inventory.ContainsKey(itemName) || inventory[itemName] < amount)
            {
                Debug.Log($"[PlayerCharacter] Failed to remove {amount} {itemName} - not enough items");
                return false;
            }

            inventory[itemName] -= amount;
            if (inventory[itemName] <= 0)
            {
                inventory.Remove(itemName);
            }
            Debug.Log($"[PlayerCharacter] Removed {amount} {itemName}. Remaining: {GetItemCount(itemName)}");
            OnInventoryChanged?.Invoke();
            return true;
        }

        public Dictionary<string, int> GetAllItems()
        {
            return new Dictionary<string, int>(inventory);
        }

        public float GetItemWeight(string itemName)
        {
            return ItemManager.Instance.GetItemWeight(itemName);
        }

        public float GetTotalWeight()
        {
            float totalWeight = 0f;
            foreach (var item in inventory)
            {
                totalWeight += GetItemWeight(item.Key) * item.Value;
            }
            return totalWeight;
        }

        public float GetMaxWeight()
        {
            return maxWeight;
        }

        public bool HasSpaceForItem(string itemName, int amount = 1)
        {
            float itemWeight = GetItemWeight(itemName);
            float newTotalWeight = GetTotalWeight() + (itemWeight * amount);
            return newTotalWeight <= maxWeight;
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
            // Unregister from PlayerManager
            var playerManager = FindObjectOfType<PlayerManager>();
            if (playerManager != null)
            {
                playerManager.UnregisterPlayer(this);
            }
        }
    }
} 