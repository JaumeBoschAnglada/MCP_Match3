# Popup System Refactor

## Overview
The popup system has been refactored to centralize popup prefab management in the **PopupManager**, rather than distributing prefab references across individual buttons.

## Architecture

### Canvas Structure (Required Setup)
The system now uses **two Canvas layers**:

1. **Canvas 1 - HUDCanvas** (Sort Order: 0)
   - Contains base HUD elements
   - Score display
   - Movement counter
   - Pause button
   - Other permanent UI controls

2. **Canvas 2 - PopupCanvas** (Sort Order: 1)
   - Contains all popups and modals
   - Pause popup
   - Shop popup (when added)
   - Settings popup (when added)
   - Game Over popup (when added)
   - Any overlay elements

### Key Changes

#### Before (❌ Old Pattern)
```csharp
// In PauseButton.cs
[SerializeField] private PausePopup pausePopupPrefab; // ← Direct prefab reference
popupManager.ShowPopup(pausePopupPrefab);
```

#### After (✅ New Pattern)
```csharp
// In PauseButton.cs
// NO prefab reference
public void OnPausePressed()
{
    popupManager.ShowPopupByType(PopupType.Pause); // ← Just pass the type
}
```

### PopupManager Setup

#### 1. Assign Prefabs in Inspector
The **PopupManager** GameObject in the scene has inspector slots for each popup type:
- **Pause Popup Prefab** ← Drag your PausePopup prefab here
- Add more slots as you create new popup types (Shop, Settings, etc.)

#### 2. Implementation
```csharp
public class PopupManager : MonoBehaviour
{
    [SerializeField] private PausePopup pausePopupPrefab;
    // Add more SerializeFields for other popup types
    
    private Dictionary<PopupType, PopupBase> popupPrefabs;
    
    private void Start()
    {
        InitializePopupPrefabs(); // Maps prefab references to types
        SetupCanvases();           // Creates HUDCanvas + PopupCanvas
    }
    
    // Main entry point for buttons
    public PopupBase ShowPopupByType(PopupType popupType)
    {
        // Internally looks up the prefab and shows it
    }
}
```

## Adding New Popup Types

### Step 1: Extend PopupType Enum
```csharp
// PopupType.cs
public enum PopupType
{
    Pause,
    Shop,        // ← Add new type
    Settings,    // ← Add new type
    GameOver,    // ← Add new type
}
```

### Step 2: Create Your Popup Script
```csharp
public class ShopPopup : PopupBase
{
    // Implement OnShow(), OnHide(), button handlers, etc.
}
```

### Step 3: Add Prefab Reference to PopupManager
```csharp
public class PopupManager : MonoBehaviour
{
    [SerializeField] private PausePopup pausePopupPrefab;
    [SerializeField] private ShopPopup shopPopupPrefab;      // ← Add here
    [SerializeField] private SettingsPopup settingsPopupPrefab; // ← Add here
    
    private void InitializePopupPrefabs()
    {
        if (pausePopupPrefab != null)
            popupPrefabs[PopupType.Pause] = pausePopupPrefab;
        if (shopPopupPrefab != null)
            popupPrefabs[PopupType.Shop] = shopPopupPrefab;  // ← Register
        if (settingsPopupPrefab != null)
            popupPrefabs[PopupType.Settings] = settingsPopupPrefab; // ← Register
    }
}
```

### Step 4: Update Your Button
```csharp
public class ShopButton : MonoBehaviour
{
    private PopupManager popupManager;
    
    public void OnShopPressed()
    {
        popupManager.ShowPopupByType(PopupType.Shop); // ← Simple call
    }
}
```

## Flow Diagram

```
User clicks "Pause Button"
        ↓
PauseButton.OnPausePressed()
        ↓
PopupManager.ShowPopupByType(PopupType.Pause)
        ↓
PopupManager looks up "Pause" in popupPrefabs dictionary
        ↓
Instantiates prefab under PopupCanvas (layer 2)
        ↓
Popup appears on top of HUD
        ↓
Time pauses (Time.timeScale = 0)
        ↓
User interacts with popup...
        ↓
PopupBase.Close() → PopupManager.ClosePopup()
        ↓
Destroys popup, resumes time
```

## Benefits

✅ **Centralized Management**: All popup prefabs in one place (PopupManager)  
✅ **Loose Coupling**: Buttons don't reference prefabs  
✅ **Easy to Extend**: Add new popup types without modifying existing buttons  
✅ **Layer Separation**: Clear visual separation between HUD and popups  
✅ **Type Safety**: PopupType enum prevents typos and mistakes  
✅ **Consistent Behavior**: All popups follow the same lifecycle  

## Scene Setup Checklist

- [ ] Create GameManager > PopupManager script with prefab references
- [ ] Create HUDCanvas for base UI (sort order 0)
- [ ] Create PopupCanvas for popups (sort order 1)
- [ ] Assign PausePopup prefab to PopupManager inspector
- [ ] Verify PauseButton calls `ShowPopupByType(PopupType.Pause)`
- [ ] Test pause functionality

## Notes

- PopupManager is a singleton (only one instance per scene)
- Popups automatically pause the game (Time.timeScale = 0)
- Game resumes when all popups are closed (Time.timeScale = 1)
- Popups can be stacked (e.g., Shop popup on top of Pause popup)
- Close button in a popup triggers `PopupBase.Close()` automatically
