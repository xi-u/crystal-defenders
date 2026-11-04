# Part 1: The State Pattern

## Why bother?

Your turret now works, but, hopefully, you noticed the shortcomings when you answered the questions in Part 1. Now, suppose we ask you to add a new behavior. Say the turret should lock onto one target and stay on it until the clip empties or the enemy dies, instead of re-picking the nearest enemy every single frame.

Where does that logic go? You would be adding more if/else branches and carefully thinking about the order this logic needs to happen in without breaking what already exists. Now imagine we ask you to add a third or fourth behavior. It's not sustainable.

The more behaviors and rules you add, the more complicated your code becomes, even with separate functions, and the more likely you are to start encountering bugs. Not to mention your code will be difficult to read for other developers, or for you a couple of weeks later.

This is one of the most common situations in game development, and the State pattern is the standard answer to it.

---

## The state pattern

Before the general idea, watch one moment of the fight. The turret is shooting and the clip runs dry. Here is that moment told twice.

**As if/else bookkeeping.** One flag flips to say the turret is reloading. A second field records when the reload will be over. The firing indicator has to be switched off right there, in the same branch. And now every other branch matters: the aiming code and the firing code must not run while the flag is up, so each of them needs a guard, and the guards have to sit in the right order. The moment itself is one line. Protecting the rest of the update loop from that moment is most of the work, and every new behavior adds more branches to protect.

**As a state handover.** The turret is in Firing. Firing notices the clip is empty, builds a Reloading state, and asks the machine to switch. On the way out, Firing turns its indicator off, because that is its exit duty. On the way in, Reloading starts its timer. Idle and Aiming never hear about any of it. There is no flag and there are no guards, because there is nothing to guard: while Reloading is active, no other state's code runs at all.

That handover is the whole pattern. Now the general shape.

A state machine is a set of states where exactly one is active at a time. Each state:

- is **isolated**: it contains its own logic and its own data (timers, targets), which exist only while the state is active;
- has **one or more exit conditions**: when one is met, the state names its successor and the machine switches;
- has an **entry and an exit moment**: one-time setup when it becomes active, one-time cleanup when it stops, no matter which exit fired.

The machine guarantees the handover: the old state's exit runs, then the new state's entry.

### The shape in code

You don't need a framework for this. The mechanism is a few lines: a base type every state shares, and one method on the turret that swaps the active state.

```csharp
// One base type all states share.
abstract class TurretState
{
    public virtual void Enter() { }   // runs once, when this state becomes active
    public abstract void Tick();      // runs every frame while active
    public virtual void Exit() { }    // runs once, when leaving this state
}

// On the turret: exactly one active state, and one funnel for every swap.
TurretState _current;

void ChangeState(TurretState next)
{
    _current?.Exit();   // let the old state clean up
    _current = next;
    _current.Enter();   // let the new state set up
}

void Update() => _current.Tick();   // the turret just runs whoever is active
```

That's the whole engine. What the skeleton leaves open: how a state reaches the turret, what a finished state looks like, how a transition happens, and how the machine gets its first state. Take those one at a time.

### How a state reaches the turret's verbs

A state's `Tick` needs the turret's verbs: `RotateTowards`, `FireOnce`, `ClipEmpty`, and so on. The plain mechanism: the state takes the brain in its constructor and keeps it in a field.

```csharp
class ReloadingState : TurretState
{
    readonly TurretBrain brain;   // the state's line back to the turret

    public ReloadingState(TurretBrain brain)
    {
        this.brain = brain;
    }
}
```

A separate class like this can only reach the brain's public members, so `ChangeState` and the reload duration have to be visible to it. A one-line property covers the duration: `public float ReloadTime => reloadTime;`. Nesting the state classes inside `TurretBrain` is an alternative: a nested class can touch the brain's private members directly.

### One state, done: Reloading

Here is Reloading in full. It is the only state you get for free.

```csharp
class ReloadingState : TurretState
{
    readonly TurretBrain brain;
    float startTime;   // exists only while this state is active

    public ReloadingState(TurretBrain brain)
    {
        this.brain = brain;
    }

    public override void Enter()
    {
        startTime = Time.time;
    }

    public override void Tick()
    {
        if (Time.time - startTime >= brain.ReloadTime)
        {
            brain.ChangeState(new IdleState(brain));
        }
    }

    public override void Exit()
    {
        brain.RefillClip();   // runs on every way out of this state
    }
}
```

Two things to see here. The timer lives inside the state: no field on the brain, nothing to reset from some other branch, and it stops existing when the state is left. And the refill sits in `Exit`, not in `Tick`, so it happens no matter how the state is left. Today the timer is the only exit. If a future version adds another one, the clip still gets refilled, because `Exit` runs on every handover.

### One transition, end to end

Reloading starts because Firing decides it should. Somewhere in Firing's `Tick`, alongside the shooting logic:

```csharp
if (brain.ClipEmpty)
{
    brain.ChangeState(new ReloadingState(brain));
    return;
}
```

That is the rule to hold onto: **exit conditions live inside the state that owns them.** Firing owns "my clip is empty", so Firing notices it and names the successor. Nothing outside a state ever pushes a state around, and the brain never says "go reload now".

One aside: `new ReloadingState(brain)` allocates a small object on every transition, and at this scale that is fine.

### Where the machine starts

`_current` starts as null, so the machine needs its first state before the first frame. Build it and change to it in `Start`:

```csharp
void Start()
{
    ChangeState(new IdleState(this));
}
```

From then on the machine runs itself. `Update` ticks whoever is active, and the states hand over to each other.

That leaves three states for you to write: Idle, Aiming, and Firing. Their `Tick` bodies, their entry and exit duties, and the exit conditions that connect them are your work. You will find no code for them in this document.

---

## A note on pragmatism

Design patterns are best practices, not laws. You don't have to follow them exactly as written every time. They can be modified, trimmed, combined, or even skipped entirely in your project. A state machine is worth it here because the turret's behavior is about to grow. But there is real value in prototyping simple things with plain if/else first, exactly as you did in Part 1. Reaching for a pattern on a problem that doesn't have the pain the pattern solves can be over-engineering. Part of the skill is judging when the overhead is worth it. Don't shy away from the simple version when it's good enough.

---

## Your task

Refactor `TurretBrain` from the Part 1 if/else version into a state machine, and give it the locked-target behavior from the top of this document. As before, all your code stays in `TurretBrain.cs`, and you still use only `TurretBody`'s public members.

**Keep what Part 1 already did:** scan when there is nothing to shoot, rotate to aim, fire paced by `FireInterval`, and reload for `reloadTime` seconds when the clip is empty.

**What changes:**

- Break the behavior into separate states, each owning its own actions and its own exit conditions, with one active at a time and one `ChangeState` method for every transition.
- While firing, stay on the target. Once the turret starts shooting an enemy, it stays on that enemy until the clip empties or the target is gone. It does not re-pick the nearest enemy every frame anymore.
- Reloading cannot be interrupted. Once a reload starts, it runs to the end no matter what happens in the world.
- One state owns the firing indicator. The red tint turns on when firing starts and off when it stops, and that should be the only place in your code that touches it.

### The machine

Four states, and the conditions that move between them:

```
                     ┌─────────────────────────────────────────┐
                     │            target lost → Idle            │
                     ▼                                          │
   ┌──────────┐  target found   ┌──────────────┐  aimed   ┌─────┴─────┐
   │          │───────────────► │    AIMING    │────────► │           │
   │   IDLE   │                 │  rotate to   │          │  FIRING   │
   │  scan    │ ◄───────────────│  the target  │          │ lock tgt, │
   │          │  target died /  │              │          │ shoot per │
   └──────────┘  left range     └──────────────┘          │ interval  │
        ▲                                                  └─────┬─────┘
        │                                                        │
        │                       ┌──────────────┐                 │
        │      reload done      │  RELOADING   │   ClipEmpty      │
        └───────────────────────│ wait, then   │◄────────────────┘
             (RefillClip)       │  refill clip │
                                │  NO EXITS    │
                                │ until timer  │
                                └──────────────┘
```

### Done when

Press Play and leave it running. The turret locks onto an enemy, fires until the clip empties, reloads without being distracted, then scans and re-engages, and keeps doing this. On screen it behaves like your Part 1 fix. The win here is in the shape of the code, not new fireworks.

---

## Check your design against this

Once it works, compare your states to the reference below. If they line up, you built it the way the pattern intends. If they don't, that is worth understanding before you move on, since a mismatch here is often a bug waiting for a fifth state to arrive.

**Enter / Exit duties**

| State | On enter | On exit |
|---|---|---|
| IDLE | — | — |
| AIMING | — | — |
| FIRING | indicator **ON** | indicator **OFF** *(the only place)* |
| RELOADING | start the reload timer | `RefillClip()` |

**Invariants** — things that stay true no matter what:

- FIRING never calls `FindNearestEnemy`. Its target is fixed the moment the state is entered. That is what "locked on" means.
- RELOADING has no exit conditions except its own timer. That is how it is made uninterruptible: not with a guard flag, but by giving it nothing else to react to.
- Exactly one state is active at a time, and every transition goes through `ChangeState`. No state swaps the active reference directly.

---

## Reflect

Look back at the "add a locked-target behavior" problem from the top of this document. In the Part 1 code it would have meant editing branches all over `Update`. Here it is a property of one state, and the other three never had to know about it. That is the payoff, and it is why saying "I made it a state machine" tells another developer so much in so few words.
