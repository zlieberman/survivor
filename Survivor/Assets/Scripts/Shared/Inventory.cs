using UnityEngine;
using System.Collections.Generic;

namespace Survivor.Shared
{
    [System.Serializable]
    public class ItemData
    {
        public string name;
        public string category;
        public int maxStackSize = 99;
        public string description;
        public Sprite icon;
    }

    public class Inventory : MonoBehaviour
    {
        [Header("Inventory Settings")]
        [SerializeField] private int maxSlots = 20;
        [SerializeField] private Dictionary<string, ItemData> itemDefinitions = new Dictionary<string, ItemData>();
        [SerializeField] private Dictionary<string, int> items = new Dictionary<string, int>();

        private void Start()
        {
            Debug.Log("[Inventory] Initialized");
            InitializeDefaultItems();
        }

        private void InitializeDefaultItems()
        {
            // Add default item definitions
            AddItemDefinition("Coconut", "Food", 10, "A fresh coconut that can be consumed for hydration");
            AddItemDefinition("Water", "Drink", 5, "Fresh water for drinking");
            AddItemDefinition("Wood", "Resource", 99, "Wood for crafting and building");
            AddItemDefinition("Stone", "Resource", 99, "Stone for crafting and building");
        }

        private void AddItemDefinition(string name, string category, int maxStackSize, string description)
        {
            itemDefinitions[name] = new ItemData
            {
                name = name,
                category = category,
                maxStackSize = maxStackSize,
                description = description
            };
        }

        public bool AddItem(string itemName, int amount = 1)
        {
            Debug.Log($"[Inventory] Attempting to add {amount} {itemName}");
            
            // Check if item is defined
            if (!itemDefinitions.ContainsKey(itemName))
            {
                Debug.LogError($"[Inventory] Item {itemName} is not defined in item definitions!");
                return false;
            }

            // Check if we have space for the item
            if (items.Count >= maxSlots && !items.ContainsKey(itemName))
            {
                Debug.Log($"[Inventory] Cannot add {itemName} - inventory is full!");
                return false;
            }

            // Add or update item count
            if (items.ContainsKey(itemName))
            {
                int newAmount = items[itemName] + amount;
                int maxStack = itemDefinitions[itemName].maxStackSize;
                
                if (newAmount > maxStack)
                {
                    Debug.Log($"[Inventory] Cannot add {amount} {itemName} - would exceed max stack size of {maxStack}");
                    return false;
                }
                
                items[itemName] = newAmount;
            }
            else
            {
                items[itemName] = amount;
            }

            Debug.Log($"[Inventory] Added {amount} {itemName}. Total: {items[itemName]}");
            return true;
        }

        public int GetItemCount(string itemName)
        {
            int count = items.ContainsKey(itemName) ? items[itemName] : 0;
            Debug.Log($"[Inventory] Current count of {itemName}: {count}");
            return count;
        }

        public bool HasItem(string itemName)
        {
            bool hasItem = items.ContainsKey(itemName) && items[itemName] > 0;
            Debug.Log($"[Inventory] Has {itemName}: {hasItem}");
            return hasItem;
        }

        public bool RemoveItem(string itemName, int amount = 1)
        {
            Debug.Log($"[Inventory] Attempting to remove {amount} {itemName}");
            if (!items.ContainsKey(itemName) || items[itemName] < amount)
            {
                Debug.Log($"[Inventory] Failed to remove {amount} {itemName} - not enough items");
                return false;
            }

            items[itemName] -= amount;
            if (items[itemName] <= 0)
            {
                items.Remove(itemName);
            }
            Debug.Log($"[Inventory] Removed {amount} {itemName}. Remaining: {GetItemCount(itemName)}");
            return true;
        }

        public string GetItemCategory(string itemName)
        {
            return itemDefinitions.ContainsKey(itemName) ? itemDefinitions[itemName].category : "Unknown";
        }

        public int GetMaxStackSize(string itemName)
        {
            return itemDefinitions.ContainsKey(itemName) ? itemDefinitions[itemName].maxStackSize : 99;
        }

        public string GetItemDescription(string itemName)
        {
            return itemDefinitions.ContainsKey(itemName) ? itemDefinitions[itemName].description : "No description available";
        }

        public Dictionary<string, int> GetAllItems()
        {
            return new Dictionary<string, int>(items);
        }

        public int GetRemainingSlots()
        {
            return maxSlots - items.Count;
        }

        public bool IsFull()
        {
            return items.Count >= maxSlots;
        }
    }
} 