# Integrated Swimming System Setup Guide

## Overview
The swimming system is now integrated directly into the `CharacterSwimming` component, making it work automatically for any character (player or NPC) without needing scene-specific setup.

## How It Works
- **Automatic Detection**: Each character with `CharacterSwimming` automatically detects water using the Crest ocean system
- **No Scene Setup Required**: Works in any scene with a Crest OceanRenderer
- **Universal Compatibility**: Works for both players and NPCs
- **Self-Contained**: Each character handles its own water detection and swimming mechanics

## Quick Setup

### Option 1: Manual Setup (Recommended)
1. **Add to Player:**
   - Select your player GameObject
   - Add Component → Search "CharacterSwimming"
   - Configure settings as needed

2. **Add to NPCs:**
   - Select each NPC GameObject
   - Add Component → Search "CharacterSwimming"
   - Configure settings as needed

### Option 2: Automatic Setup
1. **Create Setup Helper:**
   - Create empty GameObject named "SwimmingSetup"
   - Add `SwimmingSystemSetup` component
   - Assign your player and NPC GameObjects
   - Enable "Auto Setup On Start"

## CharacterSwimming Settings

### Swimming Settings
- **Swim Speed**: Horizontal swimming speed (default: 4)
- **Swim Up Speed**: Speed when swimming upward (default: 3)
- **Swim Down Speed**: Speed when swimming downward (default: 2)
- **Water Drag**: How much water slows movement (default: 0.8)
- **Swim Depth Threshold**: Depth in units to start swimming (default: 1.0)

### Water Physics
- **Swim Gravity**: Gravity while swimming (default: -2)
- **Normal Gravity**: Normal gravity when not swimming (default: -15)
- **Buoyancy Force**: Upward force in water (default: 2)

### Water Detection
- **Detection Radius**: Radius to check for water (default: 1)
- **Detection Height**: Height to check for water (default: 2)
- **Water Layer**: Layer mask for water detection (default: Default)
- **Show Debug Gizmos**: Show visual debug information (default: false)

## Requirements
- **Crest OceanRenderer**: Must be present in the scene
- **CharacterController**: Character must have a CharacterController component
- **Animator**: Optional, for swimming animations

## How It Detects Water
1. Uses Crest's `SampleHeightHelper` to sample water height at character position
2. Compares character bottom position with water height
3. Automatically triggers swimming when character is 1 unit deep
4. Applies water resistance and buoyancy based on submersion depth

## Animation Integration
The system automatically handles swimming animations:
- Sets "IsSwimming" bool parameter
- Triggers "Swimming" animation
- Works with any animator controller that has these parameters

## Debug Features
Enable "Show Debug Gizmos" to see:
- Water detection radius (cyan/blue sphere)
- Water surface position (yellow/red sphere)
- Submersion depth line

## Public Properties
Access these from other scripts:
- `IsSwimming`: Whether character is currently swimming
- `IsInWater`: Whether character is in water
- `SubmersionDepth`: How deep the character is in water
- `WaterHeight`: Current water height at character position

## Example Usage
```csharp
// Check if character is swimming
var swimming = character.GetComponent<CharacterSwimming>();
if (swimming.IsSwimming)
{
    // Character is swimming
    Debug.Log($"Swimming at depth: {swimming.SubmersionDepth}");
}
```

## Troubleshooting

### Character doesn't swim:
- Check that Crest OceanRenderer exists in scene
- Verify CharacterController component is present
- Ensure water is at the correct height

### Swimming feels wrong:
- Adjust Swim Speed values
- Modify Water Drag for resistance
- Change Swim Depth Threshold for trigger point

### No water detection:
- Check Crest ocean system is properly configured
- Verify OceanRenderer has CollisionProvider
- Ensure water layer is set correctly 