# Part 5: The Observer Pattern

## What you built in Part 4

Look at what Part 4 actually produced. Several objects, each holding its own reference to the crystal. Several private fields, each storing a copy of some piece of the crystal's state, so that its owner can notice a change one frame later. Every one of them asking the crystal the same questions, every frame, for the whole run. Your audit numbers put figures on all of this.

Now the strange part. The crystal already knows the exact moment everything happens. `TakeDamage` is the one place health ever goes down. The frame a hit lands, the frame health crosses a threshold, the frame it reaches zero: all of it is known, in one method, at the instant it becomes true. And still, nobody is told. Everybody asks.

So turn it around. What if the crystal announced its moments, and everything else reacted?

That is the **Observer pattern**: one object announces that something happened, and any number of others react to it, without the announcer knowing who they are. You do not have to build the mechanism yourself. C# ships with it: **events**.

## Your work area

You will touch the files from Part 4, plus the crystal:

- **`Assets/Waves/Crystal.cs`**: now announces its moments.
- **`Assets/Waves/WaveDirector.cs`**, **`Assets/UI/HUD.cs`**, **`Assets/Camera/ScreenShake.cs`**, **`Assets/Turrets/TurretBrain.cs`**: stop asking, start reacting.

Leave everything else alone: the enemies, the spawn zones, the projectiles, the menu, the audio player.

## C# events

Three mechanics, one at a time.

### Declaring an event

An event is declared on the class that owns the moment:

```csharp
using System;

public class Crystal : MonoBehaviour
{
    public event Action OnDestroyed;
}
```

`Action` is a built-in delegate type: a method signature used as a type. Plain `Action` means "methods that take nothing and return nothing". `Action<int, int>` means "methods that take two ints". Underneath, an event is a list of methods. Other objects add their methods to the list, and when the event is raised, every method on the list runs.

The `event` keyword controls who may do what: anyone can add or remove a method, but only the declaring class can raise it. The crystal announces. Nobody else gets to speak for it.

### Raising an event

```csharp
OnDestroyed?.Invoke();
```

The `?.` matters. An event with no methods on its list is null, and raising it without the check throws. Raise the event in the one place where the moment becomes true. For the crystal, that place already exists: `TakeDamage`.

### Reacting to an event

Any object holding a crystal reference can add a method to the list with `+=`, and remove it with `-=`:

```csharp
crystal.OnDestroyed += HandleCrystalDestroyed;   // start reacting
crystal.OnDestroyed -= HandleCrystalDestroyed;   // stop reacting
```

Adding a method to the list is called **subscribing**, and removing it is **unsubscribing**. The method's signature must match the delegate type: for `Action`, no parameters and no return value. For `Action<int, int>`, two int parameters.

## The worked example: the director stops asking

Here is one moment converted end to end. The crystal announces its destruction:

```csharp
public event Action OnDestroyed;

public void TakeDamage(int amount)
{
    if (IsDestroyed) return;

    currentHealth = Mathf.Max(0, currentHealth - amount);

    // the existing feedback code stays here unchanged

    if (IsDestroyed)
    {
        OnDestroyed?.Invoke();   // the moment, announced where it happens
    }
}
```

The `WaveDirector` reacts:

```csharp
void OnEnable()
{
    crystal.OnDestroyed += HandleCrystalDestroyed;
}

void OnDisable()
{
    crystal.OnDestroyed -= HandleCrystalDestroyed;
}

void HandleCrystalDestroyed()
{
    EndGame();
}
```

And this, from the opening of Part 4, is deleted from the director's `Update`:

```csharp
if (crystal == null || crystal.IsDestroyed)
{
    EndGame();
    return;
}
```

A question the crystal was asked sixty times a second became one call, made at the exact moment it matters, by the object that owns the moment. The crystal still has no idea the director exists. It announces; whoever cares, reacts.

This is the only conversion you get. Every other event on the crystal, and every other handler, is your work.

### Instance events and static events

`crystal.OnDamaged` is an **instance event**: it lives on one object, and you need a reference to that object to react to it. That fits the crystal, because there is exactly one and everyone already knows it. Now imagine a hypothetical `Enemy.OnAnyKilled`, a **static event** on the class itself: with many short-lived enemies spawning and dying all run long, chasing each instance to attach to it would be churn, but the class-level moment "some enemy died" is easy to react to without holding any enemy at all. The cost is the usual one from Part 3: a static event is global state.

## Task 1: Move every consumer over

Convert all of Part 4 from asking to reacting.

**Requirements:**

- The crystal announces `OnHealthChanged(current, max)`, declared as `Action<int, int>` and raised in `TakeDamage`. Together with `OnDestroyed`, that is all it announces for now.
- The helpers and effects stay untouched: `ShowCrystalHealth`, `FlashBar`, `SetLowHealth`, `Shake`, and the music calls are made exactly as before. Only the detection code changes.
- Every Part 4 consumer reacts through the crystal's announcements. The per-frame crystal checks, and every field that stored a copy of the crystal's past state, are deleted.
- Every subscriber that can be disabled or destroyed removes its handlers. The symmetry rule: add in `OnEnable`, remove in `OnDisable`.

**Done when:** the game plays exactly like Part 4: the bar tracks and flashes and turns red, the camera shakes per hit, the music switches, the turrets speed up. And no consumer's `Update` reads the crystal anymore.

### What a stale subscription does

The symmetry rule is not decoration, so reason it through once. Suppose a turret adds a handler to the crystal and is then destroyed without removing it. The crystal's list still holds that method, which means it still holds the dead turret. On the next hit the crystal calls a method on a destroyed object: in the best case Unity throws an error in the Console, in the worst case the handler quietly runs logic on a corpse, touching things that no longer exist. Unity calls `OnDisable` both when an object is disabled and before it is destroyed, which is exactly why the rule pairs `OnEnable` with `OnDisable`: membership in the list then always matches whether the object is actually there. For each of your subscribers, ask: can this object go away while the crystal lives on?

## The fork: who owns "low"?

Task 1 works. Now a change request comes in: design wants the low-health threshold at 30% instead of 25%.

How many files do you edit? Count them. The HUD compares against its own hardcoded number. The director compares against its own. Each turret's brain compares against its own. The pattern moved the detection, but the duplication survived the pattern: three copies of one design decision, still.

## Task 2: Give the crystal the threshold

**Requirements:**

- The crystal owns the threshold number, in one place, and announces the moment its health becomes low: `OnLowHealth`.
- The bar color, the music switch, and the turret boost become dumb reactors to that announcement. Their hardcoded threshold numbers are deleted.
- The bar fill keeps using `OnHealthChanged`. It needs the numbers to draw.

**Done when:** changing the threshold on the crystal, in one place, moves all three behaviors at once, and the game plays exactly as before.

The line to take away: the raw number belongs to whoever needs numbers, and the meaning of "low" belongs to the crystal. The bar needs a fraction to draw a fill, so it gets values. The other three needed a verdict, not a value, and the verdict is the crystal's to give.

## The audit, again

Re-run your Part 4 audit with the same five questions: scripts holding a crystal reference, fields copying its past state, files containing the threshold, what breaks when the crystal is missing, and questions asked of the crystal per frame. Put the two sets of numbers side by side. Some went to zero. At least one did not, because reacting to an instance event still requires a reference to the instance. Knowing which numbers a pattern fixes, and which it leaves alone, is knowing the pattern.

## A note on pragmatism

Events are the right tool here, and they are not the right tool everywhere. A scenario to reason about: the next ticket asks for a small health bar floating above each enemy. Fresh from this document, you reach for an event: `Enemy` gets `OnHealthChanged`, the bar attaches to it, clean and decoupled.

Stop and look at the two objects. The bar and the enemy are one prefab. They are born together, destroyed together, and sit on the same GameObject, already holding each other. What did an event buy you here? A direct method call from the enemy to the bar was fine. The Observer pattern pays for distance between objects that should not know each other. These two already know each other.

To close the series, four scenarios. For each, pick one mechanism from everything you now have: **direct call, polling, injection, singleton, instance event, static event**. Defend your pick in one sentence.

| Scenario | Your mechanism, and your one sentence |
|---|---|
| An ammo readout drawn next to each turret | |
| An achievement that fires at 10 total kills | |
| A pause menu that reads the current time scale | |
| A sound on every enemy death | |

On the last one: Part 3's `AudioPlayer.Instance` call inside the enemy and a static enemy event the audio player listens to are both defensible, since they build the same feature with opposite directions of knowledge.

## Reflect

- Which of the two refactors deleted more code than it added? Look at your actual diffs, not your memory of them.
- Where in this project would an event be over-engineering? Name a concrete pair of objects and say why a simpler mechanism is enough.
- What do the audit numbers say now? Which ones did the Observer pattern fix, which did Task 2 fix, and which would still be there at ten times the scale?
