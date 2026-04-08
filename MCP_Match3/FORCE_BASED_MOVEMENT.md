# Force-Based Movement System (Black Hole Model)

## Overview

The Match3 system now includes a **force-based movement architecture** that prepares the game for dynamic interactions like explosions, wind, gravity fields, and environmental effects.

Instead of simple linear interpolation (Lerp), pieces are now capable of being moved by multiple forces while still being attracted to their final destination positions.

---

## Architecture Concept

### "Black Hole" Model
Destination positions act as **black holes** that attract pieces:

```
┌─────────────────────────────────────────┐
│      Piece at (2,0)                     │
│      Moving to (2,3)                    │
│                                         │
│  ↓ Attraction Force (toward target)     │
│        ↖ External Force (explosion)     │
│          ↓ Velocity accumulation        │
│                                         │
│            ║                            │
│            ║  Curved path               │
│            ↓                            │
│      (2,3) ●  [Black Hole Target]       │
│            Attraction pulls piece       │
│            to final destination         │
└─────────────────────────────────────────┘
```

### Forces Acting on Pieces

1. **Attraction Force** (Permanent)
   - Always pulls toward destination position
   - Strength: 2.0 units
   - Formula: `direction.normalized * 2.0`
   - Never decreases (piece always seeks target)

2. **External Forces** (Temporary)
   - Applied during special events (explosions, impacts)
   - Decay over time: `force *= 0.9f` each frame
   - Affects velocity but not final destination
   - Can come from any direction

3. **Velocity** (Accumulated)
   - Builds up from all forces
   - Damped each frame: `velocity *= 0.85f`
   - Creates smooth, inertia-based movement
   - Prevents jittery behavior

### Movement Equation (Per Frame)

```csharp
direction = targetPos - currentPos
distance = direction.magnitude

// All forces combine
attractionForce = direction.normalized * 2.0f;
totalForce = attractionForce + externalForces;

// Velocity accumulates with damping
velocity = (velocity + totalForce * deltaTime) * 0.85f;

// Position updates based on velocity
currentPos += velocity * deltaTime;

// Snap to target if very close
if (distance < 0.05f)
    currentPos = targetPos;
```

---

## API - New Methods

### PlayDynamicMovement (Core)
```csharp
public IEnumerator PlayDynamicMovement(
    Piece piece, 
    Vector3 startPos, 
    Vector3 targetPos, 
    float duration
)
```
- **Purpose**: Move piece using force-based attraction
- **Parameters**:
  - `piece`: Piece to animate
  - `startPos`: Initial position
  - `targetPos`: Final destination ("black hole")
  - `duration`: Expected animation length (soft constraint)
- **Behavior**:
  - Calculates attraction toward targetPos
  - Accumulates velocity with damping
  - Snaps to target when distance < 0.05
  - Sets `piece.IsAnimating = true/false`

### PlayFallAnimationDynamic
```csharp
public IEnumerator PlayFallAnimationDynamic(
    Piece piece, 
    Vector3 fromPos, 
    Vector3 toPos
)
```
- **Purpose**: Replace `PlayFallAnimation` with dynamic version
- **Effect**: Falling pieces use black hole attraction
- **Advantage**: Can be affected by explosions mid-fall

### PlaySwapAnimationDynamic
```csharp
public IEnumerator PlaySwapAnimationDynamic(
    Piece piece1, 
    Piece piece2
)
```
- **Purpose**: Replace `PlaySwapAnimation` with dynamic version
- **Effect**: Both pieces attracted to swap destinations simultaneously
- **Helper**: Uses `PlayDynamicMovementParallel` internally

### ApplySuddenForce (Framework)
```csharp
public void ApplySuddenForce(Vector3 force)
```
- **Purpose**: Entry point for external forces (explosions, etc)
- **Status**: Framework placeholder (not yet integrated)
- **Todo**: Connect to active coroutines

---

## Technical Details

### Attraction Strength
- **Current Value**: 2.0
- **Effect**: How strongly destination pulls pieces
- **Tuning**:
  - Lower (0.5-1.0): Lazy, drifty movement
  - Moderate (2.0): Current balanced feel
  - Higher (3.0+): Aggressive, snappy pull

### Damping Factor
- **Current Value**: 0.85 (85% retained per frame)
- **Effect**: How much velocity is retained
- **Tuning**:
  - Lower (0.7): High friction, stiff
  - Moderate (0.85): Current smooth feel
  - Higher (0.95): Floaty, underdamped

### Snap Distance
- **Current Value**: 0.05 units
- **Effect**: How close before snapping to exact target
- **Why**: Prevents oscillation around target

---

## Comparison: Lerp vs Dynamic

| Aspect | Lerp (Old) | Dynamic (New) |
|--------|-----------|--------------|
| Movement | Linear interpolation | Force-based attraction |
| Trajectory | Straight line | Can curve |
| External Forces | Impossible | Possible |
| Explosions | No support | Ready to support |
| Velocity | Instant | Accumulated |
| Damping | None | 0.85 per frame |
| Snap to Target | Automatic | Conditional |
| Performance | Very fast | Slightly slower (physics-like) |
| Realism | Simple, robotic | Natural, physical |

---

## Explosion Example (Future Implementation)

```csharp
// When explosion at position (1,3) occurs:
IEnumerator TriggerExplosion(Vector3 center, float radius, float force)
{
    foreach (Piece piece in piecesInRadius)
    {
        if (piece.IsAnimating)  // Only affects moving pieces
        {
            Vector3 direction = (piece.transform.position - center).normalized;
            Vector3 explosionForce = direction * force;
            
            // Apply force to current animation
            animator.ApplySuddenForce(explosionForce);
        }
    }
    
    yield return new WaitForSeconds(0.5f);
}
```

---

## Migration Path (Current Status)

### Phase 1 (Current)
✅ Framework implemented
✅ New methods exist and compile
✅ Old Lerp methods still work

### Phase 2 (Planned)
- [ ] Replace calls in GameManager:
  - `PlayFallAnimation` → `PlayFallAnimationDynamic`
  - `PlaySwapAnimation` → `PlaySwapAnimationDynamic`
- [ ] Remove old Lerp methods

### Phase 3 (Future)
- [ ] Implement explosion system
- [ ] Connect `ApplySuddenForce` to active animations
- [ ] Add other environmental forces (wind, gravity fields)
- [ ] Fine-tune attraction strength and damping

---

## Parameter Tuning Guide

### If pieces move too slow:
↑ `attractionStrength` from 2.0 to 3.0+
↑ `damping` from 0.85 to 0.90 (retains more velocity)

### If pieces overshoot target:
↑ `damping` to 0.95+ (more friction)
↓ `attractionStrength` to 1.0-1.5

### If movement feels floaty:
↓ `damping` to 0.8 (more friction)

### If movement feels snappy:
↑ `damping` to 0.92+ (less friction)

---

## Files Modified

- **PieceAnimator.cs**: Added entire new section (Lines 173+)
  - PlayDynamicMovement()
  - PlayFallAnimationDynamic()
  - PlaySwapAnimationDynamic()
  - PlayDynamicMovementParallel()
  - ApplySuddenForce()

---

## Notes

- Both old and new systems coexist (compatible)
- No breaking changes to existing code
- Compilation: 0 errors
- Ready for gradual migration
- Framework prepared for explosions, wind, gravity fields, etc.

---

## Next Steps

1. **Test**: Run game with current Lerp system (works unchanged)
2. **Integrate**: When ready, replace GameManager calls to use Dynamic methods
3. **Develop**: Implement explosion system
4. **Polish**: Fine-tune attraction/damping parameters
5. **Expand**: Add other force types (wind, gravity, etc)

---

**Status**: ✅ Ready for next phase  
**Created**: April 8, 2026
**Version**: 1.0 Framework
