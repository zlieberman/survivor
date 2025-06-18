using UnityEngine;
using System.Collections.Generic;

namespace Survivor.Characters
{
    public class Inventory : MonoBehaviour
    {
        private Dictionary<string, int> items = new Dictionary<string, int>();

        private void Start()
        {
            Debug.Log("[Inventory] Initialized");
        }

        public void AddItem(string itemName, int amount = 1)
        {
            Debug.Log($"[Inventory] Attempting to add {amount} {itemName}");
            if (items.ContainsKey(itemName))
            {
                items[itemName] += amount;
            }
            else
            {
                items[itemName] = amount;
            }
            Debug.Log($"[Inventory] Added {amount} {itemName}. Total: {items[itemName]}");
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
    }
} 