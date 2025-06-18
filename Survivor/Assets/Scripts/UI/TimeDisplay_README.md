# TimeDisplay Component Setup

## Overview
The TimeDisplay component shows the current game time in hour:minute format, scaling real time to game time based on the TimeManager's `realTimePerGameHour` setting.

## Features
- Displays current game time in HH:MM format
- Shows current game day
- Supports both 24-hour and 12-hour formats
- Automatically updates as time passes
- Integrates with the existing TimeManager system

## Setup Instructions

### 1. Create UI Elements
1. In your Canvas, create a new UI Panel or use an existing one
2. Add a TextMeshPro - Text (UI) component for the time display
3. Optionally add another TextMeshPro - Text (UI) component for the day display

### 2. Add TimeDisplay Component
1. Add the `TimeDisplay` script to the UI Panel
2. Assign the time TextMeshPro component to the `Time Text` field
3. Assign the day TextMeshPro component to the `Day Text` field (optional)
4. Choose your preferred time format (24-hour or 12-hour)

### 3. Configure TimeManager
1. Make sure your TimeManager has the desired `Real Time Per Game Hour` value
2. For example:
   - Setting to 20 seconds = 20 real seconds per game hour
   - Setting to 120 seconds = 2 real minutes per game hour

## Example Configuration
- **Real Time Per Game Hour**: 20 seconds
- **Time Display**: 10:30 (shows 10 hours, 30 minutes into the game day)
- **Day Display**: Day 1

## Usage
The component automatically:
- Waits for TimeManager to be available
- Calculates elapsed game time based on real time
- Updates the display when time changes
- Provides public methods for other systems to get current time

## Public Methods
- `GetCurrentGameTime()`: Returns (hour, day)
- `GetCurrentGameTimeWithMinutes()`: Returns (hour, minute, day)
- `GetFormattedTime()`: Returns formatted time string

## Notes
- The component uses `Time.deltaTime` to track elapsed time
- Time calculation is based on the TimeManager's `RealTimePerGameHour` setting
- The display updates every frame but only refreshes the UI when time actually changes 