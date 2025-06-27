using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Survivor.Characters;
using Survivor.Shared;
using StarterAssets;

namespace Survivor.Challenges
{
    /// <summary>
    /// Enhanced controller for race-based challenges with side-by-side courses and challenge segments
    /// </summary>
    public class EnhancedRaceChallengeController : BaseChallengeController
    {
        [Header("Course Configuration")]
        [SerializeField] private CourseData tribeACourse;
        [SerializeField] private CourseData tribeBCourse;
        [SerializeField] private float raceTimeout = 300f; // 5 minutes
        
        [Header("Race State")]
        [SerializeField] private Dictionary<string, CourseProgress> tribeProgress = new Dictionary<string, CourseProgress>();
        [SerializeField] private Dictionary<string, float> tribeCompletionTimes = new Dictionary<string, float>();
        [SerializeField] private float raceStartTime;
        [SerializeField] private bool raceFinished = false;
        
        private Dictionary<string, List<ChallengeParticipant>> tribes;
        
        [System.Serializable]
        public class CourseData
        {
            public string tribeId;
            public Transform startPoint;
            public Transform[] checkpoints;
            public Transform finishPoint;
            public ChallengeSegment[] segments;
        }
        
        [System.Serializable]
        public class ChallengeSegment
        {
            public int checkpointIndex;
            public ChallengeSegmentType segmentType;
            public GameObject segmentObject;
            public bool isCompleted = false;
            public float completionTime = -1f;
        }
        
        [System.Serializable]
        public class CourseProgress
        {
            public int currentCheckpoint = 0;
            public bool[] segmentCompleted;
            public bool isFinished = false;
            public float finishTime = -1f;
        }
        
        public enum ChallengeSegmentType
        {
            CrawlUnderNet,
            BalanceBeam,
            SandbagAccuracy,
            Custom
        }
        
        protected override void OnInitialized()
        {
            // Group participants by tribe
            tribes = currentSession.participants
                .GroupBy(p => p.tribeId)
                .ToDictionary(g => g.Key, g => g.ToList());
            
            // Initialize progress for each tribe
            foreach (var tribeId in tribes.Keys)
            {
                var course = GetCourseForTribe(tribeId);
                if (course != null)
                {
                    tribeProgress[tribeId] = new CourseProgress
                    {
                        currentCheckpoint = 0,
                        segmentCompleted = new bool[course.segments.Length],
                        isFinished = false,
                        finishTime = -1f
                    };
                    tribeCompletionTimes[tribeId] = -1f;
                }
            }
            
            Debug.Log($"[EnhancedRaceChallengeController] Initialized with {tribes.Count} tribes");
        }
        
        protected override void OnChallengeStarted()
        {
            raceStartTime = Time.time;
            raceFinished = false;
            
            // Reset all progress
            foreach (var tribeId in tribeProgress.Keys.ToList())
            {
                var course = GetCourseForTribe(tribeId);
                if (course != null)
                {
                    tribeProgress[tribeId] = new CourseProgress
                    {
                        currentCheckpoint = 0,
                        segmentCompleted = new bool[course.segments.Length],
                        isFinished = false,
                        finishTime = -1f
                    };
                    tribeCompletionTimes[tribeId] = -1f;
                }
            }
            
            UpdateProgress(0f);
            Debug.Log("[EnhancedRaceChallengeController] Race started!");
        }
        
        protected override void OnChallengePaused()
        {
            Debug.Log("[EnhancedRaceChallengeController] Race paused");
        }
        
        protected override void OnChallengeResumed()
        {
            Debug.Log("[EnhancedRaceChallengeController] Race resumed");
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
                
                var progress = tribeProgress[tribeId];
                if (progress.isFinished && progress.finishTime < 0f)
                {
                    progress.finishTime = Time.time - raceStartTime;
                    tribeCompletionTimes[tribeId] = progress.finishTime;
                    Debug.Log($"[EnhancedRaceChallengeController] Tribe {tribeId} finished in {progress.finishTime:F2} seconds");
                }
            }
            
            // Check if all tribes have finished
            var finishedTribes = tribeCompletionTimes.Values.Count(t => t >= 0f);
            if (finishedTribes >= tribes.Count)
            {
                EndRace();
            }
        }
        
        private void UpdateRaceProgress()
        {
            float totalProgress = 0f;
            int tribeCount = tribes.Count;
            
            foreach (var tribeId in tribes.Keys)
            {
                var progress = tribeProgress[tribeId];
                var course = GetCourseForTribe(tribeId);
                
                if (course != null)
                {
                    float tribeProgress = (float)progress.currentCheckpoint / course.checkpoints.Length;
                    if (progress.isFinished)
                    {
                        tribeProgress = 1f; // Finished
                    }
                    totalProgress += tribeProgress;
                }
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
                Debug.LogError("[EnhancedRaceChallengeController] No winning tribe found!");
            }
        }
        
        private void EndRaceByTimeout()
        {
            if (raceFinished) return;
            
            raceFinished = true;
            
            // Find the tribe with the most progress
            string winningTribeId = "";
            int bestProgress = -1;
            
            foreach (var kvp in tribeProgress)
            {
                if (kvp.Value.currentCheckpoint > bestProgress)
                {
                    bestProgress = kvp.Value.currentCheckpoint;
                    winningTribeId = kvp.Key;
                }
            }
            
            if (!string.IsNullOrEmpty(winningTribeId))
            {
                var course = GetCourseForTribe(winningTribeId);
                string notes = $"Race ended by timeout. Best progress: {bestProgress}/{course?.checkpoints.Length ?? 0} checkpoints";
                EndChallenge(winningTribeId, notes);
            }
            else
            {
                Debug.LogError("[EnhancedRaceChallengeController] No winning tribe found for timeout!");
            }
        }
        
        /// <summary>
        /// Called when a participant reaches a checkpoint
        /// </summary>
        public void OnCheckpointReached(string tribeId, int checkpointIndex)
        {
            if (!isActive || raceFinished) return;
            
            if (tribeProgress.ContainsKey(tribeId))
            {
                var progress = tribeProgress[tribeId];
                var course = GetCourseForTribe(tribeId);
                
                if (course != null && checkpointIndex == progress.currentCheckpoint + 1)
                {
                    progress.currentCheckpoint = checkpointIndex;
                    Debug.Log($"[EnhancedRaceChallengeController] Tribe {tribeId} reached checkpoint {checkpointIndex}");
                    
                    // Check if this checkpoint has a challenge segment
                    var segment = GetSegmentForCheckpoint(course, checkpointIndex);
                    if (segment != null && !segment.isCompleted)
                    {
                        StartChallengeSegment(tribeId, segment);
                    }
                    
                    // Update scores
                    AddScore(tribeId, 10);
                }
            }
        }
        
        /// <summary>
        /// Called when a participant completes a challenge segment
        /// </summary>
        public void OnChallengeSegmentCompleted(string tribeId, int checkpointIndex)
        {
            if (!isActive || raceFinished) return;
            
            var progress = tribeProgress[tribeId];
            var course = GetCourseForTribe(tribeId);
            
            if (course != null)
            {
                var segment = GetSegmentForCheckpoint(course, checkpointIndex);
                if (segment != null && !segment.isCompleted)
                {
                    segment.isCompleted = true;
                    segment.completionTime = Time.time - raceStartTime;
                    
                    // Mark segment as completed in progress
                    int segmentIndex = GetSegmentIndex(course, checkpointIndex);
                    if (segmentIndex >= 0 && segmentIndex < progress.segmentCompleted.Length)
                    {
                        progress.segmentCompleted[segmentIndex] = true;
                    }
                    
                    Debug.Log($"[EnhancedRaceChallengeController] Tribe {tribeId} completed segment at checkpoint {checkpointIndex}");
                    AddScore(tribeId, 25); // Bonus points for completing segment
                }
            }
        }
        
        /// <summary>
        /// Called when a participant crosses the finish line
        /// </summary>
        public void OnFinishLineCrossed(string tribeId, string participantId)
        {
            if (!isActive || raceFinished) return;
            
            Debug.Log($"[EnhancedRaceChallengeController] Participant {participantId} from tribe {tribeId} crossed finish line");
            
            var progress = tribeProgress[tribeId];
            var course = GetCourseForTribe(tribeId);
            
            if (course != null && progress.currentCheckpoint >= course.checkpoints.Length)
            {
                // Check if all segments are completed
                bool allSegmentsCompleted = true;
                for (int i = 0; i < progress.segmentCompleted.Length; i++)
                {
                    if (!progress.segmentCompleted[i])
                    {
                        allSegmentsCompleted = false;
                        break;
                    }
                }
                
                if (allSegmentsCompleted)
                {
                    progress.isFinished = true;
                    Debug.Log($"[EnhancedRaceChallengeController] Tribe {tribeId} completed all segments and finished race");
                }
                else
                {
                    Debug.Log($"[EnhancedRaceChallengeController] Tribe {tribeId} reached finish but hasn't completed all segments");
                }
            }
        }
        
        private CourseData GetCourseForTribe(string tribeId)
        {
            if (tribeId == "tribe_1" || tribeId == "TribeA")
                return tribeACourse;
            else if (tribeId == "tribe_2" || tribeId == "TribeB")
                return tribeBCourse;
            return null;
        }
        
        private ChallengeSegment GetSegmentForCheckpoint(CourseData course, int checkpointIndex)
        {
            return course.segments?.FirstOrDefault(s => s.checkpointIndex == checkpointIndex);
        }
        
        private int GetSegmentIndex(CourseData course, int checkpointIndex)
        {
            for (int i = 0; i < course.segments.Length; i++)
            {
                if (course.segments[i].checkpointIndex == checkpointIndex)
                    return i;
            }
            return -1;
        }
        
        private void StartChallengeSegment(string tribeId, ChallengeSegment segment)
        {
            Debug.Log($"[EnhancedRaceChallengeController] Starting challenge segment for tribe {tribeId} at checkpoint {segment.checkpointIndex}");
            
            // Activate the segment object
            if (segment.segmentObject != null)
            {
                segment.segmentObject.SetActive(true);
            }
            
            // You can add specific logic here based on segment type
            switch (segment.segmentType)
            {
                case ChallengeSegmentType.CrawlUnderNet:
                    // Logic for net crawling challenge
                    break;
                case ChallengeSegmentType.BalanceBeam:
                    // Logic for balance beam challenge
                    break;
                case ChallengeSegmentType.SandbagAccuracy:
                    // Logic for sandbag throwing challenge
                    break;
                case ChallengeSegmentType.Custom:
                    // Custom challenge logic
                    break;
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
                    var progress = tribeProgress[tribeId];
                    progress.isFinished = true;
                    progress.finishTime = Time.time - raceStartTime + UnityEngine.Random.Range(1f, 10f);
                    tribeCompletionTimes[tribeId] = progress.finishTime;
                }
                EndRace();
            }
        }
        
        [ContextMenu("Show Race Status")]
        public void ShowRaceStatus()
        {
            Debug.Log("=== Enhanced Race Challenge Status ===");
            Debug.Log($"[EnhancedRaceChallengeController] Active: {isActive}");
            Debug.Log($"[EnhancedRaceChallengeController] Finished: {raceFinished}");
            Debug.Log($"[EnhancedRaceChallengeController] Tribes: {tribes?.Count ?? 0}");
            Debug.Log($"[EnhancedRaceChallengeController] Progress: {currentProgress:P0}");
            
            if (tribes != null)
            {
                foreach (var tribeId in tribes.Keys)
                {
                    var progress = tribeProgress.ContainsKey(tribeId) ? tribeProgress[tribeId] : null;
                    var course = GetCourseForTribe(tribeId);
                    var time = tribeCompletionTimes.ContainsKey(tribeId) ? tribeCompletionTimes[tribeId] : -1f;
                    
                    if (progress != null && course != null)
                    {
                        var status = progress.isFinished ? $"Finished ({time:F2}s)" : $"Checkpoint {progress.currentCheckpoint}/{course.checkpoints.Length}";
                        Debug.Log($"[EnhancedRaceChallengeController] Tribe {tribeId}: {status}");
                        
                        // Show segment completion status
                        for (int i = 0; i < progress.segmentCompleted.Length; i++)
                        {
                            var segment = course.segments[i];
                            var segmentStatus = progress.segmentCompleted[i] ? "Completed" : "Not Started";
                            Debug.Log($"[EnhancedRaceChallengeController]   Segment {segment.checkpointIndex} ({segment.segmentType}): {segmentStatus}");
                        }
                    }
                }
            }
            
            Debug.Log("=== End Status ===");
        }
    }
} 