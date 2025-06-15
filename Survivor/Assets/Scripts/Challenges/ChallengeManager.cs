using UnityEngine;
using System;

namespace Survivor.Challenges
{
    public class ChallengeManager : MonoBehaviour
    {
        public static ChallengeManager Instance { get; private set; }
        
        public Challenge CurrentChallenge { get; private set; }
        public event Action<Challenge> OnChallengeStarted;
        public event Action<Challenge> OnChallengeEnded;
        public event Action<float> OnProgressUpdated;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void StartChallenge(Challenge challenge)
        {
            if (CurrentChallenge != null)
            {
                EndCurrentChallenge();
            }

            CurrentChallenge = challenge;
            CurrentChallenge.Initialize();
            CurrentChallenge.StartChallenge();
            OnChallengeStarted?.Invoke(CurrentChallenge);
        }

        public void EndCurrentChallenge()
        {
            if (CurrentChallenge != null)
            {
                CurrentChallenge.EndChallenge();
                OnChallengeEnded?.Invoke(CurrentChallenge);
                CurrentChallenge = null;
            }
        }

        public void UpdateProgress(float progress)
        {
            OnProgressUpdated?.Invoke(progress);
        }

        private void Update()
        {
            if (CurrentChallenge != null)
            {
                float progress = CurrentChallenge.GetProgress();
                UpdateProgress(progress);

                if (CurrentChallenge.IsCompleted())
                {
                    EndCurrentChallenge();
                }
            }
        }
    }
} 