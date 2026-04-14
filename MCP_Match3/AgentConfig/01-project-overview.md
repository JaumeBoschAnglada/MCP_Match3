# MCP Match3 — Visión General del Proyecto

## Qué es
Un juego **Match 3 basado en turnos** (limitado por movimientos) para móviles.
El jugador intercambia piezas adyacentes para formar combinaciones de 3+.
Al hacer match, las piezas explotan, caen nuevas, se ejecutan mecánicas post-match,
y al finalizar se devuelve el control al jugador.

## Motor y Tecnología
- **Unity**: 6000.3.2f1
- **Render Pipeline**: URP
- **Target**: .NET Standard 2.1
- **Plataforma**: Móvil (Android/iOS) + Editor
- **Serialización de niveles**: JSON (Newtonsoft.Json)
- **Animaciones programáticas**: DOTween
- **UI**: Unity UI (Canvas + CanvasScaler). **NO NGUI.**
- **Escena**: **3D** (cámara perspectiva/ortográfica apuntando al tablero)
- **Piezas/Tablero**: GameObjects 3D (o quads/sprites en espacio 3D)

## Decisiones de Diseño que DIFIEREN de la Documentación de Referencia

La documentación de referencia describe un juego existente. Nosotros **tomamos la lógica de gameplay**
(sistema de matches, steps, items, paneles, gravedad, misiones, etc.) pero **NO replicamos**:

1. **Jerarquía de escena** — La sección 2.1 del doc describe una jerarquía con NGUI, UIRoot, etc.
   Nosotros creamos nuestra propia jerarquía con Unity UI (Canvas) y escena 3D.
2. **NGUI** — NO se usa. Toda la UI se hace con Unity UI (Canvas, Image, Text/TMP, Button, etc.).
3. **Estructura de escena** — La escena se organiza como convenga al proyecto, no como el doc la describe.

Lo que SÍ se toma del doc como fuente de verdad:
- Lógica de gameplay completa (Board, Item, Panel, Steps, matches, gravedad, combos, misiones)
- Formato de datos de nivel (Stage JSON)
- Sistema de ObjectPool
- Sistema de input (OnMouseDown/Enter/Up en Item)
- Todas las mecánicas: combinaciones, piezas especiales, paneles, etc.

## Documentación de Referencia
- `TMP Textos Agente/Match3_Gameplay_Documentation.md` — Gameplay completo (lógica, no escena)
- `TMP Textos Agente/Interaccion_Completa_Touch_to_Touch.md` — Flujo del input

## Estado Actual: DESDE CERO

### Qué SE CONSERVA del proyecto actual:
| Asset | Motivo |
|-------|--------|
| `PopupManager.cs` | Sistema de popups funcional |
| `PopupBase.cs` | Base de popups |
| `PopupType.cs` | Enum de tipos |
| `PausePopup.cs` | Popup de pausa |
| `PauseButton.cs` | Botón de pausa |
| `PausePopup.prefab` | Prefab UI |
| `Hall.unity` | Escena menú (se reconfigurará) |
| `Gameplay.unity` | Escena juego (se reconfigurará) |

### Qué SE ELIMINA (todo lo demás):
| Tipo | Assets a eliminar |
|------|-------------------|
| **Scripts** | BoardController, PieceData, LevelData, GameManager, Piece, PieceAnimator, InputHandler, HallManager, EffectManager, SpecialPieceEffects, SpecialPieceCreator, SpecialPieceAnimator, HorizontalRowPiece, VerticalRowPiece, DebugUI, UIInitializer, PopupPrefabInitializer, PopupManagerCleanup, AutoPilot, InputDebugVisualizer, TouchInputDiagnostic, LevelLoader |
| **Prefabs** | Todos los Piece_*.prefab, Special_*.prefab |
| **Materiales** | PieceRed/Blue/Green/Yellow/Orange.mat |
| **Datos** | level_1.json |

### Qué SE CREA NUEVO (todo):
- ~50 scripts nuevos
- ~20 prefabs de Items
- ~28 prefabs de Panels
- ~30 prefabs de Efectos
- 1 prefab de Board
- Materiales/Sprites para 6 colores
- Archivos JSON de niveles en formato Stage
- Escenas reconfiguradas desde cero

## Filosofía de Desarrollo

### MCP First
- Escenas y GameObjects se crean/modifican con MCP tools
- Scripts son SOLO lógica de juego
- NUNCA crear herramientas Editor ni scripts generadores de escenas

### Input Legacy (NO InputSystem)
- El input del jugador usa `OnMouseDown/OnMouseEnter/OnMouseUp` directamente en `Item.cs`
- Estos callbacks legacy funcionan tanto con mouse como con touch
- NO se usa el nuevo Unity InputSystem

### ObjectPool para todo
- Piezas, paneles y efectos se reciclan via ObjectPool
- NUNCA usar Destroy() durante el gameplay
- Solo Restore() al pool

### Steps para el turno
- El flujo del turno se gestiona con una máquina de estados (BaseStep)
- NO coroutines largas orquestando el turno
- Cada Step tiene Step_Play() (una vez) + Step_Process() (cada frame)
