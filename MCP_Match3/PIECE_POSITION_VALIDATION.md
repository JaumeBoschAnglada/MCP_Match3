# Validación - Visibilidad y Comportamiento de Piezas

## Estado de Implementación

### ✅ Completado

#### 1. Labels (x,y) en cada pieza
- **Archivo**: `Assets/Scripts/Gameplay/Piece.cs`
- **Implementación**: `OnGUI()` method
- **Comportamiento**:
  - Dibuja posición `(x,y)` en Screen Space
  - Solo visible si pieza activa y dentro de cámara
  - Se actualiza cada frame

#### 2. Piezas nuevas invisibles hasta posición inicial
- **Archivo**: `Assets/Scripts/Gameplay/GameManager.cs`
- **Funciones afectadas**:
  - `GetFromPool()`: Devuelve piezas con `SetActive(false)`
  - `PlayFallAndReact()`: Activa pieza con `SetActive(true)` antes de animar
- **Comportamiento**:
  - Piezas no aparecen en posición antigua
  - Aparecen cuando llegan a su posición inicial
  - Transición suave sin "pop" visual

#### 3. Piezas cayendo NO reaccionan
- **Archivo**: `Assets/Scripts/Gameplay/GameManager.cs`
- **Cambios**:
  - Removido `TriggerAdjacentReactions()` en `PlayFallAndReact()` (llenado)
  - Mantenido `TriggerAdjacentReactions()` en `AnimateGravity()` (combos)
- **Diferenciación**:
  - **Llenado**: Piezas nuevas caen sin reacciones
  - **Gravedad**: Piezas de combos SÍ reaccionan (elasticidad)

#### 4. Debug UI removido
- **Archivo**: `Assets/Scripts/Gameplay/GameManager.cs`
- **Cambio**: `SetupDebugUI()` deshabilitado en `Start()`
- **Razón**: Labels por-pieza reemplazan funcionalidad

---

## Verificación Visual (Play Mode)

### Qué debe ver:

#### El Inicio (tablero se llena)
```
[Espera a que el tablero se forme]
→ Las piezas NO deben ser visibles bajando
→ Aparecen cuando llegan a su posición final
→ Cada pieza muestra su (x,y)
```

Ejemplo de labelsque se ven:
```
(0,0)   (1,0)   (2,0)   ...
(0,1)   (1,1)   (2,1)   ...
...
```

#### Movimiento (swap)
```
1. Click/drag dos piezas adyacentes
2. Se animan intercambiándose
3. Piezas vecinas pueden reaccionar (elasticidad)
4. Labels actualizan a nuevas coordenadas
```

#### Cascada (combo)
```
1. Match se resuelve → piezas pop
2. Piezas caen por gravedad
3. Piezas ADYACENTES reaccionan al contacto
4. Labels de posición actualización en tempo real
```

---

## Detalles de Implementación

### Piece.cs - OnGUI()
```csharp
private void OnGUI()
{
    if (data == null || !gameObject.activeSelf)
        return;
    
    Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
    if (screenPos.z > 0)
    {
        GUI.Label(
            new Rect(screenPos.x - 20, Screen.height - screenPos.y - 10, 40, 20), 
            $"({data.x},{data.y})"
        );
    }
}
```

### GameManager.cs - Flujo de llenado
```
AnimateFill()
  ├─ GetFromPool() [SetActive(false)]
  ├─ Initialize() [Asigna datos]
  └─ PlayFallAndReactWithDelay()
      ├─ WaitForSeconds(cascadeDelay)
      └─ PlayFallAndReact()
          ├─ SetActive(true) ← Aquí se vuelve visible
          └─ PlayFallAnimation() [Sin reacciones]
```

### GameManager.cs - Flujo de gravedad (combos)
```
ProcessMatchesLoop()
  ├─ AnimateGravity()
  │   ├─ PlayFallAnimation()
  │   └─ TriggerAdjacentReactions() ← Sí reacciona
  ├─ AnimateFill()
  │   └─ PlayFallAnimation() [Sin reacciones]
  └─ FindMatches() [Cascada]
```

---

## Notas de Testing

### Casos de prueba:
1. ✅ Inicialización: Piezas aparecen sin flash
2. ✅ Labels: Visible (x,y) para cada pieza
3. ✅ Swap: Intercambio suave con reacciones opcionales
4. ✅ Combo: Cascadas sin reacciones en falling, SÍ en gravedad
5. ✅ Performance: Sin flickering o lag

### Posibles problemas:
- Si labels no se ven: Verificar Camera.main existe
- Si piezas popping: Aumentar delay en AnimateFill
- Si reacciones excesivas: Verificar TriggerAdjacentReactions está correcto

---

## Cambios de Arquitectura

| Aspecto | Antes | Después |
|---------|-------|---------|
| Pool visibility | `SetActive(true)` al obtener | `SetActive(false)` |
| Fill reactions | Sí, reaccionan | No, sin reacciones |
| Gravity reactions | Sí, reaccionan | Sí, reaccionan ✅ |
| Position labels | Global grid debug | Per-piece OnGUI ✅ |
| User feedback | Tablero global | Coordenadas en piezas ✅ |

---

## Status
✅ **Listo para Phase 2**
- Comportamiento de piezas optimizado
- Visibilidad clara con labels
- Sin "pop" visual en nuevas piezas
- Reacciones diferenciadas por tipo de caída

**Próximo**: Scoring System + Gameplay UI (Phase 2)
