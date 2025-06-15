using UnityEngine;
using UnityEditor;
using Survivor.Challenges;

namespace Survivor.Editor
{
    public class ChallengeAssetCreator : EditorWindow
    {
        [MenuItem("Tools/Create Challenge Assets")]
        public static void CreateChallengeAssets()
        {
            // Create Sliding Puzzle Challenge
            var slidingPuzzle = CreateInstance<SlidingPuzzleChallenge>();
            slidingPuzzle.data = new ChallengeData
            {
                id = "sliding_puzzle_1",
                title = "Island Puzzle",
                description = "Solve the sliding puzzle to reveal a hidden treasure map!",
                sceneName = "SlidingPuzzleChallenge",
                difficulty = ChallengeDifficulty.Medium,
                type = ChallengeType.Puzzle,
                strengthWeight = 0.2f,
                agilityWeight = 0.3f,
                puzzleWeight = 0.5f,
                duration = 300f,
                isTeamChallenge = false
            };
            AssetDatabase.CreateAsset(slidingPuzzle, "Assets/Resources/Challenges/SlidingPuzzleChallenge.asset");

            // Create Endurance Challenge
            var endurance = CreateInstance<EnduranceChallenge>();
            endurance.data = new ChallengeData
            {
                id = "endurance_1",
                title = "Balance Beam",
                description = "Stay balanced on the beam by mashing the spacebar!",
                sceneName = "EnduranceChallenge",
                difficulty = ChallengeDifficulty.Hard,
                type = ChallengeType.Endurance,
                strengthWeight = 0.3f,
                agilityWeight = 0.4f,
                puzzleWeight = 0.3f,
                duration = 180f,
                isTeamChallenge = false
            };
            AssetDatabase.CreateAsset(endurance, "Assets/Resources/Challenges/EnduranceChallenge.asset");

            // Create Race Challenge
            var race = CreateInstance<RaceChallenge>();
            race.data = new ChallengeData
            {
                id = "race_1",
                title = "Island Dash",
                description = "Race through the obstacle course to reach the finish line!",
                sceneName = "RaceChallenge",
                difficulty = ChallengeDifficulty.Medium,
                type = ChallengeType.Race,
                strengthWeight = 0.4f,
                agilityWeight = 0.5f,
                puzzleWeight = 0.1f,
                duration = 120f,
                isTeamChallenge = false
            };
            AssetDatabase.CreateAsset(race, "Assets/Resources/Challenges/RaceChallenge.asset");

            AssetDatabase.SaveAssets();
            Debug.Log("Challenge assets created successfully!");
        }
    }
} 