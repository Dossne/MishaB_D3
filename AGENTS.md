# AGENTS.md

## Project intent

This repository contains a Unity 2D prototype for **RainbowTower**
a 2D Tower Defense casual mobile game.

Primary goal:
- quickly explore and validate the core gameplay loop 

Secondary goals:
- keep the project easy to iterate on
- avoid unnecessary complexity
- preserve project stability inside Unity

## Implementation source of truth

Full project specification is stored in:
- `Docs/RainbowTower_TechSpec.md`

Codex must treat this file as the main implementation specification for the project.

If there is a conflict between an ad-hoc implementation choice and the specification,
prefer the specification unless the user explicitly overrides it.

## Execution workflow for Codex

- Implement strictly one stage at a time.
- Never continue to the next stage automatically.
- Stop at the end of every completed stage.
- After each stage, report:
  - what was implemented;
  - which files, prefabs, ScriptableObjects, scenes, or folders were added or changed;
  - what gameplay loop or interaction loop is currently available to the player;
  - what was not verified if Unity/editor execution was not available.
- At the end of each stage, explicitly ask the user to:
  - test the current stage in Unity or a build;
  - report bugs or UX issues;
  - confirm when to continue to the next stage.
- If bugs are reported for the current stage, fix them before moving to the next stage.
- Do not treat a stage as complete if the vertical slice for that stage is not runnable.
- Keep changes scoped to the active stage unless a small supporting fix is required for stability.

## Repository priorities

When making changes, optimize for:
1. simple working prototype
2. readability
3. small diffs
4. safety for Unity assets and project structure

Do not over-engineer early systems unless explicitly requested.

## Important directories

- `Assets/Project/` — gameplay code, scenes, prefabs, art, ScriptableObjects
- `Packages/` — Unity package configuration
- `ProjectSettings/` — engine/project settings

## Project Structure

- All implementation lives under `Assets/Project/`.
- Use feature-based folders: one feature = one folder.
- Feature folder names must match the feature name.
- Organize feature contents by asset type:
  - `FeatureNameSrc` for scripts
  - `FeatureNamePfs` for prefabs
  - `FeatureNameArt` for sprites, animations, animator controllers
  - `FeatureNameCfg` for ScriptableObject assets
- ScriptableObject class definitions belong in `FeatureNameSrc`, not in `FeatureNameCfg`.

## UI Text Rules

- Use `TextMeshPro` for in-game text, HUD text, popup text, floating text, and other player-facing text.
- Do not use legacy Unity UI text components unless the user explicitly asks for an exception.
- If the project contains `Assets/Font/bangerscyrillic.otf`, use the corresponding `TextMeshPro` font asset based on `bangerscyrillic` instead of the default Unity font for visible game UI text, unless the user explicitly asks for another font.

## Working rules

- Prefer minimal, targeted changes.
- Do not rename or move files unless necessary.
- Preserve `.meta` files and Unity asset references.
- Do not edit `ProjectSettings/` unless the task requires it.
- Do not add new Unity packages without clear reason.
- Do not commit generated or cache folders such as `Library/`, `Temp/`, `Logs/`, `UserSettings/`.
- If a change is risky for scenes, prefabs, or references, explicitly mention that risk.

## Code style

- Prefer clear and boring code over clever abstractions.
- Keep classes small when possible.
- Avoid creating large frameworks for prototype-only needs.
- Use descriptive names.
- Keep public API surface small unless there is a good reason.

## Unity-specific constraints

- Be careful with scene and prefab modifications.
- Avoid unnecessary serialization churn.
- Avoid changing import settings, sorting layers, tags, or project-wide settings unless required.
- If changing a prefab or scene, keep the scope tight.

## Verification

Before considering work complete:
- check that the change is internally consistent
- confirm affected files are included
- mention what was not verified if Unity/editor execution was not available

Do not claim that gameplay was tested in-editor unless it was actually tested.

## Done means

A task is done when:
- the requested change is implemented
- the diff is reasonably small
- no obviously unrelated files were changed
- any limitations or unverified assumptions are stated clearly
