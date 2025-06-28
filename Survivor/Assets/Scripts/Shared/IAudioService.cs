using UnityEngine;

namespace Survivor.Shared
{
    public interface IAudioService
    {
        bool IsMusicPlaying { get; }
        float MusicVolume { get; }
        float SFXVolume { get; }
        AudioClip CurrentMusicClip { get; }
        
        void PlayTropicalIslandMusic();
        void StopMusic();
        void PauseMusic();
        void ResumeMusic();
        void SetMusicVolume(float volume);
        void SetSFXVolume(float volume);
        void PlaySFX(AudioClip clip, float volume = 1f);
        void PlayRandomAmbientSound();
    }
} 