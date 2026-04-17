# Interacción Completa: Del Toque a la Vuelta al Reposo

> Este documento describe paso a paso qué sucede exactamente desde que el jugador toca la pantalla hasta que puede volver a tocar. Se incluyen las clases responsables, los métodos invocados y el flujo de datos.

---

## Diagrama de flujo resumido

```
JUGADOR TOCA PIEZA A
        │
        ▼
  Item.OnMouseDown()         ← Registra Swap_A, SwitchStart = true
        │
        ▼
JUGADOR ARRASTRA A PIEZA B
        │
        ▼
  Item.OnMouseEnter()        ← Valida adyacencia, registra Swap_B
        │
        ▼
  MatchManager.Switching()   ← Inicia corrutina de intercambio
        │
        ├──────────────────────────────────────┐
        ▼                                      ▼
  ¿CheckCombine?                         Intercambio normal
  (especial + especial)                        │
        │                                      ▼
        ▼                                ¿Hay matches?
  CombineBrust()                         ├── NO → Revertir posiciones
        │                                └── SÍ ↓
        ▼                                      │
  [Explosión combinada]              Swap boards, marcar m_MatchingCheck
        │                                      │
        └──────────────┬───────────────────────┘
                       ▼
              SetStep(Matching)
                       │
                       ▼
            ╔══════════════════════════╗
            ║   LOOP DE MATCHING       ║
            ║   (ejecuta cada frame)   ║
            ╠══════════════════════════╣
            ║ 1. CheckFood()           ║
            ║ 2. Drop() (gravedad)     ║
            ║ 3. CheckMatchCondition() ║
            ║ 4. CheckRing()           ║
            ║ 5. ¿Todo terminó?        ║
            ║    NO → repetir          ║
            ║    SÍ → siguiente step   ║
            ╚══════════╤═══════════════╝
                       ▼
              SetStep(TimeBomb)
                       ▼
              SetStep(IceCream)
                       ▼
              SetStep(ConveyerBelt)
                       ▼
              SetStep(Chameleon)
                       ▼
              SetStep(MagicColor)
                       ▼
              SetStep(BearJump)
                       ▼
              SetStep(BearSpawn)
                       ▼
              SetStep(Mission)
                       │
            ┌──────────┼──────────┐
            ▼          ▼          ▼
         Clear?      Fail?    Ninguno
            │          │          │
            ▼          ▼          ▼
        BonusTime   GameFail   Wait ← EL JUGADOR PUEDE TOCAR DE NUEVO
```

---

## Fase 1: Toque inicial (`OnMouseDown`)

**Archivo:** `Item.cs`, líneas 177-206  
**Condiciones previas que se validan:**

```
1. MatchState == Playing       → Si no, se ignora el toque
2. StepType == Wait            → Si no, se ignora (hay animación en curso)
3. SwitchStart == false        → Si ya hay un intercambio en curso, se ignora
```

**Qué ocurre:**

```csharp
case TouchState.Switching:
    Item.Swap_A = this;           // Se registra la pieza tocada como origen
    Item.SwitchStart = true;      // Se activa el flag de arrastre
    Item.SwitchingTouch = false;  // Aún no se ha completado el gesto
    break;
```

**Variables estáticas involucradas:**
- `Item.Swap_A` — Pieza origen del intercambio
- `Item.Swap_B` — Pieza destino (se asigna después)
- `Item.SwitchStart` — Flag que indica que el jugador está arrastrando
- `Item.SwitchingTouch` — Se pone a `true` cuando el intercambio se confirma

**Tiempo transcurrido:** Instantáneo (1 frame)

---

## Fase 2: Arrastre a pieza adyacente (`OnMouseEnter`)

**Archivo:** `Item.cs`, líneas 208-241

Cuando el dedo (o cursor) entra en el collider de otra pieza mientras `SwitchStart == true`:

```csharp
case TouchState.Switching:
    if (SwitchStart && Swap_A != this && Swap_A.CheackNeighbor(this))
    {
        Item.Swap_B = this;               // Registra pieza destino
        Item.SwitchStart = false;          // Desactiva el arrastre
        Item.SwitchingTouch = true;        // Confirma intercambio
        this.m_MatchMgr.Switching(Swap_A, Swap_B);  // ¡Inicia el intercambio!
    }
    break;
```

### 2.1. Validación de adyacencia (`CheackNeighbor`)

```csharp
public bool CheackNeighbor(Item _item)
{
    return (m_Board[SQR_DIR.TOP]?.m_Item == _item) ||
           (m_Board[SQR_DIR.BOTTOM]?.m_Item == _item) ||
           (m_Board[SQR_DIR.LEFT]?.m_Item == _item) ||
           (m_Board[SQR_DIR.RIGHT]?.m_Item == _item);
}
```

Solo se permiten intercambios con las **4 celdas cardinales** (arriba, abajo, izquierda, derecha). No hay intercambio diagonal.

**Tiempo transcurrido:** Instantáneo (depende de la velocidad del gesto del jugador)

---

## Fase 3: Corrutina de intercambio (`Coroutine_Switching`)

**Archivo:** `MatchManager.cs`, líneas 1039-1173

Esta es la fase más compleja. Se bifurca en dos caminos:

### 3A. Combinación de piezas especiales

**Condición:** `A.CheckCombine(B.m_ItemType) || B.CheckCombine(A.m_ItemType)`

Ocurre cuando al menos una de las dos piezas es especial y puede combinarse con la otra (ej: Bomba + Rainbow).

**Flujo:**

1. **Animación de movimiento** (~0.1-0.2s):
   - La pieza A se mueve hacia B con `Vector3.Lerp`.
   - Se aplica un efecto de escala (1.5x → 1x).
   - La pieza B recibe un offset en Z para quedar "debajo" visualmente.

2. **Explosión combinada:**
   ```csharp
   if (checkCombine_A)
       A.m_Board.CombineBrust(B.m_ItemType);
   else
       B.m_Board.CombineBrust(A.m_ItemType);
   ```

3. **Contadores de turno:**
   - `MissionManager.MoveLimitApply()` → Resta 1 movimiento
   - `MissionManager.MissionInterval()` → Actualiza intervalos de misión
   - Intervalos de Donut, Spiral, TimeBomb, Mystery, Chameleon, Key
   - `SwitchingApply` delegate (si existe)

4. **Transición:**
   ```csharp
   BaseStep.isItemStep = true;
   MatchManager.Instance.SetStep(StepType.Matching);
   ```

**Duración estimada:** ~0.15-0.3 segundos (animación de movimiento)

### 3B. Intercambio normal

**Condición:** Ninguna pieza es especial combinable.

**Flujo:**

1. **Pre-evaluación de matches:**
   ```csharp
   A.m_Board.m_Item = B;    // Intercambia temporalmente
   B.m_Board.m_Item = A;
   int matchesA = A.m_Board.FindMatchesAround(2).Count;
   int matchesB = B.m_Board.FindMatchesAround(2).Count;
   ```

2. **Animación de intercambio** (~0.1-0.2s):
   - Ambas piezas se mueven simultáneamente con `Vector3.Lerp`.
   - A recibe un offset Z de -0.3 (aparece "encima" de B).
   - Efecto de escala en A (1.5x → 1x).

3. **¿Hay matches?**

   - **NO hay matches** → **Revertir:**
     ```csharp
     A.m_Board.m_Item = A;    // Deshace el intercambio
     B.m_Board.m_Item = B;
     // Animación de retorno (~0.1-0.2s adicionales)
     ```
     El jugador puede tocar de nuevo inmediatamente después de la animación de retorno. **No se gasta movimiento.**

   - **SÍ hay matches** → **Confirmar:**
     ```csharp
     // Intercambiar las referencias de Board
     Board board = A.m_Board;
     A.m_Board = B.m_Board;
     B.m_Board = board;
     A.m_Board.m_MatchingCheck = true;   // Marcar para verificación
     B.m_Board.m_MatchingCheck = true;
     
     MissionManager.Instance.MoveLimitApply();  // Resta movimiento
     // ... intervalos ...
     MatchManager.Instance.SetStep(StepType.Matching);
     ```

**Duración estimada:** 
- Con match: ~0.15-0.3s
- Sin match (revertido): ~0.3-0.5s (ida + vuelta)

---

## Fase 4: Matching Step — El loop principal

**Archivo:** `MatchingStep.cs`, líneas 19-68

Mientras `StepType == Matching`, se ejecuta `DefaultProcess()` **cada frame**:

```
┌─────────────────────────────────────────────────┐
│ DefaultProcess() — ejecutado cada Update()       │
│                                                  │
│ 1. CheckFood()        ← Comida llega a destino  │
│ 2. Drop()             ← Gravedad, caída, spawn  │
│ 3. CheckMatchCondition() ← Buscar nuevos matches│
│ 4. CheckRing()         ← Anillos recogen piezas │
│ 5. ¿Todo ha terminado?                          │
│    - Para cada Board: ¿m_ItemBrusting?           │
│                        ¿m_PanelBrusting?         │
│                        ¿m_DropAnim?              │
│    - Si alguno está activo → AllEnd = false       │
│    - Si MatchBrust → ComboPlus, reset            │
│    - Si AllEnd == true → SetStep(TimeBomb)        │
└─────────────────────────────────────────────────┘
```

### 4.1. Drop (Gravedad)

En cada frame del matching, se llama a `GravityDropItemRow` desde las celdas inferiores del tablero. El proceso:

1. Busca celdas vacías.
2. Si hay una pieza arriba que puede caer, la mueve hacia abajo (con animación `DropAnim`).
3. Si no hay pieza arriba, busca en diagonal (side drop).
4. Si es una celda superior sin nada arriba, genera una pieza nueva (spawn).
5. Se aplica un ticTok para alternar el orden de procesamiento y distribuir equitativamente.

### 4.2. CheckMatchCondition

Se itera por prioridad de pieza especial (de Rainbow=7 a Normal=0):

1. Para cada celda con `m_MatchingCheck == true` que no esté animándose:
   - `FindMatchesHorizontal()` → lista de matches horizontales
   - `FindMatchesVertical()` → lista de matches verticales  
   - `FindMatchesSquare()` → matches en cuadrado 2×2
2. `SpecailItemCondition()` evalúa el patrón y decide si crear pieza especial.
3. Celdas en match se marcan con `m_isMatchBrust = true`.
4. `MatchBrust()` ejecuta la explosión de todas las marcadas.

### 4.3. Explosión de una pieza (`Board.Brust`)

```
Board.Brust()
    └── Co_Brust()
        ├── m_ItemBrusting = true
        ├── Item.Brust(Complete)           ← Efecto visual + sonido
        │   └── Complete callback:
        │       ├── Acumula puntuación
        │       ├── PanelBrust()           ← Daña paneles de la celda
        │       └── Co_ItemBrustComplete()
        │           ├── AroundBrust()      ← Daña piezas/paneles vecinos
        │           ├── MissionApply()     ← Notifica al sistema de misiones
        │           ├── ScoreApply()       ← Suma puntuación
        │           └── Genera NextItem si corresponde (pieza especial)
        │               └── m_ItemBrusting = false
        └── Si no hay ítem → PanelBrust() directo
```

### 4.4. Reacción en cadena

Las explosiones pueden:
- Destruir piezas adyacentes (`AroundBrust`) que a su vez explotan.
- Generar piezas especiales que explotan inmediatamente si caen en un match.
- Las piezas nuevas que caen pueden formar nuevos matches → nuevo ciclo.

**El MatchingStep se queda en loop hasta que `AllEnd == true`**: no hay ninguna celda con explosión, caída o panel destruyéndose.

### 4.5. Combo

Cada vez que `MatchBrust` confirma una explosión, se incrementa `ComboCnt`. El combo afecta la puntuación de las piezas:

```csharp
public override int GetDestoryScore(bool combo)
{
    if (combo) return m_MatchMgr.ComboCnt * DestoryScore;
    return DestoryScore;
}
```

Al terminar el matching step, se muestra el texto de combo si es ≥ 2.

**Duración estimada:** Variable. Un match simple puede durar ~0.5-1s. Una reacción en cadena larga puede durar varios segundos.

---

## Fase 5: Post-Match Steps (procesamiento secuencial)

Una vez que `AllEnd == true` en MatchingStep, se ejecutan los siguientes steps **en secuencia**. Cada uno se ejecuta UNA vez en `Step_Play()` y puede necesitar varios frames en `Step_Process()`.

### 5.1. TimeBomb Step

**Archivo:** `TimeBombStep.cs`

- Si no hay bombas de tiempo (`TimeBombItem.m_Count <= 0`) → pasa a IceCream.
- Para cada bomba de tiempo: decrementar contador.
- Si alguna llega a 0 → marcar `IsTimeBombOver = true` (se resolverá en MissionStep).
- Transiciona a `IceCream`.

**Duración:** 1 frame (instantáneo)

### 5.2. IceCream Step

**Archivo:** `IceCreamStep.cs`

- Si no hay paneles de helado → pasa a ConveyerBelt.
- Cada N turnos (configurable por nivel), el helado se expande a una celda adyacente aleatoria.
- Si hubo explosión de helado este turno, resetea intervalos.
- Transiciona a `ConveyerBelt`.

**Duración:** 1 frame (instantáneo, la animación es visual)

### 5.3. ConveyerBelt Step

**Archivo:** `ConveyerBeltStep.cs`

- Si no hay cintas transportadoras → pasa a Chameleon.
- Mueve todas las piezas en las cintas un paso en su dirección configurada.
- Espera en `Step_Process()` hasta que `MoveCnt <= 0` (todas las animaciones terminaron).
- Transiciona a `Chameleon`.

**Duración:** ~0.2-0.5s (animación de movimiento de piezas en cinta)

### 5.4. Chameleon Step

**Archivo:** `ChameleonStep.cs`

- Si no hay camaleones → pasa a MagicColor.
- Cada camaleón cambia a un color aleatorio diferente al actual (con animación).
- Marca la celda con `m_MatchingCheck = true` (por si el nuevo color genera match).
- Espera hasta que todas las animaciones terminen.
- Transiciona a `MagicColor`.

**Duración:** ~0.3-0.6s (animación de cambio de color)

### 5.5. MagicColor Step

**Archivo:** `MagicColorStep.cs`

- Si no hay celdas con MagicColorPanel → pasa a BearJump.
- Para cada pieza en una celda mágica: rota y cambia de color (animación DOTween ~0.6s).
- Marca `m_MatchingCheck = true`.
- Transiciona a `BearJump`.

**Duración:** ~0.6s (animación de rotación + cambio de color)

### 5.6. BearJump Step

**Archivo:** `BearJumpStep.cs`

- Si no hay osos de gelatina → pasa a BearSpawn.
- Cada oso intenta saltar una celda hacia arriba (en dirección de gravedad inversa).
- Si el oso llega al borde del tablero sin poder saltar más → se convierte en pieza normal y se "rescata".
- Si la misión de osos ya se completó → todos los osos se convierten en piezas normales.
- Espera hasta que todas las animaciones de salto terminen.
- Transiciona a `BearSpawn`.

**Duración:** ~0.3-0.5s por salto

### 5.7. BearSpawn Step

**Archivo:** `BearSpawnStep.cs`

- Si no hay puntos de spawn de osos → pasa a Mission.
- Cada N turnos (configurable), si no se ha alcanzado el máximo de osos, genera un nuevo oso en un punto de spawn aleatorio.
- El oso reemplaza la pieza existente en esa celda.
- Transiciona a `Mission`.

**Duración:** 1 frame (instantáneo, la animación es visual)

---

## Fase 6: Mission Step — Decisión final

**Archivo:** `MissionStep.cs`

Este es el step que decide el destino del turno:

```csharp
public override void Step_Play()
{
    BaseStep.isItemStep = false;
    
    // 1. ¿Hay celdas pendientes de verificación?
    foreach (Board board in MatchMgr.m_ListBoard)
    {
        if (board.m_MatchingCheck)
        {
            MatchMgr.SetStep(StepType.Matching);  // ← Vuelve al loop
            return;
        }
    }
    
    // 2. ¿Misión completada?
    if (MissionManager.Instance.CheckMissionClear())
    {
        MatchMgr.SetStep(StepType.Clear);   // → BonusTime → GameClear
        return;
    }
    
    // 3. ¿Misión fallida?
    if (MissionManager.Instance.CheckMissionFail())
    {
        MissionManager.Instance.MissionFailAnimation(() => {
            MatchMgr.SetStep(StepType.Fail);  // → GameFail
        });
        return;
    }
    
    // 4. Nada especial → devolver control al jugador
    MatchMgr.SetStep(StepType.Wait);          // ← EL JUGADOR PUEDE TOCAR
}
```

**Casos posibles:**

| Condición | Resultado | El jugador puede tocar |
|---|---|---|
| Hay celdas con `m_MatchingCheck` (ej: camaleón generó match) | → Vuelve a `Matching` | ❌ No |
| Todos los objetivos cumplidos | → `Clear` → `BonusTime` → `GameClear` | ❌ No (partida terminada) |
| Sin movimientos o bomba explotó | → `Fail` → `GameFail` | ❌ No (partida terminada) |
| Nada de lo anterior | → `Wait` | ✅ **SÍ** |

---

## Fase 7: Vuelta al reposo (`WaitStep`)

**Archivo:** `WaitStep.cs`

### 7.1. `Step_Play()` — Se ejecuta una vez al entrar

1. `ShufflingCheck()` — Verifica que exista al menos un movimiento válido. Si no → `Shuffling`.
2. `FocusShow()` — Si es la primera vez, muestra foco visual en una pieza sugerida.
3. `ShowGravity()` — Si la gravedad no es estándar, muestra indicador visual.
4. `VisibleAdRewardBox()` — Muestra la caja de recompensa si aplica.
5. Resetea el temporizador de hint.

### 7.2. `Step_Process()` — Se ejecuta cada frame

```csharp
// Temporizador de hint
hinttime += Time.deltaTime;
if (hinttime > 5f)
{
    MatchMgr.HintOffer();   // Sugiere un movimiento visualmente
}

// Temporizador de indicador de gravedad
GravityShowTime += Time.deltaTime;
if (GravityShowTime > 8f && !GravityDisplayer.useDefaultGravity)
{
    MatchMgr.ShowGravity();
}
```

### 7.3. Estado del sistema al llegar a Wait

En este punto:
- `MatchState == Playing` ✅
- `StepType == Wait` ✅
- `SwitchStart == false` ✅
- No hay animaciones en curso ✅
- El input en `OnMouseDown` pasará todas las validaciones ✅

**→ El jugador puede tocar la pantalla de nuevo.**

---

## Resumen de tiempos

| Fase | Duración aproximada |
|---|---|
| Toque + arrastre | Depende del jugador (~0.1-0.5s) |
| Animación de intercambio | ~0.15-0.3s |
| Matching (match simple) | ~0.5-1s |
| Matching (cadena larga) | 2-10s |
| TimeBomb check | Instantáneo |
| IceCream expansion | Instantáneo (visual ~0.3s) |
| ConveyerBelt | ~0.2-0.5s |
| Chameleon | ~0.3-0.6s |
| MagicColor | ~0.6s |
| BearJump | ~0.3-0.5s |
| BearSpawn | Instantáneo |
| Mission check | Instantáneo |
| **Total mínimo (match simple sin mecánicas especiales)** | **~0.8-1.5s** |
| **Total con todas las mecánicas activas** | **~3-15s** |

---

## Diagrama de clases involucradas

```
┌──────────────────┐     ┌──────────────┐     ┌──────────────────┐
│   Item            │────▶│   Board       │────▶│   Panel          │
│                   │     │              │     │                  │
│ OnMouseDown()     │     │ m_Item       │     │ Defence          │
│ OnMouseEnter()    │     │ m_ListPanel  │     │ ItemExist/Drop/  │
│ OnMouseUp()       │     │ m_DropAnim   │     │ Match/Switch     │
│ Brust()           │     │ Brust()      │     │ Brust()          │
│ CombineBrust()    │     │ FindMatches* │     └──────────────────┘
│ CheckCombine()    │     │ GravityDrop* │
│ CheackNeighbor()  │     └──────┬───────┘
└──────────────────┘            │
                                │
┌──────────────────┐     ┌──────▼───────┐     ┌──────────────────┐
│ MissionManager    │◀────│MatchManager  │────▶│   BaseStep       │
│                   │     │              │     │                  │
│ CheckMissionClear │     │ m_ListBoard  │     │ WaitStep         │
│ CheckMissionFail  │     │ SetStep()    │     │ MatchingStep     │
│ MoveLimitApply    │     │ Switching()  │     │ TimeBombStep     │
│ MissionApply      │     │ MatchBrust() │     │ IceCreamStep     │
└──────────────────┘     │ BonusTime()  │     │ ConveyerBeltStep │
                         └──────────────┘     │ ChameleonStep    │
                                              │ MagicColorStep   │
┌──────────────────┐     ┌──────────────┐     │ BearJumpStep     │
│ ItemManager       │     │EffectManager │     │ BearSpawnStep    │
│                   │     │              │     │ MissionStep      │
│ CreateItem()      │     │ PieceEffect  │     │ ClearStep        │
│ Intervals         │     │ SwitchEffect │     │ FailStep         │
└──────────────────┘     │ LineBomb...  │     │ ShufflingStep    │
                         └──────────────┘     └──────────────────┘
```

---

## Notas importantes

1. **El input NO se gestiona con el sistema de Input de Unity (InputSystem/InputManager)**. Se usan los callbacks legacy de MonoBehaviour (`OnMouseDown/Enter/Up`) que funcionan tanto con mouse como con touch (un toque = un click de ratón).

2. **No hay cola de inputs**. Si el jugador toca mientras hay una animación, el toque se ignora silenciosamente gracias a las guardas en `OnMouseDown`.

3. **El sistema es puramente basado en movimientos**, no en tiempo. No hay temporizador de partida (excepto las bombas de tiempo que son mecánicas de nivel, no un reloj global).

4. **Las reacciones en cadena (combos) no gastan movimiento**. Solo el intercambio inicial del jugador resta un movimiento.

5. **Los Steps post-match pueden generar nuevos matches** (ej: el Camaleón cambia a un color que forma 3 en línea). En ese caso, `MissionStep` detecta las celdas marcadas y vuelve a enviar a `MatchingStep`, creando un ciclo adicional invisible para el jugador.
