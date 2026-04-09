# MCP-ONLY Policy for Unity Assets

## ⚠️ CRITICAL RULE: Never Edit YAML Files Directly

All Unity asset files (`.prefab`, `.scene`, `.asset`, `.mat`, etc.) are serialized in YAML format and contain internal GUIDs and references. **Direct YAML editing breaks Unity's asset referencing system.**

## Approved Workflows

### ✅ For Prefabs / GameObjects:
- Use MCP tools: `mcp_unitymcp_manage_gameobject` with `action="create"`
- Use MCP tools: `manage_asset` for prefab operations
- Use Inspector assignments in Unity (manual)

### ✅ For Scenes:
- Use MCP tools: `mcp_unitymcp_manage_scene`
- Use Unity Editor UI manually

### ✅ For Materials/Shaders:
- Use MCP tools: `manage_asset` with type filters
- Use Inspector in Unity (manual)

### ❌ FORBIDDEN:
- Directly editing `.prefab` files in text editor
- Directly editing `.scene` files in text editor
- Directly editing `.asset` files in text editor
- Creating `.meta` files manually (Unity auto-generates these)

## Why This Matters

1. **GUIDs are sacred**: Each asset has a unique GUID. Editing YAML can corrupt these.
2. **References break**: GameObjects, components, and fields reference assets by GUID. Manual editing creates references to nothing.
3. **Serialization inconsistency**: Unity's serialization format can change between versions. Manual editing is fragile.
4. **m_FileID relationships**: YAML cross-references use `fileID` numbers that must match exactly.

## If You Need to Create Assets

**Always use MCP APIs, never manual YAML creation:**

```
❌ DON'T: Write to `*.prefab` file in text
✅ DO: Use mcp_unitymcp_manage_gameobject(action="create") 
✅ DO: Then PrefabUtility.SaveAsPrefabAsset() via MCP if available
✅ DO: Use Unity Editor UI to create and assign
```

## The Only Exception

MCP server availability limitations. If MCP tools are unavailable, **ask the user to use the Unity Editor UI directly**. Do NOT fall back to YAML editing.

---

**Last Updated**: 2026-04-09  
**Policy Owner**: Copilot  
**Severity**: CRITICAL
