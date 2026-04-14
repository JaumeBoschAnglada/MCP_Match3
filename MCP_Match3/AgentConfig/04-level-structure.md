# MCP Match3 — Formato de Nivel (Stage JSON)

## Visión General
Cada nivel es un archivo JSON que define TODO lo necesario para construir una partida.
Se basa en el formato documentado en Match3_Gameplay_Documentation.md sección 3.

## Ubicación y Carga
- **Directorio**: `Assets/Resources/Levels/`
- **Nombres**: `{número}.txt` o `{número}.json`
- **Carga**: `DataManager.LoadStage(int stage)` → Newtonsoft.Json

## Clase Stage (C#)

```csharp
[Serializable]
public class Stage
{
    // === TABLERO (81 celdas = 9×9) ===
    public List<DROP_DIR[]> DropDirs;       // Gravedad por celda
    public bool isUseGravity;              // Si usa gravedad personalizada
    public Pannels[] panels;               // Paneles de cada celda (forma + obstáculos)
    public ItemType[] items;               // Tipo de pieza inicial (81 elementos)
    public ColorType[] colors;             // Color de pieza inicial (81 elementos)
    public List<int> focus;                // Índices de celdas para foco visual

    // === SPAWN LINES (9 bools por eje, uno por columna/fila) ===
    public bool[] defaultSpawnLine;        // Piezas normales (X)
    public bool[] defaultSpawnLineY;       // Piezas normales (Y, solo con gravedad custom)
    public bool[] foodSpawnLine;
    public bool[] SpiralSpawnLine;
    public bool[] DonutSpawnLine;
    public bool[] TimeBombSpawnLine;
    public bool[] MysterySpawnLine;
    public bool[] ChameleonSpawnLine;
    public bool[] KeySpawnLine;
    // (+ variantes *Y para cada uno)

    // === COLORES Y PUNTUACIÓN ===
    public bool[] appearColor;             // 6 bools: RED, YELLOW, GREEN, BLUE, PURPLE, ORANGE
    public long scoreStar1, scoreStar2, scoreStar3;
    public int limit_Move;

    // === MISIONES ===
    public MissionType missionType;
    public bool isOrderNMission, isOrderSMission, isWaferMission;
    public bool isFoodMission, isBearMission, isIceCreamMission;
    public bool isJamMission, isScoreMission;
    public List<MissionData> missionInfo;  // {type, kind, count}

    // === PARÁMETROS DE SPAWN ÍTEMS ESPECIALES ===
    // Cada tipo: *_MinExist, *_MaxExist, *_Interval, *_SpawnCnt
    public int Spiral_MinExist, Spiral_MaxExist, Spiral_Interval, Spiral_SpawnCnt;
    public int Donut_MinExist, Donut_MaxExist, Donut_Interval, Donut_SpawnCnt;
    public int TimeBomb_MinExist, TimeBomb_MaxExist, TimeBomb_Interval, TimeBomb_SpawnCnt;
    public int TimeBomb_FirstCount;         // Valor inicial del contador (default: 15)
    public int Mystery_MinExist, Mystery_MaxExist, Mystery_Interval, Mystery_SpawnCnt;
    public int Chameleon_MinExist, Chameleon_MaxExist, Chameleon_Interval, Chameleon_SpawnCnt;
    public int Key_MinExist, Key_MaxExist, Key_Interval, Key_SpawnCnt;
    public int food_MaxExist, food_Interval, food_SpawnCnt;
    public int Bear_MaxExist, Bear_Interval;
    public int IceCream_Interval, IceCreamCreator_Interval;

    // Método helper
    public void CreateGravity() { /* genera DropDirs todo "U" si !isUseGravity */ }
}

[Serializable]
public class Pannels
{
    public List<PanelData> listinfo;
}

[Serializable]
public class PanelData
{
    public PanelType paneltype;
    public int defence;    // -1 = indestructible/no aplica
    public int value;
    public string addData; // JSON extra (cintas, warps)
}

[Serializable]
public class MissionData
{
    public MissionType type;
    public MissionKind kind;
    public int count;      // 0 = eliminar todos los del tablero
}
```

## DropDirs — Gravedad por celda

```json
"DropDirs": [["U"], ["U"], ["L"], ["R","L"], ...]
```

| Valor | Significado | Piezas llegan desde... |
|-------|------------|----------------------|
| `"U"` | Up (defecto) | Arriba |
| `"D"` | Down | Abajo |
| `"L"` | Left | Izquierda |
| `"R"` | Right | Derecha |
| `"List"` | Especial: marca celda como drop start |

Si `isUseGravity == false`, Stage.CreateGravity() genera todo "U".
Si una celda tiene múltiples dirs (ej: `["R","L"]`), ChangeDropDir() elige una.

## panels — Forma del tablero + obstáculos

```json
"panels": [
  { "listinfo": [{ "paneltype": "Default_Empty", "defence": -1, "value": 0 }] },
  { "listinfo": [
    { "paneltype": "Default_Full", "defence": -1, "value": 0 },
    { "paneltype": "Ice_Cage", "defence": 2, "value": 0 }
  ]},
  ...
]
```

- `Default_Empty` → celda NO existe
- `Default_Full` → celda jugable
- Múltiples paneles se apilan (base + modificadores)
- `addData` para datos extra (ConveyerBelt → `conveyertype`, Warp → índice celda conectada)

## items + colors — Piezas iniciales

```json
"items": ["None", "Normal", "Normal", "TimeBomb", ...],
"colors": ["None", "Rnd", "RED", "YELLOW", ...]
```

- `None` = sin pieza
- `Normal` = pieza básica (color en array colors)
- `Rnd` = color aleatorio de los disponibles en appearColor
- ItemTypes específicos = pieza especial precolocada

## appearColor — Colores activos

```json
"appearColor": [true, true, false, false, false, true]
```
Orden: RED, YELLOW, GREEN, BLUE, PURPLE, ORANGE
Mínimo 2 colores activos por nivel.

## missionInfo — Objetivos

```json
"missionInfo": [
  { "type": "OrderN", "kind": "Orange", "count": 50 },
  { "type": "OrderS", "kind": "Bomb", "count": 3 }
]
```

## Coordenadas
- Índice lineal: `i = X + Y * 9`
- Recorrido: fila a fila, izquierda a derecha, arriba a abajo
- (0,0) = esquina superior-izquierda

## Ejemplo Mínimo (nivel simple)

```json
{
  "isUseGravity": false,
  "panels": [ /* 81 Pannels con Default_Full o Default_Empty */ ],
  "items": [ /* 81 ItemTypes, mayoritariamente "Normal" */ ],
  "colors": [ /* 81 ColorTypes, mayoritariamente "Rnd" */ ],
  "focus": [],
  "defaultSpawnLine": [true, true, true, true, true, true, true, true, true],
  "appearColor": [true, true, true, false, false, false],
  "limit_Move": 20,
  "scoreStar1": 3000,
  "scoreStar2": 6000,
  "scoreStar3": 10000,
  "missionType": "OrderN",
  "isOrderNMission": true,
  "missionInfo": [{"type": "OrderN", "kind": "Red", "count": 30}]
}
```

## Flujo de Carga

```
DataManager.LoadStage(stageIndex)
    └── JSON → Stage (Newtonsoft.Json)

MatchManager.StartGame(stage)
    ├── StepInit()
    ├── VariableInit()
    ├── ItemManager.Init()
    ├── MissionManager.MissionSetting(stage)
    ├── StageSetting(stage)          ← Carga appearColor
    ├── CrazyLevelSetting()
    ├── BoardSetting(stage)          ← Board[81].Init(x, y, stage)
    ├── PanelSetting(stage)          ← Crea paneles, calcula DropStart/DropHead
    ├── ItemSetting(stage)           ← Coloca piezas + RegenMatches()
    ├── BoardPosionSetting()         ← Centra tablero
    └── SetMatchState(PrepareGame)   ← Muestra popup de misión
```
