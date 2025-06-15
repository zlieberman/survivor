using UnityEngine;

namespace Survivor.Challenges
{
    [CreateAssetMenu(fileName = "RaceChallenge", menuName = "Challenges/Race")]
    public class RaceChallenge : Challenge
    {
        [SerializeField] private float timeLimit = 180f; // 3 minutes
        [SerializeField] private int checkpointCount = 5;
        
        private float currentTime;
        private int currentCheckpoint;
        private bool isActive;

        public override void Initialize()
        {
            currentTime = timeLimit;
            currentCheckpoint = 0;
            isActive = false;
        }

        public override void StartChallenge()
        {
            isActive = true;
            currentTime = timeLimit;
            currentCheckpoint = 0;
        }

        public override void EndChallenge()
        {
            isActive = false;
        }

        public override bool IsCompleted()
        {
            return currentCheckpoint >= checkpointCount;
        }

        public override float GetProgress()
        {
            return (float)currentCheckpoint / checkpointCount;
        }

        public void UpdateTime(float deltaTime)
        {
            if (!isActive) return;

            currentTime -= deltaTime;
            if (currentTime <= 0f)
            {
                EndChallenge();
            }
        }

        public void ReachedCheckpoint()
        {
            if (!isActive) return;

            currentCheckpoint++;
            if (currentCheckpoint >= checkpointCount)
            {
                EndChallenge();
            }
        }
    }
} 