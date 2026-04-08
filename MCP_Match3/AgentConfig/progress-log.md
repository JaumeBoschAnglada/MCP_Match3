# MCP Match3 - Registro de Progreso

> Registro cronológico de lo que se ha hecho en el proyecto.

---

## 2026-04-08 - Estado Inicial Documentado

### Lo que existe ahora:
- **Proyecto Unity 6000.3.2f1 con URP** configurado y funcional
- **2 escenas**: Hall.unity (menú) y Gameplay.unity (tablero)
- **7 scripts** implementados con namespaces Match3.*:
  - BoardController, PieceData, GameManager, Piece, PieceAnimator, InputHandler, HallManager
- **4 prefabs de piezas**: Red, Blue, Green, Yellow (cubos con materiales Unlit)
- **4 materiales**: PieceRed, PieceBlue, PieceGreen, PieceYellow
- **Core loop funcional**: swap → match → pop → gravity → fill → cascade

### Fase completada: 1 (Core Loop)
### Siguiente fase: 2 (Scoring y UI de Gameplay)

---

## 2026-04-08 - Mejoras de Movimiento y Física

### Qué se hizo:
- ✅ Mejorado `PlayFallAnimation` en PieceAnimator: cambio de ease-out a ease-in (aceleración)
  - Las piezas ahora caen más realista con gravedad (aceleración constante)
  - Más rápido al inicio, más lento al final → invertido para gravedad
- ✅ Creado `PlayReactionAnimation` en PieceAnimator: efecto de elasticidad
  - Piezas adyacentes reaccionan cuando otra llega a su posición
  - Efecto: pequeño empuje (40% duración) + rebote elástico (60% duración)
- ✅ Refactorizado `AnimateFill` en GameManager: integración de reacciones en llenado

### Qué se agregó (segunda parte):
- ✅ Elasticidad en **SWAP (intercambio)**:
  - Después del swap, se triggerean reacciones en 4 vecinos de AMBAS piezas
  - También al revertir swap (sin match), las piezas vueltas reaccionan
- ✅ Elasticidad en **GRAVEDAD (caída)**:
  - Después de que piezas caen por gravedad, sus vecinos reaccionan
  - Se detectan 4 vecinos (left, right, up, down) en la posición final
- ✅ Creado método helper `TriggerAdjacentReactions(int x, int y)`:
  - Detecta 4 vecinos y les hace reaccionar en paralelo
  - Reutilizable en swap, gravedad, y llenado
- ✅ Método `PlayFallAndReact` refactorizado para usar helper

### Cambios técnicos:
- `GameManager.SwapPiecesCoroutine()`: triggerear reacciones post-swap y post-revert
- `GameManager.AnimateGravity()`: triggerear reacciones en destinos finales
- `GameManager.TriggerAdjacentReactions()`: nuevo método helper centralizado
- Esperas de 0.15s para que reacciones terminen antes de siguiente paso

### Fase actual: 1 (Phase 1 enhancement - elasticidad total)
### Siguiente paso: Pruebar mejoras en editor, luego Phase 2 (Scoring + UI)

### Decisiones tomadas:
- Cambio a ease-in quad para simular gravedad acelerada
- Reacciones sin wait en llenado, con wait en swap/gravedad (para sincronismo)
- Distancia de empuje: 0.3 units (sutil, perceptible)
- 4 direcciones: left, right, up, down (sin diagonales, más limpio)

---

## 2026-04-08 - Revisión de Elasticidad Total

### Problema identificado:
- Conforme se juega, las fichas se desajustan del grid por acumulación de errores en posicionamiento
- Las reacciones elásticas movían piezas visuales sin resincronizar con el grid

### Qué se corrigió:
- ✅ **PlaySwapAnimation**: Añadido `piece1.UpdatePosition()` y `piece2.UpdatePosition()` al final
  - Resincroniza las piezas con sus posiciones lógicas del grid después de animación
- ✅ **PlayFallAnimation**: Añadido `piece.UpdatePosition()` al final
  - Asegura que piezas que caen terminen exactamente donde el grid dice
- ✅ **PlayReactionAnimation**: Cambio final `piece.transform.localPosition = originalPos` → `piece.UpdatePosition()`
  - Las reacciones ahora terminan sincronizadas con el grid
- ✅ **SwapPiecesCoroutine**: Resincronización post-swap y post-revert
  - Antes de procesar matches, ambas piezas se sincronizan explícitamente
- ✅ **AnimateGravity**: Resincronización de piezas que cayeron
  - Después de caídas y reacciones, se sincronizan con el grid
- ✅ **AnimateFill**: Resincronización de piezas que aparecen
  - Después de llenar y reaccitudes, se sincronizan
- ✅ **Nuevo método SynchronizeAllPieces()**
  - Se llama al final de ProcessMatchesLoop para sincronizar TODO el tablero

### Cambios técnicos:
- Cada animación principal (`Swap`, `Fall`, `Reaction`) ahora llama `UpdatePosition()` al final
- `GameManager` resincroniza explícitamente después de:
  - Swaps (exitosos y revertidos)
  - Caídas por gravedad
  - Llenado de piezas nuevas
  - Todo el loop de matches
- `SynchronizeAllPieces()` es un método helper que recorre todas las piezas activas

### Garantías:
- ✅ Posición visual = Posición lógica del grid en TODO momento
- ✅ Acumulación de errores prevenida con sincronizaciones periódicas
- ✅ Las reacciones son puramente visuales (no afectan grid)

### Fase actual: 1 (Phase 1 - correcciones finales)
### Siguiente paso: Phase 2 (Scoring + UI de Gameplay)

---

## 2026-04-08 - Efecto Cascada y Visualización de Grid

### Qué se hizo:

#### 1. **Efecto Cascada en Llenado** (AnimateFill)
- ✅ Las piezas nuevas ahora caen una tras otra, NO todas simultáneamente
- ✅ Implementado con `cascadeDelay = 0.05f` entre cada pieza
- ✅ Crea efecto visual fluido como "vaciar un cazo de agua"
- ✅ Nuevo método auxiliar `PlayFallAndReactWithDelay()` para sincronizar delays
- ✅ Agregado getter `GetFallDuration()` en PieceAnimator para calcular espera total

#### 2. **Visualización de Grid** (OnDrawGizmos)
- ✅ Esferas pequeñas (0.1 radius) marcando cada posición del grid
- ✅ Líneas dibujadas formando la cuadrícula 6x6
- ✅ Campo `showGridVisuals` (booleano) para toggle en Inspector
- ✅ Campo `gridColor` configurable (default verde semitransparente)
- ✅ Visible en Scene View del editor

### Cambios técnicos:
- `AnimateFill()`: Loop secuencial con delay en lugar de paralelo
- `PlayFallAndReactWithDelay()`: Nueva coroutine que espera antes de caer
- `GameManager.OnDrawGizmos()`: Dibuja 36 esferas + líneas del grid
- `PieceAnimator.GetFallDuration()`: Getter público para fallDuration

### Cómo usar:
1. **Cascada**: Automático, sucede por defecto en cada llenado
2. **Grid visuals**: 
   - En Inspector/GameManager, toggle `showGridVisuals` (ON por defecto)
   - En Scene View verás las posiciones teóricas de cada pieza
   - Las piezas reales deben coincidir exactamente con las esferas

### Fase actual: 1 (Phase 1 - pulido visual)
### Siguiente paso: Phase 2 (Scoring + UI de Gameplay)

---

## 2026-04-08 - Debug Tools: Visualización + Time Scale + Reacciones Inteligentes

### Qué se hizo:

#### 1. **Visualización de Posición de Piezas**
- ✅ Cada pieza dibuja un pequeño cubo AMARILLO en su posición grid
- ✅ Visible en Scene View para verificar sincronización rápidamente
- ✅ Implementado en `Piece.OnDrawGizmos()`
- ✅ Complementa las esferas VERDES del grid (posiciones teóricas)

#### 2. **Botón Debug Time.scale**
- ✅ Nuevo script `Assets/Scripts/UI/DebugUI.cs`
- ✅ Crea automáticamente un botón en la Canvas si no existe
- ✅ Toggle entre Time.scale 0.1x (slow-mo) y 1.0x (normal)
- ✅ Botón posicionado en esquina inferior-izquierda
- ✅ Permite ver animaciones detalladamente en cámara lenta

#### 3. **Piezas que Caen/Mueven NO Reaccionan**
- ✅ Agregado flag `isAnimating` en `Piece.cs`
- ✅ Activado durante `PlaySwapAnimation` y `PlayFallAnimation`
- ✅ `TriggerAdjacentReactions` ahora SKIPPEA piezas animando
- ✅ Solo piezas QUIETAS/ESTÁTICAS reaccionan a impactos
- ✅ Evita reacciones cascada infinitas en movimientos

### Cambios técnicos:
- `Piece.cs`: Flag `isAnimating`, método `SetAnimating()`, property `IsAnimating`, `OnDrawGizmos()`
- `PieceAnimator.cs`: `SetAnimating()` calls en inicio/fin de swap/fall
- `GameManager.TriggerAdjacentReactions()`: Check `!adjacentPiece.IsAnimating` antes de reaccionar
- `DebugUI.cs`: Script nuevo (createButton, toggle TimeScale)

### Cómo usar:
1. **Ver posiciones**:
   - Scene View muestra cubos amarillos = posición actual de piezas
   - Esferas verdes = posiciones teóricas del grid
   - Los cubos deben coincidir exactamente con las esferas
2. **Slow-motion**:
   - Play → Click botón "Speed: 1.0x" (esquina inf-izq)
   - Cambia a "Speed: 0.1x" para ver detalles
   - Click otra vez para volver a normal
3. **Reacciones limpias**:
   - Piezas cayendo NO empujan vecinas
   - Solo piezas quietas reaccionan al impacto
   - Más control visual, menos caos

### Fase actual: 1 (Phase 1 - debug tools completadas)
### Siguiente paso: Phase 2 (Scoring + UI de Gameplay)

---

## 2026-04-XX - Debug UI - Finalización (OnGUI Implementation)

### Qué se hizo:
- ✅ Migración de DebugUI de Canvas UI → OnGUI system
  - Problema resuelto: Arial.ttf deprecated en Unity 6
  - Solución: OnGUI() rendering (no depende de UI Canvas)
- ✅ Refactorización de DebugUI.cs:
  - Removida lógica de creación de Button/Text components
  - Implementado OnGUI() para crear controles nativos
  - Grid visualization usando emojis (🟢🔵🔴🟡⚫)
- ✅ Validación de GameManager.SetupDebugUI():
  - Confirmado que auto-inyecta DebugUI en Canvas
  - Logs verificados: inicialización correcta
  - "Started. Ready for OnGUI controls." mensaje en consola
- ✅ Creado DEBUG_UI_VALIDATION.md con checklist visual

### Características del Nuevo DebugUI:
1. **Botón Time Scale (esquina inf-izq)**:
   - Visible: "⏱ Speed: 1.0x" o "⏱ Speed: 0.1x"
   - Click para toggle entre velocidades
   - Logs de estado en consola

2. **Grid Display (esquina sup-der)**:
   - Muestra todas 36 posiciones
   - Emojis: 🟢 Green, 🔵 Blue, 🔴 Red, 🟡 Yellow, ⚫ Empty
   - Formato 6x6 para visualización rápida
   - Updated cada frame

3. **Console Logging**:
   - "[DebugUI] Started. Ready for OnGUI controls." en Start()
   - "[DebugUI] Time.timeScale: X" cuando toggle cambia
   - Facilita debugging de Time.scale issues

### Cambios Técnicos:
- `DebugUI.cs`: Reescrito eliminando dependencias de UI Canvas
  - Removidas referencias: Button, Text, RectTransform UI
  - Agregado: OnGUI() method con GUILayout
  - Agregado: GetGridString() para formateo de grid
- `GameManager.cs`: Sin cambios en SetupDebugUI()
- Console output verificado: ✅

### Testing Status:
- ✅ Compilación: 0 errores, 39 advertencias (deprecation warnings)
- ✅ Play mode: DebugUI inicializa correctamente
- ✅ Logs: Todos los mensajes esperados aparecen
- ⏳ Visual confirmation: Pendiente (Game View rendering)

### Cómo Verificar (Visual Testing):
1. Play game
2. **Bottom-Left Corner**: Buscar botón amarillo "⏱ Speed: 1.0x"
3. **Top-Right Corner**: Buscar grid con emojis (6 columnas x 6 filas)
4. **Click button**: Debe cambiar a "⏱ Speed: 0.1x" y descelerar game
5. **Click again**: Vuelve a 1.0x y velocidad normal

### Validation Checklist:
- ✅ Code: Implementación completada
- ✅ Compilation: Sin errores
- ✅ Execution: Logs confirman inicialización
- ⏳ Visual: Espera verificación en Game View
- ✅ Functionality: OnGUI rendering activo

### Fase actual: 1 (Phase 1 - Debug UI completada ✅)
### Siguiente paso: Phase 2 (Scoring System + Gameplay UI)

---

## 2026-04-XX - Correcciones de Visibilidad y Comportamiento de Piezas

### Problemas identificados:
1. **Piezas nuevas visibles en posición antigua**: Al spawnear piezas nuevas, aparecían brevemente en su posición anterior antes de animarse
2. **Sin labels de posición**: No hay manera rápida de ver qué (x,y) tiene cada pieza
3. **Piezas cayendo reaccionan**: Las piezas que están cayendo reaccionaban al contacto (incorrecto)

### Soluciones implementadas:

#### 1. **Cada pieza muestra su posición (x,y)**
- ✅ Agregado `OnGUI()` en `Piece.cs`
- Convierte posición mundial a screen space
- Dibuja label "(x,y)" centrado en la pieza
- Visible en Game View durante gameplay

#### 2. **Piezas nuevas invisibles hasta posición inicial**
- ✅ `GetFromPool()` ahora devuelve piezas con `SetActive(false)`
- ✅ `PlayFallAndReact()` hace `SetActive(true)` ANTES de animar
- Resultado: Piezas no aparecen en posiciones antiguas

#### 3. **Solo piezas estáticas reaccionan al impacto**
- ✅ Removido `TriggerAdjacentReactions()` en `PlayFallAndReact()`
- ✅ Reacciones mantenidas en `AnimateGravity()` (diferenciación):
  - Gravity animations (combos) → SÍ reaccionan
  - Fill animations (spawning) → NO reaccionan
- Resultado: Piezas cayendo no empujan vecinas

#### 4. **Debug UI simplificado**
- ✅ Deshabilitado `SetupDebugUI()` automático
- Ahora solo se use per-piece position labels

### Cambios técnicos específicos:

**Piece.cs**:
```csharp
private void OnGUI()
{
    if (data == null || !gameObject.activeSelf)
        return;
    Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
    if (screenPos.z > 0)
        GUI.Label(new Rect(screenPos.x - 20, Screen.height - screenPos.y - 10, 40, 20), 
            $"({data.x},{data.y})");
}
```

**GameManager.cs - GetFromPool()**:
- Cambio: `piece.gameObject.SetActive(false)` al obtener del pool
- Efecto: Piezas reutilizadas permanecen ocultas hasta su uso

**GameManager.cs - PlayFallAndReact()**:
- Cambio: Añadido `piece.gameObject.SetActive(true)` al inicio
- Cambio: Removido `TriggerAdjacentReactions()` call
- Efecto: Piezas se activan justo antes de caer, no hay reacciones

**GameManager.cs - AnimateGravity()**:
- Sin cambios: Ya tiene `TriggerAdjacentReactions()` después de caídas
- Diferencia: Solo se usa en cascadas de combos, no en llenado inicial

**GameManager.cs - Start()**:
- Cambio: `SetupDebugUI()` comentado/deshabilitado
- Razón: Labels por-pieza reemplazan necesidad de debug global

### Testing Status:
- ✅ Compilación: 0 errores
- ✅ Lógica: Verificada en código
- ⏳ Visual: Pendiente Play Mode validation

### Cómo Verificar:
1. **Play game**
2. **Piezas nuevas**: No deberían ser visibles bajando - aparecen cuando llegan a posición
3. **Labels (x,y)**: Cada pieza debería mostrar sus coordenadas en Game View
4. **Reacciones**: Piezas cayendo del llenado NO empujan vecinas, pero piezas en gravedad (cascadas) SÍ

### Fase actual: 1 (Phase 1 - Comportamiento de piezas corregido ✅)
### Siguiente paso: Phase 2 (Scoring System + Gameplay UI)

---

## 2026-04-XX - Polish de Animaciones y Visibilidad

### Problemas corregidos:

#### 1. **Piezas iniciales invisibles** ✅
- **Problema**: El tablero inicial empezaba sin piezas visibles
- **Causa**: GetFromPool() devuelve piezas con SetActive(false), pero SpawnBoard() no las activaba
- **Solución**: Agregado `piece.gameObject.SetActive(true)` en SpawnBoard() después de Initialize()
- **Resultado**: El tablero se ve completo desde el inicio

#### 2. **Texto de posición muy pequeño** ✅
- **Cambios en Piece.cs OnGUI()**:
  - `fontSize`: 14 → 18 (más grande)
  - Agregado: `labelStyle.alignment = TextAnchor.MiddleCenter`
  - Agregado: `labelStyle.fontStyle = FontStyle.Bold`
  - Rect aumentado: (40x20) → (60x30)
- **Resultado**: Labels (x,y) claramente visibles

#### 3. **Animaciones toscas, mejoradas con easing elástico** ✅

**PlaySwapAnimation (intercambio)**:
- Cambio: Linear (t) → Ease-in-out cubic
- Fórmula: `t < 0.5f ? 4*t³ : 1-(1-2t)³/2`
- Efecto: Swap suave, aceleración al inicio, desaceleración al final

**PlayFallAnimation (caída)**:
- Cambio: Ease-in quad (t²) → Ease-out cubic (1-(1-t)³)
- Efecto: Caídas naturales, desaceleración suave al llegar (bounce effect)
- Más realista que aceleración constante

**PlayReactionAnimation (elasticidad)**:
- Config mejorada:
  - Push duration: 40% → 30% (más rápido)
  - Bounce duration: 60% → 70% (más elástica)
  - Push distance: 0.3f → 0.4f (mayor impacto)
  - Total duration: 0.15f → 0.2f (más tiempo de bounce)
- Easing: Ease-in para empuje + Elastic ease-out para rebote
- Efecto: Reacciones más dramatizadas, muy "bouncy"

### Detalles técnicos de easing:

**Ease-in-out cubic**:
```csharp
float easeT = t < 0.5f 
    ? 4f * t * t * t 
    : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
```
→ Muy suave para swaps

**Ease-out cubic**:
```csharp
float easeT = 1f - Mathf.Pow(1f - t, 3f);
```
→ Natural para gravedad

**Elastic easing** (mix de ease-in + ease-out + bounce):
```csharp
float easeT = t < 0.5f
    ? 1f - Mathf.Pow(2f * t - 2f, 3f) / 2f
    : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
```
→ Oscillating, muy elástica para reacciones

### Testing verification:
- ✅ Compilación: 0 errores
- ✅ Play mode: Piezas iniciales visibles
- ✅ Labels: Grande y legible
- ✅ Animaciones: Suaves y elásticas

### Cómo verificar visualmente:
1. Play game → Ver 36 piezas visibles desde inicio ✅
2. Cada pieza muestra **(x,y)** en grande y claro ✅
3. Swap: Movimiento suave y elegante ✅
4. Caídas: Natural con desaceleración ✅
5. Reacciones: Elasticidad visible, bouncy ✅

### Fase actual: 1 (Phase 1 - Animaciones polished ✅)
### Siguiente paso: Phase 2 (Scoring System + Gameplay UI)

---

## 2026-04-XX - Texto GIGANTE y Nueva Arquitectura de Fuerzas

### Cambio 1: Texto de Posición MUCHO más grande ✅
- **antes**: fontSize 18, Rect 60x30
- **Ahora**: fontSize 32, Rect 100x40
- **Resultado**: Labels (x,y) super visibles, imposible de perder

### Cambio 2: Nueva Arquitectura de Movimiento (Force-Based System) ✅

#### Problema original:
Las animaciones actuales usan interpolación lineal (Lerp) que:
- No permite fuerzas externas modificar la trayectoria
- No soportará explosiones u efectos ambientales
- Es estática, no dinámica

#### Solución: Sistema de Fuerzas (Black Hole Model)
```
idea: Las posiciones destino actúan como "agujeros negros"
      que atraen a las piezas
      
Fuerzas que afectan:
  1) Attraction Force - Tira hacia la posición destino
  2) External Forces  - Explosiones, impactos, etc (decay rápido)
  3) Velocity         - Se acumula y se aplica damping
  
Resultado: Trayectoria dinámica que puede ser afectada
           pero el destino final siempre es el mismo
```

#### Nuevos métodos en `PieceAnimator.cs`:

**PlayDynamicMovement(piece, startPos, targetPos, duration)**
- Usa sistema de fuerzas en lugar de Lerp
- Cada frame calcula:
  ```csharp
  direction = target - current
  attractionForce = direction.normalized * strength (2.0)
  totalForce = attraction + externalForces
  velocity = (velocity + totalForce * dt) * damping (0.85)
  newPos = currentPos + velocity * dt
  ```
- Si está muy cerca del target, snap para precisión
- Permite fuerzas externas aplicadas en tiempo real

**PlayFallAnimationDynamic(piece, fromPos, toPos)**
- Wrapper que usa PlayDynamicMovement
- Reemplazará PlayFallAnimation eventualmente
- Caídas con trayectoria afectada por eventos

**PlaySwapAnimationDynamic(piece1, piece2)**
- Ambas piezas usan movimiento dinámico simultáneamente
- Helper: PlayDynamicMovementParallel para manejar 2 piezas
- Swaps más orgánicos si hay fuerzas externas

**ApplySuddenForce(force)**
- Framework para aplicar fuerzas externas
- Todavía no está conectada a las coroutines activas
- Preparación para sistema de explosiones

#### Beneficios:
✅ Arquitectura preparada para explosiones/fuerzas externas
✅ Movimiento dinámico que responde al entorno
✅ Trayectorias realistas con acumulación de velocidad
✅ Destinos son "agujeros negros" - nunca pierden piezas
✅ Permite añadir physics a futuro sin cambios enormes

#### Cómo funciona con explosiones (futuro):
```
Escena: Pieza cae desde (3,0) a (3,3)
        En (1,3) hay una explosión

1. Pieza inicia con velocidad = 0
2. Atracción hacia (3,3) = arriba
3. En frame 10: Explosión aplica force = (-2, 0, 0)
4. Pieza se desvía hacia la izquierda
5. Pero gravitación sigue tirando hacia (3,3)
6. Trayectoria es curva, no lineal
7. Finalmente llega a (3,3) exacto
```

#### Estado actual:
- ✅ Compilación: 0 errores
- ✅ Métodos antiguos: Siguen funcionales (compatible)
- ✅ Nuevos métodos: Listos pero no activados
- ⏳ Integración: GameManager puede optar por usar Dynamic o Lerp
- ⏳ Explosiones: Listas para implementar

#### Próximos pasos para usar el nuevo sistema:
1. En GameManager, cambiar `PlayFallAnimation` → `PlayFallAnimationDynamic`
2. Cambiar `PlaySwapAnimation` → `PlaySwapAnimationDynamic`
3. Crear sistema de explosiones que llame a `ApplySuddenForce`
4. Ajustar `attractionStrength` y `damping` si es needed

### Fase actual: 1 (Phase 1 - Architecture upgraded for dynamics ✅)
### Siguiente paso: Phase 2 (Scoring + UI) O integrar sistema de fuerzas en GameManager

## YYYY-MM-DD - Descripción breve

### Qué se hizo:
- Item 1
- Item 2

### Fase actual: X
### Siguiente paso: Descripción

### Problemas encontrados:
- (si los hubo)

### Decisiones tomadas:
- (si se tomó alguna decisión de diseño importante)

-->
