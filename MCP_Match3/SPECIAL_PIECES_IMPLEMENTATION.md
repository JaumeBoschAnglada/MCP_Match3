# Sistema de Fichas Especiales: Special_Horizontal y Special_Vertical

## Resumen

Se han agregado dos tipos de fichas especiales al juego Match3:
- **Special_Horizontal**: Se crea al detectar 4+ fichas del mismo color en línea horizontal. **Efecto**: Elimina TODAS las fichas en su fila
- **Special_Vertical**: Se crea al detectar 4+ fichas del mismo color en línea vertical. **Efecto**: Elimina TODAS las fichas en su columna

### Creación de Piezas Especiales
Cuando 4 fichas del mismo color se alinean (horizontalmente o verticalmente):
1. Las 4 piezas originales se desplazan hacia la posición central con **animación de gravedad** (spring physics)
2. La 4ª pieza (centr central) se reemplaza con la pieza especial que emerge animada desde escala 0
3. Las otras 3 piezas se eliminan con animación de pop

La eliminación es **gradual**: desde la posición de la ficha especial hacia los extremos.

---

## Cambios Realizados

### 1. **PieceData.cs** - Actualizado PieceType enum
```csharp
public enum PieceType
{
    Empty = 0,
    Red = 1,
    Blue = 2,
    Green = 3,
    Yellow = 4,
    HorizontalRow = 5,  // ← NUEVO
    VerticalRow = 6     // ← NUEVO
}
```

### 2. **SpecialPieceCreator.cs** (NUEVO)
- **Responsabilidad**: Detectar cuando se ha encontrado un match de 4+ fichas y convertir una en ficha especial
- **Métodos principales**:
  - `CreateSpecialPiecesFromMatches()`: Convierte 4+ matches horizontales/verticales
  - `ProcessHorizontalMatches()`: Busca y convierte matches horizontales
  - `ProcessVerticalMatches()`: Busca y convierte matches verticales

**Flujo**:
1. Detecta 4+ fichas del mismo color en línea
2. Selecciona la ficha central como especial
3. Reemplaza su tipo por `HorizontalRow` o `VerticalRow`

### 3. **SpecialPieceEffects.cs** (NUEVO)
- **Responsabilidad**: Cuando se combina una ficha especial, determina qué fichas deben eliminarse
- **Métodos principales**:
  - `ApplySpecialEffects()`: Aplica el efecto de la ficha especial
  - `GetRowPieces()`: Obtiene todas las fichas de una fila
  - `GetColumnPieces()`: Obtiene todas las fichas de una columna
  - `GetEliminationOrder()`: Ordena las fichas para eliminación gradual (centro → extremos)

**Flujo**:
1. Si hay una HorizontalRow en el match → obtiene todas las fichas de esa fila
2. Si hay una VerticalRow en el match → obtiene todas las fichas de esa columna
3. Ordena las fichas por distancia al centro (para animación gradual)

### 4. **SpecialPieceAnimator.cs** (NUEVO)
- **Responsabilidad**: Anima la eliminación gradual de fichas
- **Métodos principales**:
  - `EliminateGradually()`: Inicia la animación
  - `EliminateGraduallyCoroutine()`: Corrutina que controla el timing
  - `FadeOutPiece()`: Anima el fade-out de cada pieza

**Flujo**:
1. Recibe lista de fichas en orden de eliminación
2. Espera ELIMINATION_DELAY (0.05s) entre fichas
3. Hace fade-out gradual (0.3s) de cada ficha

### 5. **BoardController.cs** - Actualizado
- **Cambios en `FindMatches()`**:
  - Ahora llama a `SpecialPieceCreator.CreateSpecialPiecesFromMatches()` después de detectar matches
  - Convierte 4+ matches en fichas especiales

- **Cambios en `MarkPiecesForRemoval()`**:
  - Ahora llama a `SpecialPieceEffects.ApplySpecialEffects()` antes de marcar para eliminación
  - Expande la lista de piezas a eliminar si hay especiales

---

## Integración con GameManager (PENDIENTE)

Necesitas actualizar `GameManager.cs` para:

1. Almacenar referencias a los Piece GameObjects (ya lo hace)
2. Cuando se detecta una match con fichas especiales:
   - Obtener las fichas a eliminar en orden
   - Llamar a `SpecialPieceAnimator.EliminateGradually()` 
   - Esperar a que termine la animación antes de resolver la eliminación

**Pseudocódigo**:
```csharp
// En GameManager, después de FindMatches():
if (matchedPieces.Count > 0)
{
    bool hasSpecial = matchedPieces.Any(p => p.type == PieceType.HorizontalRow || p.type == PieceType.VerticalRow);
    
    if (hasSpecial)
    {
        // Obtener fichas especiales
        var specialPiece = matchedPieces.FirstOrDefault(p => p.type == PieceType.HorizontalRow || p.type == PieceType.VerticalRow);
        
        // Obtener orden de eliminación
        var eliminationOrder = SpecialPieceEffects.GetEliminationOrder(matchedPieces, specialPiece);
        
        // Convertir PieceData a Piece GameObjects
        List<Piece> piecesToAnimate = new List<Piece>();
        foreach (var data in eliminationOrder)
        {
            Piece piece = FindPieceGameObject(data.x, data.y);
            if (piece != null) piecesToAnimate.Add(piece);
        }
        
        // Animar
        pieceAnimator.EliminateGradually(specialPiecePiece, piecesToAnimate);
        
        // Esperar animación... (await o corrutina)
    }
}

// Luego continuar con RemoveMarkedPieces(), ApplyGravity(), etc.
```

---

## Cómo Funciona en Ingame

### Escenario: HorizontalRow
1. **Setup**: 4 fichas rojas seguidas horizontalmente en la fila Y=2
   ```
   [Empty] [Empty] [Red] [Red] [Red] [Red]
   ```

2. **Detección**: `FindMatches()` detecta el match
   - SpecialPieceCreator convierte una red (la central) en HorizontalRow
   - Grid ahora tiene: `[Empty] [Red] [HorizontalRow] [Red]`

3. **Combinación**: Jugador hace match de 3 HorizontalRow + 2 rojos
   - SpecialPieceEffects detecta HorizontalRow en el match
   - Obtiene TODAS las fichas de la fila Y=2
   - Ordena por distancia: centro primero, luego izquierda/derecha alternando

4. **Eliminación**: SpecialPieceAnimator anima:
   - Ficha central fade-out
   - Delay 0.05s
   - Ficha izquierda fade-out
   - Delay 0.05s
   - Ficha derecha fade-out
   - ... continúa hasta los extremos

5. **Resultado**: Toda la fila está vacía

---

## Parámetros Configurables

En `SpecialPieceEffects.cs`:
```csharp
public const float ELIMINATION_DELAY = 0.05f; // Delay entre fichas (segundos)
```

En `SpecialPieceAnimator.cs`:
```csharp
private const float FADE_DURATION = 0.3f;      // Duración del fade de cada ficha
private const float ELIMINATION_DELAY = 0.05f; // Delay entre inicios de fade
```

Ajusta estos valores para modificar la velocidad de la animación.

---

## Testing

Para probar rápidamente:
1. Coloca 4 fichas rojas horizontales en el tablero inicial (en Gameplay)
2. Inicia el juego
3. Debería crear una HorizontalRow
4. Haz match de 3 fichas incluyendo la HorizontalRow
5. Debería eliminar toda la fila gradualmente

---

## Próximos Pasos

1. ✅ Crear fichas especiales
2. ✅ Detectar y aplicar efectos
3. ⏳ **Integrar animación en GameManager** (NECESARIO para ver en acción)
4. ⏳ Pruebas y ajustes de timing
5. ⏳ Agregar más tipos especiales (Bombas, Dynamite, etc.)
