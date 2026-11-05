# Design Patterns — Crystal Defenders reflection

**Name:** Mai Kieu

**Student number:** 500956115

**Repository link:** https://github.com/xi-u/crystal-defenders

Answer every question in your own words about your own code. Short answers are fine; specific beats long.

---

## Part 1 — Setting the stage

**1. How many separate fields do you need to keep in sync by hand to answer "what is this turret doing right now?"**

Around 4 pieces of state to keep in sync:
- Current target
- Clip/ammo state
- Reload timer
- Next fire time 

**2. Does the order of your `if` branches matter? What happens if you reorder them, and does anything in the code explain why that order is required?**

Yes the order matters a lot. The code checks the following:
- reloading first
- then checks if the clip is empty
- then missing target
- then not aimed target
- then timing for firing

The reason is that each branch ends with a return statment, which basically means that "this state takes priority and 
everything else is ignored". If the order is changes the turret will behave not as expected.

**3. If you had to add a fourth behavior (say a cooldown before the first shot), how many existing branches would you have to revisit to make sure it cannot be entered at the wrong moment?**

You would have to revisit all the existing branches. You would have to check if the turret is in cooldown state before 
checking for reloading, clip empty, missing target, not aimed target, and timing for firing.

---

## Part 2 — The State pattern

**1. Where does the locked-target behavior live now, and how many of the other states had to know about it?**

It now lives in the TurretAimingState and TurretFiringState. They each have a reference to the target.
The other states (TurretIdleState and TurretReloadingState) do not need to know about the target.

**2. Answer the three Part 1 questions again for the state-machine version:**
   - **Fields to keep in sync**: Only the currenst state pointer is shared by the turret brain. The important
   data is owned by the active state, so there is no need to keep multiple fields in sync.
   - **Does branch order still matter**: Yes, but only inside a single state. Each state has its own priority order of checks
   so the risk is much more localized instead of being spread across the entire turret brian.
   - **Cost of a fourth behavior**: Much lower, because you can add a new state without having to touch the existing states. 
   You add one new state class and a few transitions to the existing states, instead of having to touch all the existing 
   branches in a single method.

**3. Name one thing in this project where a state machine would be over-engineering, and the simpler solution:**

Maybe the turret firing indicator. A simple boolean flag or direct method call would be sufficient enough. 
A state machine would be too overkill for this.

---

## Part 3 — The Singleton pattern

**1. (Task 2, written before reading on) Why did the serialized-field wiring not work for the enemy? What other way could you think of?**

Serialized field did not work because the enemy is a prefab and it is spawned at runtime. A prefab is not part of the scene
so it can not reference a scene object. A method to make it work would be a use a singleton pattern, which makes
the AudioPlayer accessible through its static instance property.

**2. Which other classes in this project would be suitable for a singleton, and which would be a mistake? Why?**

The WaveDirector class would be suitable for a singleton because there is only one wave director in the scene.

The Crystal class would be suitable for a singleton because there is only one crystal in the scene. And also a few
other classes need to access the crystal, so it is suitable. 

A singleton would be a mistake for the TurretBody class 
for example because there are multiple turrets in the scene, and each turret has its own state. Other examples
are Enemy, Projectile 

**3. What alternatives are there to the singleton for getting the `AudioPlayer` to a spawned enemy, and what is the trade-off?**

- Dependency injection: you can pass the AudioPlayer reference to the enemy when it is spawned. Trade
off is that you have to modify the spawn code to pass the reference, which is extra work and is sensitive to mistakes.
- event system: THe enemy can send an event when it is hit or dies, and the AudioPlayer can listen to those events. Trade
off is that it is also extra work to set up the event system, and it is also sensitive to mistakes.
- 

---

## Part 4 and Part 5 — The audit

Fill in the Part 4 column after Part 4, the Part 5 column after Part 5 Task 2.

| Question | Part 4 (polling) | Part 5 (events) |
|---|---|---|
| Scripts holding a reference to the crystal | | |
| Fields storing a copy of the crystal's past state | | |
| Files containing the low-health threshold (0.25 / 25%) | | |
| What breaks, and where, if the crystal is missing from the scene | | |
| Questions asked of the crystal in one single frame | | |

Part 4 only: where are you calling `Shake()` from, and why there?

---

## Part 5 — The Observer pattern

1. Which of the two refactors (Part 2 state machine, Part 5 events) deleted more code than it added? Use your actual diffs.

2. Where in this project would an event be over-engineering? Name a concrete pair of objects and say why a simpler mechanism is enough.

3. What do the audit numbers say now? Which did the Observer pattern fix, which did Task 2 fix, and which would still be there at ten times the scale?

### Scenario picks

Pick one mechanism per scenario: direct call, polling, injection, singleton, instance event, static event. Defend it in one sentence.

| Scenario | Mechanism | Why (one sentence) |
|---|---|---|
| An ammo readout drawn next to each turret | | |
| An achievement that fires at 10 total kills | | |
| A pause menu that reads the current time scale | | |
| A sound on every enemy death | | |
