using UnityEngine;
using Survivor.Core;
using Survivor.Environment;

public class SceneSetup : MonoBehaviour
{
    [Header("Core Systems")]
    public NPCManager npcManager;
    public GameDayManager gameDayManager;
    public ChallengeSystem challengeSystem;
    public VotingSystem votingSystem;
    public DialogueSystem dialogueSystem;

    [Header("Environment Systems")]
    public IslandGenerator islandGenerator;
    public WaterSystem waterSystem;
    public Campfire campfire;

    [Header("Environment Settings")]
    public Vector3 islandCenter = Vector3.zero;
    public float islandRadius = 100f;
    public float waterHeight = 0f;

    private void Awake()
    {
        InitializeEnvironment();
        InitializeGameSystems();
    }

    private void InitializeEnvironment()
    {
        // Setup water system
        if (waterSystem == null)
        {
            GameObject waterObj = new GameObject("WaterSystem");
            waterSystem = waterObj.AddComponent<WaterSystem>();
            waterSystem.transform.position = new Vector3(0, waterHeight, 0);
        }

        // Generate island
        if (islandGenerator == null)
        {
            GameObject islandObj = new GameObject("IslandGenerator");
            islandGenerator = islandObj.AddComponent<IslandGenerator>();
            islandGenerator.GenerateIsland();
        }

        // Setup campfire
        if (campfire == null)
        {
            GameObject campfireObj = new GameObject("Campfire");
            campfire = campfireObj.AddComponent<Campfire>();
            campfire.transform.position = islandCenter + Vector3.up * 0.5f; // Place slightly above ground
        }
    }

    private void InitializeGameSystems()
    {
        // Initialize NPC Manager
        if (npcManager == null)
        {
            GameObject npcObj = new GameObject("NPCManager");
            npcManager = npcObj.AddComponent<NPCManager>();
        }

        // Initialize Game Day Manager
        if (gameDayManager == null)
        {
            GameObject dayObj = new GameObject("GameDayManager");
            gameDayManager = dayObj.AddComponent<GameDayManager>();
        }

        // Initialize Challenge System
        if (challengeSystem == null)
        {
            GameObject challengeObj = new GameObject("ChallengeSystem");
            challengeSystem = challengeObj.AddComponent<ChallengeSystem>();
        }

        // Initialize Voting System
        if (votingSystem == null)
        {
            GameObject voteObj = new GameObject("VotingSystem");
            votingSystem = voteObj.AddComponent<VotingSystem>();
        }

        // Initialize Dialogue System
        if (dialogueSystem == null)
        {
            GameObject dialogueObj = new GameObject("DialogueSystem");
            dialogueSystem = dialogueObj.AddComponent<DialogueSystem>();
        }

        // Link systems together
        gameDayManager.Initialize(npcManager, challengeSystem, votingSystem);
        challengeSystem.Initialize(npcManager);
        votingSystem.Initialize(npcManager);
        dialogueSystem.Initialize(npcManager);
    }
} 