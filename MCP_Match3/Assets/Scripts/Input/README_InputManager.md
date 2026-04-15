# InputManager - Sistema de Input con Unity Input System

## 📱 Descripción

InputManager usa el **nuevo Input System de Unity** para proporcionar input unificado que funciona automáticamente en **desktop (mouse)** y **móviles (touch)** sin código específico de plataforma.

## 🎯 Características

- ✅ **Unity Input System** - Usa el sistema moderno basado en eventos
- ✅ **Cross-platform automático** - `<Pointer>` funciona para mouse Y touch
- ✅ **Event-driven** - Callbacks en lugar de polling, mejor performance
- ✅ **Raycast 3D** - Detecta items mediante `Physics.Raycast` desde la cámara
- ✅ **Swaps direccionales** - Solo permite intercambios en 4 direcciones cardinales
- ✅ **Distancia mínima configurable** - Evita swaps accidentales (25px por defecto)
- ✅ **Singleton** - Una sola instancia global
- ✅ **Integración con MatchManager** - Llama directamente a `Switching(itemA, itemB)`

## 🔧 Configuración Requerida

### Player Settings

El proyecto debe tener **Active Input Handling** configurado en **"Input System Package (New)"**:

1. Edit → Project Settings → Player
2. Other Settings → Configuration
3. Active Input Handling → **Input System Package (New)**

**Archivo:** `ProjectSettings/ProjectSettings.asset`
```yaml
activeInputHandler: 1  # 0=Old, 1=New, 2=Both
```

### Assets Requeridos

**Input Actions Asset:** `Assets/Settings/Match3InputActions.inputactions`

Este asset define:
- **Gameplay** action map
- **Point** action → `<Pointer>/position` (posición del mouse/touch)
- **Press** action → `<Pointer>/press` (click/tap)
- **Delta** action → `<Pointer>/delta` (movimiento)

### Escena

InputManager debe estar en la escena con el Input Actions asset asignado:

```
CommonManager
├── ObjectPool
├── ItemManager
└── InputManager ← REQUERIDO
    └── m_InputActions: Match3InputActions
    └── m_MinDragDistance: 25
```

## 🎮 Funcionamiento

### Sistema basado en eventos:

1. **Press Started** → Raycast en posición del pointer → Guarda `SwapA` e inicia drag
2. **Press Canceled** → Calcula delta del drag → Detecta dirección → Ejecuta swap si es válido

El sistema **NO usa Update()**, todo funciona mediante callbacks del Input System.

### Detección automática:
- **En PC/Editor**: `<Pointer>` = Mouse
- **En móviles**: `<Pointer>` = Touch
- **Sin código específico de plataforma** - Unity lo maneja automáticamente

## 📐 Direcciones Detectadas

El sistema calcula el drag delta al soltar (Press Canceled) y detecta la dirección **dominante**:

- **Horizontal** (|deltaX| > |deltaY|):
  - Derecha (deltaX > 0) → Swap con `Board.Right`
  - Izquierda (deltaX < 0) → Swap con `Board.Left`

- **Vertical** (|deltaY| > |deltaX|):
  - Arriba (deltaY > 0) → Swap con `Board.Top`
  - Abajo (deltaY < 0) → Swap con `Board.Bottom`

**Nota:** Screen Y está invertido (arriba = positivo en Input System)

## 🔍 Requisitos Técnicos

### Cámara
- Debe existir una `Main Camera` con tag "MainCamera"
- Posición típica: `(0, 0, -10)` para vista 2D/3D ortográfica

### Items
- Deben tener `BoxCollider` (3D, no 2D)
- Collider debe estar enabled
- Items deben implementar `Match3.Items.Item` o heredar de él

### Layers
- Los items deben estar en un layer visible para la cámara (culling mask)
- Por defecto, layer 0 (Default) funciona correctamente

## 🐛 Troubleshooting

### "No detecta clicks/toques"
1. Verifica que Input Actions asset está asignado en InputManager
2. Verifica que existe Main Camera con tag correcto
3. Verifica que los items tienen `BoxCollider` enabled
4. Abre Input Debugger (Window → Analysis → Input Debugger) y verifica que las actions se activan

### "Actions no se activan"
1. Verifica que el Input Actions asset está **habilitado**
2. InputManager llama `m_InputActions.Enable()` en `OnEnable()`
3. Revisa Input Debugger para ver si los eventos llegan

### "Swaps no se ejecutan"
1. Verifica que `MatchManager.Instance` existe
2. Verifica que `m_MatchState == Playing` y `m_StepType == Wait`
3. Verifica que los items tienen `m_Board` asignado correctamente
4. Verifica que el drag supera `m_MinDragDistance` (25px por defecto)

## 📊 Código de Ejemplo

```csharp
// El InputManager funciona automáticamente, pero puedes:

// Deshabilitar input temporalmente:
InputManager.Instance.enabled = false;

// Rehabilitar:
InputManager.Instance.enabled = true;

// Cambiar distancia mínima de drag en runtime:
// (Accediendo al campo serializado - requiere hacerlo público)
```

## 🔄 Ventajas vs Input Manager Viejo

| Input Manager Viejo | Input System Nuevo |
|---------------------|-------------------|
| `Update()` polling | Event callbacks |
| `#if UNITY_ANDROID` | Cross-platform automático |
| `Input.GetMouseButtonDown()` | `InputAction.started` |
| Touch y Mouse separados | `<Pointer>` unificado |
| Performance variable | Mejor performance |

## ⚙️ Configuración Avanzada

### Ajustar sensibilidad del drag:

En el Inspector del InputManager, cambia:
- `m_MinDragDistance` (por defecto: 25px)

### Añadir más actions:

1. Abre `Match3InputActions.inputactions` en Unity
2. Edit Asset
3. Añade nuevas actions (ej: "Cancel", "Rotate", etc.)
4. Suscríbete en `InputManager.Awake()`

## 🧪 Testing

### En Editor:
1. Da Play
2. Abre **Window → Analysis → Input Debugger**
3. Verifica que "Press" y "Point" se activan al hacer clic
4. Haz clic en una pieza y arrastra
5. Suelta - debería swap si hay vecino válido

### En Build Android/iOS:
1. Build el proyecto (asegúrate de que Input System está en Player Settings)
2. Instala en dispositivo
3. Toca y arrastra una pieza
4. Levanta el dedo - debería swap

## 📝 Notas Importantes

- **Requiere Unity 2019.3+** con Input System package instalado
- **No necesita reinicio** de Unity después de cambios (a diferencia del viejo sistema)
- **Funciona con Physics.Raycast** (3D) - No funciona con Raycast2D
- **Swaps diagonales NO soportados** (solo 4 direcciones cardinales)
- **La detección es al soltar** (Press Canceled), no durante el drag

## 🔗 Archivos Relacionados

- `Assets/Scripts/Input/InputManager.cs` - Código principal
- `Assets/Settings/Match3InputActions.inputactions` - Input Actions asset
- `Assets/Scripts/Core/MatchManager.cs` - Método `Switching(Item, Item)`
- `ProjectSettings/ProjectSettings.asset` - activeInputHandler: 1
