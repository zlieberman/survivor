using UnityEngine;
using System.Collections.Generic;
using Survivor.Shared;
using Survivor.Characters.UI;
using Survivor.Characters.Core;

namespace Survivor.Characters
{
    public enum Gender
    {
        Male,
        Female
    }

    public class Character : MonoBehaviour, IDialogueInteractable
    {
        [Header("Character Properties")]
        [SerializeField] protected string characterName;
        [SerializeField] protected string tribeName;
        [SerializeField] protected bool isPlayer;
        [SerializeField] protected Survivor.Shared.CharacterStats stats;
        [SerializeField] protected bool isInChallenge = false;
        [SerializeField] protected Dictionary<string, float> Relationships = new Dictionary<string, float>();
        [SerializeField] protected Gender gender;

        // Public properties for external access
        public string CharacterName 
        { 
            get => characterName;
            set => characterName = value;
        }
        public string TribeName 
        { 
            get => tribeName;
            set 
            {
                Debug.Log($"[Character] Setting tribe name for {gameObject.name} from '{tribeName}' to '{value}'");
                tribeName = value;
            }
        }
        public bool IsPlayer 
        { 
            get => isPlayer;
            set => isPlayer = value;
        }
        public Survivor.Shared.CharacterStats Stats 
        { 
            get => stats;
            set => stats = value;
        }
        public bool IsInChallenge 
        { 
            get => isInChallenge;
            set => isInChallenge = value;
        }
        public Gender Gender
        {
            get => gender;
            set => gender = value;
        }
        public IReadOnlyDictionary<string, float> CharacterRelationships => Relationships;

        public float Energy
        {
            get => stats.energy;
            set
            {
                stats.energy = value;
            }
        }

        public float Hunger
        {
            get => stats.hunger;
            set
            {
                stats.hunger = value;
            }
        }

        public float Thirst
        {
            get => stats.thirst;
            set
            {
                stats.thirst = value;
            }
        }

        // Property to access speed stat with change detection
        public float SpeedStat
        {
            get => stats?.speed ?? 0f;
            set
            {
                if (stats != null)
                {
                    float oldSpeed = stats.speed;
                    stats.speed = Mathf.Clamp(value, 0f, 100f);
                    Debug.Log($"[Character] Speed stat changed for {characterName}: {oldSpeed} -> {stats.speed}");
                    
                    // Trigger stat modifier update
                    var statModifier = GetComponent<CharacterStatModifier>();
                    if (statModifier != null)
                    {
                        statModifier.RefreshAllModifiers();
                    }
                }
            }
        }

        protected NameTag nameTag;
        protected bool isPlayerInRange = false;
        protected virtual void Awake()
        {
            Debug.Log($"[Character] Awake called for {gameObject.name}");
            // Initialize relationships dictionary if needed
            if (Relationships == null)
            {
                Relationships = new Dictionary<string, float>();
            }

            // Create name tag
            CreateNameTag();
        }

        protected virtual void CreateNameTag()
        {
            Debug.Log($"[Character] Creating name tag for {gameObject.name}");
            // Create name tag object
            GameObject nameTagObj = new GameObject("NameTag");
            nameTagObj.transform.SetParent(transform);
            nameTagObj.transform.localPosition = Vector3.zero;
            nameTagObj.transform.localRotation = Quaternion.identity;
            nameTag = nameTagObj.AddComponent<NameTag>();

            // If we already have a name, set it
            if (!string.IsNullOrEmpty(characterName))
            {
                nameTag.SetName(characterName);
            }
        }

        public virtual void Initialize(string name, string tribe, bool isPlayerCharacter, int id)
        {
            Debug.Log($"[Character] Initialize called for {gameObject.name} with name: {name}, tribe: {tribe}, isPlayer: {isPlayerCharacter}");
            characterName = name;
            tribeName = tribe;
            isPlayer = isPlayerCharacter;
            
            // Only initialize stats if they don't exist and haven't been set by a derived class
            if (stats == null)
            {
                Debug.Log($"[Character] Creating new stats for {gameObject.name}");
                stats = new Survivor.Shared.CharacterStats();
            }
            else
            {
                Debug.Log($"[Character] Using existing stats for {gameObject.name}");
            }

            // Ensure speed stat is initialized
            EnsureSpeedStatInitialized();

            // Set the name tag
            if (nameTag != null)
            {
                nameTag.SetName(characterName);
            }
            else
            {
                CreateNameTag();
            }

            Debug.Log($"[Character] Initialization complete for {gameObject.name}. Current values - Name: {characterName}, Tribe: {tribeName}, IsPlayer: {isPlayer}, Gender: {gender}");
        }

        protected virtual void EnsureSpeedStatInitialized()
        {
            // Only initialize speed if it's 0 (default value) and this is a player character
            if (stats.speed == 0 && isPlayer)
            {
                // Generate speed using normal distribution for players
                stats.speed = GenerateNormalRandom(50, 30);
                Debug.Log($"[Character] Generated speed stat for {characterName}: {stats.speed:F1}");
            }
        }

        protected float GenerateNormalRandom(int mean, int stdDev)
        {
            // Box-Muller transform for normal distribution
            float u1 = Random.value;
            float u2 = Random.value;
            float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Cos(2.0f * Mathf.PI * u2);
            float randNormal = mean + stdDev * randStdNormal;
            
            // Clamp to 0-100 range
            return Mathf.Clamp(randNormal, 0f, 100f);
        }

        public void UpdateRelationship(string otherCharacterName, float delta)
        {
            if (!Relationships.ContainsKey(otherCharacterName))
            {
                Relationships[otherCharacterName] = 0f;
            }
            Relationships[otherCharacterName] = Mathf.Clamp(Relationships[otherCharacterName] + delta, -1f, 1f);
        }

        public float GetRelationship(string otherCharacterName)
        {
            return Relationships.ContainsKey(otherCharacterName) ? Relationships[otherCharacterName] : 0f;
        }

        // IDialogueInteractable implementation
        public string GetDialogueId()
        {
            return characterName;
        }

        public string GetDisplayName()
        {
            return characterName;
        }

        public virtual void OnDialogueStart()
        {
            // Notify any listeners that dialogue has started\
        }

        public virtual void OnDialogueEnd()
        {
            // Notify any listeners that dialogue has ended
        }

        // Public method to manually set speed stat and update speed controller
        public void SetSpeedStat(float newSpeedStat)
        {
            stats.speed = Mathf.Clamp(newSpeedStat, 0f, 100f);
            Debug.Log($"[Character] Manually set speed stat for {characterName}: {stats.speed:F1}");
            
            // Trigger stat modifier update
            var statModifier = GetComponent<CharacterStatModifier>();
            if (statModifier != null)
            {
                statModifier.RefreshAllModifiers();
            }
        }

        // Generic method to set any stat and trigger updates
        public void SetStat(string statName, float newValue)
        {
            float clampedValue = Mathf.Clamp(newValue, 0f, 100f);
            
            switch (statName.ToLower())
            {
                case "speed":
                    stats.speed = clampedValue;
                    break;
                case "strength":
                    stats.strength = clampedValue;
                    break;
                case "stamina":
                    stats.stamina = clampedValue;
                    break;
                case "agility":
                    stats.agility = clampedValue;
                    break;
                case "swimming":
                    stats.swimming = clampedValue;
                    break;
                default:
                    Debug.LogWarning($"[Character] Unknown stat: {statName}");
                    return;
            }
            
            Debug.Log($"[Character] Manually set {statName} stat for {characterName}: {clampedValue:F1}");
            
            // Trigger stat modifier update
            var statModifier = GetComponent<CharacterStatModifier>();
            if (statModifier != null)
            {
                statModifier.RefreshAllModifiers();
            }
        }
    }
} 