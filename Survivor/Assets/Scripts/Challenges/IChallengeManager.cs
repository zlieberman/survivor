using UnityEngine;

namespace Survivor.Challenges
{
    public interface IChallengeManager
    {
        void StartChallenge(Challenge challenge);
        Challenge GetCurrentChallenge();
        bool IsInChallenge();
        float GetChallengeProgress();
        float GetRemainingTime();
        float GetCooldownTime();
    }
} 