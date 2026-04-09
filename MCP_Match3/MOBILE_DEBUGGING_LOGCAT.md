# Debugging Touch Input on Mobile (Android)

The InputHandler now has **extreme verbose logging** to help identify why touches aren't working on mobile devices.

## Quick Debugging Steps

### Step 1: Install APK with Debugging Enabled

```
File > Build Settings
├─ Platform: Android
├─ Development Build: ☑ ON (important!)
└─ Build And Run
```

### Step 2: Connect Android Device & View Logcat

**Option A: Android Studio (Recommended)**

1. Open Android Studio
2. Connect device via USB
3. Window > Logcat (or Ctrl+6)
4. Leave it running while you play

**Option B: ADB Command Line**

```powershell
# List connected devices
adb devices

# Stream all logs filtered by "InputHandler"
adb logcat | findstr "InputHandler"

# Alternative with different filter
adb shell
logcat | grep InputHandler
```

**Option C: Unity Console (if configured)**

- Build with Development Build ON
- In-game console may show some logs (depending on configuration)

### Step 3: Play the Game & Watch Logs

**Expected flow on successful touch:**

```
[InputHandler] 👆 TOUCH BEGAN at screen pos (320.0, 640.0)
[InputHandler] >>> OnPointerDown called with screenPos=(320.0, 640.0)
[InputHandler] 📍 Raycast from camera: origin=..., direction=...
[InputHandler] ✅ Raycast HIT at distance 5.50: Piece_Blue(Clone)
[InputHandler] 🎮 ✅ PIECE SELECTED: (0,3) Blue
[InputHandler] 👆 TOUCH MOVING - delta: 15.2px, dir: (15.0, 0.0)
[InputHandler] 👆 TOUCH MOVING - delta: 35.0px, dir: (35.0, 2.0)
[InputHandler] 👆 TOUCH ENDED at (355.0, 640.0)
[InputHandler] >>> OnPointerUp called. isDragging=True, draggedPiece=Piece_Blue(Clone)
[InputHandler] 📏 Swipe info: delta=(35.0, 2.0), magnitude=35.0, threshold=15
[InputHandler] 🔄 Swipe VALID! Direction: (1, 0), Source: (0,3), Target: (1,3)
[InputHandler] ✅ Target piece found, initiating SWAP
```

---

## Troubleshooting by Log Pattern

### Problem: NO LOGS AT ALL

**Logs you should see immediately:**
```
[InputHandler] Initialized. Using Input System. Platform: Android
[InputHandler] Swipe Threshold: 15px
[InputHandler] Main Camera: Main Camera
[InputHandler] GameManager: GameManager
```

If you don't see these, the InputHandler script is not running.

**Fix:**
- [ ] Verify InputHandler is in the scene (Hierarchy)
- [ ] Check InputHandler is on active GameObject
- [ ] Check Development Build is ON
- [ ] Rebuild and reinstall APK

---

### Problem: Logs appear but "TOUCH BEGAN" never shows

Means: Touch input is not being detected by Input System

**Expected to see:**
```
[InputHandler] 👆 TOUCH BEGAN at screen pos (X, Y)
```

If missing:
- [ ] Verify Player Settings > Input > Touchscreen active
- [ ] Test with touchscreen emulator if available
- [ ] Try tapping different areas of screen (not just pieces)
- [ ] Check if device touchscreen is working (test with other app)

---

### Problem: "TOUCH BEGAN" logs appear but "Raycast HIT" shows MISS

Means: Touch is detected but no piece was hit by raycast

```
[InputHandler] 👆 TOUCH BEGAN at screen pos (320.0, 640.0)
[InputHandler] >>> OnPointerDown called with screenPos=(320.0, 640.0)
[InputHandler] 📍 Raycast from camera: origin=..., direction=...
[InputHandler] ❌ Raycast MISS - no collider hit at (320, 640)
```

**Probable causes:**
1. Piece colliders not visible to raycast
2. Camera position/angle wrong
3. Pieces too small or off-screen
4. Wrong rendering/physics layer

**Fixes to try:**
- [ ] In Editor, select piece prefab and verify BoxCollider exists (not grayed out)
- [ ] Check collider size/position covers the visual mesh
- [ ] Verify Main Camera is looking at the board
- [ ] Check camera far clip plane is >= 100
- [ ] Try tapping in center of screen to ensure pieces are on-screen

---

### Problem: Raycast HIT but "PIECE SELECTED" shows NULL

Means: Raycast hit something but it doesn't have Piece component

```
[InputHandler] ✅ Raycast HIT at distance 5.50: SomeObject
[InputHandler] ⚠️ Hit SomeObject but NO Piece component!
```

**Fixes:**
- [ ] Ensure raycast is hitting piece colliders, not UI or other objects
- [ ] Check piece prefabs have Piece script attached
- [ ] Verify colliders not on wrong layer
- [ ] Add collider layer masks if needed

---

### Problem: Piece selected but "SWIPE too SHORT" message

Means: Touch detected and piece selected, but didn't drag far enough (< 15px)

```
[InputHandler] 🎮 ✅ PIECE SELECTED: (0,3) Blue
[InputHandler] ⚠️ Swipe too SHORT: 8.5px < 15px threshold
```

**Fix:**
- [ ] Drag further (15+ pixels)
- [ ] Or reduce threshold in Inspector: InputHandler > Swipe Threshold = 10
- [ ] Note: Threshold was reduced from 30 to 15 for easier mobile testing

---

### Problem: Swipe valid but "No target piece"

Means: Swipe detected and calculated correctly, but no adjacent piece found

```
[InputHandler] 🔄 Swipe VALID! Direction: (1, 0), Source: (0,3), Target: (1,3)
[InputHandler] ❌ No target piece at (1,3)
```

**Fixes:**
- [ ] Verify board grid is properly initialized
- [ ] Check GameManager.GetPieceAt() is implemented correctly
- [ ] Try swapping in different directions
- [ ] Ensure pieces are within board bounds

---

### Problem: Everything works in Editor but NOT on Android

**Common causes:**

1. **Different screen resolution**: Raycast positions calculated wrong
   - Editor: 1920x1080 (example)
   - Android: 1440x2560 (different aspect ratio)
   - **Fix**: Add logs showing actual screen dimensions
     ```csharp
     Debug.Log($"Screen: {Screen.width}x{Screen.height}, DPI: {Screen.dpi}");
     ```

2. **Touch coordinates different**:
   - Input System may report touch position differently
   - **Fix**: Check raycast origin/direction logs

3. **Colliders not built into APK**
   - **Fix**: Rebuild APK from scratch: Clean > Build

---

## Advanced Debugging

### Add Screen Resolution Debug

Add to InputHandler.Start():

```csharp
Debug.Log($"Screen Resolution: {Screen.width}x{Screen.height}");
Debug.Log($"Screen DPI: {Screen.dpi}");
Debug.Log($"Target Frame Rate: {Application.targetFrameRate}");
Debug.Log($"Time.deltaTime: {Time.deltaTime}");
```

### Check Input System Status

```csharp
Debug.Log($"Touchscreen available: {Touchscreen.current != null}");
Debug.Log($"Mouse available: {Mouse.current != null}");
Debug.Log($"Input Devices: {string.Join(", ", InputSystem.devices)}");
```

### Temporary Tap Anywhere to Swap

To rule out raycast issues, add temporary code:

```csharp
if (touchPhase == TouchPhase.Began)
{
    Debug.Log("[DEBUG] Touch detected - attempting swap with first adjacent piece");
    // Try to swap with piece to the right
    Piece leftPiece = gameManager.GetPieceAt(0, 0);
    Piece rightPiece = gameManager.GetPieceAt(1, 0);
    if (leftPiece && rightPiece)
        gameManager.SwapPieces(leftPiece, rightPiece);
}
```

If this works, raycast is the problem. If not, SwapPieces is the problem.

---

## Collecting Logs for Support

When asking for help, provide:

1. **Full log output from first touch to swap**
2. **Device model and Android version**
3. **Build Settings screenshot** (showing Development Build ON)
4. **Player Settings screenshot** (Input section)

**Example good bug report:**
```
Device: Samsung Galaxy S21, Android 12
Build: Development Build ON, IL2CPP

Logs from first 5 seconds:
[InputHandler] Initialized...
[InputHandler] 👆 TOUCH BEGAN at screen pos (480, 960)
[InputHandler] ❌ Raycast MISS...

Expected: Raycast to hit pieces
Actual: Raycast always misses
```

---

## Quick Checklist

- [ ] Development Build is ON
- [ ] Logcat window is open
- [ ] All InputHandler init logs appear
- [ ] Tap piece → see "TOUCH BEGAN" + "PIECE SELECTED"
- [ ] Drag piece → see "TOUCH MOVING" + swipe distance increasing
- [ ] Release → see "SWIPE VALID" or "Swipe too SHORT"
- [ ] Piece swaps (if all above passed)

If any step missing, refer to troubleshooting section above.
