# MCP Match3 — Registro de Progreso

> Registro cronológico. Se construye todo desde cero.

---

## Estado Inicial — Punto de Partida del Refactor

### Decisión: DESDE CERO
- NO se reutiliza ningún script, prefab, material ni dato del proyecto actual
- SOLO se conserva el sistema de popups (PopupManager, PopupBase, PausePopup, PauseButton, PopupType)
- Todo lo demás se elimina y se construye nuevo basándose en los documentos de referencia

### Documentos de referencia (fuente de verdad):
- `TMP Textos Agente/Match3_Gameplay_Documentation.md`
- `TMP Textos Agente/Interaccion_Completa_Touch_to_Touch.md`

### Assets a ELIMINAR en Fase 0:
**Scripts (22 archivos):**
- Core/: BoardController.cs, SpecialPieceEffects.cs, SpecialPieceCreator.cs
- Data/: PieceData.cs, LevelData.cs
- Gameplay/: GameManager.cs, Piece.cs, PieceAnimator.cs, HorizontalRowPiece.cs, VerticalRowPiece.cs
- Input/: InputHandler.cs, InputDebugVisualizer.cs
- Effects/: EffectManager.cs
- Animation/: SpecialPieceAnimator.cs
- Scenes/: HallManager.cs, AutoPilot.cs
- Debugging/: TouchInputDiagnostic.cs
- UI/: DebugUI.cs, UIInitializer.cs
- Setup/: PopupPrefabInitializer.cs, PopupManagerCleanup.cs
- Loaders/: LevelLoader.cs

**Prefabs (12):**
- Pieces/: Piece_Red, Piece_Blue, Piece_Green, Piece_Yellow
- Pieces/: Special_Horizontal_Red/Blue/Green/Yellow
- Pieces/: Special_Vertical_Red/Blue/Green/Yellow

**Materiales (5):** PieceRed, PieceBlue, PieceGreen, PieceYellow, PieceOrange
**Datos (1):** Resources/Levels/level_1.json

### Assets que SE CONSERVAN:
- Scripts/UI/: PopupManager.cs, PopupBase.cs, PopupType.cs, PausePopup.cs, PauseButton.cs
- Prefabs/UI/: PausePopup.prefab
- Scenes/: Hall.unity, Gameplay.unity (se reconfiguran)

### Scripts NUEVOS a crear (~50):
**Core (4):** Board.cs, MatchManager.cs, ObjectPool.cs, Border.cs
**Data (2):** Enums.cs, Stage.cs
**Items (20):** Item.cs, NormalItem.cs, LineXItem.cs, LineYItem.cs, LineCItem.cs, BombItem.cs, RainbowItem.cs, ButterflyItem.cs, DonutItem.cs, SpiralItem.cs, TimeBombItem.cs, ChameleonItem.cs, MysteryItem.cs, JellyBearItem.cs, JellyMon.cs, GhostItem.cs, KeyItem.cs, BonusCrossItem.cs, BonusBombItem.cs, FoodItem.cs
**Panels (~15):** Panel.cs, DefaultFullPanel.cs, DefaultEmptyPanel.cs, CreatorEmptyPanel.cs, FixedPanel.cs, IceCagePanel.cs, BottleCagePanel.cs, LollyCagePanel.cs, BreadPanel.cs, CrackerPanel.cs, WaferFloorPanel.cs, JamPanel.cs, IceCreamPanel.cs, ConveyerBeltPanel.cs, MagicColorPanel.cs, etc.
**Steps (13):** BaseStep.cs, WaitStep.cs, MatchingStep.cs, TimeBombStep.cs, IceCreamStep.cs, ConveyerBeltStep.cs, ChameleonStep.cs, MagicColorStep.cs, BearJumpStep.cs, BearSpawnStep.cs, MissionStep.cs, ClearStep.cs, FailStep.cs, ShufflingStep.cs
**Managers (5):** ItemManager.cs, PanelManager.cs, MissionManager.cs, EffectManager.cs, AbilityManager.cs
**Other (2):** GravityDisplayer.cs, HallManager.cs

### Próximo paso: Fase 1 (Board + MatchManager + ObjectPool)

---

## Fase 0: Limpieza Total + Estructura Base ✅ COMPLETADA

### Limpieza ejecutada:
- ✅ Eliminados 23 scripts viejos (22 originales + SpecialPieceCreatorTool.cs del Editor + PopupPrefabInitializer.cs)
- ✅ Eliminados 12 prefabs de piezas (Pieces/)
- ✅ Eliminados 5 materiales (PieceBlue/Green/Orange/Red/Yellow.mat)
- ✅ Eliminado level_1.json
- ✅ Eliminado InputSystem_Actions.inputactions (no usamos InputSystem)
- ✅ Limpiadas carpetas vacías: Core, Data, Gameplay, Input, Effects, Animation, Scenes, Debugging, Setup, Loaders, Editor, Pieces, Materials

### Arreglos en código conservado:
- ✅ PausePopup.cs: eliminada dependencia de `Match3.Gameplay.GameManager`, reemplazada con `SceneManager.LoadScene()` para restart

### Estructura de carpetas creada:
- ✅ Scripts/Core/, Scripts/Data/, Scripts/Items/, Scripts/Panels/
- ✅ Scripts/Steps/, Scripts/Managers/, Scripts/Scenes/
- ✅ Prefabs/Board/, Prefabs/Items/, Prefabs/Panels/, Prefabs/Effects/

### Scripts nuevos creados:
- ✅ **Enums.cs** (Match3.Data) — Todos los enums: ColorType, ItemType, PanelType, StepType, MatchState, TouchState, DROP_DIR, SQR_DIR, MissionType, MissionKind, FailType, MysterySettingType, S_TreeSettingType
- ✅ **Stage.cs** (Match3.Data) — Clase serializable completa con Newtonsoft.Json annotations: Pannels, PanelData, MissionData, CreateGravity()
- ✅ **HallManager.cs** (Match3.Scenes) — Singleton simple con LoadLevel() y CurrentLevel estático

### Paquete instalado:
- ✅ com.unity.nuget.newtonsoft-json 3.2.2 (necesario para Stage.cs)

### Estado del proyecto tras Fase 0:
```
Assets/Scripts/ (8 archivos .cs):
  Data/Enums.cs          ← NUEVO
  Data/Stage.cs          ← NUEVO
  Scenes/HallManager.cs  ← NUEVO
  UI/PauseButton.cs      ← CONSERVADO
  UI/PausePopup.cs       ← CONSERVADO (arreglado)
  UI/PopupBase.cs        ← CONSERVADO
  UI/PopupManager.cs     ← CONSERVADO
  UI/PopupType.cs        ← CONSERVADO

Assets/Prefabs/UI/PausePopup.prefab  ← CONSERVADO
Assets/Scenes/Gameplay.unity         ← CONSERVADO (pendiente reconfigurar)
Assets/Scenes/Hall.unity             ← CONSERVADO (pendiente reconfigurar)
```

### Compilación: ✅ 0 errores (Unity + VS)

---

## Fase 1: Board + MatchManager + ObjectPool ✅ COMPLETADA

### Scripts creados:
- ✅ **ObjectPool.cs** (Match3.Core) — Sealed Singleton, Dictionary<prefab, List<instance>>, CreatePool/GetObject/GetObject<T>/Restore/Restore_Obj/Restore_All
- ✅ **Board.cs** (Match3.Core) — MonoBehaviour por celda, X/Y/Index, navegación 8 dirs, indexadores SQR_DIR+DROP_DIR, gravity (PossibleDrop_Dirs, ChangeDropDir, DropLeft/DropRight), Init(), flags completos, IsActiveCell, IsNowItemMatch, SetDisplayer
- ✅ **MatchManager.cs** (Match3.Core) — Singleton, Board[81], BoardCreate() con vecinos, BoardSetting(stage), StageSetting(appearColor), BoardPositionSetting(), GetBoardDropStartSetting(), GetGravitySetting(), StartGame(stage), GetRandomColor(), carga nivel JSON desde Resources
- ✅ **GravityDisplayer.cs** (Match3.Core) — Flecha visual por celda, Init/Show/Hide, rotación por DROP_DIR

### Prefab creado:
- ✅ **Board.prefab** (Assets/Prefabs/Board/) — Board + GravityDisplayer + hijo Visual (SpriteRenderer)

### Escena Gameplay.unity configurada:
- ✅ EventSystem (InputSystemUIInputModule)
- ✅ Main Camera (tag MainCamera, URP)
- ✅ Directional Light (URP)
- ✅ CommonManager → ObjectPool
- ✅ MatchManager (MatchManager component) → Field (contenedor de boards)
- ✅ SoundManager
- ✅ GameplayCanvas (Canvas + CanvasScaler + GraphicRaycaster)

### Datos de test:
- ✅ Resources/Levels/1.json — Nivel de prueba para verificar carga

### Notas:
- Board.m_Item tipado como Component (se cambiará a Item en Fase 2)
- Board.m_ListPanel tipado como List<Component> (se cambiará a List<Panel> en Fase 6)
- MatchManager aún no tiene: PanelSetting, ItemSetting, Steps/Update, Switching, MatchBrust (fases posteriores)
- SpecialPieceCreatorTool.cs (Editor/) — archivo vacío residual, no afecta compilación

### Compilación: ✅ 0 errores (Unity + VS)

### Próximo paso: Fase 2 (Item System + Input)

---

## Fases 2-5 ✅ COMPLETADAS
> Ver plan detallado en 03-development-plan.md. Todos los criterios de "hecho" superados.

---

## Fase 6: Misiones + Victoria/Derrota ✅ COMPLETADA

### Scripts creados/modificados:
- ✅ **MissionManager.cs** — Singleton, MissionSetting, MissionApply (fix cast), CheckMissionClear (fix lista vacía), CheckMissionFail, MoveLimitApply, AddScore, ScoreStar1/2/3
- ✅ **PopupBase.cs** — Lifecycle Show/Close con callback, hooks OnShow/OnClose. Sin dependencia circular.
- ✅ **PopupManager.cs** — Singleton stackable. Auto-registra hijos PopupBase en Awake. Show<T>(setup, onClosed), CloseTop(), CloseAll()
- ✅ **PopupType.cs** — Enum: Pause, Mission, Victory, Defeat
- ✅ **PausePopup.cs** — Pausa timeScale, botones Continuar/Reiniciar
- ✅ **PauseButton.cs** — [RequireComponent(Button)] → PopupManager.Show<PausePopup>()
- ✅ **VictoryPopup.cs** — SetContent(score, stars, onNext), usa PopupManager
- ✅ **DefeatPopup.cs** — SetContent(reason, onRetry), usa PopupManager
- ✅ **MissionPopup.cs** — SetContent(title, desc, onStart), script listo (escena Fase 12)
- ✅ **TopUIController.cs** — Singleton Instance, Init() public, Refresh(). Init() llamado desde MissionSetting()
- ✅ **MovesDisplay.cs** — UpdateMoves(remaining), color rojo cuando ≤5
- ✅ **ScoreDisplay.cs** — UpdateScore(score)
- ✅ **MissionDisplay.cs** — SetMission(data), UpdateCount(current), SetEmpty()
- ✅ **ClearStep.cs** — SetMatchState(GameClear) + PopupManager.Show<VictoryPopup>
- ✅ **FailStep.cs** — SetMatchState(GameFail) + PopupManager.Show<DefeatPopup>
- ✅ **MissionStep.cs** — lazy-init, pass-through si no hay MissionManager, Refresh tras MoveLimitApply
- ✅ **MatchManager.cs** — swap fallido → SetStep(Wait) directo (sin consumir movimiento)

### Escena Gameplay.unity:
- ✅ CommonManager/MissionManager
- ✅ GameplayCanvas/PopupManager (auto-registra popups hijos)
- ✅ GameplayCanvas/TopUI (TopUIController + MovesDisplay + ScoreDisplay + 3×MissionDisplay)
- ✅ GameplayCanvas/VictoryPopup (inactivo)
- ✅ GameplayCanvas/DefeatPopup (inactivo)
- ✅ GameplayCanvas/PausePopup (inactivo)

### Datos de test:
- ✅ Resources/Levels/2.json — 3 misiones OrderN (15 rojos, 15 amarillos, 10 azules), 50 movimientos, score stars 1000/3000/6000

### Bugs corregidos en esta fase:
- ✅ Swap fallido consumía movimiento → ahora va directo a Wait
- ✅ CheckMissionClear() devolvía true con lista vacía → victoria falsa en primer movimiento
- ✅ ClearStep/FailStep no bloqueaban el input → añadido SetMatchState(GameClear/GameFail)
- ✅ TopUI inicializaba antes que MissionSetting() → ahora MissionSetting() llama Init()
- ✅ MissionApply tenía cast incorrecto (ColorType)mission.type → (int)mission.kind == (int)color
- ✅ Scripts generadores de Editor (Phase6Setup.cs, CreateUIHelper.cs) eliminados (directriz MCP-first)

### Compilación: ✅ 0 errores

### Próximo paso: Fase 7 (Paneles)

---

## Fase 7: Paneles (Modificadores de Celda) ✅ COMPLETADA

### Scripts creados:
- ✅ **Panel.cs** (Match3.Panels) — Base abstracta: Defence, Value, addData, m_Board, Brust(), ItemExist/Drop/Match/Switch, hooks OnDamage/OnDestroyed, DestroyPanel()
- ✅ **DefaultFullPanel.cs** — Celda jugable, indestructible
- ✅ **DefaultEmptyPanel.cs** — Celda inactiva, bloquea todo
- ✅ **FixedPanel.cs** — Bloque fijo indestructible, bloquea gravedad y swap
- ✅ **IceCagePanel.cs** — Jaula de hielo (1-2 capas), bloquea swap, SetLayers(n)
- ✅ **BreadPanel.cs** — Obstáculo destruible por burst adyacente (1 hit)
- ✅ **CrackerPanel.cs** — Obstáculo destruible por burst adyacente (2 hits)
- ✅ **WaferFloorPanel.cs** — Suelo destruido cuando el item encima hace match
- ✅ **PanelManager.cs** (Match3.Managers) — Singleton factoría: BuildMap, CreatePanel(type, board), RestorePanel

### Board.cs modificado:
- ✅ m_ListPanel tipado a List<Panel> (antes List<Component>)
- ✅ PanelBrust() — notifica ItemMatch() a todos los paneles de la celda
- ✅ AroundBrust() — burst adyacente para Bread/Cracker/IceCage en las 4 direcciones
- ✅ Co_Brust() — llama PanelBrust() y AroundBrust() (antes eran TODOs)

### MatchManager.cs modificado:
- ✅ PanelSetting(stage) — crea paneles desde stage.panels[], sincroniza flags de Board
- ✅ StartGame() llama PanelSetting() entre BoardSetting() e ItemSetting()

### Escena Gameplay.unity:
- ✅ CommonManager/PanelManager (con 7 prefabs asignados)
- ✅ Prefabs en Assets/Prefabs/Panels/ (7 prefabs con SpriteRenderer de color diferente)

### Criterio de "hecho": ✅
- Board.m_ListPanel tipado correctamente
- PanelSetting crea paneles desde JSON
- Bursts activan PanelBrust y AroundBrust
- PanelManager factoría con ObjectPool

### Fix post-fase aplicado:
- ✅ Fixed_Block ahora desactiva la celda en BoardSetting (antes solo Default_Empty lo hacía)
- ✅ PanelSetting salta Default_Full y Default_Empty (solo crea paneles especiales)
- ✅ Nivel 3 rediseñado con distribución clara por tipo de panel:
  - Fila 1: 3× Ice_Cage 1 capa (cols 2,4,6)
  - Fila 3: 2× Ice_Cage 2 capas (cols 3,5)
  - Col 0 y Col 8, filas 2-6: Fixed_Block (pasillo lateral)
  - Fila 5: 3× Cracker (cols 2,4,6)
  - Fila 6: 4× Bread (cols 1,3,5,7)
  - Fila 7: 7× Wafer_floor (cols 1-7)
- ✅ Prefabs de panel con colores diferenciados y sorting order=5
- ✅ Fix visual post-fase: el sorting ya no depende del prefab; todos los Items fuerzan sorting order=10 y los paneles de bloqueo (IceCage, Fixed, Bread, Cracker) fuerzan sorting order=20 para verse por encima de la ficha
- ✅ Fix gameplay post-fase: IceCage ahora bloquea el flujo de gravedad; una ficha enjaulada no puede caer ni ser arrastrada por gravedad a otra celda
- ✅ Fix gameplay post-fase: IceCage vuelve a bloquear la interacción de swap; InputManager y MatchManager consultan el estado de paneles antes de intercambiar fichas, sin dañar la jaula por un intento de arrastre

### Próximo paso: Fase 8 (Gravedad Configurable)

---

## Alineacion Documental — Abril 2026

### Objetivo
- Dejar consistente la lectura entre documentacion de referencia, estado del repo y roadmap ejecutable.

### Mapa Doc vs Repo fijado
- ✅ La documentacion principal ya se interpreta como mezcla de referencia funcional + notas del repo, no como fotografia exacta de implementacion.
- ✅ Queda explicitado que la Fase 7 esta cerrada en este repo.
- ✅ Queda explicitado que la Fase 8 esta empezada pero no terminada.
- ✅ Queda explicitado que la referencia base si contempla caida diagonal (`SideDrop`), pero el repo aun no la implementa.

### Estado real consolidado
- ✅ Implementado: tablero 9x9, carga de `Stage`, matches, cascadas, piezas especiales base, misiones `OrderN`, victoria/derrota, paneles base de Fase 7.
- ✅ Implementado parcial: gravedad configurable por celda (`PossibleDrop_Dirs`, `ChangeDropDir`, `DropLeft`, `DropRight`, `isListDrop`, `m_ListDropStart`, `m_ListDropHead`).
- ❌ Pendiente de Fase 8: `SideDrop()` real, warps, validacion completa de gravedad lateral y bifurcaciones.
- ❌ Pendiente de fases posteriores: paneles avanzados, items avanzados, steps post-match con logica real y `EffectManager` completo.

### Decision de roadmap
- ✅ Fase 7 queda oficialmente cerrada para el subset implementado.
- 🟡 Fase 8 pasa a estado "en curso".
- ✅ La siguiente continuacion operativa del proyecto se centra en cerrar Fase 8 antes de retomar paneles y mecanicas avanzadas de la referencia.
