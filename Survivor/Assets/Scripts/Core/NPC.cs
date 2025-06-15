using UnityEngine;
using System;
using System.Collections.Generic;

namespace Survivor.Core
{
    /// <summary>
    /// Represents a contestant in the game with personality traits, stats, and relationships
    /// </summary>
    [Serializable]
    public class NPC : MonoBehaviour
    {
        [Header("Identity")]
        public string npcName;
        public int playerId;

        [Header("Personality Traits")]
        [Range(1, 10)]
        public int loyalty = 5;
        [Range(1, 10)]
        public int sneakiness = 5;
        [Range(1, 10)]
        public int charisma = 5;
        [Range(1, 10)]
        public int aggression = 5;

        [Header("Challenge Stats")]
        [Range(1, 10)]
        public int strength = 5;
        [Range(1, 10)]
        public int agility = 5;
        [Range(1, 10)]
        public int puzzleSkill = 5;

        [Header("Social State")]
        public float currentMood = 1.0f; // 0 = terrible, 1 = neutral, 2 = excellent
        public Dictionary<int, float> trustScores = new Dictionary<int, float>(); // PlayerID -> Trust Level
        public List<int> currentAlliances = new List<int>(); // List of PlayerIDs
        public List<string> recentMemories = new List<string>(); // Store recent significant events

        private void Awake()
        {
            // Initialize any required components
        }

        /// <summary>
        /// Updates trust score towards another player
        /// </summary>
        public void UpdateTrust(int targetPlayerId, float delta)
        {
            if (!trustScores.ContainsKey(targetPlayerId))
            {
                trustScores[targetPlayerId] = 1.0f; // Start at neutral
            }
            
            trustScores[targetPlayerId] = Mathf.Clamp(trustScores[targetPlayerId] + delta, 0f, 2f);
        }

        /// <summary>
        /// Adds or removes an alliance with another player
        /// </summary>
        public void UpdateAlliance(int targetPlayerId, bool addAlliance)
        {
            if (addAlliance && !currentAlliances.Contains(targetPlayerId))
            {
                currentAlliances.Add(targetPlayerId);
            }
            else if (!addAlliance)
            {
                currentAlliances.Remove(targetPlayerId);
            }
        }

        /// <summary>
        /// Adds a new memory to the NPC's recent history
        /// </summary>
        public void AddMemory(string memory)
        {
            recentMemories.Insert(0, memory);
            if (recentMemories.Count > 10) // Keep only recent memories
            {
                recentMemories.RemoveAt(recentMemories.Count - 1);
            }
        }

        /// <summary>
        /// Calculates performance in a challenge based on relevant stats
        /// </summary>
        public float CalculateChallengePerformance(float strengthWeight, float agilityWeight, float puzzleWeight)
        {
            return (strength * strengthWeight + 
                    agility * agilityWeight + 
                    puzzleSkill * puzzleWeight) / 
                    (strengthWeight + agilityWeight + puzzleWeight);
        }
    }
} 