using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Survivor.Shared;

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
            public string[] maleFirstNames;
            public string[] femaleFirstNames;
            public string[] lastNames;
        }

        private List<string> availableMaleFirstNames = new List<string>();
        private List<string> availableFemaleFirstNames = new List<string>();
        private List<string> availableLastNames = new List<string>();
        private HashSet<string> usedNames = new HashSet<string>();
        
        // Track gender counts per tribe
        private Dictionary<string, int> tribeMaleCounts = new Dictionary<string, int>();
        private Dictionary<string, int> tribeFemaleCounts = new Dictionary<string, int>();

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
                availableMaleFirstNames = new List<string>(nameData.maleFirstNames);
                availableFemaleFirstNames = new List<string>(nameData.femaleFirstNames);
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
            // Fisher-Yates shuffle for male first names
            for (int i = availableMaleFirstNames.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                string temp = availableMaleFirstNames[i];
                availableMaleFirstNames[i] = availableMaleFirstNames[j];
                availableMaleFirstNames[j] = temp;
            }

            // Fisher-Yates shuffle for female first names
            for (int i = availableFemaleFirstNames.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                string temp = availableFemaleFirstNames[i];
                availableFemaleFirstNames[i] = availableFemaleFirstNames[j];
                availableFemaleFirstNames[j] = temp;
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

        public string GenerateRandomName(Gender gender)
        {
            // Check if we need to reset the name pools
            if ((gender == Gender.Male && availableMaleFirstNames.Count == 0) ||
                (gender == Gender.Female && availableFemaleFirstNames.Count == 0) ||
                availableLastNames.Count == 0)
            {
                LoadNameData();
            }

            string firstName;
            if (gender == Gender.Male)
            {
                firstName = availableMaleFirstNames[0];
                availableMaleFirstNames.RemoveAt(0);
            }
            else
            {
                firstName = availableFemaleFirstNames[0];
                availableFemaleFirstNames.RemoveAt(0);
            }

            string lastName = availableLastNames[0];
            availableLastNames.RemoveAt(0);

            string fullName = $"{firstName} {lastName}";
            usedNames.Add(fullName);

            Debug.Log($"[NPCGenerator] Generated {gender} name: {fullName}");
            return fullName;
        }

        public Gender AssignGender(string tribeName)
        {
            // Initialize tribe counts if not present
            if (!tribeMaleCounts.ContainsKey(tribeName))
            {
                tribeMaleCounts[tribeName] = 0;
                tribeFemaleCounts[tribeName] = 0;
            }

            // Ensure even split within the tribe
            if (tribeMaleCounts[tribeName] <= tribeFemaleCounts[tribeName])
            {
                tribeMaleCounts[tribeName]++;
                Debug.Log($"[NPCGenerator] Assigned Male gender for tribe {tribeName} (Male: {tribeMaleCounts[tribeName]}, Female: {tribeFemaleCounts[tribeName]})");
                return Gender.Male;
            }
            else
            {
                tribeFemaleCounts[tribeName]++;
                Debug.Log($"[NPCGenerator] Assigned Female gender for tribe {tribeName} (Male: {tribeMaleCounts[tribeName]}, Female: {tribeFemaleCounts[tribeName]})");
                return Gender.Female;
            }
        }

        public void ReleaseName(string name)
        {
            if (usedNames.Contains(name))
            {
                string[] nameParts = name.Split(' ');
                if (nameParts.Length == 2)
                {
                    // We don't know which gender pool to return the name to
                    // So we'll just keep it out of circulation
                    Debug.Log($"[NPCGenerator] Released name: {name} (not returned to pool due to gender-specific pools)");
                }
                usedNames.Remove(name);
            }
        }

        public Survivor.Shared.CharacterStats GenerateRandomStats()
        {
            Survivor.Shared.CharacterStats stats = new Survivor.Shared.CharacterStats();
            
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