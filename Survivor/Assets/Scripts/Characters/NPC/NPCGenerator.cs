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

        [System.Serializable]
        private class NameData
        {
            public string[] firstNames;
            public string[] lastNames;
        }

        private List<string> availableFirstNames = new List<string>();
        private List<string> availableLastNames = new List<string>();
        private HashSet<string> usedNames = new HashSet<string>();

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
            TextAsset configFile = Resources.Load<TextAsset>("names");
            if (configFile != null)
            {
                NameData nameData = JsonUtility.FromJson<NameData>(configFile.text);
                availableFirstNames = new List<string>(nameData.firstNames);
                availableLastNames = new List<string>(nameData.lastNames);
                ShuffleNames();
                Debug.Log("[NPCGenerator] Loaded name data successfully");
            }
            else
            {
                Debug.LogError("[NPCGenerator] Failed to load names.json from Resources folder!");
            }
        }

        private void ShuffleNames()
        {
            // Fisher-Yates shuffle for first names
            for (int i = availableFirstNames.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                string temp = availableFirstNames[i];
                availableFirstNames[i] = availableFirstNames[j];
                availableFirstNames[j] = temp;
            }

            // Fisher-Yates shuffle for last names
            for (int i = availableLastNames.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                string temp = availableLastNames[i];
                availableLastNames[i] = availableLastNames[j];
                availableLastNames[j] = temp;
            }
        }

        public string GenerateRandomName()
        {
            if (availableFirstNames.Count == 0 || availableLastNames.Count == 0)
            {
                // If we run out of names, reset the pools
                LoadNameData();
            }

            string firstName = availableFirstNames[0];
            string lastName = availableLastNames[0];
            string fullName = $"{firstName} {lastName}";

            availableFirstNames.RemoveAt(0);
            availableLastNames.RemoveAt(0);
            usedNames.Add(fullName);

            Debug.Log($"[NPCGenerator] Generated name: {fullName}");
            return fullName;
        }

        public void ReleaseName(string name)
        {
            if (usedNames.Contains(name))
            {
                string[] nameParts = name.Split(' ');
                if (nameParts.Length == 2)
                {
                    availableFirstNames.Add(nameParts[0]);
                    availableLastNames.Add(nameParts[1]);
                }
                usedNames.Remove(name);
                Debug.Log($"[NPCGenerator] Released name: {name}");
            }
        }

        public CharacterStats GenerateRandomStats()
        {
            CharacterStats stats = new CharacterStats();
            
            // Generate stats using normal distribution
            stats.perception = GenerateNormalRandom(50, 30);
            stats.deception = GenerateNormalRandom(50, 30);
            stats.persuasion = GenerateNormalRandom(50, 30);
            stats.puzzleSolving = GenerateNormalRandom(50, 30);
            stats.swimming = GenerateNormalRandom(50, 30);
            stats.speed = GenerateNormalRandom(50, 30);
            stats.strength = GenerateNormalRandom(50, 30);
            stats.agility = GenerateNormalRandom(50, 30);
            stats.intelligence = GenerateNormalRandom(50, 30);
            stats.stamina = GenerateNormalRandom(50, 30);
            stats.charisma = GenerateNormalRandom(50, 30);
            stats.honesty = GenerateNormalRandom(50, 30);
            stats.trust = GenerateNormalRandom(50, 30);
            stats.honor = GenerateNormalRandom(50, 30);

            Debug.Log("[NPCGenerator] Generated random stats");
            return stats;
        }

        private int GenerateNormalRandom(int mean, int stdDev)
        {
            // Box-Muller transform
            float u1 = Random.value;
            float u2 = Random.value;
            float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Cos(2.0f * Mathf.PI * u2);
            float randNormal = mean + stdDev * randStdNormal;
            
            // Clamp to 0-100 range
            return Mathf.Clamp(Mathf.RoundToInt(randNormal), 0, 100);
        }
    }
} 