# Input Testing Guide (Editor)

## How to Test Input in the Editor (PC/Mac)

The InputHandler now works on both Editor and Mobile platforms.

### Prerequisites

1. **Main Camera** must exist in scene and be tagged "MainCamera"
2. **GameManager** must be in scene
3. **InputHandler** must be assigned to a GameObject in scene with references set:
   - Main Camera: reference to Main Camera
   - Game Manager: reference to GameManager
   - Swipe Threshold: 30 (default)
4. **All piece prefabs** must have BoxCollider component (not set as Trigger)

### Testing in Play Mode

**Step 1: Start Play Mode**
- Press Play button (▶ or Ctrl+P)
- Game should initialize with Match3 board
- Check Console for initialization logs

**Step 2: Open Console Window (Ctrl+Shift+C)**
You should see startup messages:
```
[InputHandler] Initialized. Touch supported: False
```

**Step 3: Test Single Click (Select a Piece)**
1. In Scene view or Game view, click on a game piece (any colored square)
2. Check Console - should see:
   ```
   [InputHandler] 🖱️ Mouse button down at (320, 640)
   [InputHandler] 🎮 Piece selected at 0,3
   ```
3. The piece visual should show selection feedback (if implemented)

**Step 4: Test Drag-and-Swap (Drag to Adjacent Piece)**
1. Click on a piece and hold the mouse button
2. Drag to an adjacent piece (up, down, left, or right)
3. Move at least 30 pixels to trigger swap (adjustable in Inspector)
4. Release the mouse button
5. Check Console - should show:
   ```
   [InputHandler] 🖱️ Mouse button down at (320, 640)
   [InputHandler] 🖱️ Dragging... delta=35.2
   [InputHandler] 🖱️ Mouse button up at (355, 640)
   [InputHandler] 🔄 Swap: (0,3) -> (1,3)
   ```

**Step 5: Verify Match Detection**
After successful swap:
- Pieces should animate into position
- If 3+ match, they should eliminate
- New pieces should drop down
- Score/UI should update

### Console Log Reference

| Log Message | Meaning | Status |
|-------------|---------|--------|
| `👆 Touch began at` | Touch detected on mobile | ✅ Working |
| `🖱️ Mouse button down` | Mouse click detected | ✅ Working |
| `🎮 Piece selected at` | Piece raycast successful | ✅ Working |
| `🔄 Swap:` | Swap initiated | ✅ Working |
| `💨 No piece hit` | Raycast failed - missing collider | ❌ Problem |
| `❌ GameManager is null` | InputHandler not assigned | ❌ Problem |
| `⚠️ Swipe too short` | Drag distance < 30 pixels | ⏳ Normal (increase drag) |

### Troubleshooting Input Not Working

**Problem: "No piece hit" in console**

Cause: Raycast not detecting pieces
- [ ] Check each piece prefab has **BoxCollider** component
- [ ] Collider should NOT be set as Trigger
- [ ] Verify collider is on correct layer (use Default or custom)
- [ ] Check Main Camera far clip plane (should be at least 100)

Actions:
1. Select piece prefab in Project
2. In Inspector, verify BoxCollider exists (not grayed out)
3. Check collider bounds cover the visual mesh
4. Use Gizmo toggle (top-right of Scene) to visualize colliders

**Problem: "GameManager is null"**

Cause: InputHandler not properly wired
- [ ] Select InputHandler GameObject in Hierarchy
- [ ] In Inspector, check:
  - Main Camera field is NOT empty (should show camera name)
  - Game Manager field is NOT empty (should show GameManager name)
- [ ] If empty, drag-and-drop components into fields

**Problem: "Swipe too short" message**

Cause: Drag distance is less than threshold (30 pixels)
- Solution: Drag further or reduce swipeThreshold in Inspector
  - Select InputHandler in Hierarchy
  - Find "Swipe Threshold" in Inspector
  - Reduce from 30 to 10-15 for easier testing

**Problem: No logs appearing at all**

Cause: HandleMouseInput() not being called
- [ ] Verify InputHandler script is on active GameObject
- [ ] Check that InputHandler.Update() is being called
- [ ] Verify Application.platform is NOT mobile (if on Windows/Mac)
- [ ] Check that Play Mode is actually running (not paused)

Debug steps:
1. Add a simple test: pause game and check if logs are appearing
2. In InputHandler.Start(), should see initialization log
3. Every click should produce a 🖱️ log

### Advanced Testing

**Test Ray Visualization (Advanced Debug)**

Add this to InputHandler temporarily:
```csharp
private void OnDrawGizmos()
{
    if (!Application.isPlaying) return;
    
    Ray ray = mainCamera.ScreenPointToRay(UnityEngine.Input.mousePosition);
    Debug.DrawRay(ray.origin, ray.direction * 100f, Color.yellow);
}
```

Then in Scene view, you'll see yellow ray from camera through mouse cursor.

**Test Collider Bounds**

Gizmos menu > Check Colliders box to visualize all active colliders.
- Green = non-colliding
- Red = colliding with something
- Should be green for all pieces in normal state

### Performance Notes

- InputHandler Update runs every frame (60 FPS default)
- Raycast is cheap operation (~0.1ms)
- No noticeable performance impact
- Can monitor with Profiler (Window > Analysis > Profiler)

---

## Game View Notes

- Game view should be set to **16:9** (or 9:16 for portrait)
- Pieces should be clearly visible and large enough to click
- Consider adding UI button for testing if click area is too small
- Test on different resolutions using Game view dropdown

## Next Steps after Successful Editor Testing

Once input works perfectly in Editor:
1. Build for Android: File > Build Settings > Android > Build And Run
2. Connect Android device via USB
3. Install APK and test with actual touch

