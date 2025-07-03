# Input Mapping Checkpoint System

A modular and extendable framework for creating checkpoints that change player input controls when triggered. This system allows you to create dynamic gameplay experiences where player controls are modified based on their location or progress through a course.

## Overview

The Input Mapping Checkpoint System consists of several components:

1. **InputMapping** - Data structure for defining input configurations
2. **InputMappingManager** - Singleton manager that handles input state globally
3. **InputMappingCheckpoint** - Basic checkpoint that only changes inputs
4. **EnhancedInputMappingCheckpoint** - Advanced checkpoint that combines race functionality with input changes
5. **InputMappingCheckpointSetup** - Utility for quickly setting up checkpoints

## New Features: Advanced Movement Control

The system now includes dedicated controls for **crawling movement** and **swimming movement**, making it easy to create sophisticated movement challenges:

### Crawling Movement Control
- **`allowCrouch`** - Controls whether player can enter/exit crawl mode (C key)
- **`allowCrawlMovement`** - Controls whether space bar mashing works while crawling
- This separation allows for scenarios like "can enter crawl mode but can't move while crawling"

### Swimming Movement Control
- **`allowSwimming`** - Controls whether player can move while in water
- Allows disabling swimming even when player is in water zones
- Perfect for water-based challenges where movement is restricted

## Quick Start

### 1. Set Up the Manager

The system requires an `InputMappingManager` in your scene. You can add it in several ways:

**Option A: Automatic Setup**
- Add the `InputMappingCheckpointSetup` component to any GameObject
- Enable "Create InputMappingManager" in the inspector
- The manager will be created automatically when the scene starts

**Option B: Manual Setup**
- Create an empty GameObject named "InputMappingManager"
- Add the `InputMappingManager` component to it

### 2. Create Your First Checkpoint

**Option A: Using the Setup Utility**
1. Add `InputMappingCheckpointSetup` to a GameObject
2. Use the context menu or inspector buttons to create preset checkpoints
3. Position them where you want the input changes to occur

**Option B: Manual Creation**
1. Create a GameObject (can be a cube, sphere, or any object)
2. Add the `InputMappingCheckpoint` component
3. Configure the input mapping in the inspector
4. Position it in your scene

### 3. Configure Input Mapping

Each checkpoint has an `InputMapping` that defines:
- **Key Codes**: Which keys control each action
- **Permissions**: Which inputs are allowed/disallowed
- **Description**: What this checkpoint does

## Components

### InputMapping

A serializable data structure that defines input configurations:

```csharp
[Header("Movement Controls")]
public Key forwardKey = Key.W;
public Key backwardKey = Key.S;
public Key leftKey = Key.A;
public Key rightKey = Key.D;

[Header("Action Controls")]
public Key jumpKey = Key.Space;
public Key sprintKey = Key.LeftShift;
public Key crouchKey = Key.C;

[Header("Special Movement Controls")]
public Key crawlMovementKey = Key.Space;

[Header("Input Permissions - Basic Movement")]
public bool allowForward = true;
public bool allowBackward = true;
public bool allowLeft = true;
public bool allowRight = true;

[Header("Input Permissions - Actions")]
public bool allowJump = true;
public bool allowSprint = true;
public bool allowCrouch = true;

[Header("Input Permissions - Special Movement")]
public bool allowCrawlMovement = true;
public bool allowSwimming = true;
```

**Preset Mappings:**
- `InputMapping.CreateDefault()` - All inputs enabled
- `InputMapping.CreateForwardOnly()` - Only forward movement
- `InputMapping.CreateDisabled()` - All inputs disabled
- `InputMapping.CreateJumpOnly()` - Only jumping allowed
- `InputMapping.CreateCrawlModeOnly()` - Can enter crawl mode but can't move while crawling
- `InputMapping.CreateCrawlMovementOnly()` - Crawling movement only
- `InputMapping.CreateSwimmingOnly()` - Swimming movement only
- `InputMapping.CreateNoSwimming()` - All movement except swimming

### InputMappingManager

Singleton manager that:
- Applies input mappings to the player
- Maintains the current input state
- Provides events for other systems
- Offers debug information

**Key Methods:**
- `SetInputMapping(InputMapping mapping)` - Apply a new mapping
- `ResetToDefault()` - Return to default controls
- `IsInputAllowed(InputType type)` - Check if an input is currently allowed

### InputMappingCheckpoint

Basic checkpoint component that:
- Triggers when a player enters its area
- Changes the input mapping
- Provides visual and audio feedback
- Can be reset to allow re-triggering

**Features:**
- Automatic trigger collider setup
- Visual material changes
- Audio feedback
- Debug gizmos for scene view

### EnhancedInputMappingCheckpoint

Advanced checkpoint that combines:
- Race challenge functionality (checkpoint progress, finish lines)
- Input mapping changes
- Integration with existing race controllers

**Additional Features:**
- Works with `RaceChallengeController` and `EnhancedRaceChallengeController`
- Option to reset inputs when leaving the checkpoint area
- Enhanced visual feedback options

## Usage Examples

### Example 1: Forward-Only Section

Create a checkpoint that only allows forward movement:

```csharp
// Create the mapping
var mapping = InputMapping.CreateForwardOnly();
mapping.checkpointName = "Forward Only Section";
mapping.description = "Player can only move forward";

// Apply to checkpoint
checkpoint.SetInputMapping(mapping);
```

### Example 2: Jump-Only Challenge

Create a checkpoint that only allows jumping:

```csharp
var mapping = InputMapping.CreateJumpOnly();
mapping.checkpointName = "Jump Challenge";
mapping.description = "Player can only jump";

checkpoint.SetInputMapping(mapping);
```

### Example 3: Crawling Challenge

Create a checkpoint where players can enter crawl mode but must mash space to move:

```csharp
var mapping = InputMapping.CreateCrawlMovementOnly();
mapping.checkpointName = "Crawling Challenge";
mapping.description = "Player must crawl and use space bar mashing to move";

checkpoint.SetInputMapping(mapping);
```

### Example 4: Swimming Restriction

Create a checkpoint that prevents swimming movement:

```csharp
var mapping = InputMapping.CreateNoSwimming();
mapping.checkpointName = "No Swimming Zone";
mapping.description = "Player cannot move while in water";

checkpoint.SetInputMapping(mapping);
```

### Example 5: Advanced Crawling Control

Create a checkpoint where players can enter crawl mode but cannot move while crawling:

```csharp
var mapping = InputMapping.CreateCrawlModeOnly();
mapping.checkpointName = "Crawl Lock Challenge";
mapping.description = "Player can enter crawl mode but cannot move while crawling";

checkpoint.SetInputMapping(mapping);
```

### Example 6: Custom Advanced Mapping

Create a custom mapping with specific crawling and swimming controls:

```csharp
var mapping = new InputMapping
{
    checkpointName = "Advanced Movement Challenge",
    forwardKey = Key.W,
    backwardKey = Key.S,
    leftKey = Key.A,
    rightKey = Key.D,
    jumpKey = Key.Space,
    sprintKey = Key.LeftShift,
    crouchKey = Key.C,
    crawlMovementKey = Key.Space,
    allowForward = true,
    allowBackward = false,  // Disable backward movement
    allowLeft = true,
    allowRight = true,
    allowJump = false,      // Disable jumping
    allowSprint = false,    // Disable sprinting
    allowCrouch = true,     // Allow entering crawl mode
    allowCrawlMovement = true,  // Allow crawl movement
    allowSwimming = false,  // Disable swimming
    description = "Advanced movement with crawling but no swimming"
};
```

## Advanced Movement System Integration

### Movement Modes

The system integrates with a modular movement system that includes:

#### CrawlingMovementMode
- **Activation**: Controlled by `allowCrouch` permission
- **Movement**: Controlled by `allowCrawlMovement` permission
- **Space Bar Mashing**: Only works when `allowCrawlMovement` is true
- **Character Controller**: Automatically adjusts collision box when crawling

#### SwimmingMovementMode
- **Movement**: Controlled by `allowSwimming` permission
- **Water Detection**: Still handled by external water trigger systems
- **Input Filtering**: Movement inputs are disabled when `allowSwimming` is false

#### NormalMovementMode
- **Standard Movement**: Walking, running, jumping
- **Always Available**: Fallback mode when other modes are disabled

### How Movement Modes Work with Input Mappings

1. **Mode Activation**: Each mode checks if it `CanActivate()` based on current input mapping
2. **Input Processing**: Each mode processes only allowed inputs from the current mapping
3. **Automatic Switching**: System automatically switches between modes based on conditions and permissions

## Integration with Existing Systems

### Race Challenges

The `EnhancedInputMappingCheckpoint` integrates with your existing race challenge system:

1. **Checkpoint Progress**: Still tracks race progress normally
2. **Input Changes**: Additionally changes player controls
3. **Finish Lines**: Can change inputs when crossing finish lines
4. **Tribe Support**: Works with multiple tribes/teams

### Player Controller

The system works with your existing `StarterAssetsInputs` component:
- Automatically finds the player input component
- Applies filtered inputs in real-time
- Maintains compatibility with existing movement systems

## Debug Features

### Visual Debugging

- **Gizmos**: Checkpoints show trigger radius and status in scene view
- **Labels**: Checkpoint names and status are displayed
- **Colors**: Green = triggered, Yellow = active

### Debug Methods

Each checkpoint has context menu options:
- **Test Trigger**: Manually trigger the checkpoint
- **Reset Checkpoint**: Reset to allow re-triggering
- **Set Preset**: Quickly apply preset mappings (including new crawling/swimming presets)

### On-Screen Debug Info

The `InputMappingManager` can display current input state:
- Current checkpoint name
- Which inputs are allowed/disallowed
- Real-time status updates
- **New**: Crawl Movement and Swimming status

## Best Practices

### 1. Planning Your Checkpoints

- **Clear Purpose**: Each checkpoint should have a clear gameplay purpose
- **Progressive Difficulty**: Start simple, increase complexity
- **Player Feedback**: Use visual and audio cues to indicate changes
- **Movement Variety**: Use crawling and swimming controls to create unique challenges

### 2. Input Mapping Design

- **Intuitive**: Keep key bindings consistent when possible
- **Accessible**: Consider alternative input methods
- **Documented**: Use clear descriptions for each mapping
- **Layered Control**: Use separate controls for mode entry vs. movement within mode

### 3. Advanced Movement Challenges

- **Crawling Scenarios**: 
  - Use `CreateCrawlModeOnly()` for "get into position but don't move" challenges
  - Use `CreateCrawlMovementOnly()` for full crawling obstacle courses
- **Swimming Scenarios**:
  - Use `CreateSwimmingOnly()` for water-only movement challenges
  - Use `CreateNoSwimming()` to create "dangerous water" zones

### 4. Performance

- **Efficient**: The system runs every frame, keep it lightweight
- **Cached References**: Components cache references to avoid repeated searches
- **Event-Driven**: Use events for complex interactions

### 5. Testing

- **Test Each Checkpoint**: Verify inputs change as expected
- **Test Transitions**: Ensure smooth transitions between mappings
- **Test Edge Cases**: What happens if the manager is missing?
- **Test Movement Modes**: Verify crawling and swimming work correctly with each mapping

## Troubleshooting

### Common Issues

1. **Inputs Not Changing**
   - Check that `InputMappingManager` exists in the scene
   - Verify that checkpoints have valid input mappings configured
   - Look for error messages in the console

2. **Crawling Not Working**
   - Ensure `allowCrouch` is true to enter crawl mode
   - Check that `allowCrawlMovement` is true for space bar mashing
   - Verify that `MovementSystemManager` exists in the scene

3. **Swimming Not Working**
   - Check that `allowSwimming` is true in the current mapping
   - Ensure player is actually in a water trigger zone
   - Verify that the swimming system is properly configured

4. **Movement System Not Found**
   - Add `MovementSystemSetup` component to any GameObject in the scene
   - The movement system will be created automatically

### Debug Steps

1. **Enable Debug Info**: Turn on `showDebugInfo` in `InputMappingManager`
2. **Check Console**: Look for debug messages about input state changes
3. **Use Context Menus**: Right-click on checkpoints to test them manually
4. **Verify Components**: Ensure all required components exist in the scene

## Context Menu Quick Actions

### On InputMappingCheckpoint objects:
- Set Forward Only
- Set Disabled  
- Set Jump Only
- **New**: Set Crawl Mode Only
- **New**: Set Crawl Movement Only
- **New**: Set Swimming Only
- **New**: Set No Swimming

### On InputMappingCheckpointSetup objects:
- Create InputMappingManager
- Create Debug Checkpoints
- Create Forward Only Checkpoint
- Create Disabled Checkpoint
- Create Jump Only Checkpoint
- **New**: Create Crawl Mode Only Checkpoint
- **New**: Create Crawl Movement Only Checkpoint
- **New**: Create Swimming Only Checkpoint
- **New**: Create No Swimming Checkpoint

This enhanced system provides fine-grained control over player movement, making it easy to create sophisticated challenges that involve crawling, swimming, and standard movement restrictions through simple checkpoint placement. 