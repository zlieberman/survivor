using UnityEngine;
using System.Collections.Generic;
using Survivor.Shared;
using Survivor.Characters.Dialogue;
using Survivor.Characters.UI;

namespace Survivor.Characters
{
    public enum Gender
    {
        Male,
        Female
    }

    public class Character : MonoBehaviour, IDialogueInteractable, ICharacterStats
    {
        [Header("Character Properties")]
        [SerializeField] protected string characterName;
        [SerializeField] protected string tribeName;
        [SerializeField] protected bool isPlayer;
        [SerializeField] protected Survivor.Shared.CharacterStats stats;
        [SerializeField] protected bool isInChallenge = false;
        [SerializeField] protected Dictionary<string, float> Relationships = new Dictionary<string, float>();
        [SerializeField] protected Gender gender;

        // ICharacterStats implementation
        public float thirst
        {
            get => stats.thirst;
            set => stats.thirst = value;
        }

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

        protected NameTag nameTag;
        protected bool isPlayerInRange = false;
        protected DialogueManager dialogueManager;

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

            // Get dialogue manager
            dialogueManager = FindObjectOfType<DialogueManager>();
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
            // Notify any listeners that dialogue has started
            if (dialogueManager != null)
            {
                dialogueManager.StartDialogue(GetDisplayName(), "Starting conversation...");
            }
        }

        public virtual void OnDialogueEnd()
        {
            // Notify any listeners that dialogue has ended
            if (dialogueManager != null)
            {
                dialogueManager.EndDialogue();
            }
        }
    }
} 