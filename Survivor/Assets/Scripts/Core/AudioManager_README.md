# Audio Manager Setup Guide

This guide explains how to set up background music for your Tropical Island scene using the built-in AudioManager system.

## Architecture Overview

The audio system uses a clean architecture with:
- **IAudioService Interface** (Shared.Interfaces) - Defines the contract for audio services
- **AudioManager** (Core) - Implements the audio service with Unity's audio system
- **AudioController** (UI) - UI controls that work with any IAudioService implementation
- **AudioServiceLocator** (Shared) - Provides easy access to audio services without dependencies

This design keeps UI decoupled from Core game logic and allows for easy testing and extension.

## Quick Setup

### 1. Add AudioManager to Your Scene

**Manual Setup (Recommended):**
1. Create an empty GameObject in your TropicalIsland scene
2. Name it "AudioManager"
3. Add the `AudioManager.cs` script component to it
4. The script will automatically create the necessary AudioSource components

### 2. Add Your Music File

1. Import your tropical island music file into Unity (supports .mp3, .wav, .ogg, .aiff)
2. In the AudioManager component inspector:
   - Drag your music file into the "Tropical Island Music" field
   - Adjust the "Music Volume" slider (default: 0.7)
   - Set "Play Music On Start" to true if you want music to start automatically
   - Set "Loop Music" to true for continuous playback

### 3. Configure Audio Settings

**Fade Settings:**
- `Fade In Duration`: How long it takes for music to fade in (default: 2 seconds)
- `Fade Out Duration`: How long it takes for music to fade out (default: 1 second)

**Volume Settings:**
- `Music Volume`: Controls background music volume (0-1)
- `SFX Volume`: Controls sound effects volume (0-1)

## Using the Audio System

### From Code (Recommended - Using Service Locator)

```csharp
// Get the audio service using the service locator
IAudioService audioService = AudioServiceLocator.GetAudioService();

if (audioService != null)
{
    // Play the tropical island music
    audioService.PlayTropicalIslandMusic();
    
    // Stop the music
    audioService.StopMusic();
    
    // Pause/Resume music
    audioService.PauseMusic();
    audioService.ResumeMusic();
    
    // Adjust volume
    audioService.SetMusicVolume(0.5f);
    audioService.SetSFXVolume(0.8f);
    
    // Play sound effects
    audioService.PlaySFX(yourAudioClip, 1.0f);
}
```

### From Code (Alternative - Direct Search)

```csharp
// Find the audio service directly
IAudioService audioService = FindObjectOfType<MonoBehaviour>() as IAudioService;

// Use the service...
audioService?.PlayTropicalIslandMusic();
```

### From UI (Using AudioController)

1. Add the `AudioController.cs` script to a UI GameObject
2. The script will automatically find the AudioManager using the service locator
3. Connect UI elements in the inspector:
   - Buttons for Play/Stop/Pause/Resume
   - Sliders for volume control
   - Toggles for music/SFX on/off

### From Other Scripts

```csharp
// Check if audio service is available
if (AudioServiceLocator.HasAudioService())
{
    IAudioService audioService = AudioServiceLocator.GetAudioService();
    
    // Check if music is playing
    if (audioService.IsMusicPlaying)
    {
        // Do something
    }
    
    // Get current volume levels
    float musicVol = audioService.MusicVolume;
    float sfxVol = audioService.SFXVolume;
}
```

## Advanced Features

### Ambient Sounds

1. Add ambient sound clips to the "Ambient Sounds" array in the AudioManager
2. Call `audioService.PlayRandomAmbientSound()` to play a random ambient sound

### Audio Mixer Integration

1. Create an Audio Mixer in Unity (Window > Audio > Audio Mixer)
2. Assign it to the "Audio Mixer" field in the AudioManager
3. Set up mixer groups for Music and SFX
4. The AudioManager will automatically route audio to the appropriate groups

### Scene Persistence

The AudioManager uses the singleton pattern and `DontDestroyOnLoad()`, so it will persist between scenes. This means:
- Music will continue playing when switching scenes
- Volume settings are maintained
- Only one AudioManager instance will exist
- The service locator cache is automatically cleared when the AudioManager is destroyed

## Troubleshooting

**Music doesn't play:**
- Check that the audio clip is assigned in the inspector
- Verify the AudioManager is in the scene
- Check the console for error messages
- Ensure the music volume is not set to 0

**Audio cuts out:**
- Make sure the AudioManager GameObject is not being destroyed
- Check that the audio file is properly imported
- Verify the AudioSource components are created

**UI controls don't work:**
- Ensure the AudioController script is attached to a UI GameObject
- Check that the AudioManager is present in the scene
- Verify UI elements are properly connected in the inspector

**Service locator returns null:**
- Make sure the AudioManager is in the scene
- Check that the AudioManager implements IAudioService
- Try calling `AudioServiceLocator.ClearCache()` to refresh the cache

**Performance issues:**
- Use compressed audio formats for large files
- Set appropriate compression settings in the audio import settings
- Consider using streaming for very large audio files

## File Structure

```
Assets/
├── Scripts/
│   ├── Core/
│   │   └── AudioManager.cs          # Main audio management script
│   ├── UI/
│   │   └── AudioController.cs       # UI integration script
│   └── Shared/
│       ├── AudioServiceLocator.cs   # Service locator helper
│       └── Interfaces/
│           └── IAudioService.cs     # Audio service interface
└── [Your Audio Files]               # Add your music files here
```

## Best Practices

1. **Audio Format**: Use .ogg for music (good compression, small file size)
2. **File Size**: Keep music files under 10MB for web builds
3. **Compression**: Use "Compressed In Memory" for music, "Decompress On Load" for short SFX
4. **Volume Levels**: Set music volume to 70% or lower to avoid drowning out SFX
5. **Looping**: Enable looping for background music, disable for SFX
6. **Fade Effects**: Use fade in/out for smooth transitions
7. **Interface Usage**: Always use IAudioService interface instead of direct AudioManager references
8. **Service Locator**: Use AudioServiceLocator.GetAudioService() for easy access without dependencies
9. **Error Handling**: Always check if the audio service is null before using it

## Integration with Existing Systems

The AudioManager is designed to work alongside your existing audio systems:
- Footstep sounds in ThirdPersonController will continue to work
- The AudioManager handles background music separately from SFX
- Volume controls are independent for music and SFX
- No conflicts with existing AudioSource components
- UI is properly decoupled from Core game logic
- Service locator provides clean access without creating dependencies 