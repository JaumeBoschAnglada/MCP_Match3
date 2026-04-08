# Debug UI - Validation Checklist

## Overview
The Debug UI system has been implemented and is ready for visual testing. It provides:
1. **Time Scale Control Button** - Toggle gameplay speed (1.0x ↔ 0.1x)
2. **Grid State Visualization** - Shows current piece positions in the grid

## What to See in Game View

### Bottom-Left Corner
- ✅ Yellow button labeled "⏱ Speed: 1.0x"
- When clicked, should change to "⏱ Speed: 0.1x"
- Game should slow down visibly when toggled

### Top-Right Corner
- ✅ Grid visualization showing all 36 positions
- Each piece displays as:
  - 🟢 Green pieces
  - 🔵 Blue pieces
  - 🔴 Red pieces
  - 🟡 Yellow pieces
  - ⚫ Empty slots (-30 frames: pieces still falling/animating)
- Should update continuously as pieces move

## Testing Procedure

### Test 1: Button Visibility
1. Play the game
2. Look at bottom-left corner
3. Verify yellow button is visible with text "⏱ Speed: 1.0x"

### Test 2: Time Scale Toggle
1. Click the speed button
2. Game should slow to 10% speed (0.1x)
3. Piece animations should move much slower
4. Button should show "⏱ Speed: 0.1x"
5. Click again to return to normal speed (1.0x)

### Test 3: Grid Visualization
1. Look at top-right corner
2. Verify grid shows in format:
   ```
   🟢 🔵 🟡 🔴 🔵 -
   🟡 🟡 🔴 🟢 - -
   ...
   ```
3. Verify 36 total positions visible
4. Confirm piece positions match actual gameplay

### Test 4: Grid Alignment
1. Move pieces with mouse
2. Verify grid display updates correctly
3. Empty slots show as ⚫
4. Pieces show correct colors
5. Grid always shows correct 6x6 layout

## Implementation Status

### Completed ✅
- DebugUI component created and auto-initialized
- OnGUI rendering system working
- Time.timeScale toggle functional
- Grid state calculation implemented
- Console logging for debugging

### Technical Details
- **Script**: `Assets/Scripts/UI/DebugUI.cs`
- **Initialization**: Via `GameManager.SetupDebugUI()` in `Start()`
- **Rendering**: Uses `OnGUI()` for immediate viewport rendering
- **Dependencies**: GameManager.GetPieceAt(int x, int y)

## Troubleshooting

### If button doesn't appear:
1. Check console for "[DebugUI] Started. Ready for OnGUI controls." message
2. Verify DebugUI component is added to Canvas in hierarchy
3. Ensure OnGUI is being called (check console logs)

### If grid doesn't update:
1. Verify GameManager.GetPieceAt() is returning valid pieces
2. Check that piece data is being updated when pieces move
3. Confirm piece.gameObject.activeSelf returns correct value

### If Time.timeScale doesn't change:
1. Verify button click is registering (add log to ToggleTimeScale)
2. Check Time.deltaTime in Scene View (should change)
3. Confirm no other code is overriding Time.timeScale

## Console Output Expected

On game start, you should see:
```
[GameManager] SetupDebugUI() called!
[GameManager] Canvas found!
[GameManager] Adding DebugUI component to Canvas...
[GameManager] DebugUI component added successfully!
[DebugUI] Started. Ready for OnGUI controls.
```

When button is clicked:
```
[DebugUI] Time.timeScale: 0.1x
[DebugUI] Time.timeScale: 1x
```

## Next Phase

After validating Debug UI works:
1. Proceed with Phase 2: Scoring System
2. Use Debug UI to slow down animations for UI testing
3. Verify piece movements and reactions are correct

---
**Status**: Ready for Visual Testing ✅
**Last Updated**: [Current Session]
