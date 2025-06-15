using UnityEngine;
using System.Collections.Generic;
using Survivor.Challenges;
using Survivor.Tribes;

namespace Survivor.Core
{
    public interface IUIManager
    {
        void Initialize(GameObject mainMenuPanel, GameObject gameHudPanel, GameObject dialoguePanel, GameObject challengePanel, GameObject pauseMenuPanel);
        void ShowMainMenu();
        void ShowGameHUD();
        void ShowDialogue();
        void HideDialogue();
        void ShowChallenge();
        void HideChallenge();
        void ShowPauseMenu();
        void HidePauseMenu();
        void ShowChallengeUI(Challenge challenge);
        void HideChallengeUI();
        void UpdateChallengeProgress(float progress);
        void ShowVotingUI();
        void HideVotingUI();
        void UpdateVoteCounts(Dictionary<string, int> voteCounts);
    }
} 