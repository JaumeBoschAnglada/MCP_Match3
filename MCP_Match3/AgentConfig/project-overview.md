# MCP Match3 - Visión General del Proyecto

## Qué es
Un juego **Match 3** clásico para móviles desarrollado en **Unity 6000.3.2f1** con **URP** (Universal Render Pipeline). El jugador intercambia piezas en un tablero 6x6 para formar combinaciones de 3+ piezas del mismo color.

## Motor y Tecnología
- **Unity**: 6000.3.2f1
- **Render Pipeline**: URP (Universal Render Pipeline)
- **Input System**: Nuevo InputSystem (UnityEngine.InputSystem + EnhancedTouch)
- **Plataforma objetivo**: Móvil (Android/iOS), con soporte completo en Editor
- **Input soportado**: Mouse (editor), Touch simulado (editor Game View), Touch real (móvil)

## Filosofía de Desarrollo

### MCP First
- Las escenas y GameObjects se crean/modifican **directamente con MCP**
- **NUNCA** crear scripts generadores de escenas, menús Editor, o herramientas de setup
- Los scripts son SOLO lógica de juego
- MCP es la fuente de verdad para modificación del proyecto Unity

### Arquitectura Limpia
- `[SerializeField]` para TODAS las referencias (no FindObjectOfType, no GetComponentInChildren)
- Toda la lógica visual en escenas o prefabs, la lógica de código en scripts
- Object Pooling para piezas (evitar Instantiate/Destroy constantes)
- Namespace `Match3.*` para organización de código

### Input System Moderno
- **NO usar** legacy Input (UnityEngine.Input)
- Usar `UnityEngine.InputSystem` exclusivamente
- Soportar mouse y touch de forma unificada

## Estructura de Escenas
| Escena | Propósito |
|--------|-----------|
| `Hall.unity` | Menú principal con UI Button PLAY |
| `Gameplay.unity` | Tablero 6x6 + GameManager + todos los managers |

## Estructura de Scripts (7 scripts)
| Script | Namespace | Responsabilidad |
|--------|-----------|-----------------|
| `Core/BoardController.cs` | Match3.Core | Lógica de grid 6x6, matches, gravedad, relleno |
| `Data/PieceData.cs` | Match3.Data | Modelo de datos (PieceType enum + PieceData class) |
| `Gameplay/GameManager.cs` | Match3.Gameplay | Orquestador principal, spawn, swap, cascade loop |
| `Gameplay/Piece.cs` | Match3.Gameplay | Componente visual en prefabs de pieza |
| `Gameplay/PieceAnimator.cs` | Match3.Animation | Coroutines de animación (swap, fall, pop) |
| `Input/InputHandler.cs` | Match3.Input | Mouse/touch via InputSystem, detección de swipe |
| `Scenes/HallManager.cs` | Match3.Scenes | Botón Play → carga Gameplay scene |

## Assets
### Prefabs (Assets/Prefabs/Pieces/)
- `Piece_Red.prefab` - Cube + BoxCollider + Piece component + PieceRed.mat
- `Piece_Blue.prefab` - Cube + BoxCollider + Piece component + PieceBlue.mat
- `Piece_Green.prefab` - Cube + BoxCollider + Piece component + PieceGreen.mat
- `Piece_Yellow.prefab` - Cube + BoxCollider + Piece component + PieceYellow.mat

### Materials (Assets/Materials/)
- `PieceRed.mat` - Unlit/Color (r=0.9)
- `PieceBlue.mat` - Unlit/Color (b=0.9)
- `PieceGreen.mat` - Unlit/Color (g=0.8)
- `PieceYellow.mat` - Unlit/Color (r=0.95, g=0.85)

## Mecánicas Implementadas (Core Loop)
1. **Tablero 6x6** con 4 tipos de piezas (Red, Blue, Green, Yellow)
2. **Swipe para intercambiar** piezas adyacentes
3. **Detección de matches** horizontales y verticales (3+)
4. **Animación de pop** al destruir matches
5. **Gravedad** - piezas caen para llenar huecos
6. **Relleno** - nuevas piezas aparecen desde arriba
7. **Cascading** - loop automático de matches tras gravedad/relleno
8. **Revert** - si no hay match, las piezas vuelven a su posición
9. **Object Pooling** - reutilización de piezas destruidas

## MCP Gotchas (Tips para el Agente)
- `manage_components set_property` usa nombres de propiedades públicas (fontSize, no m_FontSize)
- `manage_gameobject modify component_properties` usa nombres serializados (m_Color, m_RenderMode)
- RectTransform Vector2: usar `manage_components set_property` con anchorMin/anchorMax/offsetMin/offsetMax
- Material assignment: usar `component_properties {"MeshRenderer": {"m_Materials": [{"path": "..."}]}}`
- Canvas enum: m_RenderMode 0=Overlay, 1=ScreenSpaceCamera, 2=WorldSpace
