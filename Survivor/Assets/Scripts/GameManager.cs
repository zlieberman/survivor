using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Survivor.Generation;
using UnityEngine.InputSystem;
using UnityEngine.AI;
using System.Collections;
using Survivor.Characters;
using Survivor.Characters.UI;
using Survivor.Shared.Interfaces;
using Survivor.Shared;
using Survivor.UI;
using Survivor.Interactables;
using Survivor.Core.Interaction;
using Survivor.Core;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player Settings")]
    public GameObject playerPrefab;
    public GameObject npcPrefab;

    [Header("Prefabs")]
    [SerializeField] private GameObject dialogueUIPrefab;

    [Header("Managers")]
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private ChallengeManager challengeManager;

    private CharacterStats mainPlayer;
    private ProceduralIslandGenerator islandGenerator;
    private CampGenerator campGenerator;
    private bool isGameInitialized = false;
    private bool isInitializing = false;

    private void Awake()
    {
        Debug.Log("GameManager Awake called");
        
        // Check if this is the first instance
        if (Instance == null)
        {
            Debug.Log("First GameManager instance - setting up singleton");
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Initialize immediately if we're starting in the TropicalIsland scene
            if (SceneManager.GetActiveScene().name == "TropicalIsland")
            {
                Debug.Log("Starting in TropicalIsland scene - initializing immediately");
                InitializeGame();
            }
            InitializeDialogueSystem();
            InitializeChallengeSystem();
        }
        else
        {
            Debug.Log("GameManager instance already exists - destroying duplicate");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Debug.Log("GameManager Start called");
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        Debug.Log("GameManager OnDestroy called");
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void StartGame()
    {
        Debug.Log("StartGame called");
        
        if (isInitializing)
        {
            Debug.LogWarning("Game initialization already in progress!");
            return;
        }

        if (isGameInitialized)
        {
            Debug.LogWarning("Game already initialized!");
            return;
        }

        isInitializing = true;
        Debug.Log("Starting game initialization");

        // Initialize game
        InitializeGame();
        
        // Load the TropicalIsland scene
        Debug.Log("Loading TropicalIsland scene");
        SceneManager.LoadScene("TropicalIsland");
    }

    private void InitializeGame()
    {
        Debug.Log("Initializing game");
        
        // Create main player stats
        mainPlayer = new CharacterStats();
        mainPlayer.perception = Random.Range(50, 101);
        mainPlayer.deception = Random.Range(50, 101);
        mainPlayer.persuasion = Random.Range(50, 101);
        mainPlayer.puzzleSolving = Random.Range(50, 101);
        mainPlayer.swimming = Random.Range(50, 101);
        mainPlayer.speed = Random.Range(50, 101);
        mainPlayer.strength = Random.Range(50, 101);
        mainPlayer.agility = Random.Range(50, 101);
        mainPlayer.intelligence = Random.Range(50, 101);
        mainPlayer.stamina = Random.Range(50, 101);
        mainPlayer.charisma = Random.Range(50, 101);
        mainPlayer.honesty = Random.Range(50, 101);
        mainPlayer.trust = Random.Range(50, 101);
        mainPlayer.honor = Random.Range(50, 101);
        Debug.Log("Main player stats created");

        isGameInitialized = true;
        isInitializing = false;
        Debug.Log("Game initialization complete");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Scene loaded: {scene.name}");
        if (scene.name == "TropicalIsland")
        {
            Debug.Log("TropicalIsland scene loaded, starting island generation wait");
            
            // Ensure game is initialized
            if (!isGameInitialized)
            {
                Debug.LogWarning("Game not initialized - initializing now");
                InitializeGame();
            }
            
            StartCoroutine(WaitForIslandGeneration());
        }
    }

    private IEnumerator WaitForIslandGeneration()
    {
        Debug.Log("Starting WaitForIslandGeneration coroutine");

        // Find the island generator
        ProceduralIslandGenerator islandGenerator = FindObjectOfType<ProceduralIslandGenerator>();
        if (islandGenerator == null)
        {
            Debug.LogError("Could not find ProceduralIslandGenerator in scene!");
            yield break;
        }
        Debug.Log("Found ProceduralIslandGenerator");

        // Wait for island generation to complete
        while (!islandGenerator.IsGenerationComplete())
        {
            Debug.Log("Waiting for island generation to complete");
            yield return new WaitForSeconds(0.5f);
        }
        Debug.Log("Island generation complete");

        // Find the camp generator
        CampGenerator campGenerator = FindObjectOfType<CampGenerator>();
        if (campGenerator == null)
        {
            Debug.LogError("Could not find CampGenerator in scene!");
            yield break;
        }
        Debug.Log("Found CampGenerator");

        // Wait for camp placement
        while (!campGenerator.CampPlaced)
        {
            Debug.Log("Waiting for camp placement");
            yield return new WaitForSeconds(0.5f);
        }
        Debug.Log("Camp placement complete");

        // Spawn player
        SpawnPlayer();
    }

    private Transform FindCampSpawnPointRecursive(Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (child.name == "CampSpawnPoint")
            {
                return child;
            }

            Transform result = FindCampSpawnPointRecursive(child);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }

    private void SpawnPlayer()
    {
        Debug.Log("Spawning player");
        
        // Find camp spawn point
        Transform campSpawnPoint = FindCampSpawnPointRecursive(transform.root);
        if (campSpawnPoint == null)
        {
            Debug.LogError("Could not find CampSpawnPoint in scene!");
            return;
        }
        Debug.Log("Found camp spawn point");

        // Spawn player at camp
        if (playerPrefab != null)
        {
            GameObject player = Instantiate(playerPrefab, campSpawnPoint.position, campSpawnPoint.rotation);
            Debug.Log("Player spawned at camp");
            
            // Set up camera to follow player
            var mainCamera = Camera.main;
            if (mainCamera != null)
            {
                var cameraController = mainCamera.GetComponent<CameraController>();
                if (cameraController != null)
                {
                    cameraController.SetTarget(player.transform);
                    Debug.Log("Camera set to follow player");
                }
            }
            
            // Set up player stats
            var playerController = player.GetComponent<ThirdPersonController>();
            if (playerController != null)
            {
                // Get the PlayerCharacter component to set stats
                var playerCharacter = player.GetComponent<PlayerCharacter>();
                if (playerCharacter != null)
                {
                    playerCharacter.Stats = mainPlayer;
                }
                Debug.Log("Player stats set");
            }
        }
        else
        {
            Debug.LogError("Player prefab not assigned!");
        }

        // Spawn NPCs
        SpawnNPCs();
    }

    private void SpawnNPCs()
    {
        Debug.Log("Spawning NPCs");
        
        if (npcPrefab == null)
        {
            Debug.LogError("NPC prefab not assigned!");
            return;
        }

        // Find NPC spawn points
        var npcSpawnPoints = GameObject.FindGameObjectsWithTag("NPCSpawnPoint");
        if (npcSpawnPoints.Length == 0)
        {
            Debug.LogWarning("No NPC spawn points found!");
            return;
        }

        // Spawn NPCs at spawn points
        for (int i = 0; i < npcSpawnPoints.Length; i++)
        {
            var spawnPoint = npcSpawnPoints[i];
            if (spawnPoint != null)
            {
                GameObject npc = Instantiate(npcPrefab, spawnPoint.transform.position, spawnPoint.transform.rotation);
                npc.name = $"NPC_{i + 1}";
                
                // Set up NPC stats
                var npcCharacter = npc.GetComponent<NPCCharacter>();
                if (npcCharacter != null)
                {
                    var npcStats = new CharacterStats();
                    npcStats.perception = Random.Range(30, 101);
                    npcStats.deception = Random.Range(30, 101);
                    npcStats.persuasion = Random.Range(30, 101);
                    npcStats.puzzleSolving = Random.Range(30, 101);
                    npcStats.swimming = Random.Range(30, 101);
                    npcStats.speed = Random.Range(30, 101);
                    npcStats.strength = Random.Range(30, 101);
                    npcStats.agility = Random.Range(30, 101);
                    npcStats.intelligence = Random.Range(30, 101);
                    npcStats.stamina = Random.Range(30, 101);
                    npcStats.charisma = Random.Range(30, 101);
                    npcStats.honesty = Random.Range(30, 101);
                    npcStats.trust = Random.Range(30, 101);
                    npcStats.honor = Random.Range(30, 101);
                    
                    npcCharacter.Stats = npcStats;
                }
                
                Debug.Log($"NPC {i + 1} spawned at {spawnPoint.name}");
            }
        }
    }

    private void InitializeDialogueSystem()
    {
        Debug.Log("Initializing dialogue system");
        
        if (dialogueManager == null)
        {
            dialogueManager = FindObjectOfType<DialogueManager>();
        }
        
        if (dialogueManager == null)
        {
            Debug.LogWarning("No DialogueManager found - dialogue system not initialized");
        }
        else
        {
            Debug.Log("Dialogue system initialized");
        }
    }

    private void InitializeChallengeSystem()
    {
        Debug.Log("Initializing challenge system");
        
        if (challengeManager == null)
        {
            challengeManager = FindObjectOfType<ChallengeManager>();
        }
        
        if (challengeManager == null)
        {
            Debug.LogWarning("No ChallengeManager found - challenge system not initialized");
        }
        else
        {
            // Subscribe to challenge events
            challengeManager.OnChallengeScheduled += OnChallengeScheduled;
            challengeManager.OnChallengeStarted += OnChallengeStarted;
            challengeManager.OnChallengeCompleted += OnChallengeCompleted;
            challengeManager.OnImmunityGranted += OnImmunityGranted;
            
            Debug.Log("Challenge system initialized");
        }
    }

    private void OnChallengeScheduled(ChallengeSession session)
    {
        Debug.Log($"Challenge scheduled: {session.challenge.challengeName} for Day {session.dayNumber}");
        // Could show UI notification here
    }

    private void OnChallengeStarted(ChallengeSession session)
    {
        Debug.Log($"Challenge started: {session.challenge.challengeName}");
        // Could show challenge start UI here
    }

    private void OnChallengeCompleted(ChallengeResult result)
    {
        Debug.Log($"Challenge completed: {result.challengeName} - Winner: {result.winningTribeName}");
        // Could show results UI here
    }

    private void OnImmunityGranted(string tribeId)
    {
        Debug.Log($"Immunity granted to tribe: {tribeId}");
        // Could show immunity notification here
    }
} 