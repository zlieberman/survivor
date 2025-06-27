using System;
using UnityEngine;
using Survivor.Shared;

namespace Survivor.Challenges
{
    /// <summary>
    /// Interface for challenge controllers that manage specific challenge types
    /// </summary>
    public interface IChallengeController
    {
        /// <summary>
        /// Initialize the challenge with session data
        /// </summary>
        void InitializeChallenge(ChallengeSession session);
        
        /// <summary>
        /// Start the challenge
        /// </summary>
        void StartChallenge();
        
        /// <summary>
        /// End the challenge with a result
        /// </summary>
        void EndChallenge(string winningTribeId, string notes = "");
        
        /// <summary>
        /// Pause the challenge
        /// </summary>
        void PauseChallenge();
        
        /// <summary>
        /// Resume the challenge
        /// </summary>
        void ResumeChallenge();
        
        /// <summary>
        /// Get the current progress of the challenge (0-1)
        /// </summary>
        float GetProgress();
        
        /// <summary>
        /// Get the current scores for each tribe
        /// </summary>
        System.Collections.Generic.Dictionary<string, int> GetScores();
        
        /// <summary>
        /// Event fired when the challenge ends
        /// </summary>
        event Action<ChallengeResult> OnChallengeEnded;
        
        /// <summary>
        /// Event fired when challenge progress updates
        /// </summary>
        event Action<float> OnProgressUpdated;
        
        /// <summary>
        /// Event fired when scores update
        /// </summary>
        event Action<System.Collections.Generic.Dictionary<string, int>> OnScoresUpdated;
    }
} 