using UnityEngine;
using System;
using System.Collections.Generic;
using Survivor.Characters;

namespace Survivor.Challenges
{
    public enum ChallengeDifficulty
    {
        Easy,
        Medium,
        Hard
    }

    public enum ChallengeType
    {
        Physical,      // Strength-focused
        Agility,      // Speed and dexterity
        Puzzle,       // Mental challenges
        Hybrid,       // Mix of different skills
        Mental,       // Mental challenges
        Social,       // Social challenges
        Endurance,    // Endurance challenges
        Race         // Race challenges
    }

    [Serializable]
    public class ChallengeData
    {
        public string id;
        public string title;
        public string description;
        public string sceneName;
        public Sprite thumbnail;
        public ChallengeDifficulty difficulty;
        public ChallengeType type;
        public float strengthWeight;
        public float agilityWeight;
        public float puzzleWeight;
        public float duration;
        public bool isTeamChallenge;
    }

    [Serializable]
    public class ChallengeRuntime
    {
        public ChallengeData data;
        public Character winner;
        public Dictionary<string, float> participantScores = new Dictionary<string, float>();
        public float remainingTime;
        public float progress;
        public bool isActive;
    }

    public abstract class Challenge : ScriptableObject
    {
        public ChallengeData data;
        protected ChallengeRuntime runtime;

        // Public properties to access runtime data
        public Dictionary<string, float> ParticipantScores => runtime.participantScores;
        public float RemainingTime => runtime.remainingTime;
        public float Progress => runtime.progress;
        public bool IsActive => runtime.isActive;
        public Character Winner => runtime.winner;

        public virtual void Initialize()
        {
            runtime = new ChallengeRuntime
            {
                data = data,
                participantScores = new Dictionary<string, float>(),
                remainingTime = data.duration,
                progress = 0f,
                isActive = false
            };
        }

        public virtual void StartChallenge()
        {
            runtime.isActive = true;
            runtime.remainingTime = data.duration;
            runtime.progress = 0f;
            runtime.winner = null;
            runtime.participantScores.Clear();
        }

        public virtual void EndChallenge()
        {
            runtime.isActive = false;
        }

        public virtual bool IsCompleted()
        {
            return runtime.progress >= 1f;
        }

        public virtual float GetProgress()
        {
            return runtime.progress;
        }

        public virtual void UpdateProgress(float deltaTime)
        {
            if (!runtime.isActive) return;

            runtime.remainingTime -= deltaTime;
            runtime.progress = 1f - (runtime.remainingTime / data.duration);
        }

        public virtual void AddParticipant(string memberName)
        {
            if (!runtime.participantScores.ContainsKey(memberName))
            {
                runtime.participantScores[memberName] = 0f;
            }
        }

        public virtual void UpdateScore(string memberName, float score)
        {
            if (runtime.participantScores.ContainsKey(memberName))
            {
                runtime.participantScores[memberName] += score;
            }
        }
    }
} 