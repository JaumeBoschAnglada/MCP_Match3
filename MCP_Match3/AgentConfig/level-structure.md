# Level Structure Definition

## Overview
Levels are defined in JSON format and can be loaded into the game. Each level describes the initial board layout with grid dimensions and piece placement.

## File Location
- **Directory**: `Assets/Resources/Levels/`
- **Naming Convention**: `level_{number}.json` (e.g., `level_1.json`, `level_2.json`)
- **Loading**: Levels are loaded via `LevelLoader.cs` (to be implemented)

## JSON Schema

### Root Object
```json
{
  "levelNumber": 1,
  "name": "First Level",
  "width": 6,
  "height": 6,
  "pieces": [...]
}
```

### Fields

| Field | Type | Required | Description | Constraints |
|-------|------|----------|-------------|------------|
| `levelNumber` | Integer | Yes | Unique identifier for the level | Must be > 0 |
| `name` | String | Yes | Display name shown in UI | Max 50 characters |
| `width` | Integer | Yes | Board width | Must be 6 (future: configurable) |
| `height` | Integer | Yes | Board height | Must be 6 (future: configurable) |
| `pieces` | Array | Yes | Array of piece objects | Must contain exactly (width × height) pieces |

### Piece Object
```json
{
  "x": 0,
  "y": 0,
  "type": "Red"
}
```

| Field | Type | Required | Description | Valid Values |
|-------|------|----------|-------------|--------------|
| `x` | Integer | Yes | Column position (0-indexed from left) | 0 - (width-1) |
| `y` | Integer | Yes | Row position (0-indexed from bottom) | 0 - (height-1) |
| `type` | String | Yes | Piece color/type | "Red", "Blue", "Green", "Yellow", "Empty" |

## Rules & Constraints

### Board State
- **Exact Coverage**: Must have exactly `width × height` pieces
- **No Duplicates**: Each (x, y) coordinate must appear exactly once
- **Continuous Grid**: All positions from (0,0) to (width-1, height-1) must be defined

### Piece Placement
- **No Matches at Start**: Initial layout must not contain 3+ of the same type in a row (horizontal/vertical)
- **Empty Pieces**: Use type="Empty" for cells that start empty (rare - mostly for testing)
- **Valid Types Only**: Only use: "Red" | "Blue" | "Green" | "Yellow" | "Empty"

### Coordinate System
- **Origin**: (0, 0) is **bottom-left**
- **X-axis**: Increases left-to-right
- **Y-axis**: Increases bottom-to-top
- **Example**: Top-right corner = (5, 5) for a 6×6 board

## Example: Complete Valid Level

```json
{
  "levelNumber": 1,
  "name": "Tutorial Level",
  "width": 6,
  "height": 6,
  "pieces": [
    {"x": 0, "y": 0, "type": "Red"},
    {"x": 1, "y": 0, "type": "Blue"},
    {"x": 2, "y": 0, "type": "Green"},
    {"x": 3, "y": 0, "type": "Yellow"},
    {"x": 4, "y": 0, "type": "Red"},
    {"x": 5, "y": 0, "type": "Blue"},
    
    {"x": 0, "y": 1, "type": "Green"},
    {"x": 1, "y": 1, "type": "Yellow"},
    ...
    {"x": 5, "y": 5, "type": "Green"}
  ]
}
```

## Implementation Notes for Agents

### When modifying levels:
1. **Always maintain the piece count**: width × height pieces
2. **Verify no starting matches**: Use the `FindMatches()` logic from BoardController to validate
3. **Update levelNumber incrementally**: Never reuse level numbers
4. **Coordinate system**: Remember (0,0) is bottom-left, y increases upward
5. **Type validation**: Only use string values from the valid list

### Validation Checklist
- [ ] JSON is valid (proper brace/bracket matching, no trailing commas)
- [ ] Piece count = width × height
- [ ] No duplicate (x, y) coordinates
- [ ] All pieces have valid type values
- [ ] No 3+ starting matches
- [ ] levelNumber is unique and > 0
- [ ] All x values in range [0, width-1]
- [ ] All y values in range [0, height-1]

## Future Extensions

- **Special Pieces**: Modifier field (e.g., "modifier": "bomb", "obstacle")
- **Obstacles**: New type for immovable blocks
- **Target Goals**: Goals object defining level objectives (e.g., "match 20 reds")
- **Moves Limit**: Maximum swaps allowed per level
- **Dynamic Dimensions**: Support boards > 6×6
- **Difficulty Descriptor**: String field ("Easy", "Medium", "Hard")

## Loading Implementation (LevelLoader.cs)

When implemented, LevelLoader should:
1. Read JSON from `Resources/Levels/level_{number}.json`
2. Validate structure against constraints above
3. Deserialize to LevelData class
4. Initialize BoardController with deserialized data
5. Log warnings if validation fails (e.g., starting matches detected)

---

**Last Updated**: During Phase 1 Polish
**Version**: 1.0 (Board structure only, no special pieces yet)
