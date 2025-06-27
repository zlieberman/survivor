using System;
using System.Collections.Generic;
using UnityEngine;
using Survivor.Shared;

namespace Survivor.Challenges
{
    /// <summary>
    /// Base class for challenge controllers that provides common functionality
    /// </summary>
    public abstract class BaseChallengeController : MonoBehaviour, IChallengeController
    {
        [Header("Challenge State")]
        [SerializeField] protected ChallengeSession currentSession;
        [SerializeField] protected bool isInitialized = false;
        [SerializeField] protected bool isActive = false;
        [SerializeField] protected float currentProgress = 0f;
        [SerializeField] protected Dictionary<string, int> teamScores = new Dictionary<string, int>();
        
        // Events
        public event Action<ChallengeResult> OnChallengeEnded;
        public event Action<float> OnProgressUpdated;
        public event Action<Dictionary<string, int>> OnScoresUpdated;
        
        public virtual void InitializeChallenge(ChallengeSession session)
        {
            currentSession = session;
            isInitialized = true;
            isActive = false;
            currentProgress = 0f;
            teamScores.Clear();
            
            // Initialize team scores
            if (session.participants != null)
            {
                foreach (var participant in session.participants)
                {
                    if (!teamScores.ContainsKey(participant.tribeId))
                    {
                        teamScores[participant.tribeId] = 0;
                    }
                }
            }
            
            OnInitialized();
            Debug.Log($"[{GetType().Name}] Challenge initialized: {session.challenge.challengeName}");
        }
        
        public virtual void StartChallenge()
        {
            if (!isInitialized) return;
            
            isActive = true;
            currentSession.state = ChallengeState.InProgress;
            currentSession.startTime = DateTime.Now;
            
            OnChallengeStarted();
            Debug.Log($"[{GetType().Name}] Challenge started: {currentSession.challenge.challengeName}");
        }
        
        public virtual void EndChallenge(string winningTribeId, string notes = "")
        {
            if (!isInitialized) return;
            
            isActive = false;
            currentSession.state = ChallengeState.Completed;
            currentSession.endTime = DateTime.Now;
            
            var result = CreateResult(winningTribeId, notes);
            
            OnChallengeEnded?.Invoke(result);
            Debug.Log($"[{GetType().Name}] Challenge ended. Winner: {winningTribeId}");
        }
        
        public virtual void PauseChallenge()
        {
            if (!isActive) return;
            
            isActive = false;
            OnChallengePaused();
            Debug.Log($"[{GetType().Name}] Challenge paused");
        }
        
        public virtual void ResumeChallenge()
        {
            if (isActive) return;
            
            isActive = true;
            OnChallengeResumed();
            Debug.Log($"[{GetType().Name}] Challenge resumed");
        }
        
        public virtual float GetProgress()
        {
            return currentProgress;
        }
        
        public virtual Dictionary<string, int> GetScores()
        {
            return new Dictionary<string, int>(teamScores);
        }
        
        protected virtual void UpdateProgress(float newProgress)
        {
            currentProgress = Mathf.Clamp01(newProgress);
            OnProgressUpdated?.Invoke(currentProgress);
        }
        
        protected virtual void UpdateScore(string tribeId, int score)
        {
            if (teamScores.ContainsKey(tribeId))
            {
                teamScores[tribeId] = score;
                OnScoresUpdated?.Invoke(new Dictionary<string, int>(teamScores));
            }
        }
        
        protected virtual void AddScore(string tribeId, int points)
        {
            if (teamScores.ContainsKey(tribeId))
            {
                teamScores[tribeId] += points;
                OnScoresUpdated?.Invoke(new Dictionary<string, int>(teamScores));
            }
        }
        
        protected virtual ChallengeResult CreateResult(string winningTribeId, string notes = "")
        {
            var duration = currentSession.endTime.HasValue 
                ? (float)(currentSession.endTime.Value - currentSession.startTime).TotalSeconds 
                : 0f;
            
            var winningTribeName = "";
            var winningParticipants = new List<string>();
            
            // Find winning tribe name and participants
            if (currentSession.participants != null)
            {
                foreach (var participant in currentSession.participants)
                {
                    if (participant.tribeId == winningTribeId)
                    {
                        if (string.IsNullOrEmpty(winningTribeName))
                        {
                            winningTribeName = participant.tribeName;
                        }
                        winningParticipants.Add(participant.playerName);
                    }
                }
            }
            
            return new ChallengeResult
            {
                sessionId = currentSession.sessionId,
                challengeId = currentSession.challenge.challengeId,
                challengeName = currentSession.challenge.challengeName,
                dayNumber = currentSession.dayNumber,
                completionTime = DateTime.Now,
                winningTribeId = winningTribeId,
                winningTribeName = winningTribeName,
                winningParticipants = winningParticipants,
                immunityGranted = currentSession.challenge.grantsImmunity,
                immunityDaysGranted = currentSession.challenge.immunityDays,
                challengeDuration = duration,
                participantTimes = new Dictionary<string, float>(), // To be filled by specific challenges
                participantScores = new Dictionary<string, int>(teamScores),
                notes = notes
            };
        }
        
        // Abstract methods that must be implemented by derived classes
        protected abstract void OnInitialized();
        protected abstract void OnChallengeStarted();
        protected abstract void OnChallengePaused();
        protected abstract void OnChallengeResumed();
        
        // Optional virtual methods that can be overridden
        protected virtual void OnChallengeEndedCallback() { }
        
        // Unity lifecycle methods
        protected virtual void Update()
        {
            if (isActive && isInitialized)
            {
                OnChallengeUpdate();
            }
        }
        
        protected virtual void OnChallengeUpdate()
        {
            // Override in derived classes to implement challenge-specific logic
        }
    }
} 