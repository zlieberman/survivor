using UnityEngine;
using UnityEngine.UI;
using Survivor.Shared.Interfaces;
using Survivor.Shared;

namespace Survivor.UI
{
    public class AudioController : MonoBehaviour
    {
        [Header("UI Controls")]
        [SerializeField] private Button playMusicButton;
        [SerializeField] private Button stopMusicButton;
        [SerializeField] private Button pauseMusicButton;
        [SerializeField] private Button resumeMusicButton;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Toggle musicToggle;
        [SerializeField] private Toggle sfxToggle;

        [Header("Audio Settings")]
        [SerializeField] private AudioClip[] musicClips;
        [SerializeField] private AudioClip[] sfxClips;

        private IAudioService audioService;

        private void Start()
        {
            // Find the audio service using the service locator
            audioService = AudioServiceLocator.GetAudioService();
            if (audioService == null)
            {
                Debug.LogWarning("AudioController: No IAudioService implementation found in scene!");
                return;
            }

            SetupUI();
        }

        private void SetupUI()
        {
            // Setup play music button
            if (playMusicButton != null)
            {
                playMusicButton.onClick.AddListener(PlayMusic);
            }

            // Setup stop music button
            if (stopMusicButton != null)
            {
                stopMusicButton.onClick.AddListener(StopMusic);
            }

            // Setup pause music button
            if (pauseMusicButton != null)
            {
                pauseMusicButton.onClick.AddListener(PauseMusic);
            }

            // Setup resume music button
            if (resumeMusicButton != null)
            {
                resumeMusicButton.onClick.AddListener(ResumeMusic);
            }

            // Setup music volume slider
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.value = audioService?.MusicVolume ?? 0.7f;
                musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
            }

            // Setup SFX volume slider
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = audioService?.SFXVolume ?? 1f;
                sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
            }

            // Setup music toggle
            if (musicToggle != null)
            {
                musicToggle.isOn = audioService?.IsMusicPlaying ?? false;
                musicToggle.onValueChanged.AddListener(OnMusicToggleChanged);
            }

            // Setup SFX toggle
            if (sfxToggle != null)
            {
                sfxToggle.isOn = true;
                sfxToggle.onValueChanged.AddListener(OnSFXToggleChanged);
            }
        }

        public void PlayMusic()
        {
            if (audioService != null)
            {
                audioService.PlayTropicalIslandMusic();
                UpdateMusicToggle();
            }
        }

        public void StopMusic()
        {
            if (audioService != null)
            {
                audioService.StopMusic();
                UpdateMusicToggle();
            }
        }

        public void PauseMusic()
        {
            if (audioService != null)
            {
                audioService.PauseMusic();
                UpdateMusicToggle();
            }
        }

        public void ResumeMusic()
        {
            if (audioService != null)
            {
                audioService.ResumeMusic();
                UpdateMusicToggle();
            }
        }

        public void SetMusicVolume(float volume)
        {
            if (audioService != null)
            {
                audioService.SetMusicVolume(volume);
            }
        }

        public void SetSFXVolume(float volume)
        {
            if (audioService != null)
            {
                audioService.SetSFXVolume(volume);
            }
        }

        public void PlayRandomSFX()
        {
            if (audioService != null && sfxClips != null && sfxClips.Length > 0)
            {
                AudioClip randomClip = sfxClips[Random.Range(0, sfxClips.Length)];
                audioService.PlaySFX(randomClip);
            }
        }

        public void PlayAmbientSound()
        {
            if (audioService != null)
            {
                audioService.PlayRandomAmbientSound();
            }
        }

        private void OnMusicToggleChanged(bool isOn)
        {
            if (audioService != null)
            {
                if (isOn)
                {
                    audioService.PlayTropicalIslandMusic();
                }
                else
                {
                    audioService.StopMusic();
                }
            }
        }

        private void OnSFXToggleChanged(bool isOn)
        {
            if (audioService != null)
            {
                audioService.SetSFXVolume(isOn ? 1f : 0f);
            }
        }

        private void UpdateMusicToggle()
        {
            if (musicToggle != null && audioService != null)
            {
                musicToggle.isOn = audioService.IsMusicPlaying;
            }
        }

        private void OnDestroy()
        {
            // Clean up event listeners
            if (playMusicButton != null)
                playMusicButton.onClick.RemoveListener(PlayMusic);
            if (stopMusicButton != null)
                stopMusicButton.onClick.RemoveListener(StopMusic);
            if (pauseMusicButton != null)
                pauseMusicButton.onClick.RemoveListener(PauseMusic);
            if (resumeMusicButton != null)
                resumeMusicButton.onClick.RemoveListener(ResumeMusic);
            if (musicVolumeSlider != null)
                musicVolumeSlider.onValueChanged.RemoveListener(SetMusicVolume);
            if (sfxVolumeSlider != null)
                sfxVolumeSlider.onValueChanged.RemoveListener(SetSFXVolume);
            if (musicToggle != null)
                musicToggle.onValueChanged.RemoveListener(OnMusicToggleChanged);
            if (sfxToggle != null)
                sfxToggle.onValueChanged.RemoveListener(OnSFXToggleChanged);
        }
    }
} 