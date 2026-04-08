# Instrucciones para Copilot - MCP Match3

## 📚 Lee primero

Toda la documentación del proyecto está en la carpeta `AgentConfig/`:
- **[project-overview.md](AgentConfig/project-overview.md)** → Visión general, tecnología, estructura completa
- **[development-plan.md](AgentConfig/development-plan.md)** → Roadmap de 8 fases con checklist de tareas
- **[progress-log.md](AgentConfig/progress-log.md)** → Registro cronológico de avances
- **[architecture.md](AgentConfig/architecture.md)** → Diagrama de dependencias, flujo del core loop, convenciones

## 🎮 Contexto

Este es un juego **Match 3 para móviles** en **Unity 6000.3.2f1 con URP**. 
- **Fase actual**: Fase 2 (Scoring y UI de Gameplay)
- **Fase completada**: Fase 1 (Core Loop funcional)

## ⚡ Reglas Fundamentales

1. **MCP First**: Las escenas y GameObjects se crean/modifican con MCP. NUNCA crear scripts generadores de escenas ni herramientas Editor.
2. **Scripts = Lógica pura**: No incluir código que cree GameObjects, escenas o menús Editor.
3. **Input System moderno**: Usar `UnityEngine.InputSystem`. NUNCA `UnityEngine.Input` (legacy).
4. **SerializeField siempre**: Todas las referencias via `[SerializeField]`. No usar `FindObjectOfType`, `GetComponentInChildren`, ni similares.
5. **Namespace Match3.***: Cada script pertenece a un namespace bajo `Match3`.

## 🛠️ Al Trabajar en una Tarea

1. **Lee** `AgentConfig/development-plan.md` para saber qué fase toca y qué tareas hay pendientes.
2. **Consulta** `AgentConfig/architecture.md` para entender cómo encaja la nueva funcionalidad.
3. **Implementa** siguiendo las convenciones existentes.
4. **Actualiza** `AgentConfig/progress-log.md` con lo que se hizo.
5. **Marca como completada** la tarea en `AgentConfig/development-plan.md` cambiando `[ ]` por `[x]`.

## 🐛 Gotchas de MCP
- `manage_components set_property` → nombres de propiedades **públicas** (fontSize)
- `manage_gameobject modify component_properties` → nombres **serializados** (m_FontSize)
- Canvas: m_RenderMode 0=Overlay, 1=ScreenSpaceCamera, 2=WorldSpace
- Material: `component_properties {"MeshRenderer": {"m_Materials": [{"path": "..."}]}}`
