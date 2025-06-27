# Survivor Challenge System

This document describes the challenge system infrastructure for the Survivor game, which handles the scheduling, execution, and management of challenges that occur at 12:00 PM every day (starting from day 2).

## Overview

The challenge system consists of several key components:

1. **ChallengeManager** - Main orchestrator that handles scheduling and coordination
2. **ChallengeData** - Data structures for challenges, sessions, and results
3. **IChallengeController** - Interface and base class for challenge implementations
4. **ChallengeSceneManager** - Handles scene transitions and player spawning
5. **RaceChallengeController** - Example implementation of a race challenge

## Core Components

### ChallengeManager
The main manager that:
- Monitors game time and schedules challenges at 12:00 PM
- Selects random challenges from available pool
- Manages team formation and "sit out" players
- Handles scene transitions between main game and challenge scenes
- Saves challenge results and grants immunity

**Key Features:**
- Automatic challenge scheduling at 12:00 PM daily
- Random challenge selection with repeat avoidance
- Team balancing with automatic "sit out" player selection
- Immunity system for winning tribes
- Persistent challenge history

### ChallengeData
Data structures that define the challenge system:

- **ChallengeDefinition** - Defines a challenge's properties, rules, and requirements
- **ChallengeSession** - Represents an active challenge instance
- **ChallengeResult** - Stores the outcome of a completed challenge
- **ChallengeParticipant** - Represents a player in a challenge
- **ChallengeConfig** - Configuration for the challenge system

### IChallengeController Interface
Provides a consistent API for all challenge implementations:

```csharp
public interface IChallengeController
{
    void InitializeChallenge(ChallengeSession session);
    void StartChallenge();
    void PauseChallenge();
    void ResumeChallenge();
    void EndChallenge(ChallengeResult result);
    float GetChallengeProgress();
    Dictionary<string, int> GetTeamScores();
    bool IsChallengeComplete();
    string GetWinningTribe();
    
    // Events
    event Action<ChallengeSession> OnChallengeStarted;
    event Action<ChallengeResult> OnChallengeEnded;
    event Action<float> OnProgressUpdated;
    event Action<Dictionary<string, int>> OnScoresUpdated;
}
```

### BaseChallengeController
Abstract base class that provides common functionality:
- Team management and scoring
- Progress tracking
- Event handling
- Result creation

## Challenge Types

The system supports different challenge types through the `ChallengeType` enum:

- **Race** - Basic race challenges (implemented example)
- **Puzzle** - Logic/puzzle challenges
- **Physical** - Strength/endurance challenges
- **Mental** - Memory/knowledge challenges
- **Social** - Communication/teamwork challenges
- **Hybrid** - Combination of multiple types

## Creating a New Challenge

To create a new challenge type:

1. **Create the Challenge Definition:**
```csharp
var newChallenge = new ChallengeDefinition
{
    challengeId = "puzzle_001",
    challengeName = "Logic Puzzle Challenge",
    description = "Solve puzzles to win immunity",
    challengeType = ChallengeType.Puzzle,
    minPlayers = 4,
    maxPlayers = 8,
    challengeSceneName = "PuzzleChallenge_001",
    estimatedDuration = 300f,
    isTribeChallenge = true,
    requiresEvenTeams = true,
    maxSitOutPlayers = 1,
    grantsImmunity = true,
    immunityDays = 1,
    isEnabled = true,
    difficulty = 3
};
```

2. **Create the Challenge Controller:**
```csharp
public class PuzzleChallengeController : BaseChallengeController
{
    // Implement challenge-specific logic
    public override void StartChallenge()
    {
        base.StartChallenge();
        // Initialize puzzle-specific state
    }
    
    // Add challenge-specific methods
    public void OnPuzzleSolved(string tribeId, int puzzleId)
    {
        // Handle puzzle completion
    }
}
```

3. **Create the Challenge Scene:**
- Create a new Unity scene
- Add spawn points for teams
- Add challenge-specific game objects
- Add the challenge controller
- Add UI elements

4. **Register the Challenge:**
Add the challenge definition to the `ChallengeManager`'s available challenges list.

## Challenge Scene Setup

Each challenge scene should include:

1. **ChallengeSceneManager** - Handles player spawning and scene setup
2. **Challenge Controller** - Implements the specific challenge logic
3. **Spawn Points** - For team 1, team 2, and sit-out players
4. **UI Elements** - Challenge-specific UI components
5. **Challenge Elements** - Interactive objects for the challenge

### Required Components in Challenge Scene:

```csharp
// ChallengeSceneManager setup
[SerializeField] private Transform[] team1SpawnPoints;
[SerializeField] private Transform[] team2SpawnPoints;
[SerializeField] private Transform[] sitOutSpawnPoints;
[SerializeField] private GameObject playerPrefab;
[SerializeField] private GameObject npcPrefab;
```

## Team Management

The system automatically handles team formation:

- **Even Teams**: Ensures both teams have the same number of players
- **Sit Out Players**: Automatically selects players to sit out if teams are uneven
- **Tribe Assignment**: Players are grouped by their tribe ID
- **Main Player**: The main player is always included in challenges

## Immunity System

Winning tribes receive immunity:
- Immunity prevents tribe elimination
- Immunity duration is configurable per challenge
- Immunity status is tracked and saved

## Configuration

The challenge system can be configured through `ChallengeConfig`:

```csharp
var config = new ChallengeConfig
{
    challengeHour = 12,           // Challenge time (12:00 PM)
    challengeMinute = 0,
    firstChallengeDay = 2,        // First challenge on day 2
    randomizeChallenges = true,   // Random challenge selection
    avoidRepeatingChallenges = true,
    maxConsecutiveRepeats = 1,
    autoBalanceTeams = true,
    minTeamSize = 2,
    maxTeamSize = 4,
    enableImmunity = true,
    defaultImmunityDays = 1
};
```

## Integration with Main Game

The challenge system integrates with the main game through:

1. **GameManager** - Initializes the challenge system
2. **TimeManager** - Provides time tracking for challenge scheduling
3. **Scene Management** - Handles transitions between main game and challenge scenes
4. **Player Management** - Preserves player state during transitions

## Debug Features

The system includes comprehensive debug features:

- **Context Menu Actions**: Test challenge scheduling, spawning, and completion
- **Status Display**: Show current challenge state and configuration
- **Logging**: Detailed logging for troubleshooting

## Example Usage

### Testing Challenge Scheduling:
```csharp
// In ChallengeManager
[ContextMenu("Test Schedule Challenge")]
public void TestScheduleChallenge()
{
    var (_, _, day) = GameTimeService.GetCurrentGameTime();
    ScheduleChallenge(day);
}
```

### Checking Challenge Status:
```csharp
// In ChallengeManager
[ContextMenu("Show Challenge Status")]
public void ShowChallengeStatus()
{
    Debug.Log($"Available challenges: {availableChallenges.Count}");
    Debug.Log($"Completed challenges: {completedChallenges.Count}");
    Debug.Log($"Challenge time: {isChallengeTime}");
    Debug.Log($"In challenge scene: {isInChallengeScene}");
}
```

## File Structure

```
Survivor/Assets/Scripts/Challenges/
├── ChallengeData.cs              # Core data structures
├── IChallengeController.cs       # Interface and base class
├── ChallengeManager.cs           # Main challenge orchestrator
├── ChallengeSceneManager.cs      # Scene management
├── RaceChallengeController.cs    # Example race challenge
└── README.md                     # This documentation
```

## Future Enhancements

Potential improvements for the challenge system:

1. **ScriptableObject Challenges** - Store challenge definitions as assets
2. **Challenge Editor** - Visual editor for creating challenges
3. **Advanced Team Balancing** - More sophisticated team formation algorithms
4. **Challenge Progression** - Difficulty scaling based on game progress
5. **Multiplayer Support** - Network synchronization for multiplayer challenges
6. **Challenge Replays** - Record and replay challenge sessions
7. **Statistics Tracking** - Detailed challenge performance metrics

## Troubleshooting

### Common Issues:

1. **Challenges not scheduling**: Check if TimeManager is properly initialized
2. **Players not spawning**: Verify spawn points are assigned in ChallengeSceneManager
3. **Challenge not starting**: Ensure challenge controller implements IChallengeController
4. **Scene not loading**: Verify challenge scene name matches ChallengeDefinition

### Debug Steps:

1. Check ChallengeManager status using context menu
2. Verify TimeManager is providing correct game time
3. Ensure all required components are present in challenge scene
4. Check console for error messages and warnings 