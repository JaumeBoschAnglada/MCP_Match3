# Jewels Palace Match 3 — Documentación del Gameplay

> Documentación técnica del sistema de juego Match 3 contenido en la carpeta `ThreematchSinMapa`.
> En distintas ramas, builds o repositorios este mismo juego también aparece bajo el nombre `JewelHunterMatch3`.
> Se excluye intencionalmente toda funcionalidad de la carpeta `1UP` y redes de analíticas.

---

## Índice

1. [Visión general](#1-visión-general)
2. [Arquitectura de clases principales](#2-arquitectura-de-clases-principales)
3. [Datos de nivel (Stage) — Formato JSON](#3-datos-de-nivel-stage--formato-json)
4. [El tablero (Board)](#4-el-tablero-board)
5. [Las piezas (Item)](#5-las-piezas-item)
6. [Sistema de colores](#6-sistema-de-colores)
7. [Paneles (Panel)](#7-paneles-panel)
8. [Sistema de input](#8-sistema-de-input)
9. [Sistema de Steps (máquina de estados del turno)](#9-sistema-de-steps-máquina-de-estados-del-turno)
10. [Detección de matches](#10-detección-de-matches)
11. [Generación de piezas especiales](#11-generación-de-piezas-especiales)
12. [Combinaciones entre piezas especiales](#12-combinaciones-entre-piezas-especiales)
13. [Sistema de gravedad y caída (Drop)](#13-sistema-de-gravedad-y-caída-drop)
14. [Sistema de misiones y condiciones de victoria/derrota](#14-sistema-de-misiones-y-condiciones-de-victoriaderrota)
15. [Bonus Time](#15-bonus-time)
16. [Sistema Crazy Level (ayuda dinámica)](#16-sistema-crazy-level-ayuda-dinámica)
17. [Sistema de Hints y Shuffling](#17-sistema-de-hints-y-shuffling)
18. [Object Pool](#18-object-pool)
19. [Sistema de efectos visuales (EffectManager)](#19-sistema-de-efectos-visuales-effectmanager)
20. [Ítems de tienda (Cash Items)](#20-ítems-de-tienda-cash-items)
21. [Resumen de prefabs del gameplay](#21-resumen-de-prefabs-del-gameplay)

---

## 1. Visión general

El juego es un **Match 3 clásico basado en turnos** (limitado por movimientos). El jugador intercambia piezas adyacentes para formar líneas o figuras de 3 o más piezas del mismo color. Al hacer match, las piezas explotan ("brust"), caen nuevas desde arriba (o desde creadores especiales) y se repite el ciclo hasta que no haya más reacciones en cadena. Entonces el turno termina y el jugador puede volver a interactuar.

**Estados principales de la partida** (`MatchState`):

| Estado | Descripción |
|---|---|
| `PrepareGame` | Configurando nivel, mostrando popup de misión |
| `Playing` | Jugando activamente |
| `Shuffling` | Recolocando piezas porque no hay movimientos posibles |
| `BonusTime` | Nivel superado, convirtiendo movimientos sobrantes en piezas especiales |
| `GameClear` | Victoria confirmada |
| `GameFail` | Derrota confirmada |

---

## 2. Arquitectura de clases principales

### 2.1. Jerarquía de la escena (`ThreematchSinMapa`)

```
ThreematchSinMapa (Escena)
├── ThreeMatch                      ← Raíz del gameplay (se activa/desactiva al entrar/salir)
│   ├── Main Camera                 ← Camera + PauseMusicAudioListener
│   ├── CommonManager               ← Contenedor de managers compartidos
│   │   ├── ItemManager             ← Factoría de ítems, control de intervalos de spawn
│   │   ├── PanelManager            ← Factoría de paneles, mapeo PanelType → Prefab
│   │   └── ObjectpPool             ← Pool de objetos reutilizables
│   └── MatchManager                ← Cerebro del gameplay (Singleton)
│       ├── EffectManger            ← Partículas y efectos visuales
│       ├── MissonManager           ← Gestión de objetivos del nivel
│       ├── AbilityManager          ← Habilidades especiales (mariposa, líneas, etc.)
│       ├── Background              ← Sprite de fondo del tablero
│       ├── ThreeMath_Field         ← Contenedor del campo de juego (Animation)
│       │   └── Board[81]           ← Celdas del tablero (generadas en runtime)
│       └── SkipEnd                 ← Botón para saltar animación final
├── UI Root (NGUI)                  ← Interfaz de usuario completa
│   ├── Camera                      ← Cámara de UI (NGUI UICamera)
│   ├── TimeManager                 ← Gestión de tiempos (energía, etc.)
│   ├── UI_Main                     ← Menú principal
│   ├── UI_WorldMap                 ← Mapa de niveles
│   ├── UI_ThreeMatch               ← UI en partida
│   │   ├── UI_Block                ← Bloqueo de input durante animaciones
│   │   ├── TopUI                   ← Barra superior (misiones, puntuación, movimientos)
│   │   ├── BottomUI                ← Barra inferior (ítems de tienda)
│   │   ├── Editor_Mode             ← Herramientas de editor (desactivado en build)
│   │   └── RewardBoxManager        ← Caja de recompensas (RewardBox)
│   ├── Root_Popup (Popup_Manager)  ← 34 popups (misión, victoria, derrota, tienda, etc.)
│   ├── TutorialManager             ← Sistema de tutoriales paso a paso
│   └── Loading                     ← Pantalla de carga
├── GameManager                     ← Singleton global (estado del juego, datos del jugador)
├── SoundManager                    ← Audio (música y efectos)
├── EventSystem                     ← Input del sistema de Unity UI
└── ... (1UP managers, ads, etc.)   ← Excluidos de esta documentación
```

### 2.2. Managers del gameplay

```
MatchManager (Singleton, MonoBehaviour)
├── Board[81]              ← Rejilla 9×9
│   ├── Item               ← La pieza sobre la celda
│   ├── Panel[]            ← Modificadores de celda (hielo, jalea, etc.)
│   └── GravityDisplayer   ← Indicador visual de dirección de gravedad
├── ItemManager            ← Factoría de ítems vía ObjectPool + control de intervalos de spawn
├── PanelManager           ← Factoría de paneles (mapeo PanelType → Prefab)
├── MissionManager         ← Gestión de objetivos del nivel
├── UI_ThreeMatch          ← Interfaz de usuario en partida (hereda de UI_Base)
├── EffectManager          ← Partículas y efectos visuales
├── AbilityManager         ← Lógica de habilidades especiales (mariposa, combinaciones)
├── Border                 ← Borde visual del tablero (mesh generado dinámicamente)
├── RewardBox              ← Caja de recompensas durante la partida
└── BaseStep (Dictionary)  ← Máquina de estados del turno
```

### 2.3. Descripción de cada manager

| Manager | Clase | Patrón | Descripción |
|---|---|---|---|
| **MatchManager** | `MatchManager` | Singleton | Cerebro del gameplay. Crea el tablero, ejecuta la máquina de estados, gestiona turnos y el ciclo completo de la partida. |
| **ItemManager** | `ItemManager` | Singleton | Factoría de piezas. Crea ítems del tipo solicitado usando ObjectPool. Controla los intervalos de spawn de ítems especiales (espiral, donut, bomba, etc.). |
| **PanelManager** | `PanelManager` | Singleton | Factoría de paneles. Mantiene una lista `List_Panel` de `PanelObj` que mapea cada `PanelType` a su prefab correspondiente. Crea paneles con `CreatePanel(type, parent)`. |
| **MissionManager** | `MissionManager` | Singleton | Gestiona los objetivos del nivel. Recibe notificaciones cuando un ítem es destruido (`MissionApply()`). Controla la generación de ítems de comida (`CreatFoodItem()`). |
| **EffectManager** | `EffectManager` | Singleton | Genera efectos visuales: explosiones de piezas, efectos de bomba, arcoíris, mariposa, paneles, etc. Cada efecto se obtiene del ObjectPool. Los colores de partículas se mapean a `ColorType`. |
| **AbilityManager** | `AbilityManager` | Singleton | Ejecuta las habilidades de las piezas especiales: vuelo de mariposa (usando DOTween), combinaciones entre especiales, efectos de Cash Items (martillo, bomba, rayo). |
| **ObjectPool** | `ObjectPool` | Singleton | Reutilización de GameObjects. Evita `Instantiate`/`Destroy` constantes. Ver sección 18. |
| **GameManager** | `GameManager` | Singleton | Estado global del juego. Gestiona datos del jugador (`m_GameData`), estado del juego (`GAMESTATE`), suscripciones, y transiciones entre escenas. No es parte del gameplay Match 3 directamente. |
| **SoundManager** | `SoundManager` | Singleton (`Singleton<T>`) | Reproducción de música de fondo y efectos de sonido (`PlayEffect(name)`). |
| **Border** | `Border` | MonoBehaviour | Genera dinámicamente una mesh 2D para el borde visual del tablero usando Marching Squares sobre las celdas activas (`Default_Full`). Configurable: grosor de línea, redondeo, escala UV. |
| **GravityDisplayer** | `GravityDisplayer` | MonoBehaviour (por Board) | Componente en cada celda que muestra visualmente la dirección de gravedad (flecha sprite). Se activa cuando el jugador toca una pieza y `isUseGravity == true`. |

**`MatchManager`** es el cerebro del gameplay. Orquesta la creación del tablero, la gestión de turnos, la máquina de estados (Steps) y el ciclo completo de la partida.

### 2.4. Dependencias externas

| Librería | Uso |
|---|---|
| **NGUI** | Toda la interfaz de usuario (UIRoot, UIPanel, UICamera, UIAnchor, UIBlock, etc.) |
| **DOTween** | Animaciones programáticas (vuelo de mariposa, efectos de combinación, transiciones) |
| **Newtonsoft.Json** | Serialización/deserialización de datos de nivel (Stage JSON) |
| **Unity Addressables** | Carga asíncrona de assets (usado en DataManager) |

### 2.5. Equivalencias de nomenclatura entre ramas/builds

En este mismo juego conviven dos familias de nombres. La rama documentada arriba usa `MatchManager`/`Board`/`Panel`/`Item`; otra rama o build equivalente usa `GameMain`/`BoardManager`/`Slot`/`BlockInterface`/`Chip`.

| Nomenclatura principal de este documento | Nomenclatura alternativa en otras ramas | Equivalencia práctica |
|---|---|---|
| `MatchManager` | `GameMain` | Orquestador principal de la partida |
| `Board` | `Slot` | Celda lógica del tablero |
| `Panel` | `BlockInterface` | Obstáculo o modificador fijo de la casilla |
| `Item` | `Chip` | Ficha móvil o elemento que vive sobre la casilla |
| `Stage` | `MapData` / `MapBoardData` | Datos serializados del nivel |
| `PanelType` | `IBlockType` | Catálogo de obstáculos y casillas especiales |
| `ItemType` | `ChipType` / `Powerup` | Catálogo de fichas, especiales y powerups |
| `MissionType` / `MissionKind` | `GoalTarget` / `CollectBlockType` | Objetivos del nivel y elementos recolectables |

La semántica es la misma: cambian la organización de clases y algunos nombres de runtime, pero no el hecho de que se trata del mismo Match 3.

---

## 3. Datos de nivel (Stage) — Formato JSON

**Clase:** `Stage` (serializable)  
**Ruta de archivos:** `Assets/Resources_moved/Data/Stage/{número}.txt`  
**Carga:** `DataManager.LoadStage(int stage)` → deserializa JSON con Newtonsoft.Json

Cada nivel es un archivo `.txt` que contiene un objeto JSON serializado de la clase `Stage`. Este archivo define **todo** lo necesario para construir y configurar una partida: la forma del tablero, la gravedad, las piezas iniciales, los paneles, las misiones, las líneas de spawn y los parámetros de los ítems especiales.

### 3.1. Estructura completa del JSON de nivel

```json
{
  // ── TABLERO ──────────────────────────────────────────────────────────────
  "DropDirs": [...],              // Array[81]: dirección de gravedad por celda
  "isUseGravity": false,          // true = gravedad personalizada (lateral/invertida)
  "panels": [...],                // Array[81]: paneles de cada celda (tipo + obstáculos)
  "items": [...],                 // Array[81]: tipo de pieza inicial en cada celda
  "colors": [...],                // Array[81]: color de la pieza inicial en cada celda
  "focus": [],                    // List<int>: índices de celdas con foco visual

  // ── SPAWN DE PIEZAS NORMALES ─────────────────────────────────────────────
  "defaultSpawnLine":    [9×bool], // Columnas X habilitadas para spawn de piezas normales
  "defaultSpawnLineY":   [9×bool], // Filas Y habilitadas (solo activo si isUseGravity=true)

  // ── SPAWN DE ÍTEMS ESPECIALES (columna X) ────────────────────────────────
  "foodSpawnLine":       [9×bool], // Columnas X donde puede aparecer comida
  "SpiralSpawnLine":     [9×bool], // Columnas X donde puede aparecer una espiral
  "DonutSpawnLine":      [9×bool], // Columnas X donde puede aparecer un donut
  "TimeBombSpawnLine":   [9×bool], // Columnas X donde puede aparecer una bomba de tiempo
  "MysterySpawnLine":    [9×bool], // Columnas X donde puede aparecer una pieza misteriosa
  "ChameleonSpawnLine":  [9×bool], // Columnas X donde puede aparecer un camaleón
  "KeySpawnLine":        [9×bool], // Columnas X donde puede aparecer una llave

  // ── SPAWN DE ÍTEMS ESPECIALES (fila Y, solo si isUseGravity=true) ────────
  "foodSpawnLineY":      [9×bool],
  "SpiralSpawnLineY":    [9×bool],
  "DonutSpawnLineY":     [9×bool],
  "TimeBombSpawnLineY":  [9×bool],
  "MysterySpawnLineY":   [9×bool],
  "ChameleonSpawnLineY": [9×bool],
  "KeySpawnLineY":       [9×bool],

  // ── COLORES Y PUNTUACIÓN ─────────────────────────────────────────────────
  "appearColor": [6×bool],        // Qué colores (RED/YEL/GRN/BLU/PRP/ORG) están activos
  "scoreStar1": 5000,             // Puntuación mínima para 1 estrella
  "scoreStar2": 10000,            // Puntuación mínima para 2 estrellas
  "scoreStar3": 18000,            // Puntuación mínima para 3 estrellas
  "limit_Move": 30,               // Movimientos disponibles en el nivel

  // ── MISIONES ─────────────────────────────────────────────────────────────
  "missionType": "OrderN",        // Tipo de misión principal (MissionType enum)
  "isOrderNMission":   false,     // Recoger piezas normales por color
  "isOrderSMission":   false,     // Recoger piezas especiales
  "isWaferMission":    false,     // Destruir suelos de oblea
  "isFoodMission":     false,     // Llevar comida al destino
  "isBearMission":     false,     // Rescatar osos de gelatina
  "isIceCreamMission": false,     // Misión de helado
  "isJamMission":      false,     // Expandir/destruir jalea
  "isScoreMission":    false,     // Alcanzar puntuación objetivo
  "missionInfo": [...],           // List<MissionInfo>: objetivos concretos

  // ── SPAWN: COMIDA ─────────────────────────────────────────────────────────
  "food_MaxExist":  0,            // Máximo de unidades de comida simultáneas en tablero
  "food_Interval":  0,            // Cada cuántos movimientos se intenta generar comida
  "food_SpawnCnt":  0,            // Máximo de spawns de comida por ciclo de intervalo

  // ── SPAWN: ESPIRAL ────────────────────────────────────────────────────────
  "Spiral_MinExist": 0,           // Mínimo de espirales antes de dejar de generar
  "Spiral_MaxExist": 0,           // Máximo simultáneo de espirales en tablero
  "Spiral_Interval": 0,           // Cada cuántos movimientos se intenta generar una espiral
  "Spiral_SpawnCnt": 0,           // Máximo de spawns de espiral por ciclo de intervalo

  // ── SPAWN: DONUT ─────────────────────────────────────────────────────────
  "Donut_MinExist": 0,
  "Donut_MaxExist": 0,
  "Donut_Interval": 0,
  "Donut_SpawnCnt": 0,

  // ── SPAWN: BOMBA DE TIEMPO ────────────────────────────────────────────────
  "TimeBomb_MinExist":   0,
  "TimeBomb_MaxExist":   0,
  "TimeBomb_Interval":   0,
  "TimeBomb_SpawnCnt":   0,
  "TimeBomb_FirstCount": 15,      // Contador inicial de cada bomba de tiempo (default: 15)

  // ── SPAWN: OSO DE GELATINA ────────────────────────────────────────────────
  "Bear_MaxExist": 0,             // Máximo de osos simultáneos (sin Min ni SpawnCnt)
  "Bear_Interval": 0,             // Cada cuántos movimientos se genera un oso

  // ── SPAWN: HELADO ─────────────────────────────────────────────────────────
  "IceCream_Interval":        1,  // Cada cuántos turnos se expande el helado
  "IceCreamCreator_Interval": 1,  // Cada cuántos turnos el creador genera helado nuevo

  // ── SPAWN: MISTERIO ───────────────────────────────────────────────────────
  "Mystery_SettingType": "Mystery_Basic", // Dificultad de la pieza misteriosa
  "Mystery_MinExist": 0,
  "Mystery_MaxExist": 0,
  "Mystery_Interval": 0,
  "Mystery_SpawnCnt": 0,

  // ── SPAWN: CAMALEÓN ───────────────────────────────────────────────────────
  "Chameleon_MinExist": 0,
  "Chameleon_MaxExist": 0,
  "Chameleon_Interval": 0,
  "Chameleon_SpawnCnt": 0,

  // ── SPAWN: LLAVE ──────────────────────────────────────────────────────────
  "Key_MinExist": 0,
  "Key_MaxExist": 0,
  "Key_Interval": 0,
  "Key_SpawnCnt": 0,

  // ── ÁRBOL DE JOYAS ────────────────────────────────────────────────────────
  "S_Tree_SettingType":  "Basic", // Dificultad del árbol de joyas
  "JewelTreeItem":       [4×ItemType],  // Ítems de los 4 niveles del árbol
  "JewelTreeItemColor":  [4×ColorType]  // Color de esos ítems
}
```

### 3.2. `DropDirs` — Gravedad por celda

```json
"DropDirs": [
    ["U"],           // Celda 0: cae desde arriba (estándar)
    ["L"],           // Celda 1: cae desde la izquierda
    ["R", "L"],      // Celda 4: puede recibir de derecha O izquierda
    ...
]
```

**Tipo:** `List<DROP_DIR[]>` — Array de 81 elementos, uno por celda (recorrido fila a fila, de izquierda a derecha, de arriba a abajo).

Cada celda tiene un array de una o más direcciones de caída:

| Valor | Significado | Las piezas llegan desde... |
|---|---|---|
| `"U"` | Up (por defecto) | Arriba de la celda |
| `"D"` | Down | Debajo de la celda |
| `"L"` | Left | La izquierda de la celda |
| `"R"` | Right | La derecha de la celda |

**Cuando una celda tiene múltiples direcciones** (ej: `["R", "L"]`), la celda puede recibir piezas desde cualquiera de esas direcciones. En `Board.Init()` se cargan todas en `PossibleDrop_Dirs`. Cuando la celda queda vacía, `ChangeDropDir()` elige aleatoriamente una de las disponibles y busca la que tenga pieza.

**Ejemplo del nivel 102 (gravedad lateral):**
```
Columnas 0-3: ["L"]  → las piezas entran desde la izquierda
Columna 4:    ["R","L"] → bifurcación, puede recibir de ambos lados
Columnas 5-8: ["R"]  → las piezas entran desde la derecha
```

**`isUseGravity`:** Si es `true`, el JSON contiene un `DropDirs` explícito. Si es `false`, `Stage.CreateGravity()` genera un array con todas las celdas en `"U"` (gravedad estándar hacia abajo).

**Valor especial `DROP_DIR.List`:** Si una celda contiene `"List"` en su `DropDirs`, se elimina esa entrada y se marca `isListDrop = true` en el `Board`. Esto indica que esa celda es un **punto de inicio de caída** (drop start) y se añade a `m_ListDropStart` directamente.

### 3.3. `panels` — Forma del tablero y obstáculos

```json
"panels": [
    {
        "listinfo": [
            { "paneltype": "Default_Empty", "defence": -1, "value": 0 }
        ]
    },
    {
        "listinfo": [
            { "paneltype": "Default_Full", "defence": -1, "value": 0 },
            { "paneltype": "Ice_Cage", "defence": 2, "value": 0 }
        ]
    },
    ...
]
```

**Tipo:** `Pannels[81]` — cada elemento contiene un `List<PanelData> listinfo`.

Array de 81 elementos, uno por celda. Cada celda tiene una lista `listinfo` con uno o más paneles apilados. El primer panel suele ser el **panel base** (`Default_Full` o `Default_Empty`), y los siguientes son **modificadores**.

#### PanelData — Estructura de cada panel

| Campo | Tipo | Descripción |
|---|---|---|
| `paneltype` | `PanelType` (string) | Tipo del panel (ver sección 7) |
| `defence` | `int` | Número de golpes para destruirlo. `-1` = indestructible/no aplica |
| `value` | `int` | Valor adicional configurable por tipo de panel |
| `addData` | `string` | Datos extra (usado por cintas transportadoras, warps, etc.) |

#### Configuración del tablero con paneles

- **`Default_Empty`**: La celda **no existe** en el tablero visible. Sin pieza, sin interacción.
- **`Default_Full`**: La celda **es jugable**. Puede contener piezas.
- Si una celda tiene `Default_Full` + `Ice_Cage` con `defence: 2`, es una celda activa con una jaula de hielo de 2 capas encima.

#### `addData` — Datos adicionales por tipo de panel

El campo `addData` es un string JSON serializado que contiene configuración extra. Se usa principalmente en:

- **`ConveyerBelt`**: Contiene un objeto `ConveyerData` serializado con `conveyertype` (int) que codifica tipo, dirección de entrada y salida.
- **`Warp_In` / `Warp_Out`**: Contiene el índice de la celda conectada.

### 3.4. `items` — Piezas iniciales

```json
"items": [
    "None", "None", "None", "None", "Normal", "None", "None", "None", "None",
    "None", "None", "Normal", "Normal", "Normal", "Normal", "Normal", "None", "None",
    ...
    "TimeBomb", "Chameleon",
    ...
]
```

**Tipo:** `ItemType[81]` — Array de 81 elementos.

Define qué tipo de pieza se coloca inicialmente en cada celda al cargar el nivel:
- `"None"`: No se coloca pieza (la celda es vacía o es un `Default_Empty`).
- `"Normal"`: Pieza básica de color (el color se define en el array `colors`).
- `"TimeBomb"`, `"Chameleon"`, `"Spiral"`, etc.: Pieza especial precolocada.

Se usa en `MatchManager.GenerateNewItems()`:
```csharp
board.GenItem(stage.items[num], stage.colors[num], 0, false, false, false);
```

### 3.5. `colors` — Colores iniciales

```json
"colors": [
    "None", "None", "None", "None", "Rnd", "None", "None", "None", "None",
    "None", "None", "Rnd", "Rnd", "Rnd", "Rnd", "Rnd", "None", "None",
    ...
    "RED", "YELLOW", "BLUE",
    ...
]
```

**Tipo:** `ColorType[81]` — Array de 81 elementos.

Define el color de cada pieza inicial:
- `"None"`: No tiene color (celda vacía o ítem sin color como `Spiral`).
- `"Rnd"`: Color aleatorio elegido de entre los `appearColor` del nivel.
- `"RED"`, `"YELLOW"`, etc.: Color fijo preconfigurado por el diseñador.

### 3.6. `focus` — Foco visual inicial

```json
"focus": [40, 41, 42]
```

**Tipo:** `List<int>` — Lista de **índices lineales** (0-80) de celdas.

Si no está vacío, al iniciar la partida se resaltan visualmente las piezas en esas celdas (efecto de "foco"). Se usa para guiar la atención del jugador hacia una zona del tablero o para tutoriales.

En `MatchManager.FocusShow()`:
```csharp
for (int i = 0; i < m_CSD.focus.Count; i++)
    List_FocusItem.Add(m_ListBoard[m_CSD.focus[i]].m_Item);
```

### 3.7. SpawnLines — Líneas de generación de piezas

Cada tipo de ítem que puede aparecer dinámicamente tiene **dos arrays booleanos** de 9 elementos que controlan desde qué columna (X) o fila (Y) puede generarse:

| Array X | Array Y | Ítem controlado |
|---|---|---|
| `defaultSpawnLine[9]` | `defaultSpawnLineY[9]` | Piezas normales (y todos los ítems especiales vía `TopSpawnItem`) |
| `foodSpawnLine[9]` | `foodSpawnLineY[9]` | Comida de misión |
| `SpiralSpawnLine[9]` | `SpiralSpawnLineY[9]` | Espirales |
| `DonutSpawnLine[9]` | `DonutSpawnLineY[9]` | Donuts |
| `TimeBombSpawnLine[9]` | `TimeBombSpawnLineY[9]` | Bombas de tiempo |
| `MysterySpawnLine[9]` | `MysterySpawnLineY[9]` | Piezas misteriosas |
| `ChameleonSpawnLine[9]` | `ChameleonSpawnLineY[9]` | Camaleones |
| `KeySpawnLine[9]` | `KeySpawnLineY[9]` | Llaves |

Cada posición del array corresponde a la columna (X) o fila (Y) con el mismo índice (0–8). `true` = esa línea puede generar ese tipo de ítem; `false` = no puede.

**Ejemplo JSON:**
```json
"defaultSpawnLine":  [true, true, true, true, true, true, true, true, true],
"TimeBombSpawnLine": [false, false, false, true, true, true, false, false, false],
"TimeBombSpawnLineY": [true, true, true, true, true, true, true, true, true]
```
En este ejemplo las bombas de tiempo solo pueden entrar por las columnas 3, 4 y 5.

---

#### Cómo funcionan en el código

**`defaultSpawnLine` / `defaultSpawnLineY`** son la puerta de entrada de TODO el spawn superior. En `Board.TopSpawnItem()` es la **primera comprobación**; si falla, no se genera nada en esa celda, ni piezas normales ni especiales:

```csharp
// Board.cs — TopSpawnItem()
if (!m_CSD.defaultSpawnLine[this.X] ||
    (m_CSD.isUseGravity && !m_CSD.defaultSpawnLineY[this.Y]))
    return false;  // ← No spawn en esta celda, sin excepción
```

Solo si `defaultSpawnLine[X]` es `true` (y `defaultSpawnLineY[Y]` también cuando `isUseGravity` es `true`), se continúa con el orden de prioridad de spawn:

```
1. Comida de misión   (MissionManager.CreatFoodItem)
2. Espiral            (ItemManager.IsCreateSpiral)
3. Donut              (ItemManager.IsCreateDonut)
4. Bomba de tiempo    (ItemManager.IsCreateTimeBomb)
5. Misterio           (ItemManager.IsCreateMystery)
6. Camaleón           (ItemManager.IsCreateChameleon)
7. Llave              (ItemManager.IsCreateKey)
8. Normal             (fallback: siempre)
```

**Los arrays `*SpawnLine[X]` de cada ítem especial** añaden un segundo filtro por columna **dentro** de cada `IsCreate*()`. La tabla a continuación indica en qué escenarios se aplica ese filtro:

| Ítem | Se filtra por columna X desde `TopSpawnItem` | Se ignora el filtro X en creadores especiales |
|---|:-:|:-:|
| Comida (`foodSpawnLine`) | ✅ (`istop=true`) | ✅ (pasa `istop=false`) |
| Espiral (`SpiralSpawnLine`) | ✅ (`istop=true`) | ✅ (pasa `istop=false`) |
| Donut (`DonutSpawnLine`) | ✅ (sin `istop`, siempre filtra) | ❌ (siempre filtra, incluso desde creadores) |
| Bomba de tiempo (`TimeBombSpawnLine`) | ✅ (`istop=true`) | ✅ (pasa `istop=false`) |
| Misterio (`MysterySpawnLine`) | ✅ (sin `istop`, siempre filtra) | ❌ (siempre filtra, incluso desde creadores) |
| Camaleón (`ChameleonSpawnLine`) | ✅ (sin `istop`, siempre filtra) | ❌ (siempre filtra, incluso desde creadores) |
| Llave (`KeySpawnLine`) | ✅ (`istop=true`) | ✅ (pasa `istop=false`) |

> **Nota `istop`:** Los ítems con `istop` tienen la firma `IsCreate*(Board bd, bool istop = true)`. Cuando el spawn lo inicia `TopSpawnItem` (borde superior del tablero), `istop=true` y el filtro de columna se aplica. Cuando lo inicia un panel creador especial (`Creator_Sprial`, `Creator_TimeBomb`, `Creator_Key`…), se pasa `istop=false` y el filtro X se omite, porque el creador ya define físicamente la columna.

---

#### Arrays `*SpawnLineY` — Solo activos con gravedad personalizada

Los arrays Y (fila) **no se usan en ningún `IsCreate*()` de ítems especiales**. Únicamente `defaultSpawnLineY` tiene comprobación activa, y solo cuando `isUseGravity == true`:

```csharp
// Solo defaultSpawnLine tiene comprobación Y
if (!m_CSD.defaultSpawnLine[this.X] ||
    (m_CSD.isUseGravity && !m_CSD.defaultSpawnLineY[this.Y]))
    return false;
```

Los demás `*SpawnLineY` están declarados en `Stage` y serializados en el JSON, pero **el engine runtime solo consulta el array X**. Los arrays Y se reservan para uso futuro del editor o extensibilidad.

**Valor por defecto en `Stage()` (constructor):**
- `defaultSpawnLine[9]` → todos `false` (sin inicializar; el editor los pone a `true` en `NewStage()`)
- `defaultSpawnLineY[9]` → todos `true` (inicializados explícitamente en el constructor)
- Resto de `*SpawnLine[9]` → todos `false`
- Resto de `*SpawnLineY[9]` → todos `true`

**Implicación práctica:** si `isUseGravity == false` (gravedad estándar, la mayoría de niveles), los arrays `*SpawnLineY` están en el JSON pero **no tienen efecto en gameplay**. Solo `defaultSpawnLine[X]` importa.

### 3.8. `appearColor` — Colores disponibles en el nivel

```json
"appearColor": [true, true, false, false, false, true]
```

**Tipo:** `bool[6]` — Un booleano por color, en orden: RED, YELLOW, GREEN, BLUE, PURPLE, ORANGE.

Define qué colores están activos en este nivel. En el ejemplo, solo aparecen Rojo, Amarillo y Naranja (3 colores). Menos colores = nivel más fácil (más probabilidad de matches).

Se carga en `MatchManager.StageSetting()`:
```csharp
for (int i = 0; i < stage.appearColor.Length; i++)
    if (stage.appearColor[i])
        m_AppearColor.Add(i + ColorType.RED);
```

### 3.9. Configuración de puntuación y movimientos

| Campo | Tipo | Descripción |
|---|---|---|
| `scoreStar1` | `long` | Puntuación mínima para 1 estrella |
| `scoreStar2` | `long` | Puntuación mínima para 2 estrellas |
| `scoreStar3` | `long` | Puntuación mínima para 3 estrellas |
| `limit_Move` | `int` | Número de movimientos disponibles |

### 3.10. Configuración de misiones

| Campo | Tipo | Descripción |
|---|---|---|
| `missionType` | `MissionType` | Tipo principal de misión |
| `isOrderNMission` | `bool` | Misión de recoger piezas normales (por color) |
| `isOrderSMission` | `bool` | Misión de recoger piezas especiales (bombas, líneas, etc.) |
| `isWaferMission` | `bool` | Misión de destruir suelos de oblea |
| `isFoodMission` | `bool` | Misión de llevar comida a destino |
| `isBearMission` | `bool` | Misión de rescatar osos de gelatina |
| `isIceCreamMission` | `bool` | Misión relacionada con helado |
| `isJamMission` | `bool` | Misión de expandir jalea |
| `isScoreMission` | `bool` | Misión de alcanzar puntuación |

**`MissionType`:**
```csharp
enum MissionType { OrderN, OrderS, Wafer, Food, Bear, IceCream, Jam, Stele, Score }
```

#### `missionInfo` — Lista de objetivos concretos

```json
"missionInfo": [
    { "type": "OrderN", "kind": "Orange", "count": 1000 },
    { "type": "OrderS", "kind": "Bomb", "count": 5 }
]
```

Cada objetivo tiene:

| Campo | Tipo | Descripción |
|---|---|---|
| `type` | `MissionType` | Categoría del objetivo |
| `kind` | `MissionKind` | Qué hay que recoger exactamente |
| `count` | `int` | Cantidad necesaria (0 = eliminar todos los que hay en el tablero) |

**`MissionKind` — Valores posibles:**
```csharp
enum MissionKind {
    None,
    Red, Yellow, Green, Blue, Purple, Orange,     // Colores normales
    Donut, Spiral, Bread, LollyCage, IceCage,     // Obstáculos
    Cracker, Bottle, S_Tree,
    Line, Line_Line, Cross, Cross_Line, Cross_Cross, // Piezas especiales
    Bomb, Bomb_Line, Bomb_Cross, Bomb_Bomb,
    Rainbow, Rainbow_Line, Rainbow_Cross, Rainbow_Bomb, Rainbow_Rainbow,
    TimeBomb, Wafer,
    StrawberryCake, ChocolatePiece, MintCake, Parfait, WhiteCake, Hamburger, // Comidas
    Bear, IceCream, Jam, Stele, Score
}
```

### 3.11. Parámetros de spawn de ítems especiales

Cada tipo de ítem que puede aparecer dinámicamente tiene hasta 4 parámetros que controlan su frecuencia. La lógica es idéntica para todos: en `IsCreate*()` se comprueba primero si el número actual en tablero supera el máximo (`*_MaxExist`), luego si el intervalo de movimientos se ha cumplido (`*_Interval`), y finalmente si el spawn por ciclo no ha superado el límite (`*_SpawnCnt`).

| Parámetro | Tipo | Descripción |
|---|---|---|
| `*_MinExist` | `int` | Si el número actual en tablero **es mayor** que este valor, no se genera más. (Umbral mínimo de población.) |
| `*_MaxExist` | `int` | Máximo absoluto simultáneo en tablero. Si se alcanza, no se genera nada. |
| `*_Interval` | `int` | El contador interno se incrementa cada movimiento; solo se genera si `contador >= Interval`. |
| `*_SpawnCnt` | `int` | Máximo de spawns permitidos por ciclo de intervalo. Al generar uno se incrementa el contador; cuando supera este límite + `MinExist`, se bloquea. |

**Ítems con los 4 parámetros (`Min`, `Max`, `Interval`, `SpawnCnt`):**

| Prefijo JSON | Ítem |
|---|---|
| `Spiral_*` | Espirales |
| `Donut_*` | Donuts |
| `TimeBomb_*` | Bombas de tiempo |
| `Mystery_*` | Piezas misteriosas |
| `Chameleon_*` | Camaleones |
| `Key_*` | Llaves |

**Ítems con parámetros reducidos:**

| Prefijo JSON | Ítem | Parámetros disponibles |
|---|---|---|
| `food_*` | Comida | `food_MaxExist`, `food_Interval`, `food_SpawnCnt` (sin `Min`) |
| `Bear_*` | Osos de gelatina | `Bear_MaxExist`, `Bear_Interval` (sin `Min` ni `SpawnCnt`) |

**Parámetros adicionales de ítems especiales:**

| Campo | Tipo | Descripción |
|---|---|---|
| `TimeBomb_FirstCount` | `int` | Valor inicial del contador de cada bomba de tiempo al generarse (default en constructor: `15`) |
| `IceCream_Interval` | `int` | Cada cuántos turnos se expande el bloque de helado existente (default: `1`) |
| `IceCreamCreator_Interval` | `int` | Cada cuántos turnos el panel `IceCream_Creator` genera un nuevo bloque de helado (default: `1`) |
| `Mystery_SettingType` | `MysterySettingType` | Preset de dificultad/comportamiento de la pieza misteriosa |
| `S_Tree_SettingType` | `S_TreeSettingType` | Preset del árbol de joyas |
| `JewelTreeItem[4]` | `ItemType[4]` | Qué ítem sale de cada uno de los 4 niveles del árbol (default: todos `Line_X`) |
| `JewelTreeItemColor[4]` | `ColorType[4]` | Color de ese ítem (default: todos `Rnd`) |

**`MysterySettingType`:**
```csharp
enum MysterySettingType {
    Mystery_Basic, Mystery_Easy, Mystery_Normal, Mystery_Hard,
    Mystery_Candy, Mystery_Special, Mystery_Icecream,
    Mystery_IceCreamCreator, Mystery_Bear, Mystery_Last_Test
}
```

**`S_TreeSettingType`:**
```csharp
enum S_TreeSettingType { Basic, Easy, Normal, Hard, Last_Test }
```

### 3.12. Ejemplo completo: Nivel 1000

Reconstrucción visual del tablero a partir del JSON (`.` = Empty, `■` = Full):

```
Fila 0:  .  .  .  .  ■  .  .  .  .
Fila 1:  .  .  ■  ■  ■  ■  ■  .  .
Fila 2:  .  ■  ■  ■  ■  ■  ■  ■  .
Fila 3:  ■  ■  ■  ■  ■  ■  ■  ■  ■
Fila 4:  ■  ■  ■  ■  ■  ■  ■  ■  ■
Fila 5:  ■  ■  ■  ■  ■  ■  ■  ■  ■
Fila 6:  .  ■  ■  ■  ■  ■  ■  ■  .
Fila 7:  .  .  ■  ■  ■  ■  ■  .  .
Fila 8:  .  .  .  .  .  .  ■  .  .
```

- **Gravedad:** Todas las celdas en `"U"` (caída estándar hacia abajo)
- **Piezas iniciales:** Todas `Normal` con color `Rnd` (aleatorio)
- **Colores activos:** RED ✅, YELLOW ✅, GREEN ❌, BLUE ❌, PURPLE ❌, ORANGE ✅ → 3 colores
- **Movimientos:** 30
- **Misión:** Recoger 1000 piezas naranjas (`OrderN` / `Orange`)
- **Estrellas:** 5000 / 10000 / 18000

### 3.13. Ejemplo complejo: Nivel 100

- **Piezas iniciales:** Incluye `TimeBomb` y `Chameleon` precolocados con colores fijos.
- **Paneles:** Muchas celdas con `Default_Full` + `Ice_Cage` (defence 1 o 2).
- **SpawnLines:** Solo `ChameleonSpawnLine` activo (los camaleones se regeneran); el resto de spawns desactivados.
- **Colores activos:** RED, YELLOW, GREEN, BLUE → 4 colores.
- **Misión:** `OrderS` / `TimeBomb` con count 0 → destruir TODAS las bombas de tiempo del tablero.
- **`TimeBomb_FirstCount`:** 30 → cada bomba empieza con 30 turnos de vida.

### 3.14. Ejemplo con gravedad lateral: Nivel 102

```json
"DropDirs": [
    ["L"], ["L"], ["L"], ["L"], ["R","L"], ["R"], ["R"], ["R"], ["R"],
    ["L"], ["L"], ["L"], ["L"], ["R","L"], ["R"], ["R"], ["R"], ["R"],
    ...
]
```

Las piezas caen hacia los laterales en lugar de hacia abajo:
- Columnas 0-3: gravedad hacia la izquierda (las piezas entran por la izquierda).
- Columna 4: punto de bifurcación, puede recibir de ambos lados.
- Columnas 5-8: gravedad hacia la derecha (las piezas entran por la derecha).

### 3.15. Flujo de carga de un nivel

```
DataManager.LoadStage(stageIndex)
    └── JSON → Stage (Newtonsoft.Json)

MatchManager.StartGame(stage)
    ├── StepInit()                   ← Inicializa la máquina de Steps
    ├── VariableInit()               ← Resetea combo, score, hints
    ├── ItemManager.Init()           ← Resetea intervalos de spawn
    ├── UI_ThreeMatch.UI_Init()      ← Configura UI
    ├── MissionManager.MissionSetting(stage)  ← Carga misiones
    ├── StageSetting(stage)          ← Carga appearColor
    ├── CrazyLevelSetting()          ← Ajusta dificultad según fallos
    ├── BoardSetting(stage)          ← Board[81].Init(x, y, stage)
    │   └── Cada Board carga DropDirs, posición, reset de flags
    ├── PanelSetting(stage)          ← Crea paneles según listinfo
    │   ├── GenPanel / GenSpecialPanel / DefaultPanel
    │   └── Calcula m_ListDropStart y m_ListDropHead
    ├── ItemSetting(stage)           ← Coloca piezas iniciales
    │   ├── GenerateNewItems(stage)  ← items[] + colors[]
    │   └── RegenMatches()           ← Elimina matches iniciales
    ├── BoardPosionSetting()         ← Centra y escala el tablero
    └── SetMatchState(PrepareGame)   ← Muestra popup de misión
```

---

## 4. El tablero (Board)

**Clase:** `Board : MonoBehaviour`

El tablero es una rejilla fija de **9×9 = 81 celdas**. Cada celda es un `Board` con coordenadas `(X, Y)` y un índice lineal `X + Y * 9`.

### 4.1. Navegación entre celdas

Cada `Board` tiene propiedades para acceder a sus vecinos:

| Propiedad | Dirección |
|---|---|
| `Top` | Arriba (Y-1) |
| `Bottom` | Abajo (Y+1) |
| `Left` | Izquierda (X-1) |
| `Right` | Derecha (X+1) |
| `TopLeft`, `TopRight`, `BottomLeft`, `BottomRight` | Diagonales |

Se puede acceder por **indexador** usando `SQR_DIR` (8 direcciones) o `DROP_DIR` (4 direcciones de caída).

### 4.2. Direcciones de caída

```csharp
public enum DROP_DIR { U, D, L, R, List }
```

Cada celda tiene una lista `PossibleDrop_Dirs` que define por dónde puede recibir piezas al caer. La dirección activa se selecciona con `SeleteDropIndex`. Esto permite tableros con gravedad no estándar (piezas cayendo hacia los lados, hacia arriba, etc.).

### 4.3. Direcciones de adyacencia

```csharp
public enum SQR_DIR { NONE, TOP, BOTTOM, LEFT, RIGHT, TOPRIGHT, TOPLEFT, BOTTOMLEFT, BOTTOMRIGHT }
```

Se usan las 8 direcciones para la detección de matches (incluyendo matches en cuadrado 2×2).

### 4.4. Estado de la celda

Cada `Board` mantiene flags clave:

| Campo | Función |
|---|---|
| `m_Item` | Referencia al `Item` actual sobre la celda |
| `m_ListPanel` | Lista de paneles activos (modificadores de celda) |
| `m_DropAnim` | Si hay una animación de caída en curso |
| `m_ItemBrusting` | Si el ítem se está destruyendo |
| `m_MatchingCheck` | Flag que indica que esta celda debe verificarse para matches |
| `m_isMatchBrust` | Marcada para explosión por match |
| `m_NextItemType` | Tipo de pieza especial que se generará tras la explosión |
| `m_IsWarpOutBoard` / `m_IsWarpInBoard` | Portales (teletransporte de piezas) |
| `IsPanelFixed` | Si la celda tiene un panel fijo (bloquea caída) |
| `IsPanelCage` | Si la celda tiene una jaula (bloquea interacción) |
| `m_IsJamBoard` | Si la celda tiene jalea (se expande al hacer match adyacente) |

### 4.5. Creación y configuración

En `MatchManager.Start()` se llama a `BoardCreate()` que instancia las 81 celdas. Luego `BoardSetting()` configura cada celda según los datos del nivel (`Stage`). Finalmente `BoardPosionSetting()` centra el tablero en pantalla, ajustando escala si el tablero es demasiado ancho para la pantalla.

### 4.6. Estructura del prefab Board

```
Board.prefab
├── Board (Transform, Board, GravityDisplayer)
│   └── Displayer (Transform, SpriteRenderer)  ← Sprite visual de la celda
```

Cada celda tiene un `GravityDisplayer` que muestra una flecha de dirección de gravedad. Este componente se activa cuando:
- `isUseGravity == true` (gravedad personalizada)
- El jugador toca una pieza (`Item.OnItemClick` event)

### 4.7. Listas de caída del tablero

`MatchManager` mantiene dos listas calculadas al configurar el tablero:

| Lista | Calculada en | Propósito |
|---|---|---|
| `m_ListDropStart` | `GetBoardDropStartSetting()` | Celdas que son **puntos de inicio de caída** (bifurcaciones, fin de columna, etc.). Estas celdas se priorizan al buscar dónde caer una pieza. |
| `m_ListDropHead` | `GetGravitySetting()` | Celdas que son la **cabeza de una columna de caída** (no tienen celda superior válida). Las piezas nuevas se generan desde estas celdas. |

**`m_ListDropStart`** se rellena con celdas que cumplen alguna de estas condiciones:
- Tienen múltiples `PossibleDrop_Dirs` (pueden recibir piezas de varias direcciones).
- Su celda "drop" (la celda desde la que recibe piezas) es `null`, es fija, o tiene múltiples direcciones.

**`m_ListDropHead`** se rellena con celdas cuyas celdas "drop" (arriba según la gravedad) no existen o son celdas `CreatorEmpty`.

---

## 5. Las piezas (Item)

**Clase base:** `Item : MonoBehaviour` (abstracta)

Cada pieza es un GameObject con un `SpriteRenderer` hijo. La pieza vive sobre un `Board` y tiene un tipo (`ItemType`) y un color (`ColorType`).

### 5.1. Tipos de piezas

```csharp
public enum ItemType
{
    None = -1,
    Normal,           // Pieza básica de color
    Butterfly,        // Mariposa: vuela hacia una pieza del mismo color y la destruye
    Line_X,           // Línea horizontal: destruye toda la fila
    Line_Y,           // Línea vertical: destruye toda la columna
    Line_C,           // Cruz: destruye fila Y columna simultáneamente
    Bomb,             // Bomba: destruye un área 3×3 alrededor
    Rainbow,          // Arcoíris: destruye todas las piezas de un color
    CondItem_Last,    // Separador
    Donut = 20,       // Donut: pieza obstáculo que se destruye al hacer match adyacente
    Spiral,           // Espiral: aparece periódicamente como obstáculo
    JellyBear,        // Oso de gelatina: salta hacia arriba cada turno
    TimeBomb,         // Bomba de tiempo: tiene contador, game over si llega a 0
    Mystery = 30,     // Misterio: cambia de tipo al ser destruida
    Chameleon,        // Camaleón: cambia de color cada turno
    JellyMon = 35,    // Monstruo de gelatina: "come" piezas del match
    Ghost,            // Fantasma
    Key = 40,         // Llave
    BonusCross = 90,  // Cruz bonus (generada en Bonus Time)
    BonusBomb,        // Bomba bonus (generada en Bonus Time)
    Misson_Food1..6   // Piezas de comida para misiones específicas
}
```

### 5.2. Jerarquía de clases de Item

```
Item (abstracta)
├── NormalItem          ← Pieza básica
├── ButterFlyItem       ← Mariposa (con animación idle/fly)
├── LineXItem           ← Línea horizontal
├── LineYItem           ← Línea vertical
├── LineCItem           ← Cruz (línea + columna)
├── BombItem            ← Bomba de área
├── RainbowItem         ← Arcoíris (destruye por color)
├── DonutItem           ← Donut (obstáculo)
├── SpiralItem          ← Espiral (obstáculo periódico)
├── JellyBearItem       ← Oso de gelatina
├── TimeBombItem        ← Bomba con temporizador
├── MysteryItem         ← Pieza misteriosa
├── ChameleonItem       ← Camaleón
├── JellyMon            ← Monstruo de gelatina
├── GhostItem           ← Fantasma
├── KeyItem             ← Llave
├── BonusCrossItem      ← Cruz bonus
├── BonusBombItem       ← Bomba bonus
└── FoodItem            ← Comida de misión (Food1..Food6)
```

### 5.3. Estructura del prefab de Item

Todas las piezas siguen la misma estructura de prefab (ejemplo: `BombItem.prefab`):

```
BombItem (Transform, BombItem, BoxCollider2D)
├── Sprite (Transform, SpriteRenderer)     ← Visual de la pieza
└── Effect (Transform, ParticleSystem)     ← Efecto idle (opcional)
```

- **BoxCollider2D**: Necesario para el input del jugador (`OnMouseDown/Enter/Up`).
- **Sprite hijo**: El `SpriteRenderer` está en un hijo para poder animar la pieza sin afectar el sprite.
- **Effect hijo**: Algunas piezas especiales tienen un `ParticleSystem` integrado para efectos idle.

### 5.4. Catálogo de prefabs de Item

Todos en `Assets/Prefab/`:

| Prefab | ItemType | Notas |
|---|---|---|
| `NormalItem` | `Normal` | Pieza básica de color |
| `ButterFly` | `Butterfly` | Animación idle/fly |
| `LineXItem` | `Line_X` | Línea horizontal |
| `LineYItem` | `Line_Y` | Línea vertical |
| `LineCItem` | `Line_C` | Cruz |
| `BombItem` | `Bomb` | Bomba de área |
| `Donut` | `Donut` | Obstáculo donut |
| `JellyBear` | `JellyBear` | Oso de gelatina |
| `MysteryItem` | `Mystery` | Pieza misteriosa |
| `ChameleonItem` | `Chameleon` | Camaleón |
| `JellyMon` | `JellyMon` | Monstruo de gelatina |
| `Ghost` | `Ghost` | Fantasma |
| `Key` | `Key` | Llave |
| `BonusCrossItem` | `BonusCross` | Cruz bonus (Bonus Time) |
| `BonusBombItem` | `BonusBomb` | Bomba bonus (Bonus Time) |
| `Food1Item`..`Food6Item` | `Misson_Food1..6` | 6 tipos de comida de misión |
```

### 5.5. Propiedades clave de Item

| Propiedad | Tipo | Descripción |
|---|---|---|
| `Match` | `bool` | Si la pieza puede participar en un match (por defecto `true`) |
| `Switch` | `bool` | Si la pieza puede intercambiarse (por defecto `true`) |
| `Drop` | `bool` | Si la pieza puede caer por gravedad (por defecto `true`) |
| `m_Brust` | `bool` | Si la pieza puede explotar |
| `ArountBrust` | `bool` | Si al explotar afecta a los paneles vecinos |
| `HaveNextItem` | `bool` | Si genera una pieza sucesora al explotar |
| `m_Color` | `ColorType` | Color actual de la pieza |
| `m_ItemType` | `ItemType` | Tipo de la pieza |
| `m_Board` | `Board` | Celda donde está ubicada |
| `m_CombineType` | `ItemType` | Tipo de combinación activa (si se combinó con otra especial) |

### 5.6. Métodos virtuales principales

| Método | Descripción |
|---|---|
| `Init(board, type, color, val, copy)` | Inicializa la pieza en un tablero con tipo y color |
| `Brust(Complete, type)` | Ejecuta la destrucción de la pieza |
| `CombineBrust(combinetype, Complete)` | Ejecuta la destrucción por combinación con pieza especial |
| `CheckCombine(combinetype)` | Comprueba si puede combinarse con otro tipo |
| `MissionApply()` | Notifica al sistema de misiones que esta pieza fue destruida |
| `SetColor(color)` / `SetColorRandom()` | Asigna color y sprite correspondiente |

### 5.7. Qué hace realmente cada ficha

| Ficha | Clase | Regla real verificada |
|---|---|---|
| `Normal` | `NormalItem` | Ficha básica. Puntúa con multiplicador de combo cuando la destrucción pertenece a una cascada. Es la referencia de las misiones por color. |
| `Butterfly` | `ButterFlyItem` | Al activarse ejecuta `Ability_Butterfly` sobre su color. Tiene animación idle y de vuelo específica por color. |
| `Line_X` | `LineXItem` | Ejecuta `Ability_LineX`, o sea barrido horizontal. Cuenta como misión `Line` y puede sumar `Line_Line` en combinación. |
| `Line_Y` | `LineYItem` | Ejecuta `Ability_LineY`, o sea barrido vertical. Cuenta como misión `Line` y puede sumar `Line_Line` en combinación. |
| `Line_C` | `LineCItem` | Ejecuta `Ability_Cross`: fila y columna al mismo tiempo. Puede sumar `Cross_Line` o `Cross_Cross`. |
| `Bomb` | `BombItem` | Ejecuta `Ability_Bomb` con radio base 1. Sus combinaciones reales son con mariposa, líneas, cruz y otra bomba. |
| `Rainbow` | `RainbowItem` | No usa color normal. Puede combinar con `Normal`, `Butterfly`, líneas, cruz, `Bomb`, otra `Rainbow` y también con `Mystery`, `Chameleon`, `JellyMon`, `JellyBear`, `TimeBomb` y `Key`. |
| `Donut` | `DonutItem` | Obstáculo-ítem sin color. Al destruirse lanza su efecto propio y suma misión `Donut`. |
| `Spiral` | `SpiralItem` | Obstáculo periódico sin color. Se destruye con efecto propio y suma misión `Spiral`. |
| `JellyBear` | `JellyBearItem` | Oso de gelatina. Tiene sprite/animación por color, cuenta en misión `Bear` y es movido por `BearJumpStep` y generado por `BearSpawnStep`. |
| `TimeBomb` | `TimeBombItem` | Lleva contador visible. Cada intercambio del jugador reduce el contador en 1 mediante `SwitchingApply()`. Si llega a 0, `TimeBombStep` marca derrota. |
| `Mystery` | `MysteryItem` | La ficha por sí sola solo se destruye; la transformación real ocurre en `Board.MysteryChange`, que puede convertirla en normal, línea, bomba, rainbow, pan, helado, `JellyBear`, `Spiral`, `TimeBomb` o `Chameleon`. |
| `Chameleon` | `ChameleonItem` | Cambia de color automáticamente en `ChameleonStep` y fuerza un color distinto al actual con `SetColorRandomOther()`. |
| `JellyMon` | `JellyMon` | Monstruo de gelatina. Acumula comida hasta quedar lleno a 11. Solo lleno puede seleccionarse manualmente y soltarse sobre otra casilla. |
| `Ghost` | `GhostItem` | Ficha especial resuelta por `Ability_Ghost`. Admite combinación con normales, especiales estándar y varias fichas de objetivo. |
| `Key` | `KeyItem` | Al destruirse recorre todas las `BottleCagePanel` activas y les aplica `BottleBrust(this)`. La llave existe para abrir botellas. |
| `BonusCross` | `BonusCrossItem` | Pieza exclusiva de `BonusTime`. Ejecuta `Ability_BonusCross`. |
| `BonusBomb` | `BonusBombItem` | Pieza exclusiva de `BonusTime`. Ejecuta `Ability_BonusBomb`. |
| `Misson_Food1..6` | `FoodItem` | Objetos de misión de comida. El progreso real ocurre al llegar a `FoodArrive`, no al explotar. |

### 5.8. Reglas reales de spawn de fichas especiales

`Board.TopSpawnItem()` es el punto de entrada de todo el spawn superior. Su lógica completa es:

1. **Filtro de columna/fila:** Si `defaultSpawnLine[X]` es `false`, o si `isUseGravity=true` y `defaultSpawnLineY[Y]` es `false`, **no se genera nada** y retorna `false`. Este filtro bloquea absolutamente todo el spawn de esa celda.
2. **Tutorial seed:** Si hay semilla de tutorial activa, genera la pieza predefinida.
3. **Orden de prioridad de spawn** (el primero que cumpla sus condiciones gana):
   1. Comida de misión (`MissionManager.CreatFoodItem`)
   2. Espiral (`ItemManager.IsCreateSpiral`)
   3. Donut (`ItemManager.IsCreateDonut`)
   4. Bomba de tiempo (`ItemManager.IsCreateTimeBomb`)
   5. Misterio (`ItemManager.IsCreateMystery`)
   6. Camaleón (`ItemManager.IsCreateChameleon`)
   7. Llave (`ItemManager.IsCreateKey`)
   8. Normal (fallback — siempre ocurre si nada anterior aplica)

Los creadores especiales (`Creator_Food`, `Creator_Sprial`, `Creator_TimeBomb`, `Creator_Key` y variantes mixtas como `Creator_Sprial_TimeBomb`) también pasan por estos métodos pero con `istop=false`, lo que les permite ignorar el filtro de columna X de su ítem específico (el creador ya está físicamente en la columna correcta).

Restricciones adicionales:
- `ItemManager` no genera por probabilidad libre: cada familia usa `*_MinExist`, `*_MaxExist`, `*_Interval` y `*_SpawnCnt`.
- `Key` solo se genera si todavía quedan paneles `Bottle_Cage` activos en el tablero.
- `TimeBomb`, `Mystery`, `Chameleon` y `Key` dejan de generarse durante `BonusTime`.
- `Donut`, `Mystery` y `Chameleon` **siempre** comprueban su `*SpawnLine[X]`, incluso cuando el spawn procede de un creador (no tienen parámetro `istop`).

### 5.9. Equivalencias de fichas en la otra nomenclatura del mismo juego

| Nomenclatura principal | Nomenclatura alternativa | Observación útil |
|---|---|---|
| `NormalItem` | `SimpleChip` | Ficha normal de color |
| `BombItem` | `SimpleBomb` | Bomba radial |
| `RainbowItem` | `RainbowBomb` | Bomba/arcoíris global por color |
| `Line_X` / `Line_Y` | `HBomb` / `VBomb` | Barridos horizontal y vertical |
| `ChameleonItem` | `SimpleChip` + `Powerup.Chameleon` | Mismo concepto con implementación distinta |
| `FoodItem` | `BringDownChip` / objetivos de transporte | Objetos que deben llegar a destino |
| `Cracker` como objetivo transportable en otra rama | `OreoCracker` | Objetivo de recorrido/recogida, no ficha normal |
| secuencia especial de objetivo | `NumberChocolateChip` | Objetivo ordenado/encadenado |

En la rama `BoardManager/Chip`, varios elementos que aquí aparecen como `ItemType` separados se resuelven como un `Chip` normal con powerup adicional o como un chip-objetivo especializado.

---

## 6. Sistema de colores

```csharp
public enum ColorType { None, RED, YELLOW, GREEN, BLUE, PURPLE, ORANGE, Rnd }
```

- **6 colores disponibles**, pero cada nivel define cuáles aparecen en `Stage.appearColor[]`.
- `Rnd` indica asignación aleatoria al crear la pieza.
- El sprite se selecciona por índice: `color - ColorType.RED` (0-based).
- Las piezas del tipo `Rainbow` no tienen color asignado y actúan sobre un color elegido al explotar.

### 6.1. Reglas de color que sí afectan al gameplay

- `Item.SetColorRandom()` y `GetColorRandom()` siempre eligen desde `MatchManager.m_AppearColor`, no desde una lista fija global.
- `Chameleon` y `MagicColor` no repiten el mismo color inmediatamente: usan `SetColorRandomOther()`.
- `CrackerPanel` y `RingPanel` también almacenan color. Si el `def` del panel es `Rnd`, el color real se fija al inicializarlo usando `m_AppearColor`.
- `Rainbow` se representa como `ColorType.None`; su selección de color ocurre al resolver la habilidad, no al instanciar la ficha.
- El sistema Crazy Level puede reducir colores disponibles en `m_AppearColor`, alterando directamente la aleatoriedad de normales, camaleones, crackers aleatorios y anillos aleatorios.

---

## 7. Paneles (Panel)

**Clase base:** `Panel : MonoBehaviour` (abstracta)

Los paneles son **modificadores de celda** que se apilan sobre un `Board`. Una celda puede tener múltiples paneles simultáneamente.

### 7.1. Tipos de panel

| Valor | Tipo | Comportamiento |
|---|---|---|
| `Default_Full (100)` | Celda activa | Celda normal jugable |
| `Default_Empty (101)` | Celda vacía | No existe en el tablero visible |
| `Creator_Empty (90)` | Creador vacío | Genera piezas sin ser visible |
| `Fixed_Block (19)` | Bloque fijo | No se puede destruir, bloquea caída |
| `Ice_Cage (-11)` | Jaula de hielo | Bloquea la pieza, se destruye con match adyacente |
| `Bottle_Cage (-12)` | Jaula de botella | Similar a jaula de hielo con más defensa |
| `Lolly_Cage (-10)` | Jaula de piruleta | Similar a jaula de hielo |
| `Stele (82)` | Estela | Obstáculo fijo destructible |
| `Stele_Hide (81)` | Estela oculta | Aparece tras destruir pieza |
| `Wafer_floor (80)` | Suelo de oblea | Se destruye con matches encima |
| `Jam (79)` | Jalea | Se expande a celdas adyacentes al hacer match |
| `Bread_Block (20)` | Bloque de pan | Obstáculo destructible |
| `Cracker (28)` | Galleta | Obstáculo destructible |
| `Cake_A..D (24-27)` | Tarta | Obstáculo multi-capa |
| `JewelTree_A..D (32-35)` | Árbol de joyas | Obstáculo multi-capa |
| `S_Tree (29)` | Árbol | Obstáculo |
| `Ring (31)` | Anillo | Recoge piezas de un color específico |
| `MagicColor (30)` | Color mágico | Cambia el color de la pieza cada turno |
| `IceCream_Block (22)` | Helado | Se expande por el tablero |
| `IceCream_Creator (21)` | Creador de helado | Genera helado periódicamente |
| `ConveyerBelt (23)` | Cinta transportadora | Mueve las piezas en una dirección |
| `Warp_In (-30)` / `Warp_Out (-31)` | Portales | Teletransportan piezas de un punto a otro |
| `FoodArrive (-32)` | Llegada de comida | Punto de destino para ítems de comida |
| `JellyBearStart (-33)` | Inicio oso | Punto de aparición de osos de gelatina |
| `Creator_Food (-20)`, etc. | Creadores especiales | Generan piezas especiales (espiral, bomba de tiempo, llave, comida) |

### 7.2. Propiedades de Panel

- **`Defence`**: Número de golpes necesarios para destruirlo.
- **`Value`**: Valor adicional configurable.
- **`ItemExist`/`ItemDrop`/`ItemMatch`/`ItemSwitch`**: Controlan si la celda permite existencia, caída, match o intercambio de ítems.

### 7.3. PanelManager — Factoría de paneles

**Clase:** `PanelManager : MonoBehaviour` (Singleton)

El `PanelManager` mantiene una lista serializada `List_Panel` de `PanelObj`, donde cada entrada mapea un `PanelType` a su prefab:

```csharp
[Serializable]
public class PanelObj
{
    public PanelType m_PanelType;
    public GameObject m_PanelObj;     // Referencia al prefab del panel
}
```

Cuando se necesita crear un panel, se llama a `CreatePanel(type, parentBoard)`:
1. Busca en `List_Panel` el prefab correspondiente al `PanelType`.
2. Obtiene una instancia del `ObjectPool` (reutiliza si hay disponible).
3. Resetea la escala a `Vector3.one`.
4. Devuelve el `GameObject`.

### 7.4. Catálogo de prefabs de paneles

Todos los prefabs de panel se encuentran en `Assets/Prefab/`:

| Prefab | PanelType |
|---|---|
| `DefaultFullPanel` | `Default_Full` |
| `DefaultEmptyPanel` | `Default_Empty` |
| `CreatorEmptyPanel` | `Creator_Empty` |
| `FixedPanel` | `Fixed_Block` |
| `IceCagePanel` | `Ice_Cage` |
| `BottleCagePanel` | `Bottle_Cage` |
| `LollyCagePanel` | `Lolly_Cage` |
| `BreadPanel` | `Bread_Block` |
| `CrackerPanel` | `Cracker` |
| `CakeAPanel` / `BPanel` / `CPanel` / `DPanel` | `Cake_A..D` |
| `JewelTreeAPanel` / `BPanel` / `CPanel` / `DPanel` | `JewelTree_A..D` |
| `JamPanel` | `Jam` |
| `MagicColorPanel` | `MagicColor` |
| `IceCreamPanel` | `IceCream_Block` |
| `IceCreamCreaterPanel` | `IceCream_Creator` |
| `ConveyerBelt` | `ConveyerBelt` |
| `FoodArrive` | `FoodArrive` |
| `JellyBearStart` | `JellyBearStart` |
| `CreatorFood` | `Creator_Food` |
| `CreatorSprial` | `Creator_Spiral` |
| `CreatorTimeBomb` | `Creator_TimeBomb` |
| `CreatorKey` | `Creator_Key` |
| `CreatorFoodSprial` | `Creator_Food_Spiral` |
| `CreatorFoodTimeBomb` | `Creator_Food_TimeBomb` |
| `CreatorSprialTimeBomb` | `Creator_Spiral_TimeBomb` |
| `CreatorKeyFood` | `Creator_Key_Food` |
| `CreatorKeyTimeBomb` | `Creator_Key_TimeBomb` |

**Nota:** Los creadores combinados (ej: `CreatorFoodSprial`) generan alternadamente uno u otro tipo de ítem especial según los intervalos configurados en el nivel.

### 7.5. Qué hace realmente cada panel

| Panel | Clase | Regla real verificada |
|---|---|---|
| `Default_Full` | `DefaultPanel` | Solo define una casilla jugable y alterna sprite par/impar del tablero. No se destruye. |
| `Default_Empty` | `EmptyPanel` | Casilla inexistente. No admite pieza ni interacción real. |
| `Creator_Empty` | `CreatorEmptyPanel` | Cabeza de generación invisible. No se destruye y sirve como origen de spawn. |
| `Fixed_Block` | `FixedPanel` | Casilla fija: no se destruye y bloquea caída, porque hace que `IsPanelFixed == true`. |
| `Ice_Cage` | `IceCagePanel` | Jaula multicapa. Cada impacto baja `Defence`, cambia sprite y cuenta como misión `IceCage`. |
| `Lolly_Cage` | `LollyCagePanel` | Misma idea que la jaula de hielo, con efectos propios. |
| `Bottle_Cage` | `BottleCagePanel` | No se abre por match de color genérico: la destruye `KeyItem`. Cada llave anima un viaje hasta la botella, baja `Defence` y, al limpiar todas, las llaves restantes del tablero se convierten en fichas normales. |
| `Bread_Block` | `BreadPanel` | Obstáculo multicapa directo. Cada golpe baja `Defence`; al llegar a 0 se elimina y suma misión `Bread`. |
| `Cracker` | `CrackerPanel` | Panel con color. Solo se rompe si el impacto viene de una casilla vecina cuyo `m_Item.m_Color` coincide con el color del cracker. Si `arroundRoot == null`, acepta destrucción directa. |
| `Wafer_floor` | `WaferPanel` | Suelo destructible por capas. Reduce `Defence` y suma misión `Wafer`. |
| `Jam` | `JamPanel` | No se resuelve con `PanelBrust()` normal: la jalea se crea/expande con `JamPanelCreate()` y su progreso se aplica al crear una nueva casilla con jalea. |
| `Cake_A..D` | `CakePanel` | Entidad 2x2 coordinada. Cada segmento comparte contador con `Cake_A`; al acumular 8 golpes totales dispara `Ability_CakePanel` y destruye los 4 paneles. |
| `JewelTree_A..D` | `JewelTreePanel` | Árbol de joyas por fases. Cada golpe activa una joya visual; al cuarto, genera 4 ítems definidos por `Stage.JewelTreeItem[]` y `JewelTreeItemColor[]`, los lanza a casillas normales y marca matching. |
| `S_Tree` | `S_TreePanel` | Al llegar a 0 no solo desaparece: genera un nuevo ítem según `S_TreeData` y `S_Tree_SettingType`. |
| `Ring` | `RingPanel` | Panel con color. Consume temporalmente la pieza de la casilla, genera una `Line_X` o `Line_Y` del color del anillo y después se destruye. |
| `MagicColor` | `MagicColorPanel` | No se destruye. Durante `MagicColorStep` rota visualmente y obliga a la pieza de su casilla a cambiar a otro color. |
| `IceCream_Block` | `IceCreamPanel` | Obstáculo expansivo. Si una capa de helado se destruye, resetea el ciclo de expansión del helado. |
| `IceCream_Creator` | `IceCreamCreatorPanel` | Generador de helado. No se destruye y, cuando toca expandirse, ejecuta animación antes de copiar helado a un vecino válido. |
| `ConveyerBelt` | `ConveyerBeltPanel` | Panel logístico. Lee `ConveyerBeltInfo` desde `addInfo`, distingue inicio/medio/final y mueve ítems por animación en `ConveyerBeltStep`. |
| `Warp_In` / `Warp_Out` | `WarpInPanel`, `WarpOutPanel` | Portales. `Board` enlaza ambas casillas mediante el campo `value` y modifica tanto la caída real como la simulación virtual. |
| `FoodArrive` | `FoodArvPanel` | Meta de comida. No se destruye; marca una casilla final válida para recoger `FoodItem`. |
| `JellyBearStart` | panel especial | Punto de aparición de osos. `BearSpawnStep` lo usa para decidir dónde generar nuevos `JellyBear`. |
| `Creator_*` | `CreatorPanel` | Fuentes de spawn especiales. No se destruyen; al generar una pieza ejecutan `AddAction()` para animar visualmente el creador. |

### 7.6. Reglas estructurales del sistema de paneles

- `Board.PanelBrust()` solo intenta romper el panel destruible más alto de la pila y salta explícitamente `JamPanel`.
- Si el panel recibe daño “de alrededor” (`arroundRoot != null`), solo se procesa cuando `panel.ArountBrust == true`.
- Una celda se considera jaula (`IsPanelCage`) cuando algún panel permite existencia de ítem pero bloquea la caída.
- Una celda se considera fija (`IsPanelFixed`) cuando algún panel impide existencia de ítem y además nunca se destruye.

### 7.7. Equivalencias de obstáculos y casillas especiales en la otra nomenclatura

| Nomenclatura principal | Nomenclatura alternativa | Regla equivalente |
|---|---|---|
| `CrackerPanel` con color | `SpriteDrink` por color | Objetivo restringido por color: solo baja con impactos del color correcto |
| `Bottle_Cage` + `KeyItem` | bloque con llave/activación específica | Apertura condicionada por otro elemento de tablero |
| `Bread_Block` / `Wafer_floor` / `Ice_Cage` | `Crunky`, `Dig`, `ChocolateJail`, `RockCandy` | Obstáculos multicapa o bloqueantes de casilla |
| `Creator_*` | `SlotGenerator` | Fuentes de generación especiales |
| `FoodArrive` | `bringDownEndSlot` / `Pocket` | Casilla destino de recogida |
| `JellyBearStart` | spawn de objetivo móvil | Punto de aparición controlado por step/sistema |
| `JamPanel` | `Slot.IsPaintedJelly` / `JellyLayer` | Casilla pintada/contagiada, no obstáculo de HP clásico |
| `Warp_In` / `Warp_Out` | `teleportTarget` / rail especial | Redirección del recorrido de caída |

La diferencia principal es de modelado: aquí muchos comportamientos viven en `Panel`; en la otra rama, varias de esas reglas viven directamente en `Slot`, `BlockInterface` o en generadores del tablero.

---

## 8. Sistema de input

El input se basa en los eventos de Unity `OnMouseDown`, `OnMouseEnter` y `OnMouseUp` definidos directamente en la clase `Item`. El componente requiere un `Collider2D` en la pieza para funcionar.

### 8.1. TouchState

```csharp
public enum TouchState { Switching, CashItemUse, JellyMonDrop }
```

| Estado | Descripción |
|---|---|
| `Switching` | Modo normal: el jugador intercambia dos piezas adyacentes |
| `CashItemUse` | Usando un ítem de tienda (martillo, bomba, etc.) |
| `JellyMonDrop` | Soltando un monstruo de gelatina |

### 8.2. Flujo del input

1. **`OnMouseDown`**: El jugador toca una pieza. Se registra como `Swap_A` y se activa `SwitchStart = true`.
2. **`OnMouseEnter`**: El dedo se arrastra a una pieza vecina. Si es adyacente a `Swap_A`, se registra como `Swap_B` y se llama a `MatchManager.Switching(A, B)`.
3. **`OnMouseUp`**: Si no se arrastró a ninguna pieza válida, `SwitchStart` se resetea.

### 8.3. Validación de adyacencia

```csharp
public bool CheackNeighbor(Item _item)
```

Comprueba si `_item` está en una de las 4 celdas cardinales adyacentes (TOP, BOTTOM, LEFT, RIGHT). **No se permiten intercambios diagonales.**

### 8.4. Guardas de input

El input se ignora si:
- `MatchState` no es `Playing`
- `StepType` no es `Wait` (hay una animación o proceso en curso)
- `SwitchStart` ya está activo (evita doble-tap)

---

## 9. Sistema de Steps

El turno se gestiona mediante una **máquina de estados** implementada con el patrón Strategy. `MatchManager` mantiene un `Dictionary<StepType, BaseStep>` y ejecuta `Step_Process()` del step activo en cada `Update()`.

### 9.1. Tipos de Step

```
Wait → [Input del jugador] → Matching → TimeBomb → IceCream → ConveyerBelt → 
Chameleon → MagicColor → BearJump → BearSpawn → Mission → (Wait | Clear | Fail | Shuffling)
```

| Step | Descripción |
|---|---|
| **`Wait`** | Espera input del jugador. Muestra hints tras 5s de inactividad. Verifica shuffling. |
| **`Matching`** | Busca matches, ejecuta explosiones, gestiona caída de piezas. Loop principal del turno. |
| **`TimeBomb`** | Verifica bombas de tiempo. Si alguna llega a 0, activa game over. |
| **`IceCream`** | Expande el helado por el tablero según intervalos configurados. |
| **`ConveyerBelt`** | Mueve las piezas en las cintas transportadoras. |
| **`Chameleon`** | Cambia el color de las piezas camaleón. |
| **`MagicColor`** | Cambia el color de las piezas en casillas de color mágico. |
| **`BearJump`** | Los osos de gelatina saltan una casilla hacia arriba. |
| **`BearSpawn`** | Genera nuevos osos de gelatina en sus puntos de aparición. |
| **`Mission`** | Verifica condiciones de victoria y derrota. Decide si vuelve a `Wait`, `Clear` o `Fail`. |
| **`Clear`** | Transiciona a `BonusTime`. |
| **`Fail`** | Transiciona a `GameFail`. |
| **`Shuffling`** | Recoloca las piezas aleatoriamente cuando no hay movimientos posibles. |

### 9.2. BaseStep

```csharp
public class BaseStep
{
    public virtual void Step_Init();     // Inicialización al inicio de partida
    public virtual void Step_Play();     // Se ejecuta UNA VEZ al entrar al step
    public virtual void Step_Process();  // Se ejecuta cada frame mientras el step está activo
}
```

### 9.3. Comportamiento concreto de los steps especiales

- `TimeBombStep` no decrementa bombas: solo comprueba si alguna ya llegó a 0. El decremento ocurre en cada intercambio del jugador porque `TimeBombItem` se suscribe a `MatchManager.SwitchingApply`.
- `IceCreamStep` solo expande si no se ha destruido helado en ese ciclo. Si `IceCreamPanel.m_IceCreamBrust == true`, resetea intervalos y aplaza la expansión.
- `IceCreamStep` copia helado solo a vecinos cardinales que tengan una `NormalItem`, que sean destruibles y que no estén sobre cinta transportadora.
- `ConveyerBeltStep` primero guarda temporalmente el ítem actual de cada cinta en `CoveyerItem`, luego mueve todos los tramos y solo al final apaga las flechas visuales.
- `ChameleonStep` recorre todas las casillas con `ChameleonItem`, reproduce animación y al terminar marca `m_MatchingCheck = true` para forzar reevaluación del tablero.
- `MagicColorStep` solo actúa si en la casilla hay un ítem con color distinto de `None`. También deja `m_MatchingCheck = true` al acabar.
- `BearJumpStep` intenta mover cada `JellyBear` hacia su salida real de caída. Si el oso alcanza salida válida o un warp sin continuidad, se sustituye por una ficha normal del mismo color y se considera rescatado.
- `BearSpawnStep` respeta `Bear_MaxExist` y `Bear_Interval`, y evita sobreescribir especiales potentes si todavía quedan otros puntos de aparición disponibles.

---

## 10. Detección de matches

La detección se realiza en `MatchingStep.CheckMatchCondition()` y los métodos `FindMatches*` de `Board`.

### 10.1. Tipos de match soportados

La detección itera por **prioridad descendente** desde `Rainbow` (valor 6) hasta `Normal` (valor 0). El primer patrón que encaja para cada celda es el que se aplica. Los valores corresponden al enum `ItemType`: Normal=0, Butterfly=1, Line_X=2, Line_Y=3, Line_C=4, Bomb=5, Rainbow=6.

#### Prioridad 6 — Rainbow (5 en línea)

```
■ ■ ■ ■ ■     ■
               ■
               ■
               ■
               ■
```

- **Condición:** `list_x.Count >= 4` ó `list_y.Count >= 4` (4 vecinos + 1 central = 5 piezas).
- **Genera:** `Rainbow` en la posición central.
- **Efecto:** Al activar, destruye TODAS las piezas de un color elegido.

#### Prioridad 5 — Bomb (L con esquina)

```
■              ■          ■ ■ ■      ■
■                  ■          ■      ■
■ ■ ■      ■ ■ ■          ■      ■ ■ ■
```

- **Condición:** `list_x.Count >= 2` Y `list_y.Count >= 2`, Y los vecinos forman una **L** (no T).
- **Validación por bits:** Se verifica que los vecinos horizontales estén *todos a un solo lado* y los verticales *todos a un solo lado*:
  - bit 1 = vecino H a la izquierda, bit 2 = vecino H a la derecha
  - bit 8 = vecino V arriba, bit 4 = vecino V abajo
  - Solo se acepta si el resultado es 5 (izq+abajo), 6 (der+abajo), 9 (izq+arriba) o 10 (der+arriba).
- **Genera:** `Bomb` en la posición central.
- **Efecto:** Destruye un área de 3×3 alrededor.
- **Nota:** Una forma de T (vecinos a ambos lados de un eje) **NO** genera Bomb, genera `Line_C`.

#### Prioridad 4 — Line_C (T o cruz, 3H + 3V cruzados)

```
    ■
■ ■ ★ ■         ■
    ■         ■ ■ ★
    ■            ■
```

- **Condición:** `list_x.Count >= 2` Y `list_y.Count >= 2` (al menos 2 vecinos en cada eje).
- **Genera:** `Line_C` en la posición central.
- **Efecto:** Destruye fila + columna completa.
- **Nota:** Se evalúa *después* de Bomb. Si la forma tiene vecinos a ambos lados en algún eje (forma T en vez de L), entra aquí en lugar de Bomb.

#### Prioridad 3 — Line_Y → genera Line_X (4 en vertical)

```
■
■
★
■
```

- **Condición:** `list_y.Count >= 3` (3 vecinos verticales + 1 central = 4 piezas).
- **Genera:** `Line_X` (¡perpendicular!) en la posición central.
- **Efecto:** Destruye toda la **fila** (horizontal).

#### Prioridad 2 — Line_X → genera Line_Y (4 en horizontal)

```
■ ■ ★ ■
```

- **Condición:** `list_x.Count >= 3` (3 vecinos horizontales + 1 central = 4 piezas).
- **Genera:** `Line_Y` (¡perpendicular!) en la posición central.
- **Efecto:** Destruye toda la **columna** (vertical).

#### Prioridad 1 — Butterfly (cuadrado 2×2)

```
■ ■        ■ ■        ★ ■        ■ ★
★ ■        ■ ★        ■ ■        ■ ■
```

- **Detección especial:** En vez de `FindMatchesHorizontal/Vertical`, usa `FindMatchesSquare()` que busca las 4 esquinas posibles (TopLeft, TopRight, BottomLeft, BottomRight).
- **Condición:** `list_x.Count >= 3` (el cuadrado pone 3 vecinos en list_x).
- **Genera:** `Butterfly` en la posición central.
- **Efecto:** La mariposa vuela hasta una pieza del mismo color y la destruye.
- **Nota importante:** La Butterfly es la **única pieza especial que se genera por un cuadrado 2×2**. Este es un patrón exclusivo.

#### Prioridad 0 — Normal (3 en línea)

```
■ ■ ■         ■
              ■
              ■
```

- **Condición:** `list_x.Count == 2` ó `list_y.Count == 2` (2 vecinos + 1 central = 3 piezas).
- **Genera:** Nada (`ItemType.None`). Solo destrucción normal de las piezas.

#### Tabla resumen

| Prioridad | Patrón | Condición en `SpecailItemCondition` | Pieza generada |
|:-:|---|---|---|
| 6 | 5 en línea (H o V) | `list_x ≥ 4` ó `list_y ≥ 4` | `Rainbow` |
| 5 | L (esquina: solo un lado H + un lado V) | `list_x ≥ 2` Y `list_y ≥ 2` Y bits ∈ {5,6,9,10} | `Bomb` |
| 4 | T o + (ambos lados en algún eje) | `list_x ≥ 2` Y `list_y ≥ 2` | `Line_C` |
| 3 | 4 en vertical | `list_y ≥ 3` | `Line_X` (perpendicular) |
| 2 | 4 en horizontal | `list_x ≥ 3` | `Line_Y` (perpendicular) |
| 1 | Cuadrado 2×2 | `FindMatchesSquare` → `list_x ≥ 3` | `Butterfly` |
| 0 | 3 en línea (H o V) | `list_x == 2` ó `list_y == 2` | Ninguna (Normal) |

> **Nota sobre perpendicularidad:** Un match de 4 horizontal genera `Line_Y` (destruye columna) y un match de 4 vertical genera `Line_X` (destruye fila). La pieza especial siempre destruye en la dirección **perpendicular** al match que la creó.

### 10.2. Proceso de detección

El método `CheckMatchCondition()` en `MatchingStep` ejecuta la detección:

1. Se itera por prioridad descendente de `i=7` a `i=0` (correspondiendo a los valores de `ItemType`).
2. Para cada celda marcada con `m_MatchingCheck = true` (y que no esté cayendo ni explotando):
   - **Si `i == 1` (Butterfly):** se usa `FindMatchesSquare(ref list_x)` que busca cuadrados 2×2 en las 4 esquinas.
   - **Si `i != 1`:** se usa `FindMatchesHorizontal(ref list_x)` + `FindMatchesVertical(ref list_y)`.
3. `SpecailItemCondition(bd, (ItemType)i, list_x, list_y)` evalúa si el patrón encontrado genera la pieza especial de esa prioridad.
4. Si hay match, las celdas se marcan con `m_isMatchBrust = true` y `m_MatchingCheck = false`.
5. Si alguna celda del match tiene `m_IsJamBoard`, la jalea se expande a las celdas adyacentes del match.
6. Tras iterar todas las prioridades y celdas, `MatchBrust()` ejecuta la destrucción de todas las celdas marcadas.

### 10.3. FindMatches_Dir_Recursive

```csharp
public void FindMatches_Dir_Recursive(ColorType color, ref List<Board> countedItem, SQR_DIR dir)
```

Desde una celda, busca recursivamente en una dirección (`dir`) todas las celdas consecutivas del mismo color. Se detiene al encontrar:
- Una celda `null` (borde del tablero)
- Una celda sin ítem matcheable (`IsNowItemMatch == false`)
- Una celda con animación de caída o explosión en curso
- Un color diferente

---

## 11. Generación de piezas especiales

Las piezas especiales se crean cuando un match cumple ciertas condiciones de forma (ver sección 10.1). La pieza especial aparece en la posición de la pieza central del match. Si esa celda ya está explotando (`m_Brust == true`) y no tiene ya un `HaveNextItem`, se busca la primera celda del match que sí esté explotando para colocar la pieza especial ahí.

| Prioridad | Match | Piezas involucradas | Genera | Efecto |
|:-:|---|:-:|---|---|
| 6 | 5 en línea | 5 | `Rainbow` | Destruye todas las piezas de un color |
| 5 | L (esquina) | 5 | `Bomb` | Destruye área 3×3 |
| 4 | T o + | 5 | `Line_C` | Destruye fila + columna |
| 3 | 4 vertical | 4 | `Line_X` | Destruye fila (perpendicular) |
| 2 | 4 horizontal | 4 | `Line_Y` | Destruye columna (perpendicular) |
| 1 | Cuadrado 2×2 | 4 | `Butterfly` | Vuela y destruye pieza del mismo color |
| 0 | 3 en línea | 3 | Nada | Solo destrucción |

La pieza especial se almacena en `Board.m_NextItemType` y se genera tras la explosión de los ítems originales.

---

## 12. Combinaciones entre piezas especiales

Cuando el jugador intercambia dos piezas especiales, en lugar de buscar match se ejecuta una **CombineBrust** directa. La validación se hace con `CheckCombine()`.

### 12.1. Tabla de combinaciones

| Pieza A | Pieza B | Efecto |
|---|---|---|
| `Line_X` + `Line_X/Y` | — | `LineLine`: destruye fila Y columna |
| `Line_C` + `Line_X/Y/C` | — | `CrossLine`: destruye en cruz ampliada |
| `Bomb` + `Line_X/Y` | — | `BombLine`: bombas en línea |
| `Bomb` + `Line_C` | — | `BombCross`: bombas en cruz |
| `Bomb` + `Bomb` | — | `BombBomb`: área de explosión 5×5 |
| `Bomb` + `Butterfly` | — | `BombButterfly` |
| `Rainbow` + `Normal` | — | Destruye TODAS las piezas del color del normal |
| `Rainbow` + `Line_X/Y` | — | Convierte todas las de ese color en líneas |
| `Rainbow` + `Line_C` | — | Convierte todas las de ese color en cruces |
| `Rainbow` + `Bomb` | — | Convierte todas las de ese color en bombas |
| `Rainbow` + `Rainbow` | — | **Destruye TODO el tablero** |
| `Rainbow` + `Butterfly` | — | Efecto arcoíris + mariposa |
| `Butterfly` + `Butterfly` | — | `ButterflyButterfly` |

---

## 13. Sistema de gravedad y caída (Drop)

### 13.1. Dirección de gravedad

Cada celda tiene un `DROP_DIR` configurable que determina de dónde recibe piezas:

- `U` (Up): las piezas caen desde arriba (por defecto).
- `D` (Down): caen desde abajo.
- `L` (Left): caen desde la izquierda.
- `R` (Right): caen desde la derecha.

Esto permite diseñar niveles con gravedad invertida o lateral.

### 13.2. GravityDropItemRow

```csharp
public bool GravityDropItemRow(Board pb)
```

Es el método principal de caída. Se ejecuta recursivamente desde las celdas inferiores hacia arriba:

1. Si la celda tiene ítem, intenta propagar la caída hacia arriba.
2. Si la celda está vacía y no hay celda superior, intenta generar un ítem desde un creador (`TopSpawnItem`).
3. Si la celda superior tiene un ítem que puede caer, lo mueve hacia abajo (`ItemDrop`).
4. Si no hay ítem disponible arriba, busca lateralmente (`SideDrop`) para llenar la celda.

### 13.3. Drop lateral

Cuando una celda no puede recibir piezas desde su dirección principal, busca en las celdas `DropLeft` y `DropRight` (celdas diagonales relativas a la dirección de caída).

### 13.4. Warps

Las celdas con `Warp_Out` se conectan a una celda `Warp_In`. Cuando una pieza cae por un warp out, reaparece en el warp in correspondiente.

### 13.5. Cintas transportadoras

Las `ConveyerBeltPanel` mueven las piezas en una dirección fija cada turno, independiente de la gravedad.

### 13.6. Sistemas equivalentes de recorrido y caída en otras ramas

- En la nomenclatura `Slot`/`BoardManager`, cada celda también guarda una dirección de caída propia (`DropDirection`).
- Los generadores equivalentes a `Creator_*` calculan el offset de aparición en función de esa dirección (`Up`, `Down`, `Left`, `Right`).
- Los portales, rails y rutas especiales cumplen la misma función que aquí realizan `Warp_In`, `Warp_Out`, bifurcaciones de `DropDirs` y listas de caída.
- El concepto de “objetivo que debe llegar a una salida” es el mismo que aquí usan `FoodArrive` y `JellyBear` cuando alcanzan una casilla final válida.

---

## 14. Sistema de misiones y condiciones de victoria/derrota

### 14.1. Condiciones de derrota

```csharp
public enum FailType { None, Limit_Move, TimeBombOver, ShufflingOver }
```

- **`Limit_Move`**: Se acabaron los movimientos disponibles.
- **`TimeBombOver`**: Una bomba de tiempo llegó a 0.
- **`ShufflingOver`**: No hay movimientos posibles incluso tras shuffle (raro).

### 14.2. Verificación de victoria/derrota

En `MissionStep.Step_Play()`:

1. Si quedan celdas con `m_MatchingCheck`, vuelve a `Matching` (hay matches pendientes).
2. `CheckMissionClear()`: ¿se completaron todos los objetivos? → `Clear`.
3. `CheckMissionFail()`: ¿se cumple alguna condición de derrota? → `Fail`.
4. Si no se cumple ninguna → `Wait` (devuelve el control al jugador).

### 14.3. Tipos de misión

Las misiones se definen en los datos del nivel (`Stage.missionInfo`). Cada misión tiene:
- `MissionType`: Tipo (OrderN para normales, OrderS para especiales, Bear, etc.)
- `MissionKind`: Qué hay que recoger (Red, Blue, Bomb, Bomb_Line, Bear, etc.)
- Cantidad objetivo.

Al destruir una pieza, se llama a `Item.MissionApply()` que notifica a `MissionManager`.

### 14.4. Cómo se recalculan realmente los objetivos al iniciar el nivel

- `MissionManager.MissionSetting()` corrige varios contadores a partir del tablero real, no del número escrito en JSON.
- `Bread`, `LollyCage`, `IceCage`, `Bottle`, `S_Tree`, `Wafer` e `IceCream` recalculan su total contando paneles existentes en el tablero.
- `Cracker` usa una variante especial porque el objetivo es cuántos crackers quedan, no cuántas capas de defensa suman.
- `TimeBomb` en misión `OrderS` también se recalcula desde las bombas realmente colocadas al inicio.
- Si el nivel tiene `Stele` en el tablero pero no la incluyó en `missionInfo`, `MissionManager` inserta automáticamente una misión `Stele` al comenzar.
- En misiones de `Food`, el manager clona la lista de objetivos en `List_MsInfo_Food` y además arranca los contadores internos de spawn en función de la comida ya precolocada.

### 14.5. Relación real entre destrucción y misión

- Las piezas normales notifican por color.
- Las especiales notifican tanto su misión base como varias combinaciones derivadas, por ejemplo `Bomb_Line`, `Cross_Line`, `Rainbow_Bomb` o `Rainbow_Rainbow`.
- Muchos paneles solo aplican misión cuando su destrucción termina realmente en `Board.Co_PanelBrustComplete()`, no en el instante del impacto.
- `Jam` suma progreso cuando se crea o expande a una nueva casilla, no cuando “se rompe”.
- `FoodItem` aplica misión al ser recogido en destino, no al explotar.

### 14.6. Equivalencias de objetivos en la otra nomenclatura

| Sistema de este documento | Sistema alternativo | Equivalencia práctica |
|---|---|---|
| `MissionType.OrderN` por color | `CollectBlockType.Normal*` | Recoger fichas normales por color |
| `MissionType.OrderS` | `CollectBlockType.Make*` o collects de especial | Crear o destruir especiales concretas |
| `MissionType.Food` | `GoalTarget.BringDown`, `PocketCandy`, objetivos transportables | Llevar un objeto hasta su salida |
| `MissionType.Jam` | `GoalTarget.Jelly` | Pintar/ocupar casillas del tablero |
| `MissionType.Bear` | objetivos móviles de rescate | Rescatar un ente del tablero |
| `MissionKind.Cracker` | `CollectCracker` / `OreoCracker` / `CarbonatedDrink` según rama | Objetivo especial asociado al tema “cracker/bebida/recogida condicionada” |

Cambian los enums y los nombres de collect, pero la intención de diseño sigue siendo la misma: destruir, crear, transportar o pintar objetivos de tablero.

---

## 15. Bonus Time

Cuando se completan todos los objetivos con movimientos sobrantes:

1. Se entra en `MatchState.BonusTime`.
2. Cada movimiento restante se convierte en una pieza especial aleatoria (`BonusCross` o `BonusBomb`) que se coloca en una celda aleatoria.
3. Las piezas especiales explotan secuencialmente, generando puntuación adicional.
4. Al terminar, se muestra el popup de victoria.

---

## 16. Sistema Crazy Level

En `MatchManager.CrazyLevelSetting()`, si el jugador ha fallado un nivel múltiples veces:

| Fallos | Ayuda |
|---|---|
| 3 | (Movimientos extra desactivados en código) |
| 4-7 | Reduce un color del tablero (probabilidad variable `m_CrazyColorProb`) |
| 7 | Elimina completamente un color |
| 8-10 | Elimina un color y reduce otro (probabilidad variable) |
| 11+ | Elimina dos colores completamente |

Los colores eliminados no pueden ser colores que sean objetivo de misión.

---

## 17. Sistema de Hints y Shuffling

### 17.1. Hints

En `WaitStep.Step_Process()`:
- Tras 5 segundos de inactividad, se llama a `HintOffer()`.
- El hint busca un movimiento válido y lo sugiere visualmente.
- No se muestra durante tutoriales.

### 17.2. Gravedad visual

Cada 8 segundos en `WaitStep` se muestra un indicador de la dirección de gravedad (`ShowGravity()`), excepto cuando se usa gravedad estándar.

### 17.3. Shuffling

En `WaitStep.Step_Play()`:
- `ShufflingCheck()` verifica si existe al menos un movimiento válido.
- Si no hay movimientos, transiciona a `MatchState.Shuffling`.
- Las piezas se recolocan aleatoriamente manteniendo los mismos tipos y colores.
- Si tras el shuffle sigue sin haber movimientos posibles, se declara `ShufflingOver` (derrota).

---

## 18. Object Pool

**Clase:** `ObjectPool : MonoBehaviour` (Singleton, sealed)

Todas las piezas, paneles y efectos visuales se gestionan mediante un **pool de objetos** para evitar instanciaciones y destrucciones constantes.

### 18.1. API principal

| Método | Descripción |
|---|---|
| `CreatePool(prefab, count)` | Pre-instancia `count` copias del prefab y las desactiva. Las almacena en un `Dictionary<GameObject, List<GameObject>>` indexado por el prefab original. |
| `GetObject(prefab, parent)` | Obtiene un objeto inactivo del pool para ese prefab. Si no hay disponibles, instancia uno nuevo y lo añade al pool. Lo activa y lo reparenta bajo `parent`. |
| `GetObject<T>(prefab, parent)` | Igual que `GetObject` pero devuelve el componente `T` del objeto. |
| `Restore(gameObject)` | Devuelve un objeto al pool: lo desactiva y lo reparenta bajo el transform del ObjectPool. |
| `Restore_Obj(prefab)` | Devuelve **todas** las instancias de un prefab específico al pool. |
| `Restore_All()` | Devuelve todos los objetos de **todos** los pools al estado inactivo, excepto los `Board` (que se reutilizan entre partidas). |

### 18.2. Estructura interna

```csharp
Dictionary<GameObject, List<GameObject>> objectPools;
```

- **Clave**: El prefab original (referencia al asset).
- **Valor**: Lista de todas las instancias creadas para ese prefab.
- Un objeto está **disponible** si `activeSelf == false`.
- Un objeto está **en uso** si `activeSelf == true`.

### 18.3. Uso en el gameplay

| Sistema | Prefabs pooled | Pre-instancias |
|---|---|---|
| **EffectManager** | `DefaultEffect` | 40 |
| **ItemManager** | `NormalItem`, `BombItem`, `LineXItem`, etc. | Variable |
| **PanelManager** | Todos los prefabs de panel | Bajo demanda |
| **MatchManager** | `Board` | 81 (se reutilizan, nunca se devuelven al pool) |

### 18.4. Ciclo de vida

```
Inicio partida:
  CreatePool(DefaultEffect, 40)   ← EffectManager.Start()
  CreatePool(NormalItem, ...)      ← ItemManager.Init_ObjectPool_Item()

Durante la partida:
  GetObject(NormalItem, Field)     ← Board.GenItem()
  ...pieza se destruye...
  Restore(normalItemInstance)      ← Item.Brust() → ObjectPool.Restore()

Fin de partida:
  Restore_All()                    ← Devuelve todo excepto Boards
```

Esto es crítico para el rendimiento, ya que en cada turno pueden crearse y destruirse decenas de piezas y efectos.

---

## 19. Sistema de efectos visuales (EffectManager)

**Clase:** `EffectManager : MonoBehaviour` (Singleton)

El `EffectManager` centraliza la creación de todos los efectos de partículas y animaciones visuales durante la partida. Cada efecto se obtiene del `ObjectPool`.

### 19.1. Tipos de efecto

| Método | Efecto | Prefab |
|---|---|---|
| `BrustEffect(pos, color)` | Explosión de pieza normal | `DefaultEffect` + `PieceEffect` |
| `BrustEffect_NormalPiece(pos, color)` | Explosión con piezas de color | `DefaultEffect` + `NormalEffect` |
| `BombEffect(pos, color)` | Explosión de bomba (partículas coloreadas) | `BombEffect` |
| `BonusBombEffect(pos, color)` | Explosión de bomba bonus | `BonusBombEffect` |
| `BigShotEffect(pos, color)` | Explosión grande (5 en línea) | `BigShotEffect` |
| `LineEffect(pos, dir)` | Rayo de línea horizontal/vertical | `LineEffect` |
| `LineBombEffect(pos)` | Efecto combinado línea+bomba | `LineBombEffect` |
| `ButterflyEffect(transform, color)` | Estela de mariposa | `ButterflyEffect` |
| `SwitchEffect(pos)` | Intercambio de piezas | `DefaultEffect` |
| `LollyCageEffect(pos)` / `IceCageEffect` / `BottleEffect` | Rotura de jaulas | Prefabs dedicados |
| `BreadEffect(pos)` / `CrackerEffect` / `CakeEffect` | Rotura de obstáculos | Prefabs dedicados |
| `JamEffect(pos)` | Expansión de jalea | `JamEffect` |
| `IceCreamEffect(pos)` | Rotura de helado | `IceCreamEffect` |
| `JewelTreeEffect(pos)` | Rotura de árbol de joyas | `JewelTreeEffect` |
| `MagicColorEffect(pos)` | Cambio de color mágico | `MagicColorEffect` |
| `ChameleonEffect(pos)` | Cambio de camaleón | `ChameleonEffect` |
| `CashItemEffect(pos)` | Uso de ítem de tienda | `CashItemEffect` |
| `HammerEffect(pos)` | Efecto de martillo | `HammerEffect` |

### 19.2. Mapeo de colores a partículas

Los efectos usan un mapeo fijo de `ColorType` a `Color32`:

| ColorType | Color RGBA |
|---|---|
| `RED` | (250, 100, 100, 255) |
| `YELLOW` | (200, 200, 50, 255) |
| `GREEN` | (100, 250, 100, 255) |
| `BLUE` | (100, 100, 250, 255) |
| `PURPLE` | (150, 100, 200, 255) |
| `ORANGE` | (250, 150, 50, 255) |

---

## 20. Ítems de tienda (Cash Items)

Durante la partida, el jugador puede usar ítems comprados o ganados. El `TouchState` cambia a `CashItemUse` y el jugador selecciona una celda objetivo.

### 20.1. Tipos de Cash Item

| Prefab | Nombre | Efecto |
|---|---|---|
| `Cash_Hammer` | Martillo | Destruye una pieza individual al tocarla |
| `Cash_Bomb` | Bomba | Destruye un área alrededor de la pieza seleccionada |
| `Cash_Thunder` | Rayo | Destruye toda una fila o columna |

### 20.2. Flujo de uso

1. El jugador pulsa el botón del ítem en la barra inferior (`BottomUI`).
2. `MatchManager.m_TouchState` cambia a `CashItemUse`.
3. El jugador toca una pieza/celda del tablero.
4. `Item.OnMouseDown()` detecta `CashItemUse` y ejecuta la habilidad correspondiente vía `AbilityManager`.
5. El ítem se consume del inventario del jugador.
6. `m_TouchState` vuelve a `Switching`.

### 20.3. Equivalencias de boosters en otras ramas

| Nomenclatura principal | Nomenclatura alternativa | Equivalencia |
|---|---|---|
| `Cash_Hammer` | `BoosterMagicHammer` | Golpe directo sobre ficha u obstáculo |
| `Cash_Bomb` | `BoosterCandyPack` o booster explosivo equivalente | Limpieza localizada con animación especial |
| `Cash_Thunder` | `BoosterHBomb` / `BoosterVBomb` | Barrido horizontal o vertical inmediato |
| shuffle de UI | `BoosterShuffle` | Reordenación del tablero |

La interfaz concreta puede variar entre ramas, pero el set funcional de boosters del mismo juego es equivalente.

---

## 21. Resumen de prefabs del gameplay

Todos los prefabs del gameplay se encuentran en `Assets/Prefab/`. Se organizan en estas categorías:

| Categoría | Ejemplos | Cantidad |
|---|---|---|
| **Tablero** | `Board` | 1 |
| **Ítems (piezas)** | `NormalItem`, `BombItem`, `LineXItem`, `ButterFly`, `Donut`, etc. | ~20 |
| **Paneles** | `DefaultFullPanel`, `IceCagePanel`, `JamPanel`, `ConveyerBelt`, etc. | ~28 |
| **Efectos visuales** | `DefaultEffect`, `BombEffect`, `LineEffect`, `PieceEffect`, etc. | ~30 |
| **Cash Items** | `Cash_Hammer`, `Cash_Bomb`, `Cash_Thunder` | 3 |
| **UI en gameplay** | `GetMissionItem`, `MoveWarnning`, `BonusStar`, `BonusCircle`, `Mask_UI` | ~5 |
| **Misceláneos** | `GateWay`, `GetStarEffect`, `CircleEffect`, `ItemChangeEffect` | ~10 |

**Total**: ~186 prefabs en `Assets/Prefab/`

---

