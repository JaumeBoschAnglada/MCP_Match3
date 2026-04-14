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
