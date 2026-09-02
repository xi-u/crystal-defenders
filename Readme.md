# Crystal Defenders

![A run in progress: enemies closing in on the crystal while the turrets hold them off](screenshot.png)

## What this is

Crystal Defenders is a small top-down tower defense **simulation**. There is no player character and nothing to steer. Spawn zones send enemies marching toward the crystal, autonomous turrets defend it, and the run ends when the crystal falls. You press Play, click Start, and watch the fight.

The project exists to teach **design patterns**. It is the shared codebase for a series of assignments in the `Docs/` folder, covering the State, Singleton, and Observer patterns. The visuals are deliberately grey-box: primitives, flat colors, no art pass. Everything interesting lives in the code.

## What you are looking at

In the screenshot above:

- The **cyan cube** is the crystal: the base. Enemies exist to break it, turrets exist to protect it.
- The **grey turrets** aim and fire on their own. A turret's head tints red while it is firing.
- The **green spheres** are enemies, walking from the spawn zones toward the crystal. An enemy flashes red when a shot lands.
- The **two red discs** on the right are the spawn zones.
- The **bar at the bottom** is the crystal's health.

## How to run it

1. Open the project in **Unity 6** (it was built with **6000.3.6f1**; a nearby Unity 6 version works). The first open takes a few minutes while Unity imports everything.
2. Open the scene **`Assets/Scenes/Main.unity`**.
3. Press **Play**, then click **Start**.
4. Watch. The sim runs one wave and plays itself. When the crystal falls, the run is over, and **Restart** reloads the scene for another one.

Want a longer or meaner fight? The knobs are serialized fields, tunable in the Inspector: spawn pacing on the **WaveDirector**, enemy speed and damage on the **Enemy** prefab, clip size and aim speed on the turrets. Tune freely. Nothing in the assignments depends on specific numbers.

## Where the code lives

| Folder | What is in it |
|---|---|
| `Assets/Waves/` | The crystal and the `WaveDirector`, which runs the flow of the game. |
| `Assets/Enemies/` | The enemy and the spawn zones. |
| `Assets/Turrets/` | The turret's body, its brain, and the projectile. |
| `Assets/UI/` | The HUD and the start and game over menus. |
| `Assets/Audio/` | The `AudioPlayer` and all clips. |
| `Assets/Camera/` | The screen shake component. |
| `Docs/` | The assignments. |
