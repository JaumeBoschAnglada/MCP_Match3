# Instrucciones para Copilot - MCP Match3

## 📚 Lee primero

Toda la documentación del proyecto está en la carpeta `AgentConfig/`:
- **[01-project-overview.md](AgentConfig/01-project-overview.md)** → Qué es el proyecto, qué se conserva, qué se elimina
- **[02-architecture.md](AgentConfig/02-architecture.md)** → Arquitectura completa, escena, dependencias, tipos de datos
- **[03-development-plan.md](AgentConfig/03-development-plan.md)** → 12 fases de desarrollo desde cero
- **[04-level-structure.md](AgentConfig/04-level-structure.md)** → Formato Stage JSON para niveles
- **[05-progress-log.md](AgentConfig/05-progress-log.md)** → Registro cronológico de avances

**Documentación de referencia (fuente de verdad, NO modificar):**
- `TMP Textos Agente/Match3_Gameplay_Documentation.md` → Gameplay completo de referencia
- `TMP Textos Agente/Interaccion_Completa_Touch_to_Touch.md` → Flujo detallado del input

## 🎮 Contexto

Juego **Match 3 basado en turnos** para móviles en **Unity 6000.3.2f1 con URP**.
- **Estado**: Se construye TODO desde cero. Solo se conserva el sistema de popups.
- **Fase actual**: Fase 0 (Limpieza Total + Estructura Base)

## ⚠️ REGLA PRINCIPAL
**NO reutilizar ningún script, prefab ni asset existente** salvo PopupManager, PopupBase,
PopupType, PausePopup y PauseButton. Todo lo demás se elimina y se crea nuevo.

## ⚡ Reglas Fundamentales

1. **MCP First**: Escenas y GameObjects con MCP. NUNCA scripts generadores de escenas.
2. **Escena 3D**: Tablero y piezas en espacio 3D. Cámara 3D (ortográfica o perspectiva).
3. **UI con Unity UI (Canvas)**: CanvasScaler, Image, TextMeshProUGUI, Button. **NO NGUI.**
4. **Input Legacy en Item**: `OnMouseDown/OnMouseEnter/OnMouseUp` en `Item.cs`. NO usar UnityEngine.InputSystem.
5. **[SerializeField] siempre**: No FindObjectOfType, no GetComponent en runtime.
6. **Namespace Match3.***: Items, Panels, Steps, Core, Data, Managers, UI, Scenes.
7. **🚫 NUNCA tocar YAML**: Prefabs/escenas SOLO con MCP tools.
8. **ObjectPool para todo**: Nunca Destroy() en gameplay. Siempre Restore() al pool.
9. **Steps para el turno**: BaseStep subclases con Step_Play() + Step_Process(). NO coroutines largas.
10. **Board por celda**: Cada celda es un MonoBehaviour Board con su Item y Panel[].
11. **Grid 9×9 fijo**: 81 celdas, activas/inactivas por panel Default_Full/Default_Empty.
12. **Docs TMP = lógica de gameplay**, NO jerarquía de escena ni UI. La escena y UI se diseñan propias.

## 🛠️ Al Trabajar en una Tarea

1. **Lee** `AgentConfig/03-development-plan.md` → fase actual y tareas pendientes
2. **Consulta** `AgentConfig/02-architecture.md` → cómo encaja
3. **Consulta** `TMP Textos Agente/*.md` → detalles de implementación
4. **Implementa** todo NUEVO, sin reutilizar código existente
5. **Actualiza** `AgentConfig/05-progress-log.md` con lo hecho
6. **Marca** tareas completadas en `03-development-plan.md`: `[ ]` → `[x]`

## 🐛 Gotchas de MCP
- `manage_components set_property` → propiedades **públicas** (fontSize)
- `manage_gameobject modify component_properties` → nombres **serializados** (m_FontSize)
- Canvas: m_RenderMode 0=Overlay, 1=ScreenSpaceCamera, 2=WorldSpace
- Material: `component_properties {"MeshRenderer": {"m_Materials": [{"path": "..."}]}}`
