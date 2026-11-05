# V12.Paper

The Paper/Origami/Quill UI renderer for the V12 engine.

This project is intentionally separate from `V12-Engine` so the engine stays
renderer-agnostic and library-agnostic. It owns:

- `ProwlPaperRenderer` and the Paper/Quill canvas bridge
- `PaperUIProvider` and the Origami-backed widget providers
- Origami components (`CodeEditor`, `DockSpace`, `DockPanel`)
- Paper palette, mesh and inspector adapter helpers

It is optional. A V12 game can use this renderer, another UI renderer, or no UI
renderer at all.

## Relationship to the other repos

| Repo | Role |
|---|---|
| `V12-Engine` | World, ECS, rendering and UI contracts, no Prowl |
| `V12.Basic` | Gameplay components and systems |
| `V12.Paper` (this repo) | Prowl Paper/Origami/Quill UI implementation |
| `V12.Editor` | Editor interaction (viewport, gizmo, inspector) |
| `Nova` / `V12.Monogame` | Backend renderers |

## Building

Inside `V12-Monorepo` this project picks up the monorepo `Directory.Build.props`
through the local props shim:

```powershell
dotnet build
```

For a standalone checkout, place `V12-Engine` at `libs/V12` or pass the project
path explicitly:

```powershell
dotnet build -p:V12EngineProject="..\V12-Engine\V12.csproj"
```
