using UnityEngine;
using System;

namespace Survivor.Challenges
{
    [Serializable]
    public abstract class Challenge : ScriptableObject
    {
        public string id;
        public string title;
        public string description;
        public string sceneName;
        public Sprite thumbnail;
        public ChallengeDifficulty difficulty;
        public ChallengeType type;
        
        public abstract void Initialize();
        public abstract void StartChallenge();
        public abstract void EndChallenge();
        public abstract bool IsCompleted();
        public abstract float GetProgress();
    }

    public enum ChallengeDifficulty
    {
        Easy,
        Medium,
        Hard
    }

    public enum ChallengeType
    {
        Puzzle,
        Endurance,
        Race,
        // Add more types as needed
    }
} 