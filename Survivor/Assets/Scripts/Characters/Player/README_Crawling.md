# Crawling Mechanic Implementation

## Overview
This implementation adds a crawling mechanic to the player character that can be toggled by pressing the C key. When crawling, the player moves at a slower speed, the `IsCrawling` animation parameter is set to true, and the character controller's collision box is adjusted to allow crawling under small surfaces.

## Features
- **Toggle Crawling**: Press C to start/stop crawling
- **Reduced Movement Speed**: Crawling speed is slower than normal walking
- **Animation Integration**: Uses the existing `IsCrawling` animation parameter
- **Jump Disabled**: Players cannot jump while crawling
- **Smooth Transitions**: Movement speed changes are smoothly interpolated
- **Collision Box Adjustment**: Character controller height and center are adjusted when crawling to allow fitting under small surfaces
- **Space Key Mashing**: Players can mash the space key while crawling to move faster

## Implementation Details

### Files Modified

1. **PlayerInputActions.inputactions**
   - Added new "Crawl" action bound to the C key
   - Action ID: `05f6913d-c316-48b2-a6bb-e225f14c7964`

2. **StarterAssetsInputs.cs**
   - Added `crawl` boolean field
   - Added `OnCrawl()` method for input handling
   - Added `CrawlInput()` method for manual input setting

3. **ThirdPersonController.cs**
   - Added `CrawlSpeed` field (default: 1.0f)
   - Added `MaxCrawlSpeed` field (default: 8.0f) for space key mashing
   - Added `CrawlSpeedDecayRate` field for speed decay
   - Added `MashTimeWindow` and `MinMashRate` fields for space mashing
   - Added `CrawlHeight` field (default: 0.8f) for collision box height when crawling
   - Added `CrawlRadius` field (default: 0.3f) for collision box radius when crawling
   - Added crawling state management (`_isCrawling`, `_crawlInputPressed`)
   - Added `_animIDIsCrawling` animation parameter hash
   - Added character controller original value storage
   - Modified `HandleCrawlInput()` method to adjust character controller collision box
   - Modified `Move()` method to handle crawl speed
   - Added `HandleSpaceMashing()` method for space key mashing mechanics
   - Disabled jumping while crawling
   - Added public `IsCrawling` and `CurrentCrawlSpeed` properties

4. **Player.prefab** and **PlayerCapsule.prefab**
   - Added `CrawlHeight` and `CrawlRadius` settings to ThirdPersonController components

5. **CrawlCollisionTest.cs** (New)
   - Debug script to verify crawling collision adjustment is working properly

### Animation Controller
The implementation expects the animation controller to have:
- An `IsCrawling` boolean parameter
- A `Crawling` animation state that transitions based on the `IsCrawling` parameter

## Setup Instructions

### 1. Input System Setup
The input actions file has been updated with the new Crawl action. Unity should automatically regenerate the input system wrapper code.

### 2. Player Prefab Configuration
Ensure your player prefab has:
- `StarterAssetsInputs` component
- `ThirdPersonController` component
- `PlayerInput` component (if using Input System)
- `Animator` component with the proper animation controller

### 3. Animation Controller Setup
Make sure your animation controller includes:
- `IsCrawling` parameter (bool)
- `Crawling` animation state
- Proper transitions between walking/running and crawling states

### 4. Testing
Add the `CrawlCollisionTest` component to your player for debugging:
```csharp
// Add to player GameObject
var crawlTest = player.AddComponent<CrawlCollisionTest>();
```

## Usage

### Basic Usage
1. **Start Crawling**: Press C key
2. **Stop Crawling**: Press C key again
3. **Movement**: Use WASD to move while crawling (slower speed)
4. **Space Key Mashing**: Mash space key while crawling to move faster
5. **Jumping**: Disabled while crawling
6. **Collision**: Character controller automatically adjusts to fit under small surfaces

### Code Access
```csharp
// Check if player is crawling
bool isCrawling = thirdPersonController.IsCrawling;

// Get current crawl speed
float crawlSpeed = thirdPersonController.CurrentCrawlSpeed;

// Manually set crawl input (for testing)
starterAssetsInputs.crawl = true;
```

## Configuration

### Crawl Speed
Adjust the crawl speed in the ThirdPersonController component:
```csharp
public float CrawlSpeed = 1.0f; // Base crawl speed
public float MaxCrawlSpeed = 8.0f; // Maximum speed when mashing space
```

### Crawling Collision
Adjust the collision box dimensions when crawling:
```csharp
public float CrawlHeight = 0.8f; // Height of character controller when crawling
public float CrawlRadius = 0.3f; // Radius of character controller when crawling
```

### Space Key Mashing
Configure the space key mashing mechanics:
```csharp
public float CrawlSpeedDecayRate = 5.0f; // How quickly speed decays
public float MashTimeWindow = 1.0f; // Time window for measuring mash rate
public float MinMashRate = 1.0f; // Minimum presses per second to start moving
```

### Input Binding
To change the input key, modify the PlayerInputActions.inputactions file:
```json
{
    "name": "",
    "id": "05f6913d-c316-48b2-a6bb-e225f14c7966",
    "path": "<Keyboard>/c", // Change this to desired key
    "interactions": "",
    "processors": "",
    "groups": ";Keyboard&Mouse",
    "action": "Crawl",
    "isComposite": false,
    "isPartOfComposite": false
}
```

## Troubleshooting

### Common Issues

1. **Input not working**
   - Check that PlayerInput component is properly configured
   - Verify the input actions file is being used
   - Ensure the C key binding is correct

2. **Animation not playing**
   - Verify `IsCrawling` parameter exists in animator controller
   - Check that transitions are properly set up
   - Ensure the Crawling animation state exists

3. **Movement speed not changing**
   - Check that `CrawlSpeed` is set in ThirdPersonController
   - Verify the `HandleCrawlInput()` method is being called
   - Check console for debug messages

4. **Collision box not adjusting**
   - Verify `CrawlHeight` and `CrawlRadius` are set in ThirdPersonController
   - Check that character controller is properly referenced
   - Use CrawlCollisionTest script to debug collision box changes

### Debug Information
Enable debug logging in the CrawlCollisionTest component to see:
- Current crawl input state
- IsCrawling controller state
- Animation parameter state
- Character controller dimensions
- Visual collision box representation

## Future Enhancements

Potential improvements to consider:
- **Crawl Camera**: Lower camera position while crawling
- **Crawl Audio**: Different footstep sounds while crawling
- **Crawl Stamina**: Add stamina cost for crawling
- **Crawl Animation**: More detailed crawling animations
- **Crawl Physics**: Different physics behavior while crawling

## Dependencies
- Unity Input System
- Starter Assets Third Person Controller
- Animator component with proper animation controller 