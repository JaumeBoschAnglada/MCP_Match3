# Sistema de Movimiento y Physics - Match3

## Visión General

El juego utiliza un **sistema de spring physics** basado en resortes para animar todos los movimientos de las piezas. Este sistema proporciona un movimiento suave, natural y con inercia, simulando la atracción gravitacional hacia las posiciones objetivo sin necesidad de corrutinas complejas.

---

## Arquitectura de Spring Physics

### Componentes Clave

**PieceAnimator.cs** - Gestor del sistema de movimiento
```csharp
private class SpringState
{
    public Vector3 velocity;           // Velocidad actual del resorte
    public Vector3 targetPos;           // Posición objetivo (atractor)
    public bool settled;                // Flag cuando se estabiliza
}

[SerializeField] private float springStiffness = 100f;  // Rigidez del resorte (mayor = más rápido)
[SerializeField] private float springDamping = 14f;     // Amortiguación (mayor = menos rebote)
```

### Ecuación de Movimiento

```
acceleration = (targetPos - currentPos) * stiffness - velocity * damping
velocity += acceleration * deltaTime
position += velocity * deltaTime
```

**Cuando se "asienta"**:
- `|targetPos - currentPos| < 0.0001` (está en la posición)
- `|velocity| < 0.0001` (movimiento es negligible)

---

## Casos de Uso

### 1. **Caída por Gravedad** (PlayFallAnimation)
```csharp
public void PlayFallAnimation(Piece piece, Vector3 fromPos, Vector3 toPos)
{
    // Las piezas comienzan en fromPos y son atraídas a toPos
    piece.transform.localPosition = fromPos;
    springStates[piece].targetPos = toPos;  // Establece atractivo
    springStates[piece].settled = false;    // Despierta el resorte
}
```

**Secuencia**:
1. Pieza aparece en `fromPos` (e.g., encima del tablero)
2. Spring la atrae hacia `toPos` (su posición en el grid)
3. Movimiento es suave con amortiguación elástica integrada
4. Se estabiliza en `toPos`

### 2. **Convergencia de Piezas Especiales**
Cuando 4 piezas se combinan en una especial:
```csharp
// Las 3 piezas NO-centrales se mueven hacia el centro
foreach (var componentPiece in componentPieces)
{
    Vector3 centerPos = new Vector3(specialData.x, specialData.y, 0);
    pieceAnimator.PlayFallAnimation(componentPiece, componentPiece.transform.localPosition, centerPos);
}
```

**Efecto visual**: Las 4 piezas parecen "atraídas" magnéticamente hacia la pieza central que brota.

### 3. **Sincronización de Posiciones**
Después de cada operación, todas las piezas son atraídas a sus posiciones objetivo:
```csharp
public void EnsureTracked(Piece piece, Vector3 targetPos)
{
    // Si la posición cambió significativamente (>0.05), despierta el resorte
    if (Vector3.Distance(state.targetPos, targetPos) > 0.05f)
    {
        state.targetPos = targetPos;
        state.settled = false;  // Se remueve del equilibrio
    }
}
```

---

## Duración e Interpolación

### Animaciones de Tween (NO usa spring physics)
- **Swap** (intercambio): 0.2s, cubic ease in/out
- **Pop** (eliminación): 0.4s, crecimiento 40% + contracción 60%
- **Spawn** (aparición): 0.25s, curva elástica customizada

### Movimientos que SÍ usan Spring Physics
- Caídas por gravedad
- Convergencia de piezas especiales
- Correcciones de posición idle

---

## Parámetros de Control

### Ajuste de Rigidez (Velocidad)
```csharp
[SerializeField] private float springStiffness = 100f;
```
- **Valor bajo** (50): Movimiento muy lento, muy suave
- **Valor actual** (100): Equilibrio entre suavidad y responsividad
- **Valor alto** (200+): Movimiento muy rápido, puede parecer abrupto

### Ajuste de Amortiguación (Rebote)
```csharp
[SerializeField] private float springDamping = 14f;
```
- **Valor bajo** (< 5): Mucho rebote oscilante
- **Valor actual** (14): Sin oscilación visible ("críticamente amortiguado")
- **Valor muy alto** (> 20): Movimiento "stiff" sin naturalidad

**Nota**: El valores crítico de amortiguación es ~√(4 * stiffness * mass) ≈ 20 para nuestro sistema.

---

## Integración con GameManager

### Flujo de Movimiento Típico

```
1. Swap ejecuta
   ↓
2. PlaySwapAnimation (tween 0.2s)
   ↓
3. ApplyGravity detecta espacios vacíos
   ↓
4. StartGravityAnimations → PlayFallAnimation (spring physics)
   ↓
5. WaitUntil(IsAllSettled) espera que termine la caída
   ↓
6. FillEmptySpaces añade nuevas piezas
   ↓
7. StartFillAnimations → PlayFallAnimation desde arriba (spring physics)
   ↓
8. WaitUntil(IsAllSettled) espera nuevo equilibrio
   ↓
9. SynchronizeAllPieces asegura posiciones correctas
```

### Creación de Piezas Especiales

```
1. Se detecta match de 4+ piezas
   ↓
2. VisualSwapToSpecialPiece obtiene componentes
   ↓
3. PlayFallAnimation para cada componente NO-central → centerPos (spring)
   ↓
4. PlaySpawnAnimation para la pieza especial (tween desde escala 0)
   ↓
5. Las 3 piezas componentes se eliminan con PlayPopAnimation
```

---

## Mejores Prácticas

### ✅ CORRECTO
```csharp
// Usar para movimientos gravitacionales
pieceAnimator.PlayFallAnimation(piece, fromPos, toPos);

// Usar para convergencias
pieceAnimator.PlayFallAnimation(componentPiece, componentStartPos, centerPos);
```

### ❌ EVITAR
```csharp
// NO manipular transform.position directamente durante animaciones
// Siempre usar PlayFallAnimation, PlaySwapAnimation, etc
piece.transform.position = newPos;  // ❌ ROMPE spring physics
```

### 📋 DEBUG

Verificar que todas las piezas están en su posición correcta post-animación:
```csharp
private void SynchronizeAllPieces()
{
    foreach (var piece in piecesOnBoard.Values)
    {
        if (piece != null && piece.gameObject.activeSelf)
        {
            piece.UpdatePosition();  // Asegura sincronización
        }
    }
}
```

---

## Troubleshooting

### Problema: Piezas se "congelan" en posiciones incorrectas
**Causa**: Spring physics no actualiza el grid controller
**Solución**: Llamar `piece.UpdatePosition()` y `SynchronizeAllPieces()` después de que IsAllSettled() retorna true

### Problema: Caídas muy lentas o muy rápidas
**Causa**: Parámetros de spring incorrectos
**Solución**: Ajustar `springStiffness` (100 es default) en el inspector

### Problema: Rebotes después de aterrizar
**Causa**: Amortiguación muy baja
**Solución**: Incrementar `springDamping` a 20+ (default es 14)

---

## Referencias

- **PieceAnimator.cs**: Implementación del spring system
- **GameManager.cs**: Orquestación de secuencias de movimiento
- **BoardController.cs**: Lógica de grid y espacios vacíos
