# Water System Setup Guide

This enhanced water system provides realistic water physics and swimming mechanics for your Unity project using the Starter Assets third-person controller.

## Quick Setup

### Method 1: Automatic Setup (Recommended)
1. Add the `PlayerSwimmingSetup` component to your player GameObject
2. The script will automatically add the `CharacterSwimming` component and configure it
3. Create water volumes using the `WaterVolume` component

### Method 2: Manual Setup
1. Add the `CharacterSwimming` component to your player GameObject
2. Create water volumes using the `WaterVolume` component
3. Configure the settings as needed

## Components Overview

### CharacterSwimming
- **Purpose**: Handles swimming mechanics for the player
- **Requirements**: ThirdPersonController, CharacterController, StarterAssetsInputs
- **Key Features**:
  - Automatic swimming detection (2/3 submerged threshold)
  - Swimming movement with WASD + Jump/Sprint for vertical movement
  - Buoyancy physics
  - Water resistance when wading

### WaterVolume
- **Purpose**: Creates water volumes with visual representation and physics
- **Features**:
  - Configurable volume size and water height
  - Automatic water surface creation
  - Built-in water material
  - Gizmos for easy editing

### WaterSystem
- **Purpose**: Core water physics system
- **Features**:
  - Handles both CharacterController and Rigidbody objects
  - Buoyancy and drag forces
  - Swimming threshold detection
  - Wave animation support

## Usage Instructions

### Creating Water Volumes

1. **Using WaterVolume Component**:
   ```
   - Create an empty GameObject
   - Add the WaterVolume component
   - Configure the volume size and water height
   - The component will automatically create the water surface
   ```

2. **Using PlayerSwimmingSetup**:
   ```
   - Right-click on PlayerSwimmingSetup component
   - Select "Create Test Water Volume"
   - This creates a 20x5x20 water volume 10 units in front of the player
   ```

### Swimming Controls

When the player is more than 2/3 submerged in water:
- **WASD**: Swim horizontally
- **Jump**: Swim upward
- **Sprint**: Swim downward
- **Movement**: Normal walking with water resistance

### Configuration

#### CharacterSwimming Settings
- `swimSpeed`: Horizontal swimming speed
- `swimUpSpeed`: Upward swimming speed
- `swimDownSpeed`: Downward swimming speed
- `swimThreshold`: Submersion percentage to trigger swimming (default: 0.67 = 2/3)
- `swimGravity`: Gravity when swimming
- `buoyancyForce`: Upward force applied in water

#### WaterVolume Settings
- `waterHeight`: Height of the water surface
- `volumeSize`: Size of the water volume (X, Y, Z)
- `buoyancyForce`: Buoyancy force for Rigidbody objects
- `dragForce`: Water resistance for Rigidbody objects
- `swimThreshold`: Swimming trigger threshold
- `showWaterSurface`: Whether to show the water surface mesh

## Animation Support

The system supports swimming animations through the Animator:
- `Swimming` bool parameter: Set to true when swimming
- `Speed` float parameter: Updated with swimming speed
- `Grounded` bool parameter: Set to false when swimming

## Troubleshooting

### Player Not Swimming
1. Ensure the player has the `CharacterSwimming` component
2. Check that the player is tagged as "Player" or named "Player"
3. Verify the water volume has the `WaterVolume` component
4. Check the submersion threshold (default 0.67 = 2/3)

### Water Not Visible
1. Ensure `showWaterSurface` is enabled on the WaterVolume
2. Check that the water material is assigned
3. Verify the water surface is positioned correctly

### Physics Issues
1. Ensure the water volume has a BoxCollider with `isTrigger = true`
2. Check that the player has a CharacterController (not Rigidbody)
3. Verify the water volume size is appropriate

## Performance Notes

- The system is optimized for CharacterController-based players
- Water volumes use trigger colliders for efficient detection
- Wave animation is optional and can be disabled
- Fish system is optional and can be disabled by not assigning a fish prefab

## Extending the System

### Adding Custom Water Effects
1. Extend the `WaterSystem` class
2. Override the `UpdateWaterAnimation()` method
3. Add your custom water effects

### Custom Swimming Animations
1. Add a "Swimming" bool parameter to your Animator
2. Create swimming animation clips
3. Set up transitions based on the Swimming parameter

### Multiple Water Volumes
- You can have multiple water volumes in a scene
- Each volume operates independently
- The player will interact with all volumes they enter

## Example Usage

```csharp
// Get swimming component
CharacterSwimming swimming = player.GetComponent<CharacterSwimming>();

// Check if player is swimming
if (swimming.IsSwimming)
{
    Debug.Log("Player is swimming!");
}

// Get submersion percentage
float submersion = swimming.GetSubmersionPercentage();
Debug.Log($"Player is {submersion * 100}% submerged");

// Force swimming state (for debugging)
swimming.ForceSwimmingState(true);
``` 