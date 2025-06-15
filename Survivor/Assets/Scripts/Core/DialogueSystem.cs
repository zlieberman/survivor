using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace Survivor.Core
{
    public class DialogueSystem : MonoBehaviour
    {
        [System.Serializable]
        public class DialogueData
        {
            public string speakerName;
            public string listenerName;
            public string content;
            public float relationshipImpact;
        }

        [Header("Dialogue Settings")]
        public float minDialogueDuration = 5f;
        public float maxDialogueDuration = 15f;
        public float dialogueCooldown = 30f;

        [Header("Events")]
        public UnityEvent<DialogueData> onDialogueStart;
        public UnityEvent<DialogueData> onDialogueEnd;

        private NPCManager npcManager;
        private Dictionary<string, float> lastDialogueTime = new Dictionary<string, float>();

        public void Initialize(NPCManager npcManager)
        {
            this.npcManager = npcManager;
        }

        public void InitiateDialogue(string initiatorName, string targetName)
        {
            if (!CanInitiateDialogue(initiatorName, targetName))
                return;

            NPCManager.NPCData initiator = npcManager.GetNPCData(initiatorName);
            NPCManager.NPCData target = npcManager.GetNPCData(targetName);

            if (initiator == null || target == null)
                return;

            DialogueData dialogue = GenerateDialogue(initiator, target);
            if (dialogue != null)
            {
                StartCoroutine(ProcessDialogue(dialogue));
            }
        }

        private bool CanInitiateDialogue(string initiatorName, string targetName)
        {
            // Check cooldown
            if (lastDialogueTime.ContainsKey(initiatorName) &&
                Time.time - lastDialogueTime[initiatorName] < dialogueCooldown)
                return false;

            if (lastDialogueTime.ContainsKey(targetName) &&
                Time.time - lastDialogueTime[targetName] < dialogueCooldown)
                return false;

            return true;
        }

        private DialogueData GenerateDialogue(NPCManager.NPCData initiator, NPCManager.NPCData target)
        {
            DialogueData dialogue = new DialogueData
            {
                speakerName = initiator.name,
                listenerName = target.name,
                content = GenerateDialogueContent(initiator, target),
                relationshipImpact = CalculateRelationshipImpact(initiator, target)
            };

            return dialogue;
        }

        private string GenerateDialogueContent(NPCManager.NPCData speaker, NPCManager.NPCData listener)
        {
            float relationship = npcManager.GetRelationship(speaker.name, listener.name);
            bool areAllied = npcManager.AreAllied(speaker.name, listener.name);

            // Generate dialogue based on relationship and personality traits
            string content = "";

            if (areAllied)
            {
                if (relationship > 70)
                {
                    content = GenerateStrongAllianceDialogue(speaker, listener);
                }
                else
                {
                    content = GenerateWeakAllianceDialogue(speaker, listener);
                }
            }
            else
            {
                if (relationship > 50)
                {
                    content = GenerateFriendlyDialogue(speaker, listener);
                }
                else
                {
                    content = GenerateHostileDialogue(speaker, listener);
                }
            }

            return content;
        }

        private string GenerateStrongAllianceDialogue(NPCManager.NPCData speaker, NPCManager.NPCData listener)
        {
            string[] templates = {
                "We need to stick together in the next vote, {0}. I've got your back.",
                "I trust you completely, {0}. Let's make it to the end together.",
                "You're my closest ally here, {0}. We should discuss our strategy."
            };

            return string.Format(templates[Random.Range(0, templates.Length)], listener.name);
        }

        private string GenerateWeakAllianceDialogue(NPCManager.NPCData speaker, NPCManager.NPCData listener)
        {
            string[] templates = {
                "I hope we can continue working together, {0}. What are your thoughts?",
                "We should maintain our alliance, {0}, but we need to be careful.",
                "I want to trust you, {0}, but this game makes it difficult."
            };

            return string.Format(templates[Random.Range(0, templates.Length)], listener.name);
        }

        private string GenerateFriendlyDialogue(NPCManager.NPCData speaker, NPCManager.NPCData listener)
        {
            string[] templates = {
                "Hey {0}, would you be interested in working together?",
                "I think we could help each other in this game, {0}.",
                "We haven't talked much, {0}, but I'd like to change that."
            };

            return string.Format(templates[Random.Range(0, templates.Length)], listener.name);
        }

        private string GenerateHostileDialogue(NPCManager.NPCData speaker, NPCManager.NPCData listener)
        {
            string[] templates = {
                "I know you're plotting against me, {0}. That's not a smart move.",
                "You should be careful about your choices, {0}.",
                "We both know only one of us can win, {0}."
            };

            return string.Format(templates[Random.Range(0, templates.Length)], listener.name);
        }

        private float CalculateRelationshipImpact(NPCManager.NPCData speaker, NPCManager.NPCData listener)
        {
            float baseImpact = Random.Range(-5f, 5f);

            // Modify impact based on charisma and personality traits
            float charismaFactor = speaker.charisma / 100f;
            baseImpact *= charismaFactor;

            // Add some randomness based on sneakiness
            float sneakinessFactor = speaker.sneakiness / 100f;
            baseImpact += Random.Range(-sneakinessFactor * 2f, sneakinessFactor * 2f);

            return Mathf.Clamp(baseImpact, -10f, 10f);
        }

        private System.Collections.IEnumerator ProcessDialogue(DialogueData dialogue)
        {
            // Update last dialogue time
            lastDialogueTime[dialogue.speakerName] = Time.time;
            lastDialogueTime[dialogue.listenerName] = Time.time;

            // Start dialogue
            onDialogueStart?.Invoke(dialogue);

            // Wait for dialogue duration
            float duration = Random.Range(minDialogueDuration, maxDialogueDuration);
            yield return new WaitForSeconds(duration);

            // Update relationships
            npcManager.UpdateRelationship(dialogue.listenerName, dialogue.speakerName, dialogue.relationshipImpact);
            npcManager.UpdateRelationship(dialogue.speakerName, dialogue.listenerName, dialogue.relationshipImpact * 0.5f);

            // End dialogue
            onDialogueEnd?.Invoke(dialogue);
        }

        public void ForceEndDialogue(string speakerName, string listenerName)
        {
            lastDialogueTime[speakerName] = Time.time;
            lastDialogueTime[listenerName] = Time.time;
        }
    }
} 