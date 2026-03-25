# Codex Stage Prompts for RainbowTower

Use these prompts one by one. Do not skip stages. After each completed stage, test in Unity, report bugs if any, and only then move to the next stage.

---

## Stage 0 Prompt

```text
Read and follow these repository instructions first:
- AGENTS.md
- Docs/RainbowTower_TechSpec.md

Continue staged implementation of RainbowTower.

Mandatory workflow:
- Implement only the current stage.
- Stop at the end of the stage.
- Report implemented scope, changed files/assets, current gameplay loop, and unverified items.
- Ask me to test and report bugs.
- Do not implement anything from later stages in this run.

Current task:
Implement Stage 0 from Docs/RainbowTower_TechSpec.md:
"Bootstrap architecture"

Required result for this run:
- Add GameManager(MonoBehaviour) to define bootstrap creation, initialization, update order, and deinitialization order for runtime managers/controllers.
- Add ServiceLocator(MonoBehaviour) to provide dependency access and serialized scene references.
- Add ConfigurationProvider(ScriptableObject) to centralize references to feature configuration assets.
- Add serialized reference from GameManager to ServiceLocator.
- Add serialized reference from ServiceLocator to ConfigurationProvider.
- Ensure GameManager registers itself first in ServiceLocator.
- Ensure ConfigurationProvider is registered second in ServiceLocator.
- Set up the main scene with these objects wired correctly.
- Keep runtime asset access aligned with AGENTS.md rules.

Do not implement Stage 1 or later in this run.

When done, stop and wait for my feedback.
```

## Stage 1 Prompt

```text
Read and follow these repository instructions first:
- AGENTS.md
- Docs/RainbowTower_TechSpec.md

Continue staged implementation of RainbowTower.

Mandatory workflow:
- Implement only the current stage.
- Stop at the end of the stage.
- Report implemented scope, changed files/assets, current gameplay loop, and unverified items.
- Ask me to test and report bugs.
- Do not implement anything from later stages in this run.

Current task:
Implement Stage 1 from Docs/RainbowTower_TechSpec.md:
"Base UI shell"

Required result for this run:
- Add MainUiProvider(MonoBehaviour) as the root access point for gameplay UI.
- Add MainUiProvider to the main scene and register it in ServiceLocator.
- Create Canvas in Screen Space Overlay mode.
- Add CanvasScaler with reference resolution 1920x1080.
- Add and wire child roots:
  - FloatingTextParent
  - HudParent
  - PopupParent
- Build readable placeholder HUD for HP and wave.
- Build bottom crystal shelf placeholder layout for 3 rows:
  - top: Red, Green, Blue
  - middle: Yellow, Magenta, Cyan
  - bottom: White
- Ensure all gameplay UI access in code goes only through MainUiProvider fields.
- Use TextMeshPro for visible UI text.

Do not implement Stage 2 or later in this run.

When done, stop and wait for my feedback.
```

## Stage 2 Prompt

```text
Read and follow these repository instructions first:
- AGENTS.md
- Docs/RainbowTower_TechSpec.md

Continue staged implementation of RainbowTower.

Mandatory workflow:
- Implement only the current stage.
- Stop at the end of the stage.
- Report implemented scope, changed files/assets, current gameplay loop, and unverified items.
- Ask me to test and report bugs.
- Do not implement anything from later stages in this run.

Current task:
Implement Stage 2 from Docs/RainbowTower_TechSpec.md:
"Gameplay field layout and static level shell"

Required result for this run:
- Create the gameplay field composition in the main scene.
- Add the green field, red path, top portal area, and central tower placeholder.
- Add fixed path definition data/components for future enemy movement.
- Bind static HP and wave values into the HUD through scene/runtime wiring.
- Align the composition to Docs/template.png and Docs/fake_screenshot.png.
- Keep all scene references serialized and accessible through ServiceLocator or MainUiProvider as appropriate.

Do not implement Stage 3 or later in this run.

When done, stop and wait for my feedback.
```

## Stage 3 Prompt

```text
Read and follow these repository instructions first:
- AGENTS.md
- Docs/RainbowTower_TechSpec.md

Continue staged implementation of RainbowTower.

Mandatory workflow:
- Implement only the current stage.
- Stop at the end of the stage.
- Report implemented scope, changed files/assets, current gameplay loop, and unverified items.
- Ask me to test and report bugs.
- Do not implement anything from later stages in this run.

Current task:
Implement Stage 3 from Docs/RainbowTower_TechSpec.md:
"Enemy movement and wave pressure prototype"

Required result for this run:
- Add enemy prefab/view/config and runtime enemy movement along the fixed path.
- Add basic wave spawning.
- Update HUD with current wave and player HP.
- Reduce player HP when enemies reach the exit.
- Add a simple defeat state or restart prompt when HP reaches zero.
- Keep the stage runnable as the first actual defensive pressure slice.

Do not implement Stage 4 or later in this run.

When done, stop and wait for my feedback.
```

## Stage 4 Prompt

```text
Read and follow these repository instructions first:
- AGENTS.md
- Docs/RainbowTower_TechSpec.md

Continue staged implementation of RainbowTower.

Mandatory workflow:
- Implement only the current stage.
- Stop at the end of the stage.
- Report implemented scope, changed files/assets, current gameplay loop, and unverified items.
- Ask me to test and report bugs.
- Do not implement anything from later stages in this run.

Current task:
Implement Stage 4 from Docs/RainbowTower_TechSpec.md:
"Tower combat with base mana crystals"

Required result for this run:
- Implement base mana for Red, Green, Blue.
- Implement base crystal generation at 1 mana per second.
- Implement tower auto-fire with 1 shot per 2 seconds.
- Require at least 1 available mana of a valid color to fire.
- Consume 1 mana per shot.
- Use deterministic attack rotation across available base colors.
- Prioritize the enemy closest to the exit.
- Apply direct damage and enemy death.
- Grant XP on enemy kill.
- Show base crystal mana and levels in the crystal panel UI.

Do not implement Stage 5 or later in this run.

When done, stop and wait for my feedback.
```

## Stage 5 Prompt

```text
Read and follow these repository instructions first:
- AGENTS.md
- Docs/RainbowTower_TechSpec.md

Continue staged implementation of RainbowTower.

Mandatory workflow:
- Implement only the current stage.
- Stop at the end of the stage.
- Report implemented scope, changed files/assets, current gameplay loop, and unverified items.
- Ask me to test and report bugs.
- Do not implement anything from later stages in this run.

Current task:
Implement Stage 5 from Docs/RainbowTower_TechSpec.md:
"Base crystal unlocks and upgrades"

Required result for this run:
- Add XP spending for unlock and upgrade actions on Red, Green, Blue crystals.
- Add crystal state and UI status for locked, unlocked, and upgradeable.
- Increase generation speed, mana cap, and damage when crystal level increases.
- Add readable buttons or actions in the crystal panel UI.
- Prevent invalid purchases and show disabled states clearly.
- Keep the result as a runnable vertical slice of progression for base crystals only.

Do not implement Stage 6 or later in this run.

When done, stop and wait for my feedback.
```

## Stage 6 Prompt

```text
Read and follow these repository instructions first:
- AGENTS.md
- Docs/RainbowTower_TechSpec.md

Continue staged implementation of RainbowTower.

Mandatory workflow:
- Implement only the current stage.
- Stop at the end of the stage.
- Report implemented scope, changed files/assets, current gameplay loop, and unverified items.
- Ask me to test and report bugs.
- Do not implement anything from later stages in this run.

Current task:
Implement Stage 6 from Docs/RainbowTower_TechSpec.md:
"Mixed crystals and conversion economy"

Required result for this run:
- Add Yellow, Magenta, and Cyan crystals.
- Enforce unlock dependencies on their parent base crystals.
- Implement mixed generation by consuming the required parent mana.
- Pause generation cleanly if input mana is missing.
- Include mixed colors in the tower attack rotation when unlocked and available.
- Apply mixed damage formula:
  - own current-level damage value from config + current-level damage values from both parent crystal configs
- Extend crystal panel UI for the second shelf with color, level, mana, and status.

Do not implement Stage 7 or later in this run.

When done, stop and wait for my feedback.
```

## Stage 7 Prompt

```text
Read and follow these repository instructions first:
- AGENTS.md
- Docs/RainbowTower_TechSpec.md

Continue staged implementation of RainbowTower.

Mandatory workflow:
- Implement only the current stage.
- Stop at the end of the stage.
- Report implemented scope, changed files/assets, current gameplay loop, and unverified items.
- Ask me to test and report bugs.
- Do not implement anything from later stages in this run.

Current task:
Implement Stage 7 from Docs/RainbowTower_TechSpec.md:
"White crystal and full crystal chain"

Required result for this run:
- Add the White crystal.
- Enforce its dependency on the previous crystal chain.
- Implement White generation by consuming 1 mana of every non-white color.
- Pause generation if required mana is missing.
- Include White in tower attack rotation when available.
- Apply White damage formula:
  - sum of current-level damage values from crystal configs across the full crystal chain
- Finalize the full 3-shelf crystal panel behavior.

Do not implement Stage 8 or later in this run.

When done, stop and wait for my feedback.
```

## Stage 8 Prompt

```text
Read and follow these repository instructions first:
- AGENTS.md
- Docs/RainbowTower_TechSpec.md

Continue staged implementation of RainbowTower.

Mandatory workflow:
- Implement only the current stage.
- Stop at the end of the stage.
- Report implemented scope, changed files/assets, current gameplay loop, and unverified items.
- Ask me to test and report bugs.
- Do not implement anything from later stages in this run.

Current task:
Implement Stage 8 from Docs/RainbowTower_TechSpec.md:
"Combat readability and feedback pass"

Required result for this run:
- Add color-coded attack visuals.
- Add hit feedback and readable enemy damage response.
- Add death feedback.
- Add floating text where it improves readability.
- Add insufficient mana and blocked generation feedback.
- Add sound hooks or placeholder audio integration if reasonable.
- Keep battle timing intact and readable on mobile.

Do not implement Stage 9 or later in this run.

When done, stop and wait for my feedback.
```

## Stage 9 Prompt

```text
Read and follow these repository instructions first:
- AGENTS.md
- Docs/RainbowTower_TechSpec.md

Continue staged implementation of RainbowTower.

Mandatory workflow:
- Implement only the current stage.
- Stop at the end of the stage.
- Report implemented scope, changed files/assets, current gameplay loop, and unverified items.
- Ask me to test and report bugs.
- Do not implement anything from later stages in this run.

Current task:
Implement Stage 9 from Docs/RainbowTower_TechSpec.md:
"Wave scaling, pacing, and session flow"

Required result for this run:
- Tune wave difficulty growth.
- Tune XP reward growth.
- Ensure the session has readable escalation over several waves.
- Add a coherent defeat/restart flow.
- Add a simple prototype success or survive milestone if it improves the slice.
- Keep progression, economy, and wave pacing internally consistent.

Do not implement Stage 10 or later in this run.

When done, stop and wait for my feedback.
```

## Stage 10 Prompt

```text
Read and follow these repository instructions first:
- AGENTS.md
- Docs/RainbowTower_TechSpec.md

Continue staged implementation of RainbowTower.

Mandatory workflow:
- Implement only the current stage.
- Stop at the end of the stage.
- Report implemented scope, changed files/assets, current gameplay loop, and unverified items.
- Ask me to test and report bugs.
- Do not implement anything from later stages in this run.

Current task:
Implement Stage 10 from Docs/RainbowTower_TechSpec.md:
"Optional active mana Tap-drops"

Required result for this run:
- Add tap-drop spawn chance on tower shots using config values.
- Spawn drops near the tower.
- Choose drop color from unlocked colors.
- Set lifetime to 3 seconds.
- Grant +1 mana on successful tap.
- Enforce max simultaneous drops on the field.
- Add spawn, pulse, collect, and expire feedback.
- Keep the feature optional and non-mandatory for success.

Do not implement anything beyond Stage 10 in this run.

When done, stop and wait for my feedback.
```
