# Part 1: Setting the stage

## The situation

TODO Add git repo here.

You have inherited *Crystal Defenders*, a top-down defence simulation. Spawn zones send enemies marching toward a crystal, and autonomous turrets are supposed to defend it. There is no player. The whole thing runs itself.

There is one problem: the last developer did not finish the job.

### Task 0 — Run it and watch it fail (about 5 min)

1. Open the project in Unity and load the **Main** scene.
2. Press **Play**, then click **Start**.
3. Watch. Don't fix anything yet. Just observe what the turrets do and what happens to the crystal.

Did you figure out what goes wrong? If you need a hint, open the **Console** while it runs and read the warnings.

## Your work area

Take a look at the project before you touch anything.

For this task you will modify only one file: **`Assets/Turrets/TurretBrain.cs`**. This is where you write all your code for this assignment.

**Everything else: the enemies, the spawning, the crystal, the projectiles, the HUD, the audio are files/folders you won't modify in this task.**

It helps to understand How the Turrets work:

- **`TurretBody`** (in `Assets\Turrets\`) is the turret's **verb layer**. It knows how to turn, how to shoot, and how to reload, but it never decides when. Every method on it is safe to call at any time. Calling one at the wrong moment is a harmless no-op, never a crash.
- **`TurretBrain`** is the **decision layer**. It decides when to call those verbs. This is the part that is unfinished.

`TurretBrain` already has a working `Update()` loop that finds an enemy, aims, and fires. What it is missing is any notion of running out of ammo and reloading.

### The tools you have been given

`TurretBrain` inherits everything below from `TurretBody`. You will not need anything outside this list:

| Member | What it does |
|---|---|
| `FindNearestEnemy()` | Returns the closest enemy in range, or `null` if there is none. |
| `RotateTowards(target, dt)` | Turns one frame toward the target. Returns `true` when it is aimed. |
| `SetFiringIndicator(bool)` | Cosmetic. Tints the turret red while it is firing. |
| `FireOnce(target)` | Fires a single round and spends one bullet. No cooldown of its own. |
| `FireInterval` | The tuned seconds between shots. Read it to pace your shots. |
| `ClipEmpty` | `true` when there are no rounds left. |
| `RefillClip()` | Instantly refills the clip to full. There is no built-in reload delay. The time a reload takes is your job. |

A `reloadTime` field, set to **1.5 seconds**, is already declared in `TurretBrain` for you to use.

## Task — Make the turret reload

Finish `TurretBrain` so the turrets defend the crystal for as long as the run lasts.

**Requirements:**

- Add a check for an empty clip.
- Add reload logic. When the clip is empty, the turret spends `reloadTime` seconds reloading, does not fire during that time, and refills the clip when the time is up. Then it goes back to finding enemies and shooting.

**Constraints:**

- Work only in `TurretBrain.cs`. Do not modify anything else.
- Solve it with plain `if` / `else` / `switch` and a few extra fields on the class. That is all you need.
- Use the members from the table above. Do not add new ways of shooting or aiming.

> **We are holding you back on purpose.** This is deliberately the naive solution, a couple of fields and some branches. It will work, and it will also start to feel awkward. That awkwardness is the point. It is the problem the State pattern exists to solve, and it is exactly what you refactor in the next part. Resist the urge to build anything clever here. The good part comes after.

**Done when:** you press Play, click Start, and leave it running. The turrets fire, pause to reload, and resume, over and over. The crystal survives.

## Reflect

Once it works, look back at your `TurretBrain` and answer these:

- How many separate fields do you now need to keep in sync by hand to answer "what is this turret doing right now?"
- Does the order of your `if` branches matter? What happens if you reorder them, and does anything in the code explain why that order is required?
- If you had to add a fourth behavior, say a cooldown before the first shot, how many of your existing branches would you have to revisit to make sure it cannot be entered at the wrong moment?

Hold onto your answers. In the next part you rebuild this same turret as a proper state machine, and these are exactly the pains it removes.
