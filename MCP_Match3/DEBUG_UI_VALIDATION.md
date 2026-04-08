# Debug UI - Validation Checklist

## Overview
The Debug UI system has been implemented and is ready for visual testing. It provides:
1. **Time Scale Control Button** - Toggle gameplay speed (1.0x ↔ 0.1x)

## What to See in Game View

### Bottom-Left Corner
- ✅ Yellow button labeled "⏱ Speed: 1.0x"
- When clicked, should change to "⏱ Speed: 0.1x"
- Game should slow down visibly when toggled

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

### Test 3: Piece Bounce on Landing
1. Play the game and make moves
2. When pieces land in their final position after falling, they should have a subtle bounce
3. When pieces return from an invalid swap, they should gently bounce into place
4. Bounce should feel elastic but controlled (not overly springy)

## Implementation Status

### Completed ✅
- DebugUI component created and auto-initialized
- OnGUI rendering system working
- Time.timeScale toggle functional
- Console logging for debugging

### Technical Details
- **Script**: `Assets/Scripts/UI/DebugUI.cs`
- **Initialization**: Via `GameManager.SetupDebugUI()` in `Start()`
- **Rendering**: Uses `OnGUI()` for immediate viewport rendering

## Troubleshooting

### If button doesn't appear:
1. Check console for "[DebugUI] Started. Ready for OnGUI controls." message
2. Verify DebugUI component is added to Canvas in hierarchy
3. Ensure OnGUI is being called (check console logs)

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
