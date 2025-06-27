using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Survivor.Shared;

namespace Survivor.Core
{
    /// <summary>
    /// Main manager for handling challenges in the Survivor game
    /// </summary>
    public class ChallengeManager : MonoBehaviour
    {
        public static ChallengeManager Instance { get; private set; }
        
        [Header("Challenge Configuration")]
        [SerializeField] private ChallengeConfig config = new ChallengeConfig();
        [SerializeField] private List<ChallengeDefinition> availableChallenges = new List<ChallengeDefinition>();
        
        [Header("Current State")]
        [SerializeField] private ChallengeSession currentSession;
        [SerializeField] private List<ChallengeResult> completedChallenges = new List<ChallengeResult>();
        [SerializeField] private bool isChallengeTime = false;
        [SerializeField] private bool isInChallengeScene = false;
        
        [Header("Team Management")]
        [SerializeField] private List<ChallengeParticipant> allParticipants = new List<ChallengeParticipant>();
        [SerializeField] private List<ChallengeParticipant> sitOutPlayers = new List<ChallengeParticipant>();
        
        // Events
        public event Action<ChallengeSession> OnChallengeScheduled;
        public event Action<ChallengeSession> OnChallengeStarted;
        public event Action<ChallengeResult> OnChallengeCompleted;
        public event Action<string> OnImmunityGranted;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeChallengeManager();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            // Subscribe to time events
            if (GameTimeService.HasTimeProvider && GameTimeService.TimeProvider is TimeManager timeManager)
            {
                timeManager.onGameHourPassed.AddListener(OnGameHourPassed);
            }
            
            // Subscribe to scene loading events
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events
            if (GameTimeService.HasTimeProvider && GameTimeService.TimeProvider is TimeManager timeManager)
            {
                timeManager.onGameHourPassed.RemoveListener(OnGameHourPassed);
            }
            
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        
        private void InitializeChallengeManager()
        {
            Debug.Log("[ChallengeManager] Initializing challenge manager");
            
            // Load available challenges (this would typically be loaded from ScriptableObjects or JSON)
            LoadAvailableChallenges();
            
            // Load completed challenges from save data
            LoadCompletedChallenges();
        }
        
        private void LoadAvailableChallenges()
        {
            // For now, create some example challenges
            // In a real implementation, these would be loaded from ScriptableObjects
            
            availableChallenges.Clear();
            
            // Example Race Challenge
            var raceChallenge = new ChallengeDefinition
            {
                challengeId = "race_001",
                challengeName = "Obstacle Course Race",
                description = "Navigate through an obstacle course. First tribe to have all members cross the finish line wins.",
                challengeType = ChallengeType.Race,
                minPlayers = 4,
                maxPlayers = 8,
                challengeSceneName = "RaceChallenge_001",
                estimatedDuration = 300f,
                isTribeChallenge = true,
                requiresEvenTeams = true,
                maxSitOutPlayers = 1,
                grantsImmunity = true,
                immunityDays = 1,
                isEnabled = true,
                difficulty = 2
            };
            
            availableChallenges.Add(raceChallenge);
            
            Debug.Log($"[ChallengeManager] Loaded {availableChallenges.Count} available challenges");
        }
        
        private void LoadCompletedChallenges()
        {
            // Load from PlayerPrefs or save system
            // For now, just initialize empty list
            completedChallenges.Clear();
        }
        
        private void SaveCompletedChallenges()
        {
            // Save to PlayerPrefs or save system
            // For now, just log
            Debug.Log($"[ChallengeManager] Saving {completedChallenges.Count} completed challenges");
        }
        
        private void OnGameHourPassed()
        {
            CheckForChallengeTime();
        }
        
        private void CheckForChallengeTime()
        {
            if (isInChallengeScene) return; // Don't schedule challenges while in a challenge scene
            
            var (hour, minute, day) = GameTimeService.GetCurrentGameTime();
            
            // Check if it's challenge time (12:00 PM) and not the first day
            if (hour == config.challengeHour && minute == config.challengeMinute && day >= config.firstChallengeDay)
            {
                Debug.Log($"[ChallengeManager] Challenge time detected! Day {day}, {hour:D2}:{minute:D2}");
                ScheduleChallenge(day);
            }
        }
        
        private void ScheduleChallenge(int day)
        {
            if (isChallengeTime)
            {
                Debug.LogWarning("[ChallengeManager] Challenge already scheduled for today!");
                return;
            }
            
            // Select a challenge
            var selectedChallenge = SelectChallenge();
            if (selectedChallenge == null)
            {
                Debug.LogError("[ChallengeManager] No suitable challenge found!");
                return;
            }
            
            // Prepare participants
            PrepareParticipants();
            
            // Create challenge session
            currentSession = new ChallengeSession
            {
                sessionId = Guid.NewGuid().ToString(),
                challenge = selectedChallenge,
                dayNumber = day,
                startTime = DateTime.Now,
                state = ChallengeState.NotStarted,
                participants = new List<ChallengeParticipant>(allParticipants),
                sitOutPlayers = new List<ChallengeParticipant>(sitOutPlayers),
                isActive = false
            };
            
            isChallengeTime = true;
            
            Debug.Log($"[ChallengeManager] Challenge scheduled: {selectedChallenge.challengeName} for Day {day}");
            OnChallengeScheduled?.Invoke(currentSession);
            
            // Start the challenge after a short delay
            StartCoroutine(StartChallengeAfterDelay());
        }
        
        private System.Collections.IEnumerator StartChallengeAfterDelay()
        {
            // Give players time to prepare
            yield return new WaitForSeconds(5f);
            
            StartChallenge();
        }
        
        private void StartChallenge()
        {
            if (currentSession == null)
            {
                Debug.LogError("[ChallengeManager] No challenge session to start!");
                return;
            }
            
            Debug.Log($"[ChallengeManager] Starting challenge: {currentSession.challenge.challengeName}");
            
            // Save current scene state
            SaveCurrentSceneState();
            
            // Load challenge scene
            SceneManager.LoadScene(currentSession.challenge.challengeSceneName);
            
            OnChallengeStarted?.Invoke(currentSession);
        }
        
        private ChallengeDefinition SelectChallenge()
        {
            var enabledChallenges = availableChallenges.Where(c => c.isEnabled).ToList();
            
            if (enabledChallenges.Count == 0)
            {
                Debug.LogError("[ChallengeManager] No enabled challenges available!");
                return null;
            }
            
            if (config.randomizeChallenges)
            {
                // Filter out recently completed challenges if avoiding repeats
                if (config.avoidRepeatingChallenges)
                {
                    var recentChallenges = completedChallenges
                        .TakeLast(config.maxConsecutiveRepeats)
                        .Select(r => r.challengeId)
                        .ToList();
                    
                    enabledChallenges = enabledChallenges
                        .Where(c => !recentChallenges.Contains(c.challengeId))
                        .ToList();
                    
                    // If all challenges were recently used, allow repeats
                    if (enabledChallenges.Count == 0)
                    {
                        enabledChallenges = availableChallenges.Where(c => c.isEnabled).ToList();
                    }
                }
                
                // Random selection
                return enabledChallenges[UnityEngine.Random.Range(0, enabledChallenges.Count)];
            }
            else
            {
                // Sequential selection (for testing)
                return enabledChallenges[0];
            }
        }
        
        private void PrepareParticipants()
        {
            allParticipants.Clear();
            sitOutPlayers.Clear();
            
            // Get all players from the current scene using reflection to avoid compile-time dependencies
            var playerControllers = FindObjectsOfType<MonoBehaviour>().Where(mb => 
                mb.GetType().Name == "PlayerController").ToArray();
            var npcControllers = FindObjectsOfType<MonoBehaviour>().Where(mb => 
                mb.GetType().Name == "NPCController").ToArray();
            
            // Add main player
            var mainPlayer = FindObjectsOfType<MonoBehaviour>().Where(mb => 
                mb.GetType().Name == "ThirdPersonController").FirstOrDefault();
            if (mainPlayer != null)
            {
                var participant = new ChallengeParticipant
                {
                    playerId = "player_main",
                    playerName = "Main Player",
                    tribeId = "tribe_1", // This should come from actual tribe system
                    tribeName = "Tribe 1",
                    isMainPlayer = true,
                    lastKnownPosition = mainPlayer.transform.position,
                    lastKnownRotation = mainPlayer.transform.rotation,
                    isActive = true
                };
                allParticipants.Add(participant);
            }
            
            // Add NPCs
            foreach (var npc in npcControllers)
            {
                var participant = new ChallengeParticipant
                {
                    playerId = $"npc_{npc.GetInstanceID()}",
                    playerName = npc.name,
                    tribeId = "tribe_1", // This should come from actual tribe system
                    tribeName = "Tribe 1",
                    isMainPlayer = false,
                    lastKnownPosition = npc.transform.position,
                    lastKnownRotation = npc.transform.rotation,
                    isActive = true
                };
                allParticipants.Add(participant);
            }
            
            // Balance teams if needed
            if (config.autoBalanceTeams)
            {
                BalanceTeams();
            }
            
            Debug.Log($"[ChallengeManager] Prepared {allParticipants.Count} participants, {sitOutPlayers.Count} sitting out");
        }
        
        private void BalanceTeams()
        {
            // Group by tribe
            var tribes = allParticipants.GroupBy(p => p.tribeId).ToList();
            
            if (tribes.Count < 2)
            {
                Debug.LogWarning("[ChallengeManager] Not enough tribes for challenge!");
                return;
            }
            
            // Find the largest tribe
            var largestTribe = tribes.OrderByDescending(t => t.Count()).First();
            var otherTribes = tribes.Where(t => t.Key != largestTribe.Key).ToList();
            
            // Calculate how many players need to sit out
            int totalPlayers = allParticipants.Count;
            int maxTeamSize = Mathf.Min(config.maxTeamSize, totalPlayers / 2);
            int playersNeeded = maxTeamSize * 2;
            int playersToSitOut = totalPlayers - playersNeeded;
            
            if (playersToSitOut > 0)
            {
                // Select players to sit out (prefer non-main players)
                var candidates = allParticipants
                    .Where(p => !p.isMainPlayer)
                    .OrderBy(p => UnityEngine.Random.value)
                    .Take(playersToSitOut)
                    .ToList();
                
                foreach (var candidate in candidates)
                {
                    candidate.isActive = false;
                    sitOutPlayers.Add(candidate);
                    allParticipants.Remove(candidate);
                }
                
                Debug.Log($"[ChallengeManager] Balanced teams: {allParticipants.Count} active, {sitOutPlayers.Count} sitting out");
            }
        }
        
        private void SaveCurrentSceneState()
        {
            // Save player positions and other important state
            // This would be implemented based on your save system
            Debug.Log("[ChallengeManager] Saving current scene state");
        }
        
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == currentSession?.challenge.challengeSceneName)
            {
                isInChallengeScene = true;
                Debug.Log("[ChallengeManager] Entered challenge scene");
                
                // Find and initialize the challenge controller using reflection
                var challengeController = FindObjectsOfType<MonoBehaviour>().Where(mb => 
                    mb.GetType().Name == "BaseChallengeController").FirstOrDefault();
                if (challengeController != null)
                {
                    // Use reflection to call methods
                    var initializeMethod = challengeController.GetType().GetMethod("InitializeChallenge");
                    var startMethod = challengeController.GetType().GetMethod("StartChallenge");
                    var onChallengeEndedEvent = challengeController.GetType().GetEvent("OnChallengeEnded");
                    
                    if (initializeMethod != null)
                    {
                        initializeMethod.Invoke(challengeController, new object[] { currentSession });
                    }
                    
                    if (onChallengeEndedEvent != null)
                    {
                        // Subscribe to the event using reflection
                        var eventHandlerType = onChallengeEndedEvent.EventHandlerType;
                        var delegateType = eventHandlerType.GetMethod("Invoke");
                        // This is a simplified approach - in practice you'd need to create a proper delegate
                    }
                    
                    if (startMethod != null)
                    {
                        startMethod.Invoke(challengeController, null);
                    }
                }
                else
                {
                    Debug.LogError("[ChallengeManager] No challenge controller found in challenge scene!");
                }
            }
            else if (scene.name == "TropicalIsland")
            {
                isInChallengeScene = false;
                isChallengeTime = false;
                Debug.Log("[ChallengeManager] Returned to main island");
            }
        }
        
        private void OnChallengeEnded(ChallengeResult result)
        {
            Debug.Log($"[ChallengeManager] Challenge ended. Winner: {result.winningTribeId}");
            
            // Save the result
            completedChallenges.Add(result);
            SaveCompletedChallenges();
            
            // Grant immunity if applicable
            if (result.immunityGranted)
            {
                OnImmunityGranted?.Invoke(result.winningTribeId);
                Debug.Log($"[ChallengeManager] Immunity granted to {result.winningTribeId} for {result.immunityDaysGranted} days");
            }
            
            OnChallengeCompleted?.Invoke(result);
            
            // Return to main island after a delay
            StartCoroutine(ReturnToMainIslandAfterDelay());
        }
        
        private System.Collections.IEnumerator ReturnToMainIslandAfterDelay()
        {
            // Give time for results to be displayed
            yield return new WaitForSeconds(3f);
            
            Debug.Log("[ChallengeManager] Returning to main island");
            SceneManager.LoadScene("TropicalIsland");
        }
        
        // Public methods for external access
        public ChallengeSession GetCurrentSession()
        {
            return currentSession;
        }
        
        public List<ChallengeResult> GetCompletedChallenges()
        {
            return new List<ChallengeResult>(completedChallenges);
        }
        
        public bool IsChallengeActive()
        {
            return isChallengeTime || isInChallengeScene;
        }
        
        public ChallengeConfig GetConfig()
        {
            return config;
        }
        
        public void SetConfig(ChallengeConfig newConfig)
        {
            config = newConfig;
        }
        
        // Debug methods
        [ContextMenu("Test Schedule Challenge")]
        public void TestScheduleChallenge()
        {
            var (_, _, day) = GameTimeService.GetCurrentGameTime();
            ScheduleChallenge(day);
        }
        
        [ContextMenu("Show Challenge Status")]
        public void ShowChallengeStatus()
        {
            Debug.Log("=== Challenge Manager Status ===");
            Debug.Log($"[ChallengeManager] Available challenges: {availableChallenges.Count}");
            Debug.Log($"[ChallengeManager] Completed challenges: {completedChallenges.Count}");
            Debug.Log($"[ChallengeManager] Challenge time: {isChallengeTime}");
            Debug.Log($"[ChallengeManager] In challenge scene: {isInChallengeScene}");
            
            if (currentSession != null)
            {
                Debug.Log($"[ChallengeManager] Current session: {currentSession.challenge.challengeName} (Day {currentSession.dayNumber})");
                Debug.Log($"[ChallengeManager] Participants: {currentSession.participants.Count}");
                Debug.Log($"[ChallengeManager] Sit-out players: {currentSession.sitOutPlayers.Count}");
            }
            
            Debug.Log("=== End Status ===");
        }
    }
} 