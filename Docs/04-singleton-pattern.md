## The problem

If you have tried to use the same editor wiring as the crystal.cs, you will have noticed that it is impossible to do that with the Enemy class. 

Here is the wall you hit. The enemy is a prefab that gets created at runtime by the SpawnZone, and a prefab cannot hold a reference to an object that lives in the scene. You can add the AudioPlayer field to the enemy, but the slot stays empty on every copy that spawns, because there is nothing in the prefab to drag the scene's AudioPlayer into. The habit you just learned falls apart the moment an object is spawned instead of placed by hand.

There are many ways you can solve this problem. With design patterns, the question is usually about tradeoffs. 

One way to avoid is using `FindObjectOfType`.

It is bad for two reasons:

- It searches the **entire scene** at runtime to find one object.
- It runs for **every enemy**, and enemies spawn constantly through the whole run.

You would be doing an expensive lookup over and over to reach an object that never moves and never changes. What you really want is one object that anyone can reach directly, with no wiring and no search. That is what the singleton gives you.

## The singleton pattern

### Reading suggestions
- https://refactoring.guru/design-patterns/singleton
- https://unity.com/resources/design-patterns-solid-ebook (page 66)

The **singleton** pattern is a creational pattern where an object has exactly one instance reachable from anywhere through a static access point. Any object can ask for the instance by name.

The shape is small. A static property points at the one instance, and it is set when the object wakes up:

```csharp
public class AudioPlayer : MonoBehaviour
{
    public static AudioPlayer Instance { get; private set; }

    void Awake()
    {
        Instance = this;
        // the existing AudioSource setup stays here
    }
}
```

Now any object, spawned or not, reaches the audio manager the same way:

```csharp
AudioPlayer.Instance.PlayGameOver();
```

Two notes on this shape:

- **Scene reloads.** The restart button reloads the scene. That rebuilds every object, including the `AudioPlayer`, so its `Awake` runs again and `Instance` points at the fresh one. For a single-scene game that is all the handling you need.
- **Two instances.** The singleton assumes there is exactly one `AudioPlayer` in the scene. If two ever exist, `Instance` points at whichever woke up last. That is a good reason to keep it to one.

## Task 3: Convert to the singleton

Make every sound in the game go through `AudioPlayer.Instance` so no component needs a wired reference.

**Requirements:**

* Add the `Instance` property to `AudioPlayer` and set it in `Awake`, as shown above.
* Add sound to the enemy first. It plays its sound through `AudioPlayer.Instance`.
* Then convert everything else. The crystal, the turret, and the director drop their serialized `AudioPlayer` fields and call `AudioPlayer.Instance` instead.

**Constraints:**

* No component in the project should hold a serialized reference to `AudioPlayer` when you are done.

**Done when:** you press Play and the game sounds for everything including enemies. No Inspector slot for `AudioPlayer` is left anywhere in the scene.


## Global state, and when to be careful

A singleton is **global state**: data that any part of the program can reach from anywhere. You have already been using global state in this project without calling it that. `Enemy.Alive` and `Enemy.TotalKilled` are `static`, which means they belong to the whole program and not to any one enemy. That is exactly how the HUD reads the kill count without holding a reference to a single enemy.

Global state is convenient, and that is also the danger. Because anything can reach it, it is easy to lean on it everywhere, and then it becomes hard to see who actually depends on what. A bug in a global can come from anywhere in the codebase.

So use it with intent. A singleton fits a true one-of-a-kind service that the whole game shares, like audio. It does not fit things you might want more than one of. There is only ever one audio manager, so it is a fair use of the pattern. There are many turrets, so a turret is not.

## Reflect

- What other classes might be suitable for singleton?
- Are there alternatives? Other design patterns? 