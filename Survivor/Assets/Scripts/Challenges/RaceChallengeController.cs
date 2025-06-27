using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Survivor.Characters;
using Survivor.Shared;
using StarterAssets;

namespace Survivor.Challenges
{
    /// <summary>
    /// Controller for race-based challenges where teams compete to reach the finish line
    /// </summary>
    public class RaceChallengeController : BaseChallengeController
    {
        [Header("Race Configuration")]
        [SerializeField] private Transform[] checkpoints;
        [SerializeField] private Transform finishLine;
        [SerializeField] private float raceTimeout = 300f; // 5 minutes
        [SerializeField] private bool requireAllCheckpoints = true;
        
        [Header("Race State")]
        [SerializeField] private Dictionary<string, int> tribeCheckpointProgress = new Dictionary<string, int>();
        [SerializeField] private Dictionary<string, float> tribeCompletionTimes = new Dictionary<string, float>();
        [SerializeField] private float raceStartTime;
        [SerializeField] private bool raceFinished = false;
        
        private Dictionary<string, List<ChallengeParticipant>> tribes;
        
        protected override void OnInitialized()
        {
            // Group participants by tribe
            tribes = currentSession.participants
                .GroupBy(p => p.tribeId)
                .ToDictionary(g => g.Key, g => g.ToList());
            
            // Initialize checkpoint progress for each tribe
            foreach (var tribeId in tribes.Keys)
            {
                tribeCheckpointProgress[tribeId] = 0;
                tribeCompletionTimes[tribeId] = -1f; // -1 means not finished
            }
            
            Debug.Log($"[RaceChallengeController] Initialized with {tribes.Count} tribes");
        }
        
        protected override void OnChallengeStarted()
        {
            raceStartTime = Time.time;
            raceFinished = false;
            
            // Reset all progress
            foreach (var tribeId in tribeCheckpointProgress.Keys.ToList())
            {
                tribeCheckpointProgress[tribeId] = 0;
                tribeCompletionTimes[tribeId] = -1f;
            }
            
            UpdateProgress(0f);
            Debug.Log("[RaceChallengeController] Race started!");
        }
        
        protected override void OnChallengePaused()
        {
            Debug.Log("[RaceChallengeController] Race paused");
        }
        
        protected override void OnChallengeResumed()
        {
            Debug.Log("[RaceChallengeController] Race resumed");
        }
        
        protected override void OnChallengeUpdate()
        {
            if (raceFinished) return;
            
            // Check for timeout
            if (Time.time - raceStartTime > raceTimeout)
            {
                EndRaceByTimeout();
                return;
            }
            
            // Check if any tribe has finished
            CheckForRaceCompletion();
            
            // Update progress based on checkpoint completion
            UpdateRaceProgress();
        }
        
        private void CheckForRaceCompletion()
        {
            foreach (var tribeId in tribes.Keys)
            {
                if (tribeCompletionTimes[tribeId] >= 0f) continue; // Already finished
                
                var tribeMembers = tribes[tribeId];
                bool allMembersFinished = true;
                
                foreach (var participant in tribeMembers)
                {
                    // Check if this participant has reached the finish line
                    // This would be implemented based on your player tracking system
                    if (!HasParticipantFinished(participant))
                    {
                        allMembersFinished = false;
                        break;
                    }
                }
                
                if (allMembersFinished)
                {
                    tribeCompletionTimes[tribeId] = Time.time - raceStartTime;
                    Debug.Log($"[RaceChallengeController] Tribe {tribeId} finished in {tribeCompletionTimes[tribeId]:F2} seconds");
                }
            }
            
            // Check if all tribes have finished
            var finishedTribes = tribeCompletionTimes.Values.Count(t => t >= 0f);
            if (finishedTribes >= tribes.Count)
            {
                EndRace();
            }
        }
        
        private bool HasParticipantFinished(ChallengeParticipant participant)
        {
            // This is a simplified check - in a real implementation, you'd track each participant's position
            // For now, we'll use a random simulation
            if (participant.isMainPlayer)
            {
                // Check if main player is near finish line
                var mainPlayer = FindObjectOfType<ThirdPersonController>();
                if (mainPlayer != null && finishLine != null)
                {
                    float distance = Vector3.Distance(mainPlayer.transform.position, finishLine.position);
                    return distance < 2f; // Within 2 units of finish line
                }
            }
            
            // For NPCs, simulate progress based on time
            float expectedFinishTime = 60f + UnityEngine.Random.Range(-20f, 20f); // 40-80 seconds
            return (Time.time - raceStartTime) > expectedFinishTime;
        }
        
        private void UpdateRaceProgress()
        {
            if (checkpoints.Length == 0) return;
            
            float totalProgress = 0f;
            int tribeCount = tribes.Count;
            
            foreach (var tribeId in tribes.Keys)
            {
                float tribeProgress = (float)tribeCheckpointProgress[tribeId] / checkpoints.Length;
                if (tribeCompletionTimes[tribeId] >= 0f)
                {
                    tribeProgress = 1f; // Finished
                }
                totalProgress += tribeProgress;
            }
            
            float averageProgress = totalProgress / tribeCount;
            UpdateProgress(averageProgress);
        }
        
        private void EndRace()
        {
            if (raceFinished) return;
            
            raceFinished = true;
            
            // Find the winning tribe (fastest completion time)
            string winningTribeId = "";
            float bestTime = float.MaxValue;
            
            foreach (var kvp in tribeCompletionTimes)
            {
                if (kvp.Value >= 0f && kvp.Value < bestTime)
                {
                    bestTime = kvp.Value;
                    winningTribeId = kvp.Key;
                }
            }
            
            if (!string.IsNullOrEmpty(winningTribeId))
            {
                string notes = $"Race completed in {bestTime:F2} seconds";
                EndChallenge(winningTribeId, notes);
            }
            else
            {
                Debug.LogError("[RaceChallengeController] No winning tribe found!");
            }
        }
        
        private void EndRaceByTimeout()
        {
            if (raceFinished) return;
            
            raceFinished = true;
            
            // Find the tribe with the most progress
            string winningTribeId = "";
            int bestProgress = -1;
            
            foreach (var kvp in tribeCheckpointProgress)
            {
                if (kvp.Value > bestProgress)
                {
                    bestProgress = kvp.Value;
                    winningTribeId = kvp.Key;
                }
            }
            
            if (!string.IsNullOrEmpty(winningTribeId))
            {
                string notes = $"Race ended by timeout. Best progress: {bestProgress}/{checkpoints.Length} checkpoints";
                EndChallenge(winningTribeId, notes);
            }
            else
            {
                Debug.LogError("[RaceChallengeController] No winning tribe found for timeout!");
            }
        }
        
        /// <summary>
        /// Called when a participant reaches a checkpoint
        /// </summary>
        public void OnCheckpointReached(string tribeId, int checkpointIndex)
        {
            if (!isActive || raceFinished) return;
            
            if (tribeCheckpointProgress.ContainsKey(tribeId))
            {
                // Only allow progress to the next checkpoint
                if (checkpointIndex == tribeCheckpointProgress[tribeId] + 1)
                {
                    tribeCheckpointProgress[tribeId] = checkpointIndex;
                    Debug.Log($"[RaceChallengeController] Tribe {tribeId} reached checkpoint {checkpointIndex}");
                    
                    // Update scores (optional - could give points for checkpoints)
                    AddScore(tribeId, 10);
                }
            }
        }
        
        /// <summary>
        /// Called when a participant crosses the finish line
        /// </summary>
        public void OnFinishLineCrossed(string tribeId, string participantId)
        {
            if (!isActive || raceFinished) return;
            
            Debug.Log($"[RaceChallengeController] Participant {participantId} from tribe {tribeId} crossed finish line");
            
            // Check if all tribe members have finished
            var tribeMembers = tribes[tribeId];
            bool allFinished = true;
            
            foreach (var participant in tribeMembers)
            {
                if (participant.playerId != participantId && !HasParticipantFinished(participant))
                {
                    allFinished = false;
                    break;
                }
            }
            
            if (allFinished && tribeCompletionTimes[tribeId] < 0f)
            {
                tribeCompletionTimes[tribeId] = Time.time - raceStartTime;
                Debug.Log($"[RaceChallengeController] Tribe {tribeId} completed race in {tribeCompletionTimes[tribeId]:F2} seconds");
            }
        }
        
        // Debug methods
        [ContextMenu("Test Race Completion")]
        public void TestRaceCompletion()
        {
            if (isActive && !raceFinished)
            {
                // Simulate all tribes finishing
                foreach (var tribeId in tribes.Keys)
                {
                    tribeCompletionTimes[tribeId] = Time.time - raceStartTime + UnityEngine.Random.Range(1f, 10f);
                }
                EndRace();
            }
        }
        
        [ContextMenu("Show Race Status")]
        public void ShowRaceStatus()
        {
            Debug.Log("=== Race Challenge Status ===");
            Debug.Log($"[RaceChallengeController] Active: {isActive}");
            Debug.Log($"[RaceChallengeController] Finished: {raceFinished}");
            Debug.Log($"[RaceChallengeController] Tribes: {tribes?.Count ?? 0}");
            Debug.Log($"[RaceChallengeController] Checkpoints: {checkpoints?.Length ?? 0}");
            Debug.Log($"[RaceChallengeController] Progress: {currentProgress:P0}");
            
            if (tribes != null)
            {
                foreach (var tribeId in tribes.Keys)
                {
                    var progress = tribeCheckpointProgress.ContainsKey(tribeId) ? tribeCheckpointProgress[tribeId] : 0;
                    var time = tribeCompletionTimes.ContainsKey(tribeId) ? tribeCompletionTimes[tribeId] : -1f;
                    var status = time >= 0f ? $"Finished ({time:F2}s)" : $"Checkpoint {progress}/{checkpoints?.Length ?? 0}";
                    Debug.Log($"[RaceChallengeController] Tribe {tribeId}: {status}");
                }
            }
            
            Debug.Log("=== End Status ===");
        }
    }
} 