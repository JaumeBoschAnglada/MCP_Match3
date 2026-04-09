# MCP_Match3 - Project Guidelines

## MCP Integration Requirements

⚠️ **IMPORTANT**: The Model Context Protocol (MCP) server **MUST be connected** to interact with Unity Editor in this project.

### Prerequisites
- Unity Editor 6000.3.2f1 or compatible
- MCP Server running and active
- MCPForUnity package installed in the project

### Why MCP is Required
The project uses MCP tools to:
- Interact with the Unity Editor from external agents
- Create and modify GameObjects in scenes
- Manage prefabs and assets
- Configure scenes and UI elements
- Execute custom tools and menu items

### MCP Connection Status
Before performing any Unity-related operations:
1. Verify MCP server is running
2. Check that Unity Editor is connected to the MCP server
3. Ensure the active Unity instance is properly set if multiple instances are running

### Project Structure
```
Assets/
├── Scripts/
│   ├── UI/               # UI controllers and popup system
│   ├── Gameplay/         # Game mechanics and piece logic
│   ├── Data/            # Game data structures
│   ├── Loaders/         # Scene and level loaders
│   ├── Scenes/          # Scene managers
│   ├── Input/           # Input handling
│   └── Core/            # Core game logic
├── Scenes/
│   ├── Hall.unity       # Main menu scene
│   └── Gameplay.unity   # Gameplay scene
├── Prefabs/
│   ├── Pieces/          # Game piece prefabs
│   └── UI/              # UI prefabs (PausePopup, etc.)
└── TextMesh Pro/        # Text rendering assets
```

### Current Implementation Status

#### Completed ✅
- PopupManager system (singleton pattern)
- PausePopup prefab with Resume/Restart buttons
- PauseButton in Gameplay scene
- GameManager and core gameplay mechanics
- Piece animation system
- Input handling

#### In Progress
- Assembly compilation and validation

### Build Commands
```bash
dotnet build Assembly-CSharp.csproj
```

### Scene Setup Notes
- **Gameplay Scene**: Contains PopupManager, UICanvas with PauseButton, GameManager, InputHandler
- **Hall Scene**: Contains PopupManager, PlayButton, HallManager
- Both scenes require EventSystem and MainCamera for proper UI interaction

### MCP Tools Usage
When scripts need to interact with Unity Editor:
- Use MCP tools for scene modification
- MCP must be actively connected
- Verify Visual Studio Code connection status before operations
