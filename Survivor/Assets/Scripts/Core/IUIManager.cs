using UnityEngine;
using System.Collections.Generic;
using Survivor.Characters;

namespace Survivor.Core
{
    public interface IUIManager
    {
        void Initialize(GameObject mainMenuPanel, GameObject gameHudPanel, GameObject dialoguePanel, GameObject challengePanel, GameObject pauseMenuPanel);
        void ShowMainMenu();
        void ShowGameHUD();
        void ShowDialogue();
        void HideDialogue();
        void ShowPauseMenu();
        void HidePauseMenu();
        void HideChallengeUI();
        void UpdateChallengeProgress(float progress);
        void ShowVotingUI();
        void HideVotingUI();
        void UpdateVoteCounts(Dictionary<string, int> voteCounts);
    }
} 