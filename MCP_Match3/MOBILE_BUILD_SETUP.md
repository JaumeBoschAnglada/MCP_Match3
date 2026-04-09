# Mobile Build Setup (Android & iOS)

## Overview
Este documento detalla cómo configurar y compilar el proyecto Match3 para Android e iOS.

## Required Setup Steps

### 1. Unity Editor Settings

#### Player Settings (Edit > Project Settings > Player)

**Android:**
- Company Name: `YourCompanyName`
- Product Name: `MCP_Match3`
- Default Orientation: `Portrait`
- Target API Level: `API Level 30 (Android 11)` o superior
- Minimum API Level: `API Level 24 (Android 7)`
- Graphics APIs: `OpenGL ES 3.0`
- Scripting Backend: `IL2CPP`

**iOS:**
- Product Name: `MCP_Match3`
- Default Orientation: `Portrait`
- Supported iOS Versions: `12.0` o superior
- Graphics APIs: `Metal`
- Scripting Backend: `IL2CPP`

### 2. Build Input

**Ensure InputHandler script has:**
- ✅ Touch input support (`UnityEngine.Input.touchCount`)
- ✅ Platform detection for Android/iOS
- ✅ Fallback to mouse input for editor testing
- ✅ Proper raycast detection for pieces

**Current Implementation:**
- `Assets/Scripts/Input/InputHandler.cs` handles both touch and mouse input
- Automatic platform detection
- Swipe gesture detection (30f pixel threshold)

### 3. Required Components

**GameManager:**
- Must have reference to main Camera
- Must have reference to InputHandler
- Must implement `GetPieceAt(x, y)` method
- Must implement `SwapPieces(piece1, piece2)` method
- Must have `IsProcessing` property

**Pieces:**
- All piece prefabs must have **Colliders** (BoxCollider or SphereCollider)
- Physics layer must be set appropriately
- Main camera raycast distance: 100f (configurable in InputHandler)

**Camera:**
- Must be tagged as "MainCamera"
- Should have proper position to view entire board
- Orthographic recommended for 2D-like Match3 gameplay

### 4. Canvas/UI Considerations

If using Canvas UI:
- Ensure Canvas is set to **Screen Space - Camera** (not World Space)
- Add **GraphicRaycaster** component to Canvas
- Set layers appropriately to not interfere with game piece raycasts

### 5. Testing on Mobile

**Android Testing:**
1. Connect Android device via USB (or use Android emulator)
2. Enable USB debugging on device
3. File > Build Settings > Select Android
4. Build And Run

**iOS Testing:**
1. Connect iOS device via USB
2. File > Build Settings > Select iOS
3. Generate Xcode project
4. Open in Xcode and build on device (requires Apple Developer account)

### 6. Touch Input Debugging

During gameplay, console logs will show:
```
[InputHandler] 👆 Touch began at (320, 640)
[InputHandler] 🎮 Piece selected at 0,3
[InputHandler] 🔄 Swap: (0,3) -> (1,3)
[InputHandler] 👆 Touch ended at (350, 640)
```

**Common Issues:**
- "No piece hit" = Collider missing or raycast not reaching piece
- "GameManager is null" = InputHandler not assigned in scene
- "Swipe too short" = Gesture < 30 pixels threshold

### 7. Build Optimization

**For smaller APK/IPA:**
- Player Settings > Strip Engine Code: ✅ ON
- Player Settings > Optimization > IL2CPP Code Generation: Faster (smaller) runtime
- Remove unused assets before building
- Use Asset Compression (if Unity version supports)

### 8. Resolution & Screen Size

Recommended:
- 1080x1920 (portrait) base resolution
- Supports all modern phones (aspect ratios 16:9 to 21:9)
- Virtual resolution in game logic: 6x8 grid (width x height)

### 9. Performance Targets

- Target 60 FPS on mid-range Android phones
- Use frame rate cap if needed: `Application.targetFrameRate = 60;`
- Profile with Profiler window on device before release

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Touch not detected | Check `Input.touchCount > 0` in editor, verify Android/iOS platform |
| Pieces not selectable | Add BoxCollider to each piece, verify raycast layer |
| Swaps not working | Verify `GameManager.GetPieceAt()` returns correct piece |
| Performance lag | Check for physics-heavy operations, optimize animations |
| Screen rotation issues | Verify Player Settings > Orientation is locked to Portrait |

## Additional Notes

- The InputHandler automatically detects platform and switches between touch/mouse input
- No additional Input System (new) configuration needed - using legacy `UnityEngine.Input`
- All debugging logs prefixed with `[InputHandler]` for easy filtering
