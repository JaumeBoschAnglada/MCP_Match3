# MCP Match3 — Arquitectura (Desde Cero)

> Toda esta arquitectura se construye nueva. No se reutiliza ningún script existente salvo el sistema de popups.
> La jerarquía de escena es PROPIA (no replica la sección 2.1 del doc de referencia).
> NO se usa NGUI. Toda la UI se hace con Unity UI (Canvas).
> La escena es 3D.

## Jerarquía de la Escena Gameplay

```
Gameplay (Escena 3D)
├── Main Camera               ← Camera 3D (ortográfica o perspectiva) apuntando al tablero
├── Directional Light          ← Iluminación básica URP
├── CommonManager              ← Contenedor de managers compartidos
│   ├── ItemManager            ← Factoría de Items (piezas) via ObjectPool
│   ├── PanelManager           ← Factoría de Panels, mapeo PanelType → Prefab
│   └── ObjectPool             ← Pool de GameObjects reutilizables
├── MatchManager (Singleton)   ← Cerebro del gameplay
│   ├── EffectManager          ← Partículas y efectos visuales
│   ├── MissionManager         ← Gestión de objetivos del nivel
│   ├── AbilityManager         ← Habilidades especiales (mariposa, combinaciones)
│   ├── Field                  ← Contenedor del campo de juego (padre de Board[])
│   │   └── Board[81]          ← Celdas del tablero (generadas en runtime, GameObjects 3D)
│   ├── Background             ← Fondo visual del tablero (quad/plane 3D)
│   └── Border                 ← Borde visual (mesh generado dinámicamente)
├── SoundManager               ← Audio (música y SFX)
├── GameplayCanvas (Canvas)    ← UI de gameplay (Unity UI, ScreenSpace-Overlay o Camera)
│   ├── CanvasScaler            ← ScaleWithScreenSize para adaptación móvil
│   ├── TopUI                  ← Panel superior: misiones, puntuación, movimientos
│   ├── BottomUI               ← Panel inferior: ítems de tienda (futuro)
│   └── UI_Block               ← Panel semitransparente para bloquear input
├── PopupManager (Canvas)      ← [CONSERVADO] Sistema de popups (sort order > GameplayCanvas)
│   └── (PausePopup, MissionPopup, VictoryPopup, DefeatPopup)
└── EventSystem                ← Necesario para Unity UI
```

**Notas sobre la escena 3D:**
- El tablero vive en espacio 3D (GameObjects con MeshRenderer o SpriteRenderer en 3D)
- La cámara apunta al tablero desde arriba/frente
- Las piezas (Items) son GameObjects 3D con Collider (BoxCollider o BoxCollider2D según convenga)
- Los efectos de partículas son ParticleSystems 3D estándar
- La UI está en Canvas aparte, separada del mundo 3D del tablero

## Diagrama de Dependencias Completo

```
MatchManager (Singleton, MonoBehaviour)
    ├── Board[81]              ← Rejilla 9×9 de celdas
    │   ├── Item               ← La pieza en la celda (o null)
    │   ├── Panel[]            ← Modificadores apilados en la celda
    │   └── GravityDisplayer   ← Indicador visual de dirección de gravedad
    │
    ├── ItemManager (Singleton)
    │   ├── ObjectPool         ← Obtiene/devuelve Items
    │   ├── Prefab registry    ← ItemType → Prefab mapping
    │   └── Interval tracking  ← Controla spawn de ítems especiales
    │
    ├── PanelManager (Singleton)
    │   ├── ObjectPool         ← Obtiene/devuelve Panels
    │   └── Prefab registry    ← PanelType → Prefab mapping
    │
    ├── MissionManager (Singleton)
    │   ├── Mission[]          ← Objetivos activos del nivel
    │   ├── CheckMissionClear()
    │   ├── CheckMissionFail()
    │   └── MoveLimitApply()   ← Resta movimiento al jugador
    │
    ├── EffectManager (Singleton)
    │   ├── ObjectPool         ← Obtiene/devuelve efectos
    │   └── Color → Particle mapping
    │
    ├── AbilityManager (Singleton)
    │   └── Combinaciones entre piezas especiales
    │
    └── Dictionary<StepType, BaseStep>  ← Máquina de estados del turno
        ├── WaitStep
        ├── MatchingStep
        ├── TimeBombStep
        ├── IceCreamStep
        ├── ConveyerBeltStep
        ├── ChameleonStep
        ├── MagicColorStep
        ├── BearJumpStep
        ├── BearSpawnStep
        ├── MissionStep
        ├── ClearStep
        ├── FailStep
        └── ShufflingStep
```

## Flujo del Core Loop

```
1. WaitStep activo → el jugador puede tocar
2. Item.OnMouseDown() → registra Swap_A, SwitchStart = true
3. Item.OnMouseEnter() → valida adyacencia (CheackNeighbor)
   → registra Swap_B → MatchManager.Switching(A, B)
4. Coroutine_Switching():
   a. ¿A.CheckCombine(B) o B.CheckCombine(A)?
      SÍ → CombineBrust() → explosión directa
      NO → Intercambio normal:
           i.  Intercambio temporal en datos
           ii. FindMatchesAround() en ambas celdas
           iii. ¿Hay matches?
                NO → Revertir posiciones, NO gastar movimiento
                SÍ → Confirmar swap, boards swap, m_MatchingCheck = true
   b. MoveLimitApply() (resta movimiento)
   c. SetStep(Matching)
5. MatchingStep (loop cada frame):
   a. CheckFood()
   b. Drop() (gravedad + spawn)
   c. CheckMatchCondition() (detecta matches, crea especiales, Brust)
   d. CheckRing()
   e. ¿AllEnd? → SetStep(TimeBomb)
6. Steps post-match secuenciales:
   TimeBomb → IceCream → ConveyerBelt → Chameleon →
   MagicColor → BearJump → BearSpawn → Mission
7. MissionStep decide:
   - ¿Hay m_MatchingCheck pendiente? → vuelve a Matching
   - ¿Victoria? → Clear → BonusTime → GameClear
   - ¿Derrota? → Fail → GameFail
   - ¿Nada? → WaitStep (jugador toca de nuevo)
```

## Namespaces

```
Match3.Core        → Board, MatchManager, ObjectPool, Border
Match3.Data        → Stage, Enums (ColorType, ItemType, PanelType, StepType, etc.)
Match3.Items       → Item (base), NormalItem, BombItem, LineXItem, LineYItem,
                     LineCItem, RainbowItem, ButterflyItem, DonutItem, SpiralItem,
                     TimeBombItem, ChameleonItem, MysteryItem, JellyBearItem,
                     JellyMon, GhostItem, KeyItem, BonusCrossItem, BonusBombItem, FoodItem
Match3.Panels      → Panel (base), DefaultFullPanel, IceCagePanel, JamPanel,
                     ConveyerBeltPanel, WaferFloorPanel, etc.
Match3.Steps       → BaseStep, WaitStep, MatchingStep, TimeBombStep, IceCreamStep,
                     ConveyerBeltStep, ChameleonStep, MagicColorStep, BearJumpStep,
                     BearSpawnStep, MissionStep, ClearStep, FailStep, ShufflingStep
Match3.Managers    → ItemManager, PanelManager, MissionManager, EffectManager,
                     AbilityManager, SoundManager
Match3.UI          → [CONSERVADO] PopupManager, PopupBase, PausePopup, PauseButton, PopupType
                     + TopUI (nuevo), GameplayUI (nuevo)
Match3.Scenes      → HallManager (nuevo)
```

## Estructura de Carpetas

```
Assets/
├── Materials/              → 6 materiales de color + especiales
├── Prefabs/
│   ├── Board/              → Board.prefab (celda individual)
│   ├── Items/              → NormalItem.prefab, BombItem.prefab, LineXItem.prefab, etc.
│   ├── Panels/             → DefaultFullPanel.prefab, IceCagePanel.prefab, etc.
│   ├── Effects/            → DefaultEffect.prefab, BombEffect.prefab, LineEffect.prefab, etc.
│   └── UI/                 → [CONSERVADO] PausePopup.prefab + nuevos popups
├── Resources/
│   └── Levels/             → Archivos JSON (1.json, 2.json, ...)
├── Scenes/
│   ├── Hall.unity          → Menú principal (reconfigurado)
│   └── Gameplay.unity      → Tablero de juego (reconfigurado desde cero)
├── Scripts/
│   ├── Core/               → Board.cs, MatchManager.cs, ObjectPool.cs, Border.cs
│   ├── Data/               → Stage.cs, Enums.cs (todos los enums)
│   ├── Items/              → Item.cs, NormalItem.cs, BombItem.cs, etc.
│   ├── Panels/             → Panel.cs, DefaultFullPanel.cs, IceCagePanel.cs, etc.
│   ├── Steps/              → BaseStep.cs, WaitStep.cs, MatchingStep.cs, etc.
│   ├── Managers/           → ItemManager.cs, PanelManager.cs, MissionManager.cs,
│   │                         EffectManager.cs, AbilityManager.cs
│   ├── UI/                 → [CONSERVADO] PopupManager.cs, etc.
│   └── Scenes/             → HallManager.cs (nuevo)
├── Sprites/                → Sprites 2D de piezas, paneles, UI
└── Settings/               → URP settings
```

## Tipos de Datos Clave

### Board (MonoBehaviour, 1 por celda)
```
Coordenadas: X, Y (0-8), índice lineal = X + Y * 9
Navegación: Top, Bottom, Left, Right, TopLeft, TopRight, BottomLeft, BottomRight
             Acceso por indexador Board[SQR_DIR]
Gravedad: PossibleDrop_Dirs (DROP_DIR[]), SeleteDropIndex, ChangeDropDir()
Estado:
  m_Item          → Item actual (o null si vacía)
  m_ListPanel[]   → Lista de paneles apilados
  m_DropAnim      → Animación de caída en curso
  m_ItemBrusting  → Ítem destruyéndose
  m_PanelBrusting → Panel destruyéndose
  m_MatchingCheck → Pendiente de verificar matches
  m_isMatchBrust  → Marcada para explosión
  m_NextItemType  → Pieza especial a generar tras explosión
  IsPanelFixed    → Panel fijo (bloquea caída)
  IsPanelCage     → Jaula (bloquea interacción)
  m_IsJamBoard    → Jalea activa
  m_IsWarpOutBoard / m_IsWarpInBoard → Portales
Métodos:
  Init(), Brust(), Co_Brust()
  FindMatchesHorizontal(), FindMatchesVertical(), FindMatchesSquare()
  FindMatchesAround(), FindMatches_Dir_Recursive()
  GravityDropItemRow(), SideDrop(), TopSpawnItem()
  CheackNeighbor(), GenItem(), ItemDrop()
  PanelBrust(), AroundBrust()
```

### Item (MonoBehaviour abstracto, 1 por pieza)
```
Propiedades virtuales:
  Match     → ¿Puede participar en match? (default true)
  Switch    → ¿Puede intercambiarse? (default true)
  Drop      → ¿Puede caer? (default true)
  m_Brust   → ¿Puede explotar?
  ArountBrust → ¿Al explotar afecta vecinos?
  HaveNextItem → ¿Genera sucesora al explotar?
Datos:
  m_Color (ColorType), m_ItemType (ItemType)
  m_Board (Board), m_CombineType (ItemType)
  m_MatchMgr (MatchManager)
Variables estáticas:
  Swap_A, Swap_B (Item)
  SwitchStart, SwitchingTouch (bool)
Input (en Item.cs directamente):
  OnMouseDown() → registra Swap_A
  OnMouseEnter() → valida adyacencia, registra Swap_B, llama Switching()
  OnMouseUp() → reset SwitchStart
Métodos virtuales:
  Init(), Brust(Complete), CombineBrust(combinetype, Complete)
  CheckCombine(combinetype), MissionApply()
  SetColor(color), SetColorRandom()
  GetDestoryScore(combo)
```

### Panel (MonoBehaviour abstracto, N por celda)
```
Defence → golpes para destruirlo (-1 = indestructible)
Value → valor adicional configurable
Propiedades virtuales:
  ItemExist → ¿Puede haber Item en esta celda?
  ItemDrop  → ¿Pueden caer Items aquí?
  ItemMatch → ¿Se pueden detectar matches?
  ItemSwitch → ¿Se puede intercambiar?
Métodos virtuales:
  Brust() → destruye una capa del panel
```

### BaseStep (clase base)
```
Step_Init()    → inicialización al inicio de partida
Step_Play()    → se ejecuta UNA VEZ al entrar al step
Step_Process() → se ejecuta CADA FRAME mientras el step está activo
static isItemStep → flag compartido
```

## Prefab Structures (3D)

### Board.prefab
```
Board (Transform, Board.cs, GravityDisplayer.cs)
  └── Visual (Transform, MeshRenderer/SpriteRenderer)  ← Visual de la celda
```
> Nota: El tipo de renderer (MeshRenderer con quad o SpriteRenderer en 3D)
> se decidirá durante la implementación. Lo importante es que vive en espacio 3D.

### Item.prefab (ejemplo: BombItem)
```
BombItem (Transform, BombItem.cs, BoxCollider)
  ├── Visual (Transform, MeshRenderer/SpriteRenderer)  ← Visual de la pieza
  └── Effect (Transform, ParticleSystem)               ← Efecto idle (opcional)
```
> En 3D, los Items usan BoxCollider (no 2D). OnMouseDown/Enter/Up funcionan
> con cualquier Collider si hay un Camera con Physics Raycaster.

## Convenciones de Código
- Grid 9×9 fijo (81 celdas), activas/inactivas por panel Default_Full/Default_Empty
- (0,0) = esquina superior-izquierda, Y+ = abajo, X+ = derecha
- Índice lineal = X + Y * 9, recorrido fila a fila
- Input via OnMouseDown/OnMouseEnter/OnMouseUp en Item (requiere Collider)
- Escena 3D: piezas y tablero en espacio 3D, UI en Canvas aparte
- UI con Unity UI (Canvas + CanvasScaler + Image/TMP/Button). NO NGUI.
- Objetos reciclados via ObjectPool (nunca Destroy en gameplay)
- Todos los managers son Singletons MonoBehaviour
- [SerializeField] para referencias, no FindObjectOfType/GetComponent en runtime
