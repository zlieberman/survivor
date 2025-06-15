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
            slidingPuzzle.id = "sliding_puzzle_1";
            slidingPuzzle.title = "Island Puzzle";
            slidingPuzzle.description = "Solve the sliding puzzle to reveal a hidden treasure map!";
            slidingPuzzle.sceneName = "SlidingPuzzleChallenge";
            slidingPuzzle.difficulty = ChallengeDifficulty.Medium;
            slidingPuzzle.type = ChallengeType.Puzzle;
            AssetDatabase.CreateAsset(slidingPuzzle, "Assets/Resources/Challenges/SlidingPuzzleChallenge.asset");

            // Create Endurance Challenge
            var endurance = CreateInstance<EnduranceChallenge>();
            endurance.id = "endurance_1";
            endurance.title = "Balance Beam";
            endurance.description = "Stay balanced on the beam by mashing the spacebar!";
            endurance.sceneName = "EnduranceChallenge";
            endurance.difficulty = ChallengeDifficulty.Hard;
            endurance.type = ChallengeType.Endurance;
            AssetDatabase.CreateAsset(endurance, "Assets/Resources/Challenges/EnduranceChallenge.asset");

            // Create Race Challenge
            var race = CreateInstance<RaceChallenge>();
            race.id = "race_1";
            race.title = "Island Dash";
            race.description = "Race through the obstacle course to reach the finish line!";
            race.sceneName = "RaceChallenge";
            race.difficulty = ChallengeDifficulty.Medium;
            race.type = ChallengeType.Race;
            AssetDatabase.CreateAsset(race, "Assets/Resources/Challenges/RaceChallenge.asset");

            AssetDatabase.SaveAssets();
            Debug.Log("Challenge assets created successfully!");
        }
    }
} 