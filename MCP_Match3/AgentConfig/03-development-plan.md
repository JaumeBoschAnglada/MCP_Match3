# MCP Match3 — Plan de Desarrollo (Desde Cero)

> Se construye TODO nuevo. Solo se conserva el sistema de popups.
> Cada fase produce un juego que compila y es jugable antes de avanzar.

---

## Fase 0: Limpieza Total + Estructura Base ✅ COMPLETADA

### Objetivo: Borrar todo lo viejo, crear estructura de carpetas y enums.

### Tareas de LIMPIEZA:
- [x] Eliminar scripts que NO se conservan (23 archivos eliminados)
- [x] Eliminar prefabs viejos: `Pieces/*.prefab` (12 prefabs)
- [x] Eliminar materiales viejos: `Materials/*.mat` (5 materiales)
- [x] Eliminar datos viejos: `Resources/Levels/level_1.json`
- [x] Eliminar carpetas vacías resultantes
- [x] Eliminar InputSystem_Actions.inputactions (no usamos InputSystem)

### Tareas de CREACIÓN:
- [x] Crear estructura de carpetas
- [x] **Enums.cs** — Todos los enums del juego (14 enums)
- [x] **Stage.cs** — Clase serializable con Newtonsoft.Json annotations
- [x] **HallManager.cs** — Singleton simple para cargar escena Gameplay
- [x] Instalar com.unity.nuget.newtonsoft-json 3.2.2
- [x] Arreglar PausePopup.cs (eliminar dependencia de GameManager eliminado)

### Criterio de "hecho": ✅
- Proyecto compila sin errores en Unity y VS
- Carpetas creadas listas para recibir código

---

## Fase 1: Board + MatchManager + ObjectPool ✅ COMPLETADA

### Objetivo: Crear el tablero como grid de 81 celdas individuales MonoBehaviour.

### Scripts NUEVOS:
- [x] **ObjectPool.cs** (Match3.Core)
  - Sealed MonoBehaviour Singleton
  - `Dictionary<GameObject, List<GameObject>> objectPools`
  - CreatePool(prefab, count), GetObject(prefab, parent), GetObject<T>(prefab, parent)
  - Restore(gameObject), Restore_Obj(prefab), Restore_All()
  - Objeto disponible = activeSelf == false
- [x] **Board.cs** (Match3.Core)
  - MonoBehaviour, 1 instancia por celda
  - X, Y, índice lineal
  - Propiedades de navegación: Top, Bottom, Left, Right, diagonales
  - Indexador por SQR_DIR y DROP_DIR
  - m_Item, m_ListPanel, flags (m_DropAnim, m_ItemBrusting, m_MatchingCheck, etc.)
  - PossibleDrop_Dirs[], SeleteDropIndex
  - Init(x, y, stage) — configura celda desde datos de nivel
- [x] **MatchManager.cs** (Match3.Core)
  - MonoBehaviour Singleton, cerebro del gameplay
  - m_ListBoard (Board[81])
  - m_AppearColor (List<ColorType>) — colores activos del nivel
  - m_CSD (Stage) — datos del nivel actual
  - MatchState, StepType, TouchState
  - BoardCreate() → instancia 81 Board
  - BoardSetting(stage) → configura cada celda
  - BoardPositionSetting() → centra y escala tablero
  - SetStep(StepType) → cambia step activo
  - SetMatchState(MatchState)
  - GetRandomColor()
  - m_ListDropStart, m_ListDropHead
  - ComboCnt
  - Nota: PanelSetting, ItemSetting, Switching, MatchBrust, Update+Steps se añadirán en fases posteriores
- [x] **GravityDisplayer.cs** (Match3.Core)
  - MonoBehaviour por Board, muestra flecha de gravedad

### Prefabs NUEVOS:
- [x] **Board.prefab** — Board + GravityDisplayer + hijo Visual (SpriteRenderer)

### Escena:
- [x] Reconfigurar Gameplay.unity desde cero:
  - CommonManager con ObjectPool
  - MatchManager con Field hijo (contenedor de boards)
  - Main Camera (MainCamera tag)
  - Directional Light para iluminación URP
  - GameplayCanvas (Unity UI Canvas + CanvasScaler)
  - SoundManager
  - EventSystem

### Datos de test:
- [x] Resources/Levels/1.json — Nivel de prueba

### Criterio de "hecho": ✅
- Se crean 81 celdas Board en runtime
- Se pueden marcar como Full/Empty desde datos
- El tablero se centra en pantalla
- ObjectPool funciona (create/get/restore)

---

## Fase 2: Item System + Input ✅ COMPLETADA

### Objetivo: Sistema completo de piezas con input drag-to-swap.

### Scripts NUEVOS:
- [x] **Item.cs** (Match3.Items) — Clase base abstracta
  - MonoBehaviour con BoxCollider (3D) + visual hijo
  - Propiedades virtuales: Match, Switch, Drop, m_Brust, ArountBrust, HaveNextItem
  - m_Color, m_ItemType, m_Board, m_CombineType, m_MatchMgr
  - Variables estáticas: Swap_A, Swap_B, SwitchStart, SwitchingTouch
  - **OnMouseDown()** — Valida MatchState==Playing, StepType==Wait, !SwitchStart
    → Registra Swap_A = this, SwitchStart = true
  - **OnMouseEnter()** — Si SwitchStart && Swap_A != this && CheackNeighbor(this)
    → Swap_B = this, SwitchStart = false, SwitchingTouch = true
    → m_MatchMgr.Switching(Swap_A, Swap_B)
  - **OnMouseUp()** — SwitchStart = false
  - CheackNeighbor(item) — Verifica 4 direcciones cardinales via m_Board[SQR_DIR]
  - Init(), Brust(Complete), CombineBrust(), CheckCombine()
  - MissionApply(), SetColor(), SetColorRandom(), GetDestoryScore(combo)
- [x] **NormalItem.cs** (Match3.Items) — Pieza básica de color
- [x] **ItemManager.cs** (Match3.Items) — Factoría Singleton
  - Mapping ItemType → Prefab
  - CreateItem(ItemType, ColorType) → GetObject del pool
  - Init_ObjectPool_Item() → pre-instanciar piezas
  - Intervalos de spawn de ítems especiales
  - IsCreate*() — comprueba si se puede generar un ítem especial

### Prefabs NUEVOS:
- [x] **NormalItem.prefab** — NormalItem + BoxCollider + Visual/Sprite(SpriteRenderer)
  - Estructura: NormalItem → Visual (escala 0.85) → Sprite
- [x] Sprites/materiales para 6 colores (RED, YELLOW, GREEN, BLUE, PURPLE, ORANGE)

### En Board.cs (añadir):
- [x] GenItem(itemType, colorType) → crea y posiciona Item en la celda
- [x] TopSpawnItem() → genera pieza nueva en celdas superiores

### En MatchManager.cs (añadir):
- [x] ItemSetting() → genera items iniciales en todas las celdas activas
- [x] Switching(Item A, Item B) → inicia coroutine de intercambio
- [x] Coroutine_Switching(A, B) — Lógica completa de intercambio:
  - Swap temporal con animación Vector3.Lerp
  - CheckCombine para items especiales (placeholder)
  - Revierte si no hay match (detección de match en Fase 3)
  - Animación de movimiento fluida

### Criterio de "hecho": ✅
- Piezas normales se colocan en el tablero con 6 colores
- El jugador puede tocar y arrastrar para intercambiar
- El intercambio se revierte si no hay match
- Input funciona con mouse Y touch (cross-platform)
- Estructura Visual permite controlar escala independiente del collider
- **InputManager** implementado para soporte Android/iOS

### Configuración de Input:
- [x] **InputManager.cs** creado con soporte mouse + touch
- [x] Active Input Handling cambiado a "Both" en Player Settings
- [x] Input funciona en Editor (mouse) y móviles (touch)
- [x] Detección por raycast 3D desde cámara
- [x] Direcciones cardinales (arriba/abajo/izquierda/derecha)
- [x] Distancia mínima de drag (20px PC, 30px móvil)

---

## Fase 3: Match Detection + Explosiones + Gravedad Básica ✅ COMPLETADA

### Objetivo: Detectar matches, explotar piezas, aplicar gravedad y rellenar.

### En Board.cs (añadir):
- [x] FindMatchesHorizontal() — recursivo, busca izquierda y derecha
- [x] FindMatchesVertical() — recursivo, busca arriba y abajo
- [x] FindMatchesSquare() — matches 2×2 en las 4 esquinas
- [x] FindMatchesAround(maxRecursion) — busca en las 4 direcciones
- [x] FindMatches_Dir_Recursive(color, ref list, dir) — búsqueda recursiva por dirección
- [x] Brust() / Co_Brust() — Coroutine de explosión:
  - m_ItemBrusting = true
  - Item.Brust(callback) → efecto visual (placeholder Phase 10)
  - Callback: PanelBrust() → AroundBrust() → MissionApply() → ScoreApply() (placeholders Phase 6-7)
  - Genera NextItem si corresponde → m_ItemBrusting = false
- [x] GravityDropItemRow(Board) — Caída recursiva (gravedad estándar U)
  - Busca celdas vacías
  - Mueve piezas de arriba hacia abajo
  - Genera piezas nuevas en celdas superiores (TopSpawnItem)
- [x] ItemDrop(Board destino) — Animación de caída con DropAnim flag
- [x] Co_ItemDropAnimation() — Coroutine de animación de caída con easing
- [x] SpecialItemCondition() — Decide qué pieza especial crear:
  - 3 en línea → Nada
  - 4 horizontal → Line_Y
  - 4 vertical → Line_X
  - L o T → Line_C (Cross)
  - 2×2 → Bomb
  - 5 en línea → Rainbow

### En MatchManager.cs (añadir):
- [x] CheckMatchCondition() — Itera por prioridad, busca matches, marca celdas
  - Prioridad 1: 2×2 square → Bomb
  - Prioridad 2: 5+ en línea → Rainbow
  - Prioridad 3: 4 en línea o L/T → Line_X/Line_Y/Line_C
  - Prioridad 4: 3 en línea → Normal match
- [x] Co_MatchBurst() — Coroutine: Ejecuta Brust() en paralelo en todas las celdas marcadas
- [x] MatchBrust() — Versión síncrona de explosión
- [x] Co_Drop() — Coroutine: Aplica gravedad, espera animaciones, verifica completitud
- [x] Drop() — Versión síncrona de gravedad
- [x] Integración con Coroutine_Switching():
  - Detecta matches después del swap
  - Ejecuta secuencia: CheckMatch → Burst → Drop → Cascading
  - Sistema de combo (ComboCnt++)
  - Revierte swap si no hay match

### Criterio de "hecho": ✅
- Se detectan matches de 3, 4, 5, L, T, cuadrado
- Las piezas explotan (sin efectos visuales por ahora - Phase 10)
- Las piezas caen por gravedad con animación
- Se generan piezas nuevas desde arriba
- Las reacciones en cadena (cascading) funcionan
- El sistema determina qué pieza especial crear (aunque no se instancien aún - Phase 5)

### ⚠️ Correcciones pendientes (detectadas en auditoría — prioridad alta):
- [x] **`Board.TopSpawnItem()`** — Aplicar filtro `stage.defaultSpawnLine[X]` antes de generar cualquier pieza. Si `isUseGravity == true`, aplicar también `stage.defaultSpawnLineY[Y]`. Si no pasa el filtro, retornar sin generar nada. ✅
- [ ] **Cadena de prioridad de spawn** en `TopSpawnItem()` — Requiere items de Fase 9. Anotado como `TODO Phase 9` en el código. Prioridad:
  1. Comida de misión (`MissionManager.CreatFoodItem`)
  2. Espiral (`IsCreateSpiral`)
  3. Donut (`IsCreateDonut`)
  4. Bomba de tiempo (`IsCreateTimeBomb`)
  5. Misterio (`IsCreateMystery`)
  6. Camaleón (`IsCreateChameleon`)
  7. Llave (`IsCreateKey`)
  8. Normal (fallback)

---

## Fase 4: Steps (Máquina de Estados del Turno) ✅ COMPLETADA

### Objetivo: Toda la secuencia Wait → Matching → ... → Mission → Wait

### Scripts NUEVOS:
- [x] **BaseStep.cs** (Match3.Steps)
- [x] **WaitStep.cs**
- [x] **MatchingStep.cs**
- [x] **TimeBombStep.cs** — stub
- [x] **IceCreamStep.cs** — stub
- [x] **ConveyerBeltStep.cs** — stub
- [x] **ChameleonStep.cs** — stub
- [x] **MagicColorStep.cs** — stub
- [x] **BearJumpStep.cs** — stub
- [x] **BearSpawnStep.cs** — stub
- [x] **MissionStep.cs**
- [x] **ClearStep.cs**
- [x] **FailStep.cs**
- [x] **ShufflingStep.cs** — stub

### En MatchManager.cs (añadido):
- [x] Dictionary<StepType, BaseStep> m_DicStep
- [x] StepInit() — crea todos los steps
- [x] SetStep(StepType) — llama Step_Play() del nuevo step
- [x] Update() — llama Step_Process() del step activo

### Criterio de "hecho": ✅
- Flujo completo: Wait → Matching → TimeBomb→…→ Mission → Wait funciona
- Múltiples turnos consecutivos OK
- Steps stub avanzan inmediatamente hasta Mission → Wait

---

## Fase 5: Piezas Especiales + Combinaciones ✅ COMPLETADA

### Scripts NUEVOS (Items):
- [x] **LineXItem.cs** — Destruye fila completa (generado por 4 en vertical, perpendicular)
- [x] **LineYItem.cs** — Destruye columna completa (generado por 4 en horizontal, perpendicular)
- [x] **LineCItem.cs** — Destruye fila + columna (generado por T o +)
- [x] **BombItem.cs** — Destruye área 3×3 (generado por L-shape esquina)
- [x] **RainbowItem.cs** — Destruye todas de un color (generado por 5 en línea)
- [x] **ButterflyItem.cs** — Vuela a 3 piezas aleatorias del mismo color y las destruye (generado por 2×2)

### Prefabs NUEVOS:
- [x] LineXItem.prefab, LineYItem.prefab, LineCItem.prefab, BombItem.prefab, RainbowItem.prefab, ButterflyItem.prefab
- [x] Sprites procedurales en Assets/Art/Sprites/Items/Special/

### Integración:
- [x] ItemManager.cs — 6 prefab slots asignados en escena (Normal, Butterfly, LineX, LineY, LineC, Bomb, Rainbow)
- [x] Board.Co_Brust / Brust — spawna la pieza especial tras el burst si m_NextItemType != None
- [x] CheckMatchCondition — prioridades correctas según documentación:
  - P6: 5+ en línea → Rainbow
  - P5: L-shape (esquina, bits 5/6/9/10) → Bomb
  - P4: T o + → Line_C
  - P3: 4 en vertical → Line_X (perpendicular, destruye fila)
  - P2: 4 en horizontal → Line_Y (perpendicular, destruye columna)
  - P1: 2×2 cuadrado → Butterfly
  - P0: 3 en línea → Normal (sin especial)

### Criterio de "hecho": ✅
- Matches de 4+ generan piezas especiales visuales en el tablero
- Cada tipo explota con su efecto correcto (fila, columna, cruz, 3×3, color)
- Prefabs con sprites diferenciados por tipo

### ⚠️ Correcciones pendientes (detectadas en auditoría — prioridad alta):
- [ ] **`Item.CheckCombine()`** — Actualmente vacío en la base. Implementar override en cada ítem especial para combinaciones al hacer swap contra otro especial. Pares a cubrir:
  - `LineX/Y` + `LineX/Y` → destruye fila completa + columna completa (cruz en el destino)
  - `LineX/Y` + `Bomb` → activa la línea 3 veces (fila, columna y diagonal)
  - `Bomb` + `Bomb` → área de explosión 5×5
  - `Rainbow` + `Normal` → destruye todas las piezas del color del Normal
  - `Rainbow` + `LineX/Y` → convierte cada pieza del color en un LineX/Y y las activa
  - `Rainbow` + `Bomb` → convierte cada pieza del color en una Bomb y las activa
  - `Rainbow` + `Rainbow` → destruye todo el tablero
- [ ] **`Coroutine_Switching()`** en MatchManager — Leer `m_CombineType` de ambos ítems tras `CheckCombine()` y derivar al burst de combinación en vez del normal si hay tipo de combinación activo

---

## Fase 6: Misiones + Victoria/Derrota ✅ COMPLETADA

### Objetivo: Sistema completo de objetivos y fin de partida.

### Scripts NUEVOS:
- [x] **MissionManager.cs** (Match3.Managers)
  - Singleton
  - MissionSetting(stage) — carga misiones desde JSON + inicializa TopUI
  - MissionApply(ItemType, ColorType) — notificación de pieza destruida
  - CheckMissionClear() — ¿todos los objetivos cumplidos? (false si sin misiones)
  - CheckMissionFail() — ¿sin movimientos?
  - MoveLimitApply() — resta 1 movimiento
  - ScoreStar1/2/3 — umbrales de estrellas desde stage

### UI NUEVA (Unity UI — Canvas):
- [x] TopUI — Panel en GameplayCanvas: misiones (iconos + contadores), movimientos restantes, puntuación
- [x] MissionPopup — script creado (instancia en escena pendiente para Fase 12)
- [x] VictoryPopup — vía PopupManager, score + estrellas
- [x] DefeatPopup — vía PopupManager, motivo de derrota
- [x] PopupType.cs ampliado (Pause, Mission, Victory, Defeat)
- [x] PopupManager — singleton stackable, auto-registra hijos PopupBase
- [x] PopupBase — lifecycle Show/Close con callbacks
- [x] PausePopup + PauseButton — recreados

### Fixes aplicados:
- [x] ClearStep y FailStep llaman SetMatchState(GameClear/GameFail) → bloquea input
- [x] Swap fallido va directo a Wait (sin pasar por MissionStep → no consume movimiento)
- [x] CheckMissionClear retorna false con lista vacía (evita victoria falsa)
- [x] TopUI se inicializa desde MissionSetting (timing correcto)

### Criterio de "hecho": ✅
- Se pueden configurar misiones desde JSON (OrderN: recoger X de color Y)
- Contador de movimientos funcional (solo baja en swaps con match)
- Victoria al completar objetivo (popup + input bloqueado)
- Derrota al quedarse sin movimientos (popup + input bloqueado)
- UI muestra objetivos y progreso
- MissionDisplays instanciados dinámicamente (1 por misión del nivel, sin límite fijo)

### Pendiente (Fase 12 — Iconos de misión):
- MissionDisplay.missionIcon actualmente sin sprite asignado
- Necesita un sistema de mapping MissionKind → Sprite (ScriptableObject o atlas)
- SetMission() debe buscar el sprite del color/tipo de misión y asignarlo al Image

---

## Fase 7: Paneles (Modificadores de Celda) ✅ COMPLETADA

### Objetivo: Sistema de paneles apilables sobre celdas.

### Scripts NUEVOS:
- [x] **Panel.cs** (Match3.Panels) — Base abstracta
  - Defence, Value, addData
  - Virtuales: ItemExist, ItemDrop, ItemMatch, ItemSwitch
  - Brust() — virtual, destruye una capa
- [x] **DefaultFullPanel.cs** — Celda jugable
- [x] **DefaultEmptyPanel.cs** — Celda inexistente
- [x] **CreatorEmptyPanel.cs** — Genera piezas sin ser visible
- [x] **FixedPanel.cs** — Bloque indestructible, bloquea caída
- [x] **IceCagePanel.cs** — Jaula de hielo (1-2 capas)
- [x] **BottleCagePanel.cs** — Jaula de botella
- [x] **LollyCagePanel.cs** — Jaula de piruleta
- [x] **BreadPanel.cs** — Obstáculo destructible
- [x] **CrackerPanel.cs** — Obstáculo destructible
- [x] **WaferFloorPanel.cs** — Suelo que se destruye con matches encima
- [x] **PanelManager.cs** (Match3.Managers) — Factoría Singleton
  - List_Panel: PanelType → Prefab mapping
  - CreatePanel(type, parent) → GetObject del pool

### Prefabs NUEVOS (uno por tipo):
- [x] DefaultFullPanel.prefab, FixedPanel.prefab, IceCagePanel.prefab, etc. (set base de Fase 7)

### En MatchManager.cs:
- [x] PanelSetting(stage) — crea paneles desde stage.panels[]

### Criterio de "hecho":
- ✅ Celdas con jaulas bloquean interacción
- ✅ Jaulas se destruyen al hacer match adyacente (con defence)
- ✅ Paneles apilados funcionan para el subset implementado de la fase
- ✅ Bloques fijos bloquean caída

### Nota de cierre de fase:
- La fase se considera cerrada para el subset implementado en este repo: `Fixed`, `Ice_Cage`, `Bottle_Cage`, `Lolly_Cage`, `Bread`, `Cracker`, `Wafer_floor`, `Creator_Empty`, `Default_Full` y `Default_Empty`.
- Los paneles avanzados restantes de la referencia (`Jam`, `Ring`, `MagicColor`, `IceCream`, `Cake`, `JewelTree`, etc.) se mueven a fases posteriores porque dependen de otras mecanicas aun no implementadas.

---

## Fase 8: Gravedad Configurable 🟡 EN CURSO

### Objetivo: Gravedad multidireccional (U/D/L/R) por celda.

### En Board.cs (ampliar):
- [x] PossibleDrop_Dirs[] — Múltiples direcciones posibles desde Stage.DropDirs
- [x] ChangeDropDir() — Elige dirección aleatoria de las disponibles
- [x] SideDrop() — Busca piezas en diagonal relativa a la dirección de caída
- [x] Soporte para DROP_DIR.List → isListDrop

### En MatchManager.cs (ampliar):
- [x] m_ListDropStart — Celdas que son puntos de inicio de caída
- [x] m_ListDropHead — Cabeceras de columna de caída
- [x] GetBoardDropStartSetting() — Calcula drop starts
- [x] GetGravitySetting() — Calcula drop heads
- [ ] **TicTok** — Alternar la dirección de procesamiento de `m_ListDropStart` en cada llamada a `Co_Drop()` para evitar asimetría en gravedad lateral (flag booleano `m_TicTok` que invierte el orden de iteración)

### En GravityDisplayer.cs (completar):
- [x] `Show()` / `Hide()` — Muestra/oculta la flecha y rota según `CurrentDropDir`
- [ ] **`Board.cs`** — Llamar a `GetComponent<GravityDisplayer>().Init(this)` durante `Init()` para conectar el displayer a su celda
- [ ] **`InputManager.cs`** — Llamar `GravityDisplayer.Show()` en la celda tocada al iniciar drag si `isUseGravity == true` ; `Hide()` al soltar

### Paneles NUEVOS:
- [ ] **WarpInPanel.cs** — Portal de entrada: guarda referencia a su WarpOutPanel; en la ruta de caída redirige la pieza al WarpOutPanel
- [ ] **WarpOutPanel.cs** — Portal de salida: recibe piezas del WarpInPanel; integrar en `GravityDropItemRow()` como fuente alternativa
- [ ] Conectar flags `m_IsWarpInBoard` / `m_IsWarpOutBoard` / `m_WarpBoard` en Board.cs con los paneles creados

### Criterio de "hecho":
- 🟡 Niveles con gravedad lateral: base de datos, navegacion y side-drop implementados; validacion completa pendiente
- 🟡 Bifurcaciones (celdas con múltiples direcciones): soporte parcial, falta validacion con niveles dedicados
- [ ] Warps teletransportan piezas
- 🟡 Indicador visual de gravedad: `Show()`/`Hide()` implementados, init y activación por input pendientes

### Pendiente real para cerrar la fase:
1. **TicTok** — Añadir alternancia de iteración en `Co_Drop()` (flag `m_TicTok`)
2. **GravityDisplayer conectado** — Llamar `Init(this)` desde `Board.Init()` ; activar/desactivar desde `InputManager`
3. **WarpInPanel + WarpOutPanel** — Crear scripts, registrar en `PanelManager`, integrar en ruta de caída de `GravityDropItemRow()`
4. **Validación end-to-end** — Nivel `level_fase8.json` con gravedad lateral y warps reales

---

## Fase 9: Mecánicas Avanzadas (Steps Post-Match) 🔲 PENDIENTE

### Objetivo: Implementar TODAS las mecánicas post-match.

### Items NUEVOS:
- [ ] **DonutItem.cs** — Obstáculo que se destruye con match adyacente
- [ ] **SpiralItem.cs** — Obstáculo periódico
- [ ] **TimeBombItem.cs** — Contador, game over si llega a 0
- [ ] **ChameleonItem.cs** — Cambia color cada turno
- [ ] **MysteryItem.cs** — Cambia de tipo al destruirse
- [ ] **JellyBearItem.cs** — Salta hacia arriba cada turno
- [ ] **JellyMon.cs** — Come piezas del match
- [ ] **GhostItem.cs** — Fantasma
- [ ] **KeyItem.cs** — Llave
- [ ] **BonusCrossItem.cs**, **BonusBombItem.cs** — Para Bonus Time
- [ ] **FoodItem.cs** — Piezas de comida para misiones

### Paneles NUEVOS:
- [ ] **JamPanel.cs** — Se expande a celdas adyacentes al hacer match
- [ ] **IceCreamPanel.cs** — Se expande por el tablero cada N turnos
- [ ] **IceCreamCreatorPanel.cs** — Genera helado
- [ ] **ConveyerBeltPanel.cs** — Mueve piezas en una dirección fija
- [ ] **MagicColorPanel.cs** — Cambia color de la pieza cada turno
- [ ] **RingPanel.cs** — Recoge piezas de un color específico
- [ ] **StelePanel.cs**, **SteleHidePanel.cs** — Estelas
- [ ] **CakePanel.cs** (A-D) — Obstáculo multi-capa
- [ ] **JewelTreePanel.cs** (A-D) — Obstáculo multi-capa
- [ ] **FoodArrivePanel.cs** — Destino de comida
- [ ] **JellyBearStartPanel.cs** — Spawn de osos
- [ ] **Creator panels** — CreatorFood, CreatorSpiral, CreatorTimeBomb, CreatorKey, combos

### Steps (implementar lógica real):
- [ ] **TimeBombStep.cs** — Decrementar contadores, detectar game over
- [ ] **IceCreamStep.cs** — Expandir helado cada N turnos
- [ ] **ConveyerBeltStep.cs** — Mover piezas en cintas
- [ ] **ChameleonStep.cs** — Cambiar colores + marcar m_MatchingCheck
- [ ] **MagicColorStep.cs** — Rotar y cambiar color (DOTween)
- [ ] **BearJumpStep.cs** — Saltar osos, rescatar si llegan al borde
- [ ] **BearSpawnStep.cs** — Generar nuevos osos cada N turnos

### Criterio de "hecho":
- Cada mecánica funciona en su step
- Los steps que cambian color/posición marcan m_MatchingCheck
- MissionStep detecta pendientes y vuelve a Matching si es necesario
- Todos los ítems especiales explotan correctamente

---

## Fase 10: Efectos Visuales + EffectManager 🔲 PENDIENTE

### Objetivo: Todos los efectos de partículas y feedback visual.

### Scripts NUEVOS:
- [ ] **EffectManager.cs** (Match3.Managers) — Singleton
  - ObjectPool de ParticleSystems
  - BrustEffect(pos, color) — explosión normal
  - BombEffect(pos, color) — explosión de bomba
  - LineEffect(pos, dir) — rayo de línea
  - ButterflyEffect(transform, color) — estela de mariposa
  - SwitchEffect(pos) — intercambio
  - Efectos de paneles: IceCageEffect, JamEffect, IceCreamEffect, etc.
  - Color mapping: ColorType → Color32
- [ ] **Border.cs** (Match3.Core) — Mesh 2D de borde
  - Marching Squares sobre celdas activas
  - Grosor, redondeo, UV configurables

### Prefabs NUEVOS:
- [ ] DefaultEffect, BombEffect, LineEffect, ButterflyEffect, etc. (~30 prefabs)

### Criterio de "hecho":
- Cada tipo de explosión tiene su efecto visual
- Los colores de partículas corresponden al color de la pieza
- El borde visual del tablero se genera correctamente

---

## Fase 11: BonusTime + Crazy Level + Hints + Audio 🔲 PENDIENTE

### Objetivo: Polish y mecánicas de soporte.

### Tareas:
- [ ] **BonusTime** — Al ganar con movimientos sobrantes:
  - Cada movimiento → pieza especial aleatoria en celda aleatoria
  - Explotan secuencialmente → puntuación adicional
- [ ] **Crazy Level** — Ayuda dinámica según fallos:
  - 4-7 fallos → reduce un color
  - 7+ fallos → elimina un color
  - 11+ fallos → elimina dos colores
- [ ] **Hints** — En WaitStep, tras 5s sin input → HintOffer()
- [ ] **Shuffling mejorado** — Verificación robusta de movimientos posibles
- [ ] **Audio** — SoundManager Singleton (Match3.Managers)
  - MonoBehaviour Singleton con AudioSource(s)
  - Música de fondo (loop), SFX por acción (match, swap, explosión, especial, etc.)
  - PlayEffect(string name), PlayMusic(AudioClip)
  - AudioClips vía [SerializeField] o Resources
- [ ] **Score** — Sistema de puntuación con estrellas (1/2/3 por nivel)

### Criterio de "hecho":
- Bonus Time funciona al ganar
- Crazy Level ayuda al jugador que falla repetidamente
- Hints visuales funcionan
- Audio básico implementado

---

## Fase 12: Carga de Niveles + Contenido 🔲 PENDIENTE

### Objetivo: Crear niveles jugables y sistema de progresión.

### Tareas:
- [ ] **LevelLoader** completo — Deserializa Stage JSON
- [ ] Flujo completo de carga: LevelLoader → MatchManager.StartGame(stage)
- [ ] 10+ niveles de prueba con dificultad progresiva
- [ ] Selector de niveles en Hall
- [ ] Persistencia de progreso (PlayerPrefs o JSON)
- [ ] Sistema de estrellas por nivel

---

## Notas Generales

1. **Cada fase debe compilar** antes de avanzar
2. **NO reutilizar código viejo** — todo se crea nuevo
3. **Solo se conserva** el sistema de popups (PopupManager, PopupBase, etc.)
4. **Prefabs y escena SOLO con MCP** — NUNCA crear scripts Editor generadores de contenido Unity ([MenuItem], EditorWindow, etc.). Si existe alguno, borrarlo.
5. **Probar en Editor** — Simular touch, verificar flujo
6. **Prioridad**: Gameplay funcional > Visual > Contenido
7. **La lógica de gameplay** de los docs TMP Textos Agente/ es la fuente de verdad
8. **Escena 3D** — Tablero y piezas en espacio 3D, cámara 3D
9. **UI con Unity UI** — Canvas + CanvasScaler + Image/TMP/Button. **NO NGUI.**
10. **La jerarquía de escena es propia** — No replicamos la sección 2.1 del doc de referencia
11. **Si MCP no está conectado**, avisar al usuario antes de crear cualquier contenido en Unity
