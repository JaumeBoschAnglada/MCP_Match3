# Popup System Architecture

## Overview

La nueva arquitectura de popups centraliza toda la gestión en un **PopupManager singleton** que vive en la escena **Hall** y persiste entre escenas (DontDestroyOnLoad). No duplicar prefabs o managers entre escenas.

---

## Componentes Principales

### 1. PopupManager (Singleton Global)
**Ubicación**: Hall.unity  
**Responsabilidad**: Gestiona TODOS los popups del juego  
**Persistencia**: DontDestroyOnLoad → accesible desde cualquier escena

```csharp
public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance { get; private set; }
    
    [SerializeField] private PopupBase pausePopupPrefab;  // Asignar en Inspector
    // Agregar más tipos cuando se creen...
    
    // Métodos principales:
    public PopupBase ShowPopupByType(PopupType popupType)    // Entrada principal
    public T ShowPopup<T>(T prefabOrInstance) where T : PopupBase
    public void ClosePopup(PopupBase popup)
    public void CloseAllPopups()
}
```

### 2. PopupBase (Clase Base)
**Ubicación**: Common para todos los popups  
**Elementos comunes**:
- ✅ Canvas component
- ✅ Title (Text) - auto-encontrado o asignado
- ✅ Close button - auto-conectado
- ✅ Lifecycle: Initialize, OnShow, OnHide, Close

```csharp
public abstract class PopupBase : MonoBehaviour
{
    [SerializeField] protected Text titleText;
    [SerializeField] protected Button closeButton;
    
    public virtual void Initialize(PopupManager manager)
    public virtual void SetTitle(string title)
    public virtual void OnShow()
    public virtual void OnHide()
    public virtual void Close()
}
```

### 3. PopupType (Enum)
```csharp
public enum PopupType
{
    Pause,        // En Gameplay
    // Shop,      // Para agregar luego
    // Settings,  // Para agregar luego
    // GameOver   // Para agregar luego
}
```

### 4. Canvas Hierarchy (Por Escena)
Cada escena crea/busca dos Canvas:

```
PopupCanvas (sort order: 1) ← Popups aparecen aquí
HUDCanvas   (sort order: 0) ← UI base (puntuación, botones, etc.)
```

---

## Implementación: PausePopup

### Estructura del Prefab
```
PausePopup (Canvas con PopupBase)
├─ Panel (Image - background semitransparente)
│  ├─ Title (Text: "PAUSED")
│  ├─ CloseButton (Button)
│  ├─ ResumeButton (Button)
│  └─ RestartButton (Button)
```

### Código
```csharp
public class PausePopup : PopupBase
{
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;

    public override void OnShow()
    {
        base.OnShow();
        SetTitle("PAUSED");
        
        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResumePressed);
        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartPressed);
    }

    private void OnResumePressed() => Close();
    
    private void OnRestartPressed()
    {
        popupManager.CloseAllPopups();
        GameManager.Instance.LoadLevel(GameManager.Instance.GetCurrentLevelNumber());
    }
}
```

### Flow
1. User clicks Pause button (en Gameplay HUD)
2. `PauseButton.OnPausePressed()` → `popupManager.ShowPopupByType(PopupType.Pause)`
3. PopupManager busca el prefab en su diccionario
4. Instantía bajo PopupCanvas (layer 2)
5. Llama `Initialize()` → `OnShow()`
6. Popup aparece con título "PAUSED"
7. Time.timeScale = 0 (juego pausado)
8. User hace click en Resume/Close
9. PopupBase.Close() → PopupManager.ClosePopup()
10. Destroys popup, resumes time

---

## Flujo de Capas (Canvas)

### En Hall
```
EventSystem
PopupManager (DontDestroyOnLoad)
├─ [Persiste] pausePopupPrefab (asignado en Inspector)
```

### En Gameplay (primera carga)
```
Canvas - HUDCanvas (sort order: 0)
├─ Score display
├─ Movement counter
├─ Pause button ← llama ShowPopupByType(PopupType.Pause)
└─ Otros controles

Canvas - PopupCanvas (sort order: 1, creado por PopupManager)
├─ [Vacío hasta que se abre un popup]
```

### En Gameplay (popup abierto)
```
Canvas - HUDCanvas (sort order: 0)
└─ [Bloqueado por PopupCanvas overlay]

Canvas - PopupCanvas (sort order: 1)
└─ PausePopup instance
   ├─ Panel (overlay)
   ├─ Title: "PAUSED"
   ├─ CloseButton
   ├─ ResumeButton
   └─ RestartButton
```

---

## Setup en Editor

### 1. Hall Scene
- [ ] Crear GameObject "PopupManager" en la raíz
- [ ] Agregar componente `PopupManager`
- [ ] Drag PausePopup prefab al slot `pausePopupPrefab`
- [ ] Verificar que NO hay otro PopupManager (singleton)

### 2. PausePopup Prefab
- [ ] Crear Canvas prefab
- [ ] Agregar PopupBase (PopupBase.cs)
- [ ] Crear Panel con Image + LayoutGroup
  - [ ] Asignar color semitransparente para overlay
- [ ] Crear Title (Text component)
  - [ ] Asignar al slot `titleText` en PopupBase
- [ ] Crear CloseButton (Button)
  - [ ] Nombre: "CloseButton"
  - [ ] Auto-encontrado por PopupBase
- [ ] Crear ResumeButton
  - [ ] Asignar al slot `resumeButton` en PausePopup
- [ ] Crear RestartButton
  - [ ] Asignar al slot `restartButton` en PausePopup

### 3. Gameplay Scene
- [ ] Crear HUDCanvas (sort order: 0)
- [ ] Crear Pause button dentro de HUDCanvas
  - [ ] Script: PauseButton.cs
  - [ ] OnClick() → PauseButton.OnPausePressed()
- [ ] PopupCanvas se crea automáticamente por PopupManager

---

## Agregar Nuevo Popup (ej: Shop)

### 1. Extender PopupType enum
```csharp
public enum PopupType
{
    Pause,
    Shop,  // ← Nuevo
}
```

### 2. Crear ShopPopup.cs
```csharp
public class ShopPopup : PopupBase
{
    [SerializeField] private Button buyButton1;
    [SerializeField] private Button buyButton2;
    
    public override void OnShow()
    {
        base.OnShow();
        SetTitle("SHOP");
        if (buyButton1 != null)
            buyButton1.onClick.AddListener(OnBuy1);
        // etc...
    }
    
    private void OnBuy1() { /* Lógica de compra */ }
}
```

### 3. Asignar a PopupManager
```csharp
public class PopupManager : MonoBehaviour
{
    [SerializeField] private PopupBase pausePopupPrefab;
    [SerializeField] private PopupBase shopPopupPrefab; // ← Agregar
    
    private void InitializePopupPrefabs()
    {
        popupPrefabs[PopupType.Pause] = pausePopupPrefab;
        popupPrefabs[PopupType.Shop] = shopPopupPrefab;  // ← Registrar
    }
}
```

### 4. Crear ShopButton (opcional)
```csharp
public class ShopButton : MonoBehaviour
{
    public void OnShopPressed()
    {
        PopupManager.Instance.ShowPopupByType(PopupType.Shop);
    }
}
```

### 5. Crear ShopPopup prefab y asignar en Inspector

---

## Ventajas del Sistema

✅ **Singleton Global**: Un PopupManager para todo el juego  
✅ **DontDestroyOnLoad**: Persiste entre Hall → Gameplay  
✅ **Centralizado**: Todos los prefabs en un lugar (Hall)  
✅ **Type-Safe**: PopupType enum previene errores de typos  
✅ **Loose Coupling**: Botones no referencian prefabs  
✅ **Anidable**: Popups pueden estar sobre otros popups  
✅ **Auto-Wiring**: Close button y title se conectan automáticamente  
✅ **Escalable**: Fácil agregar nuevos popup types  

---

## Troubleshooting

### ❌ "PopupManager not found"
→ Asegúrate de que PopupManager está en Hall.unity  
→ Verifica que tiene el componente PopupManager.cs  

### ❌ "No prefab registered for popup type"
→ Arrastra el prefab al slot correspondiente en Inspector  
→ Verifica que InitializePopupPrefabs() lo registra  

### ❌ Popup no aparece
→ Chequea que PopupCanvas existe (se crea automáticamente)  
→ Verifica que prefab tiene Canvas component  
→ Revisa console para logs de error  

### ❌ Popup no responde a clicks
→ Asegúrate que GraphicRaycaster está en PopupCanvas  
→ Verifica que EventSystem existe en la escena  

### ❌ Botones de papúp no se llaman
→ PopupBase auto-conecta Close button  
→ Asegúrate de asignar otros botones en Inspector o FindByName  
→ Llama a base.OnShow() en las subclases  

---

## Convenciones

- Todos los popups heredan de `PopupBase`
- Nombres de botones: `{Action}Button` (ej: ResumeButton, RestartButton)
- Close button siempre se llama "CloseButton"
- Title siempre es el primer Text component
- Popup prefabs en: `Assets/Prefabs/UI/`
- Scripts en: `Assets/Scripts/UI/`
- PopupManager vive en Hall, NUNCA duplicar
