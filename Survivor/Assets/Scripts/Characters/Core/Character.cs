using UnityEngine;
using System.Collections.Generic;
using Survivor.Shared;
using Survivor.Characters.Dialogue;
using Survivor.Characters.UI;

namespace Survivor.Characters
{
    public class Character : MonoBehaviour, IDialogueInteractable
    {
        [Header("Character Properties")]
        [SerializeField] protected string characterName;
        [SerializeField] protected string tribeName;
        [SerializeField] protected bool isPlayer;
        [SerializeField] protected CharacterStats stats;
        [SerializeField] protected bool isInChallenge = false;
        [SerializeField] protected Dictionary<string, float> Relationships = new Dictionary<string, float>();

        // Public properties for external access
        public string CharacterName 
        { 
            get => characterName;
            set => characterName = value;
        }
        public string TribeName 
        { 
            get => tribeName;
            set => tribeName = value;
        }
        public bool IsPlayer 
        { 
            get => isPlayer;
            set => isPlayer = value;
        }
        public CharacterStats Stats 
        { 
            get => stats;
            set => stats = value;
        }
        public bool IsInChallenge 
        { 
            get => isInChallenge;
            set => isInChallenge = value;
        }
        public IReadOnlyDictionary<string, float> CharacterRelationships => Relationships;

        protected NameTag nameTag;
        protected bool isPlayerInRange = false;
        protected DialogueManager dialogueManager;

        protected virtual void Awake()
        {
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
            characterName = name;
            tribeName = tribe;
            isPlayer = isPlayerCharacter;
            
            // Initialize stats if they don't exist
            if (stats == null)
            {
                stats = new CharacterStats();
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