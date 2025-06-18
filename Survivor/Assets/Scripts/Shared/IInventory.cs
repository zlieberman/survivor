using System.Collections.Generic;

namespace Survivor.Shared
{
    public interface IInventory
    {
        event System.Action OnInventoryChanged;
        bool AddItem(string itemName, int amount = 1);
        int GetItemCount(string itemName);
        bool HasItem(string itemName);
        bool RemoveItem(string itemName, int amount = 1);
        Dictionary<string, int> GetAllItems();
        float GetItemWeight(string itemName);
        float GetTotalWeight();
        float GetMaxWeight();
        bool HasSpaceForItem(string itemName, int amount = 1);
    }
} 