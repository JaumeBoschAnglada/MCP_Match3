# MCP Match3 - Plan de Desarrollo y Roadmap

> Última actualización: 2026-04-08

## Fase 1: Core Loop ✅ COMPLETADA
El gameplay básico del Match 3 está implementado y funcional.

### Tareas completadas:
- [x] Estructura del proyecto Unity 6 + URP
- [x] Escena Hall con menú principal y botón Play
- [x] Escena Gameplay con tablero 6x6
- [x] Sistema de grid (BoardController) con 4 tipos de piezas
- [x] Prefabs de piezas (Red, Blue, Green, Yellow) con materiales
- [x] Sistema de input con swipe (InputHandler) vía nuevo InputSystem
- [x] Detección de matches horizontales y verticales (3+)
- [x] Animaciones: swap, pop (destrucción), fall (gravedad)
- [x] Gravedad y relleno automático de piezas nuevas
- [x] Loop de cascading (matches encadenados)
- [x] Revert de swap cuando no hay match
- [x] Object pooling para piezas
- [x] GameManager como orquestador (Singleton)
- [x] Navegación Hall → Gameplay

---

## Fase 2: Scoring y UI de Gameplay 🔲 PENDIENTE
Sistema de puntuación y feedback visual durante la partida.

### Tareas:
- [ ] **ScoreManager** - Sistema de puntuación
  - Puntos por match (base por 3, bonus por 4+)
  - Multiplicador por combos en cascada
  - Evento OnScoreChanged para UI
- [ ] **UI de Gameplay** - Canvas con info de partida
  - Score display (texto puntuación actual)
  - Combo counter (multiplicador visual)
  - Botón Pause / menú pausa
- [ ] **Efectos visuales de score**
  - Texto flotante "+100" al hacer match
  - Feedback visual de combo (shake de cámara leve, flash)

---

## Fase 3: Piezas Especiales 🔲 PENDIENTE
Piezas con poderes especiales al hacer matches de 4+ o formas específicas.

### Tareas:
- [ ] **Nuevos PieceTypes** en PieceData
  - Bomba (match de 4 en línea) → destruye área 3x3
  - Rayo horizontal (match de 5) → destruye fila completa
  - Rayo vertical (match de 5) → destruye columna completa
  - Arcoíris (match en L o T) → destruye todas las piezas de un color
- [ ] **Prefabs de piezas especiales** con materiales/sprites diferenciados
- [ ] **Lógica de creación** de piezas especiales al matchear
- [ ] **Lógica de activación** al intercambiar piezas especiales
- [ ] **Animaciones** de activación de poderes

---

## Fase 4: Sistema de Niveles 🔲 PENDIENTE
Progresión con objetivos y dificultad creciente.

### Tareas:
- [ ] **LevelData (ScriptableObject)** - Definición de nivel
  - Objetivo del nivel (puntuación, recolectar X piezas de un color, etc.)
  - Límite de movimientos
  - Tamaño de tablero (variable, no siempre 6x6)
  - Piezas disponibles (3, 4, 5 tipos)
- [ ] **LevelManager** - Gestión de niveles
  - Carga de nivel actual
  - Control de condición de victoria/derrota
  - Transición entre niveles
- [ ] **UI de nivel** 
  - Objetivo visible en pantalla
  - Contador de movimientos restantes
  - Pantalla de victoria / derrota
- [ ] **Selector de niveles** en Hall
  - Grid de niveles con estrellas
  - Niveles bloqueados/desbloqueados

---

## Fase 5: Persistencia y Progreso 🔲 PENDIENTE
Guardado del progreso del jugador.

### Tareas:
- [ ] **SaveManager** - Sistema de guardado (PlayerPrefs o JSON)
  - Nivel máximo desbloqueado
  - Estrellas por nivel
  - High score
  - Monedas/recursos acumulados
- [ ] **Perfil de jugador** básico

---

## Fase 6: Audio 🔲 PENDIENTE
Sonido y música para feedback satisfactorio.

### Tareas:
- [ ] **AudioManager** - Singleton para audio
  - Música de fondo (loop)
  - SFX: swap, match, cascade, special piece, victoria, derrota
  - Volumen configurable
- [ ] **Assets de audio** (placeholder o definitivos)

---

## Fase 7: Polish Visual 🔲 PENDIENTE
Mejora visual del juego (puede hacerse en paralelo con otras fases).

### Tareas:
- [ ] Sprites/modelos 2D en lugar de cubos de color
- [ ] Partículas al destruir piezas
- [ ] Animaciones de UI (score bounce, star animations)
- [ ] Background art para tablero y Hall
- [ ] Transiciones de escena con fade

---

## Fase 8: Monetización y Extras 🔲 PENDIENTE (Opcional/Futuro)

### Tareas:
- [ ] Sistema de vidas / energía
- [ ] Boosters (martillo, shuffle, etc.)
- [ ] Tienda in-game
- [ ] Daily rewards
- [ ] Integración de ads (reward video)

---

## Notas de Implementación
- Cada fase debe ser **funcional antes de pasar a la siguiente**
- Los tests se hacen en Editor y Game View (simular touch)
- Prioridad: jugabilidad > visual > extras
- Siempre MCP para escenas, scripts directos para lógica
