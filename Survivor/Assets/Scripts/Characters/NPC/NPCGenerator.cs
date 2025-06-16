using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Survivor.Characters
{
    public class NPCGenerator : MonoBehaviour
    {
        private static NPCGenerator instance;
        public static NPCGenerator Instance
        {
            get
            {
                if (instance == null)
                {
                    Debug.Log("[NPCGenerator] Creating new instance");
                    GameObject go = new GameObject("NPCGenerator");
                    instance = go.AddComponent<NPCGenerator>();
                    DontDestroyOnLoad(go);
                }
                return instance;
            }
        }

        private class NameData
        {
            public List<string> firstNames;
            public List<string> lastNames;
        }

        private NameData nameData;

        private void Awake()
        {
            Debug.Log("[NPCGenerator] Awake called");
            if (instance != null && instance != this)
            {
                Debug.Log("[NPCGenerator] Destroying duplicate instance");
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);

            LoadNameData();
        }

        private void LoadNameData()
        {
            Debug.Log("[NPCGenerator] Loading name data");
            TextAsset namesJson = Resources.Load<TextAsset>("names");
            if (namesJson != null)
            {
                Debug.Log($"[NPCGenerator] Successfully loaded names.json: {namesJson.text}");
                nameData = JsonUtility.FromJson<NameData>(namesJson.text);
                if (nameData != null)
                {
                    Debug.Log($"[NPCGenerator] Loaded {nameData.firstNames?.Count ?? 0} first names and {nameData.lastNames?.Count ?? 0} last names");
                }
                else
                {
                    Debug.LogError("[NPCGenerator] Failed to parse names.json!");
                }
            }
            else
            {
                Debug.LogError("[NPCGenerator] Failed to load names.json from Resources folder!");
                nameData = new NameData
                {
                    firstNames = new List<string> { "Unknown" },
                    lastNames = new List<string> { "Person" }
                };
            }
        }

        public string GenerateRandomName()
        {
            Debug.Log("[NPCGenerator] Generating random name");
            if (nameData == null)
            {
                Debug.LogWarning("[NPCGenerator] nameData is null, reloading...");
                LoadNameData();
            }

            if (nameData?.firstNames == null || nameData?.lastNames == null)
            {
                Debug.LogError("[NPCGenerator] nameData or its lists are null!");
                return "Unknown Person";
            }

            string firstName = nameData.firstNames[Random.Range(0, nameData.firstNames.Count)];
            string lastName = nameData.lastNames[Random.Range(0, nameData.lastNames.Count)];
            string fullName = $"{firstName} {lastName}";
            Debug.Log($"[NPCGenerator] Generated name: {fullName}");
            return fullName;
        }

        public CharacterStats GenerateRandomStats()
        {
            Debug.Log("[NPCGenerator] Generating random stats");
            CharacterStats stats = new CharacterStats();
            
            // Generate random integer values between 0 and 100 for each stat
            stats.perception = Random.Range(0, 101);
            stats.deception = Random.Range(0, 101);
            stats.persuasion = Random.Range(0, 101);
            stats.puzzleSolving = Random.Range(0, 101);
            stats.swimming = Random.Range(0, 101);
            stats.speed = Random.Range(0, 101);
            stats.strength = Random.Range(0, 101);
            stats.agility = Random.Range(0, 101);
            stats.intelligence = Random.Range(0, 101);
            stats.stamina = Random.Range(0, 101);
            stats.charisma = Random.Range(0, 101);
            stats.honesty = Random.Range(0, 101);
            stats.trust = Random.Range(0, 101);
            stats.honor = Random.Range(0, 101);

            Debug.Log($"[NPCGenerator] Generated stats - Speed: {stats.speed}, Strength: {stats.strength}, etc.");
            return stats;
        }
    }
} 