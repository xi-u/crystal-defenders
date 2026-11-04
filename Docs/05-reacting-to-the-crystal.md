# Part 4: Reacting to the crystal

## The situation

The crystal is the heart of this game. Enemies exist to break it, turrets exist to protect it, and the run ends the moment it falls. It owns its health, and `TakeDamage` is the single place that health ever goes down.

The game should react to the crystal: when it is damaged, when it is low on health, and when it is destroyed. At the moment, only the wave director is reacting to the crystal and that's when it gets destroyed. It does though by polling the crystal every frame like so:

```csharp
void Update()
{
    if (!running || gameOver) return;

    if (crystal == null || crystal.IsDestroyed)
    {
        EndGame();
        return;
    }

    // ... spawning continues below
}
```

We need more reactions to make the game more polished:
- The HUD needs to update the health bath of the crystal
- The HUD needs to change color when the cystal is low on health.
- The screen should shake when the crystal takes damage.
- The music should be more intense when the crystal is on low health.

## Your work area

You will touch these files:

- **`Assets/UI/HUD.cs`**
- **`Assets/Camera/ScreenShake.cs`**
- **`Assets/Waves/WaveDirector.cs`**
- **`Assets/Turrets/TurretBrain.cs`**

Leave everything else alone: the enemies, the spawn zones, the projectiles, the menu, the audio player.

You will not modify **`Assets/Waves/Crystal.cs`** in this part. Read it instead, because everything you build asks it questions. 

## The tools you are given

The files mentioned above have helper functions that take care of some behavior for you:

Helper variables/functions on `crystal.cs`

| Member | What it tells you |
|---|---|
| `CurrentHealth` | Current health as an `int`. |
| `MaxHealth` | Full health as an `int`. |
| `IsDestroyed` | `true` once health has reached zero. |

`HUD.cs` already has three helpers you call but do not rewrite:

| Helper | What it draws |
|---|---|
| `ShowCrystalHealth(float fraction)` | Fills the bar to the fraction. 0 is empty, 1 is full. |
| `FlashBar()` | One white flash on the bar that fades out. |
| `SetLowHealth(bool low)` | Switches the bar between its normal color and red. |

The Main Camera has a `ScreenShake.cs`: component that exposes `Shake()`, also safe to call repeatedly.

| Helper | What it draws |
|---|---|
| `Shake()` | Shakes the camera, safe to call repeatedly|

`AudioPlayer.cs` has `PlayBGMLowHealth`

**Task — Make the game react to the crystal's health**

**Requirements:**

* **Health bar:** 
    - The bar always shows the crystal's current health as a fraction of its maximum.
    - Every hit on the crystal produces one flash of the bar.
    - Below 25% health the bar is red.
* **Camera shake.** Every hit on the crystal shakes the camera once.
* **Intense music.** Below 25% crystal health, the music switches to `BGM_LowHealth` (in `Assets/Audio/Music`) using `AudioPlayer.PlayMusic(AudioClip clip)`. Give the director a serialized `AudioClip` field for the intense track and drag the clip in.

**Constraints:**

* The HUD and WaveDirector already hold a serialized `Crystal` reference. How the ScreenShake class gets it is your decision.
* Do not modify the helper methods or `ScreenShake`. Only call them.

**Done when:** you press Play, click Start, and the bar tracks the crystal's health down, flashing once per hit and turning red below a quarter. Each hit shakes the camera. The calm track plays from the start until health drops below 25%, then the intense track plays through without stuttering.


## Reflect

- How many scripts now hold a reference to the crystal? 
- Where are you calling the ScreenShake shake method? On the crystal? or on the class itself? Why?
- How many fields across the project store a copy of the crystal's past state?
- How many files contain the number 0.25?
- In one single frame, how many questions is the crystal asked?

