# MCP Match3 - Referencia de Arquitectura

## Diagrama de Dependencias entre Scripts

```
HallManager (Hall.unity)
    └── SceneManager.LoadScene("Gameplay")

GameManager (Gameplay.unity) [Singleton]
    ├── BoardController ← lógica de grid pura
    ├── PieceAnimator ← coroutines de animación
    ├── InputHandler ← detecta swipes del jugador
    │   └── GameManager.SwapPieces()
    ├── Piece (en prefabs) ← componente visual
    │   └── PieceData ← modelo de datos
    └── Object Pool (Dictionary<PieceType, Queue<Piece>>)
```

## Flujo del Core Loop

```
1. InputHandler detecta swipe sobre una pieza
2. InputHandler calcula dirección y pieza destino
3. InputHandler llama GameManager.SwapPieces(piece1, piece2)
4. GameManager inicia SwapPiecesCoroutine:
   a. boardController.SwapPieces() → intercambia datos en grid
   b. pieceAnimator.PlaySwapAnimation() → anima el intercambio
   c. boardController.FindMatches() → busca matches
   d. Si hay matches → ProcessMatchesLoop()
   e. Si NO hay matches → boardController.SwapPieces() revert + animar vuelta

5. ProcessMatchesLoop (mientras existan matches):
   a. boardController.MarkPiecesForRemoval()
   b. pieceAnimator.PlayPopAnimation() en paralelo
   c. ReturnToPool() las piezas destruidas
   d. boardController.RemoveMarkedPieces()
   e. boardController.ApplyGravity() → AnimateGravity()
   f. boardController.FillEmptySpaces() → AnimateFill()
   g. boardController.FindMatches() → ¿más matches? → repetir
```

## Convenciones de Código

### Namespaces
```
Match3.Core       → BoardController
Match3.Data       → PieceData, PieceType enum
Match3.Gameplay   → GameManager, Piece
Match3.Animation  → PieceAnimator
Match3.Input      → InputHandler
Match3.Scenes     → HallManager
```

### Patrón de Referencias
- Todo via `[SerializeField]` asignado en Inspector/MCP
- GameManager es Singleton (`Instance`)
- InputHandler referencia a GameManager y Camera via SerializeField

### Estructura de Carpetas (Assets/)
```
Assets/
├── Materials/          → PieceRed.mat, PieceBlue.mat, etc.
├── Prefabs/Pieces/     → Piece_Red.prefab, etc.
├── Scenes/             → Hall.unity, Gameplay.unity
├── Scripts/
│   ├── Core/           → BoardController.cs
│   ├── Data/           → PieceData.cs
│   ├── Gameplay/       → GameManager.cs, Piece.cs, PieceAnimator.cs
│   ├── Input/          → InputHandler.cs
│   ├── Scenes/         → HallManager.cs
│   └── UI/             → (vacío, para futuros scripts de UI)
├── Settings/           → URP settings
└── Sprites/            → (para futuros sprites)
```

## Tipos de Datos Clave

### PieceType (enum)
```
Empty  = 0
Red    = 1
Blue   = 2
Green  = 3
Yellow = 4
```

### PieceData (class)
```csharp
PieceType type
int x, y              // posición en grid
bool isMarkedForRemoval
```

### Grid
- `PieceData[6,6]` en BoardController
- Coordenada (0,0) = esquina inferior izquierda
- Y+ = arriba (gravedad = Y-)
