# ResourceSpawner Setup Guide

## Overview
The ResourceSpawner automatically spawns coconuts and firewood every game hour using the same spawn rules as the vegetation system. By default, it spawns 5 items per hour (3 coconuts + 2 firewood).

## Features
- **Hourly Spawning**: Automatically spawns resources every game hour
- **Same Spawn Rules**: Uses identical positioning logic as the vegetation system
- **Configurable**: Customize spawn counts, distances, and offsets
- **Smart Placement**: Avoids camps, water, and other resources
- **Debug Support**: Comprehensive logging and testing tools

## Setup Instructions

### 1. Add ResourceSpawner to Scene
1. Create an empty GameObject in your scene
2. Name it "ResourceSpawner"
3. Add the `ResourceSpawner` script component

### 2. Configure Settings
The ResourceSpawner will automatically find the necessary prefabs from your ProceduralIslandGenerator, but you can also assign them manually:

#### Spawn Settings
- **Coconuts Per Hour**: Number of coconuts to spawn each hour (default: 3)
- **Firewood Per Hour**: Number of firewood to spawn each hour (default: 2)
- **Coconut Offset From Tree**: Distance from tree to place coconuts (default: 1.5)
- **Firewood Offset From Tree**: Distance from tree to place firewood (default: 2.0)
- **Max Distance From Camp**: Minimum distance from camp (default: 15)
- **Min Distance Between Resources**: Minimum distance between spawned resources (default: 2)

#### References (Optional)
- **Coconut Prefab**: Coconut prefab (auto-detected from ProceduralIslandGenerator)
- **Firewood Prefab**: Firewood prefab (auto-detected from ProceduralIslandGenerator)
- **Tree Prefabs**: Tree prefabs (auto-detected from ProceduralIslandGenerator)

### 3. Verify TimeManager
Make sure your scene has a TimeManager GameObject with the TimeManager script. The ResourceSpawner automatically subscribes to the hourly events.

## How It Works

### Spawn Process
1. **Hourly Trigger**: TimeManager triggers `onGameHourPassed` event every game hour
2. **Tree Selection**: Finds a random tree in the scene to spawn near
3. **Position Calculation**: Uses polar coordinates to place resources around the tree
4. **Validation**: Checks if position is valid (within island bounds, away from camp, etc.)
5. **Spawn**: Creates the resource with proper components and colliders

### Spawn Rules
- Resources spawn near existing trees
- Uses same island bounds as vegetation (75% of island radius)
- Avoids areas too close to camp
- Maintains minimum distance between resources
- Places resources on terrain surface

## Testing

### Manual Testing
Right-click on the ResourceSpawner component in the Inspector and use:
- **Spawn Resources Now**: Manually trigger spawning
- **Clear All Resources**: Remove all spawned resources

### Debug Logs
Enable "Enable Debug Logs" to see detailed information about:
- Spawn attempts and results
- Position validation
- Resource placement

## Customization

### Adjusting Spawn Rates
To change the total number of items spawned per hour:
1. Modify "Coconuts Per Hour" and "Firewood Per Hour" values
2. The total will be the sum of both values
3. Default total is 5 items per hour

### Changing Spawn Locations
To adjust where resources spawn relative to trees:
1. Modify "Coconut Offset From Tree" and "Firewood Offset From Tree"
2. Larger values place resources further from trees
3. Smaller values place resources closer to trees

### Spawn Area Control
To control the spawn area:
1. Adjust "Max Distance From Camp" to keep resources away from camp
2. Modify "Min Distance Between Resources" to prevent clustering

## Troubleshooting

### No Resources Spawning
1. Check if TimeManager exists in scene
2. Verify TimeManager has "Real Time Per Game Hour" set (default: 120 seconds)
3. Ensure trees exist in the scene
4. Check console for error messages

### Resources Spawning in Wrong Places
1. Verify island bounds are correct
2. Check if camp position is properly set
3. Adjust spawn distance settings

### Performance Issues
1. Reduce spawn counts if too many resources accumulate
2. Use "Clear All Resources" to remove old resources
3. Consider reducing "Enable Debug Logs" in production

## Integration with Existing Systems

The ResourceSpawner integrates seamlessly with:
- **ProceduralIslandGenerator**: Uses same prefabs and spawn rules
- **TimeManager**: Subscribes to hourly events
- **Coconut/Firewood Scripts**: Automatically adds required components
- **Inventory System**: Spawned resources work with existing collection mechanics

## Notes
- Resources are automatically tagged as "Interactable" layer
- Each resource gets proper colliders and scripts
- Resources are parented to the "Vegetation" GameObject for organization
- The system respects existing island generation and camp placement 