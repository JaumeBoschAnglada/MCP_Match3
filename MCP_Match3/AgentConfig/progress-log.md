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
