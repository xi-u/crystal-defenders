# Design Patterns — Part 2: The State Pattern

## Why bother?

Your turret now works, but you noticed the shortcomings when you answered the questions in Part 1. Now, suppose we ask you to add a new behavior. Say the turret should lock onto one target and stay on it until the clip empties or the enemy dies, instead of re-picking the nearest enemy every single frame.

Where does that logic go? You would be adding more if/else branches and carefully thinking about the order this logic needs to happen in without breaking what already exists. Now imagine we ask you to add a third or fourth behavior. It's not sustainable.

The more behaviors and rules you add, the more complicated your code becomes, even with separate functions, and the more likely you are to start encountering bugs. Not to mention your code will be difficult to read for other developers, or for you a couple of weeks later.

This is one of the most common situations in game development, and the State pattern is the standard answer to it.

---

## The state pattern

The state pattern is a behavioral design pattern that is described as follows: 
- An object should change it's behavior when it's state changes
- Each state behavior is defined independently and adding new states does not impact existing states.
(reference: https://unity.com/resources/design-patterns-solid-ebook)

A popular implementation of the pattern is Finite State Machines (FDM).

A state machine is a set of states where exactly one is active at a time. Each state:

- is **isolated**: it contains its own logic and its own data (timers, targets), which exist only while the state is active;
- has **one or more exit conditions**: when one is met, the state names its successor and the machine switches;
- has an **entry and an exit moment**: one-time setup when it becomes active, one-time cleanup when it stops, no matter which exit fired.

The machine guarantees the handover: the old state's exit runs, then the new state's entry. Because each state is self-contained, changing behavior means one small edit in one state, not re-checking every condition in a long if/else chain.

You can choose one of the following sources to read more about the state pattern and Finite State Machines in detail:

https://refactoring.guru/design-patterns/state
https://unity.com/resources/design-patterns-solid-ebook (Page 80)
https://gameprogrammingpatterns.com/state.html


### The Turret FSM

For our turret we want the following states:

- **Idle** — scan until an enemy appears.
- **Aiming** — rotate toward it until on target. Lose the target, and it goes back to Idle.
- **Firing** — shoot the locked target on an interval, until it's gone or the clip is empty.
- **Reloading** — wait out the timer, refill, return to Idle. Nothing can interrupt it, because it has no exit condition until the timer says so.

Here is a diagram describing our state machine:

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


### Shape in code

To map this to code we need to do the following:

- A base state class for each sate to inherit from where the following three methods exist:
    - Enter - Runs once when the state becomes active. Useful for passing objects for the state to use.
    - Tick - The logic of the state. Somewhere here "ChangeState" will be called.
    - Exit - Runs onc when leaving the state. Useful for any clean up necessary.

- A "ChangeState" method on the TurretBody to swap active states.


To help you out initially here is how you can set up the base class and modify `TurretBrain.cs`:

Create a TurretState.cs file under the Turrets folder with the following abstract class:

```csharp
// One base type all states share.
abstract class TurretState
{
    protected readonly TurretBrainStates turret;

    protected TurretState(TurretBrainStates turret)
    {
        this.turret = turret;
    }

    public virtual void Enter() { }   // runs once, when this state becomes active
    public abstract void Tick();      // runs every frame while active
    public virtual void Exit() { }    // runs once, when leaving this state
}

// On the turret: exactly one active state, and one funnel for every swap.
```
Notice how there is a class field for `turret` that needs to be passed down in the class constructor.
This is how the state can access the methods and variables on the turret. There are many other ways you can achieve the same goal but this works for simplicity.

In `TurretBrain.cs`

Define the current state variable in the class

```csharp
TurretState _current;
```

Add the ChangeState method 

```csharp

void ChangeState(TurretState next)
{
    _current?.Exit();   // let the old state clean up
    _current = next;
    _current.Enter();   // let the new state set up
}

```

Modify the update method to only call the current state's `Tick`

```csharp
void Update() => _current.Tick();   // the turret just runs whoever is active
```

Lastly, we need the Turret to initialize the first state, you can do that in `OnEnable`

```csharp
    void OnEnable()
    {
        _current = new TurretIdleState(this);
    }
```

When you add this it will fail because you haven't created the `TurretIdleState`class yet.

The next step is to start creating State classes that inherit from the base class.
---

## A note on pragmatism

Design patterns are best practices, not laws. You don't have to follow them exactly as written every time. They can be modified, trimmed, combined, or even skipped entirely in your project. A state machine is worth it here because the turret's behavior is about to grow. But there is real value in prototyping simple things with plain if/else first, exactly as you did in Part 1. Reaching for a pattern on a problem that doesn't have the pain the pattern solves can be over-engineering. Part of the skill is judging when the overhead is worth it. Don't shy away from the simple version when it's good enough.

---

## Your task

- Implement the state machine as defined in the diagram above.
changes from the previous implementation:
    - While firing, stay on the target. Once the turret starts shooting an enemy, it stays on that enemy until the clip empties or the target is gone. It does not re-pick the nearest enemy every frame anymore.
    - Reloading cannot be interrupted. Once a reload starts, it runs to the end no matter what happens in the world.
    - One state owns the firing indicator. The red tint turns on when firing starts and off when it stops, and that should be the only place in your code that touches it.

### Done when

Press Play and leave it running. The turret locks onto an enemy, fires until the clip empties, reloads without being distracted, then scans and re-engages, and keeps doing this until the wave is over.

---

## Reflect

Look back at the "add a locked-target behavior" problem from the top of this document. In the Part 1 code it would have meant editing branches all over `Update`. Here it is a property of one state, and the other three never had to know about it. That is the payoff.
