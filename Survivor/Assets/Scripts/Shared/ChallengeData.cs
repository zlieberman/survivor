using System;
using System.Collections.Generic;
using UnityEngine;

namespace Survivor.Shared
{
    /// <summary>
    /// Types of challenges available in the game
    /// </summary>
    public enum ChallengeType
    {
        Race,
        Puzzle,
        Physical,
        Mental,
        Team,
        Individual
    }
    
    /// <summary>
    /// Current state of a challenge session
    /// </summary>
    public enum ChallengeState
    {
        NotStarted,
        InProgress,
        Completed,
        Failed
    }
    
    /// <summary>
    /// Configuration for the challenge system
    /// </summary>
    [System.Serializable]
    public class ChallengeConfig
    {
        [Header("Timing")]
        public int challengeHour = 12; // 12:00 PM
        public int challengeMinute = 0;
        public int firstChallengeDay = 2; // Start challenges on day 2
        
        [Header("Team Management")]
        public bool autoBalanceTeams = true;
        public int maxTeamSize = 4;
        public int maxSitOutPlayers = 2;
        
        [Header("Challenge Selection")]
        public bool randomizeChallenges = true;
        public bool avoidRepeatingChallenges = true;
        public int maxConsecutiveRepeats = 3;
        
        [Header("Immunity")]
        public bool enableImmunity = true;
        public int defaultImmunityDays = 1;
    }
    
    /// <summary>
    /// Definition of a specific challenge
    /// </summary>
    [System.Serializable]
    public class ChallengeDefinition
    {
        public string challengeId;
        public string challengeName;
        public string description;
        public ChallengeType challengeType;
        public int minPlayers;
        public int maxPlayers;
        public string challengeSceneName;
        public float estimatedDuration; // in seconds
        public bool isTribeChallenge;
        public bool requiresEvenTeams;
        public int maxSitOutPlayers;
        public bool grantsImmunity;
        public int immunityDays;
        public bool isEnabled;
        public int difficulty; // 1-5 scale
    }
    
    /// <summary>
    /// A participant in a challenge (player or NPC)
    /// </summary>
    [System.Serializable]
    public class ChallengeParticipant
    {
        public string playerId;
        public string playerName;
        public string tribeId;
        public string tribeName;
        public bool isMainPlayer;
        public Vector3 lastKnownPosition;
        public Quaternion lastKnownRotation;
        public bool isActive;
        public float completionTime; // For race challenges
        public int score; // For scoring challenges
    }
    
    /// <summary>
    /// A complete challenge session
    /// </summary>
    [System.Serializable]
    public class ChallengeSession
    {
        public string sessionId;
        public ChallengeDefinition challenge;
        public int dayNumber;
        public DateTime startTime;
        public DateTime? endTime;
        public ChallengeState state;
        public List<ChallengeParticipant> participants;
        public List<ChallengeParticipant> sitOutPlayers;
        public bool isActive;
        public Dictionary<string, float> tribeProgress; // For tracking progress
        public Dictionary<string, int> tribeScores; // For scoring challenges
    }
    
    /// <summary>
    /// Result of a completed challenge
    /// </summary>
    [System.Serializable]
    public class ChallengeResult
    {
        public string sessionId;
        public string challengeId;
        public string challengeName;
        public int dayNumber;
        public DateTime completionTime;
        public string winningTribeId;
        public string winningTribeName;
        public List<string> winningParticipants;
        public bool immunityGranted;
        public int immunityDaysGranted;
        public float challengeDuration; // in seconds
        public Dictionary<string, float> participantTimes; // For race challenges
        public Dictionary<string, int> participantScores; // For scoring challenges
        public string notes; // Additional notes about the challenge
    }
} 