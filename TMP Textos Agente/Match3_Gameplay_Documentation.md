# Jewels Palace Match 3 — Documentación del Gameplay

> Documentación técnica del sistema de juego Match 3 contenido en la carpeta `ThreematchSinMapa`.
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

---

## 3. Datos de nivel (Stage) — Formato JSON

**Clase:** `Stage` (serializable)  
**Ruta de archivos:** `Assets/Resources_moved/Data/Stage/{número}.txt`  
**Carga:** `DataManager.LoadStage(int stage)` → deserializa JSON con Newtonsoft.Json

Cada nivel es un archivo `.txt` que contiene un objeto JSON serializado de la clase `Stage`. Este archivo define **todo** lo necesario para construir y configurar una partida: la forma del tablero, la gravedad, las piezas iniciales, los paneles, las misiones, las líneas de spawn y los parámetros de los ítems especiales.

### 3.1. Estructura completa del JSON de nivel

```json
{
  "DropDirs": [...],            // Gravedad por celda
  "isUseGravity": true,         // Si usa gravedad personalizada
  "panels": [...],              // Paneles de cada celda (forma del tablero + obstáculos)
  "items": [...],               // Tipo de pieza inicial en cada celda
  "colors": [...],              // Color de la pieza inicial en cada celda
  "focus": [],                  // Índices de celdas para foco visual inicial
  "defaultSpawnLine": [...],    // Líneas X que generan piezas normales
  "foodSpawnLine": [...],       // Líneas X que generan comida
  "SpiralSpawnLine": [...],     // Líneas X que generan espirales
  "DonutSpawnLine": [...],      // Líneas X que generan donuts
  "TimeBombSpawnLine": [...],   // Líneas X que generan bombas de tiempo
  "MysterySpawnLine": [...],    // Líneas X que generan piezas misteriosas
  "ChameleonSpawnLine": [...],  // Líneas X que generan camaleones
  "KeySpawnLine": [...],        // Líneas X que generan llaves
  "appearColor": [...],         // Qué colores aparecen en este nivel
  "scoreStar1": 5000,           // Puntuación para 1 estrella
  "scoreStar2": 10000,          // Puntuación para 2 estrellas
  "scoreStar3": 18000,          // Puntuación para 3 estrellas
  "limit_Move": 30,             // Movimientos disponibles
  "missionType": "OrderN",      // Tipo de misión principal
  "isOrderNMission": true,      // Flags de tipo de misión activos
  "missionInfo": [...],         // Lista de objetivos
  // ... parámetros de spawn de ítems especiales ...
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

Existen **8 pares de arrays** booleanos de 9 elementos cada uno (uno por columna X y otro por fila Y):

| Campo (X) | Campo (Y) | Qué controla |
|---|---|---|
| `defaultSpawnLine[9]` | `defaultSpawnLineY[9]` | ¿Se generan piezas normales en esta columna/fila? |
| `foodSpawnLine[9]` | `foodSpawnLineY[9]` | ¿Se genera comida en esta columna/fila? |
| `SpiralSpawnLine[9]` | `SpiralSpawnLineY[9]` | ¿Se generan espirales en esta columna/fila? |
| `DonutSpawnLine[9]` | `DonutSpawnLineY[9]` | ¿Se generan donuts en esta columna/fila? |
| `TimeBombSpawnLine[9]` | `TimeBombSpawnLineY[9]` | ¿Se generan bombas de tiempo en esta columna/fila? |
| `MysterySpawnLine[9]` | `MysterySpawnLineY[9]` | ¿Se generan piezas misteriosas en esta columna/fila? |
| `ChameleonSpawnLine[9]` | `ChameleonSpawnLineY[9]` | ¿Se generan camaleones en esta columna/fila? |
| `KeySpawnLine[9]` | `KeySpawnLineY[9]` | ¿Se generan llaves en esta columna/fila? |

Cada array tiene 9 booleanos (`true`/`false`), uno por columna (X) o fila (Y).

**Ejemplo:**
```json
"defaultSpawnLine": [true, true, true, true, true, true, true, true, true],
"TimeBombSpawnLine": [false, false, false, true, true, true, false, false, false]
```
En este caso, las piezas normales se generan en todas las columnas, pero las bombas de tiempo solo se generan en las columnas 3, 4 y 5.

**Cómo se usan en el código** (`Board.TopSpawnItem()`):
```csharp
// Una pieza normal solo se genera si la columna X y la fila Y lo permiten
if (!m_CSD.defaultSpawnLine[this.X] || (m_CSD.isUseGravity && !m_CSD.defaultSpawnLineY[this.Y]))
    return false;
```

Para los ítems especiales, la comprobación se hace en `ItemManager.IsCreate*()`:
```csharp
// Ejemplo: espiral
if (!MatchMgr.m_CSD.SpiralSpawnLine[bd.X])
    return false;
```

**Nota:** Los arrays `*SpawnLineY` **solo se usan cuando `isUseGravity == true`** (gravedad personalizada). Con gravedad estándar solo se consulta la columna X.

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

Cada tipo de ítem especial que se genera dinámicamente durante la partida tiene 4 parámetros:

| Parámetro | Significado |
|---|---|
| `*_MinExist` | Nº mínimo que debe haber en el tablero antes de dejar de generar |
| `*_MaxExist` | Nº máximo permitido en el tablero simultáneamente |
| `*_Interval` | Cada cuántos movimientos del jugador se genera uno nuevo |
| `*_SpawnCnt` | Nº máximo de spawns permitidos por ciclo de intervalo |

**Ítems con estos parámetros:**

| Prefijo | Ítem |
|---|---|
| `Spiral_*` | Espirales |
| `Donut_*` | Donuts |
| `TimeBomb_*` | Bombas de tiempo |
| `Mystery_*` | Piezas misteriosas |
| `Chameleon_*` | Camaleones |
| `Key_*` | Llaves |
| `food_*` | Comida (`food_MaxExist`, `food_Interval`, `food_SpawnCnt`) |
| `Bear_*` | Osos (`Bear_MaxExist`, `Bear_Interval`, sin Min/SpawnCnt) |

**Parámetros adicionales:**

| Campo | Tipo | Descripción |
|---|---|---|
| `TimeBomb_FirstCount` | `int` | Valor inicial del contador de las bombas de tiempo (default: 15) |
| `IceCream_Interval` | `int` | Cada cuántos turnos se expande el helado |
| `IceCreamCreator_Interval` | `int` | Cada cuántos turnos el creador genera helado |
| `Mystery_SettingType` | `MysterySettingType` | Dificultad de la pieza misteriosa |
| `S_Tree_SettingType` | `S_TreeSettingType` | Dificultad del árbol |
| `JewelTreeItem[4]` | `ItemType[4]` | Qué ítems salen de los 4 niveles del árbol de joyas |
| `JewelTreeItemColor[4]` | `ColorType[4]` | Color de esos ítems |

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

---

## 6. Sistema de colores

```csharp
public enum ColorType { None, RED, YELLOW, GREEN, BLUE, PURPLE, ORANGE, Rnd }
```

- **6 colores disponibles**, pero cada nivel define cuáles aparecen en `Stage.appearColor[]`.
- `Rnd` indica asignación aleatoria al crear la pieza.
- El sprite se selecciona por índice: `color - ColorType.RED` (0-based).
- Las piezas del tipo `Rainbow` no tienen color asignado y actúan sobre un color elegido al explotar.

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
