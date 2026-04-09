# Mobile Build Verification Checklist

Run this checklist before building for Android/iOS to ensure everything is configured correctly.

## Scene Setup Verification

- [ ] Main Camera exists in scene
- [ ] Main Camera tagged as "MainCamera"
- [ ] GameManager script assigned in scene
- [ ] InputHandler script assigned in scene
- [ ] InputHandler has reference to GameManager
- [ ] InputHandler has reference to main Camera

## Piece Prefabs Verification

For each piece prefab (Piece_Red, Piece_Blue, Piece_Special_Horizontal_*, etc.):
- [ ] Has **BoxCollider** or **SphereCollider** component
- [ ] Collider is not marked as Trigger (unless using separate UI layer)
- [ ] Piece script has `GetComponent<Piece>()` accessible
- [ ] No physics Rigidbody needed (static colliders are fine)
- [ ] Layer is set to "Default" or custom game layer (not UI)

## GameManager Methods Verification

In your GameManager class:
- [ ] `GetPieceAt(int x, int y)` method exists and returns Piece or null
- [ ] `SwapPieces(Piece piece1, Piece piece2)` method exists
- [ ] `IsProcessing` property exists (returns bool)
- [ ] Main Camera reference is assigned (public or serialized)

## InputHandler Configuration

In the scene, InputHandler should have:
- [ ] **Main Camera**: Reference to main camera in scene
- [ ] **Game Manager**: Reference to GameManager script
- [ ] **Swipe Threshold**: Set to 30 (pixels) or adjust to taste

## Platform-Specific Checks

### For Android Build

In Edit > Project Settings > Player:
- [ ] Product Name: "MCP_Match3" (or your chosen name)
- [ ] Default Orientation: Portrait
- [ ] Supported Aspect Ratios: Check Mobile (16:9, 18:9, 19:9, 19.5:9, 21:9)
- [ ] Minimum API Level: 24 or higher
- [ ] Target API Level: 30 or higher
- [ ] Graphics: OpenGL ES 3.0 or Vulkan
- [ ] Scripting Backend: IL2CPP

### For iOS Build

In Edit > Project Settings > Player:
- [ ] Product Name: "MCP_Match3"
- [ ] Default Orientation: Portrait
- [ ] iOS Minimum Version: 12.0 or higher
- [ ] Graphics: Metal (not required but recommended)
- [ ] Scripting Backend: IL2CPP

## Physics & Raycast Verification

- [ ] Main Camera has far clip plane >= 100
- [ ] Pieces are positioned within camera view (Z between near and far planes)
- [ ] Physics collision detection is enabled
- [ ] No excessive layers/masks interfering with raycasts
- [ ] Test raycast with Debug.Log output in InputHandler (verify it's hitting pieces)

## Build Settings

File > Build Settings:
- [ ] Scenes in Build: Your main scene is added and has index 0
- [ ] Platform: Android or iOS selected
- [ ] Target Device: Specific phone or Generic (for testing)
- [ ] Development Build: ☑ (ON) for first testing, can turn off for release

## Input Verification

In InputHandler script:
- [ ] Script uses `UnityEngine.Input` (not InputSystem)
- [ ] `HandleTouchInput()` method processes touch events
- [ ] `HandleMouseInput()` method (for editor testing)
- [ ] Platform detection for Android/iOS is in place

## Deploy & Test

### Pre-Build Testing in Editor

1. Set Game view to portrait resolution (9:16)
2. Trigger a 4-match scenario manually
3. Verify touch input with Mouse (simulate with click+drag)
4. Check console for `[InputHandler]` debug logs
5. Ensure no errors or exceptions

### Post-Build Testing on Device

**First Run Checklist:**
- [ ] App installs without errors
- [ ] App launches and shows game board
- [ ] Touch select works (tap a piece)
- [ ] Drag to swap works (touch-hold-drag-release)
- [ ] Matches detect correctly
- [ ] Animations play smoothly
- [ ] No crashes after 5+ match cycles
- [ ] Performance is acceptable (no frame drops below 50 FPS)

**Extended Testing:**
- [ ] Test edge cases (top/bottom, left/right swaps)
- [ ] Rotate device and verify orientation lock works
- [ ] Test on different screen sizes (if possible)
- [ ] Play for 10+ minutes without crashes
- [ ] Audio/SFX works if implemented

## Common Failed Checks & Solutions

| Failed Check | Likely Cause | Solution |
|--------------|--------------|----------|
| "No piece hit at..." | Missing Collider | Add BoxCollider to piece prefab |
| Touch not detecting | Wrong platform | Verify Build Settings platform and test on device |
| GetPieceAt returns null | Grid mismatch | Ensure GameManager properly initializes board |
| Swaps not working | Method missing | Verify GameManager.SwapPieces() exists |
| Visual glitches | Physics interference | Disable Rigidbody if pieces don't need physics |
| Low FPS on device | Performance issue | Use Profiler, optimize animations, reduce draw calls |

## Notes

- After any code changes, rebuild the project and re-test on device
- Keep a backup of a known-working build for comparison
- Monitor console output for `[InputHandler]` logs to debug input issues
- Test on multiple devices if possible before release
