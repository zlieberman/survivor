# Tent Interactable System

## Overview
The tent interactable system allows players to rest in tents to restore energy and advance game time. This system is similar to the campfire interactable but focuses on rest and energy restoration.

## Circular Dependency Resolution
This system uses reflection to avoid circular dependencies between assemblies:
- **Interactables** → **Characters** (via reflection)
- **Interactables** → **UI** (via reflection)
- **Generation** → **Interactables** (via reflection)

## Components

### TentInteractable
- **Location**: `Assets/Scripts/Interactables/TentInteractable.cs`
- **Purpose**: Main interactable component that handles tent interactions
- **Features**:
  - Player proximity detection using reflection
  - Rest panel UI management using reflection
  - Time advancement using reflection
  - Energy restoration using reflection

### RestPanelUI
- **Location**: `Assets/Scripts/UI/RestPanelUI.cs`
- **Purpose**: Manages the rest interface UI
- **Features**:
  - Slider for selecting rest duration
  - Energy gain calculation display
  - Sleep and cancel buttons
  - Dynamic UI creation

## Configuration

### Energy System
- **Energy per hour**: 5 energy (configurable)
- **Maximum rest hours**: 12 hours (configurable)
- **Energy cap**: 100%

### Time Advancement
- Uses TimeManager.AdvanceTime() method via reflection
- Properly advances game clock
- Triggers time-based events

## Usage

### For Players
1. Approach tent within 3 units
2. Press E to open rest panel
3. Adjust rest duration with slider
4. View energy gain preview
5. Click "Sleep" to rest or "Cancel" to close

### For Developers
1. Tents are automatically made interactable by CampGenerator
2. No manual setup required
3. All cross-assembly access is handled via reflection
4. System is self-contained and modular

## Technical Details

### Reflection Usage
The system uses reflection to access:
- `Character` class and its `Stats` property
- `RestPanelUI` class and its methods
- `TimeManager` class and its `AdvanceTime` method

### Assembly Dependencies
- **Interactables.asmdef**: Only references Common, Shared, TextMeshPro, and Cinemachine
- **No circular dependencies**: All cross-assembly access via reflection
- **Modular design**: Easy to maintain and extend

## Testing
Use the `TentSystemTest` component in the Editor to verify:
- TimeManager access via reflection
- TentInteractable component creation
- RestPanelUI functionality
- Time advancement capabilities

## Troubleshooting
If you encounter issues:
1. Check that all required assemblies are properly referenced
2. Verify that reflection calls are finding the correct types
3. Ensure TimeManager is present in the scene
4. Check that Character components have the correct Stats property

## Integration

### CampGenerator Integration
The `CampGenerator` automatically adds `TentInteractable` components to tents when placing camps. This includes:
- Setting the layer to "Interactable"
- Adding trigger colliders for interaction
- Configuring the interactable component

### TimeManager Integration
The tent system uses `TimeManager.Instance.AdvanceTime()` to advance game time, which:
- Updates the game clock
- Triggers hour-based events
- Updates player stats (hunger, thirst, energy)

## UI Features

### Dynamic UI Creation
If no prefab is provided, the system creates a complete UI panel with:
- Dark background panel
- Title text
- Rest duration slider
- Energy gain display
- Sleep and Cancel buttons

### UI Layout
- Centered panel (400x300 pixels)
- Slider for rest duration selection
- Real-time energy gain calculation
- Responsive button layout

## Debug Features

### Context Menu Options
- `Show Tent Status`: Displays current tent state and player energy
- Available in the inspector when tent is selected

### Logging
Comprehensive debug logging for:
- Initialization
- Player interactions
- Time advancement
- Energy restoration
- UI state changes

## Example Usage

```csharp
// Manual tent setup (if not using CampGenerator)
GameObject tent = new GameObject("Tent");
TentInteractable tentInteractable = tent.AddComponent<TentInteractable>();
tentInteractable.energyPerHour = 5f;
tentInteractable.maxRestHours = 12f;
tentInteractable.interactionRadius = 3f;
```

## Dependencies
- `TimeManager`: For time advancement
- `Character`: For player stats access
- `BaseInteractable`: Base interaction functionality
- `RestPanelUI`: UI management 