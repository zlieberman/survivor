using UnityEngine;

namespace Survivor.Challenges
{
    [CreateAssetMenu(fileName = "SlidingPuzzleChallenge", menuName = "Challenges/Sliding Puzzle")]
    public class SlidingPuzzleChallenge : Challenge
    {
        [SerializeField] private int gridSize = 3;
        [SerializeField] private float timeLimit = 300f; // 5 minutes
        private float currentTime;
        private bool isActive;

        public override void Initialize()
        {
            currentTime = timeLimit;
            isActive = false;
        }

        public override void StartChallenge()
        {
            isActive = true;
            currentTime = timeLimit;
        }

        public override void EndChallenge()
        {
            isActive = false;
        }

        public override bool IsCompleted()
        {
            // Check if puzzle is solved
            return false; // Implement puzzle completion check
        }

        public override float GetProgress()
        {
            if (!isActive) return 0f;
            return 1f - (currentTime / timeLimit);
        }

        public void UpdateTime(float deltaTime)
        {
            if (isActive)
            {
                currentTime -= deltaTime;
                if (currentTime <= 0)
                {
                    EndChallenge();
                }
            }
        }
    }
} 