# Campfire Interactable System

## Overview
The campfire interactable system allows players to interact with campfires in the camp by pressing the E key. When interacted with, the campfire switches from an unlit state to a lit state by spawning a new lit campfire prefab.

## Implementation Details

### CampfireInteractable.cs
- **Location**: `Assets/Scripts/Interactables/CampfireInteractable.cs`
- **Inherits from**: `BaseInteractable`
- **Key Features**:
  - Interaction radius of 3 units (configurable)
  - 2-second cooldown between interactions
  - Switches from unlit to lit state
  - Hides the original campfire and spawns a lit version

### Configuration
The campfire interactable requires:
1. **litCampfirePrefab**: The prefab to spawn when the campfire is lit (configured in CampGenerator)
2. **interactionRadius**: How close the player needs to be (default: 3f)
3. **litFireOffset**: Optional offset for the lit fire position (default: Vector3.zero)

### Integration with Camp Generation
The campfire is automatically set up as an interactable when the camp is generated:
- Layer is set to "Interactable"
- Physical collider is added for collision
- Trigger collider is added for interaction detection
- CampfireInteractable component is automatically added
- Lit campfire prefab is automatically assigned from CampGenerator

## Usage Instructions

### For Unity Developers:
1. **Create a lit campfire prefab** that will be spawned when the campfire is lit
2. **Assign the lit campfire prefab** to the `litCampfirePrefab` field in the CampGenerator component
3. **Test the interaction** by approaching the campfire and pressing E

### For Players:
1. **Approach the campfire** within 3 units
2. **Press E** to light the campfire
3. **Wait for the cooldown** (2 seconds) if you want to interact again

## Testing
A test script is provided at `Assets/Scripts/Editor/CampfireTest.cs`:
- Automatically finds campfire interactables in the scene
- Allows testing via the T key or context menu
- Provides debug logging for troubleshooting

## Technical Notes
- The system follows the same pattern as the WaterWellInteractable
- Uses Unity's trigger collider system for interaction detection
- Maintains the same parent hierarchy when spawning the lit campfire
- Includes proper error handling and debug logging
- Lit campfire prefab is centrally configured in CampGenerator for easy management 