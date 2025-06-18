using System.Collections.Generic;
using Survivor.Shared;
using UnityEngine;

namespace Survivor.Items
{
    public class ItemManager : MonoBehaviour
    {
        private static ItemManager instance;
        public static ItemManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<ItemManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("ItemManager");
                        instance = go.AddComponent<ItemManager>();
                    }
                }
                return instance;
            }
        }

        [Header("Item Definitions")]
        [SerializeField] private List<IItem> registeredItems = new List<IItem>();

        private Dictionary<string, IItem> itemLookup = new Dictionary<string, IItem>();

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeItems();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void InitializeItems()
        {
            // Register default items
            RegisterItem(new CoconutItem());
            RegisterItem(new BananaItem());
            
            // Register any items from the inspector
            foreach (var item in registeredItems)
            {
                if (item != null)
                {
                    RegisterItem(item);
                }
            }
        }

        public void RegisterItem(IItem item)
        {
            if (item != null && !string.IsNullOrEmpty(item.ItemName))
            {
                itemLookup[item.ItemName] = item;
                Debug.Log($"[ItemManager] Registered item: {item.ItemName} with weight {item.Weight}");
            }
        }

        public IItem GetItem(string itemName)
        {
            if (itemLookup.TryGetValue(itemName, out IItem item))
            {
                return item;
            }
            Debug.LogWarning($"[ItemManager] Item not found: {itemName}");
            return null;
        }

        public float GetItemWeight(string itemName)
        {
            IItem item = GetItem(itemName);
            return item?.Weight ?? 1f; // Default weight of 1 if item not found
        }

        public bool ItemExists(string itemName)
        {
            return itemLookup.ContainsKey(itemName);
        }

        public List<string> GetAllItemNames()
        {
            return new List<string>(itemLookup.Keys);
        }
    }
} 