# Part 3: The Singleton Pattern

## The audio manager

Sound in this project runs through one object: the **`AudioPlayer`**. It holds the clips and their volumes, and it exposes one method per sound.

It follows the same rule as the turret body. It knows **how** to play a sound, not **when**. Something else decides the moment and asks it to play. Every method is safe to call at any time.

## Your work area

You will touch these files:

- **`Assets/Audio/AudioPlayer.cs`**
- **`Assets/Turrets/TurretBody.cs`**
- **`Assets/Waves/WaveDirector.cs`**
- **`Assets/Enemies/Enemy.cs`**
- **`Assets/Waves/Crystal.cs`**

Leave everything else alone: the spawn zones, the projectiles, the HUD, the menu, the camera.

### The tools you have been given

These are the `AudioPlayer` methods you will call. Each one plays its clip at its tuned volume:

| Method | The sound |
|---|---|
| `PlayTurretShot()` | A turret fires. |
| `PlayEnemyHit()` | An enemy is hit. |
| `PlayEnemyDied()` | An enemy dies. |
| `PlayCrystalDamage()` | The crystal takes a hit. |
| `PlayGameOver()` | The run ends. |
| `PlayBGM()` | Starts the looping background track. |


### Task 1: Wire more sounds (about 10 min)

You are going to wire more sounds to 

## Requirements:

- **Turret shot**: Every time a turret fires, play the shot sound.
- **Game over**: When the run ends, play the game-over sound.
- **Background music**: Start the music when the run begins. Stop it on game over.

## Constraints:
- Look at Crystal.cs for inspiration. It already plays a sound, and the pattern is the same for everything else.
- You are only wiring calls to the audio system. Nothing here requires new logic or clever timing. 

Done when: you press Play, click Start, and hear the turrets fire. When the crystal falls, you hear the game-over sound. Background music plays throughout the run and stops on game over.

### Adjust the wave settings

Since your turrets reload and work well now, you will probably not get a gameover screen. You can play around with the wave variables on the `WaveDirector` gameObject in the editor to overwhelm the turrets with enemies.

[](wave_conf.png)


### Task 2: Give the enemy a sound (set a timer)

Now do the same for the enemy. Make it play a sound, for example `PlayEnemyDied()` when it dies.

**Set a timer for 15 minutes and try.**

## Requirements:

- **Enemy hit**: Every time an enemy is hit, play the shot EnemyHit soud.
- **Enemy death**: When an enemy dies, play the EnemyDied sound

## Constraints:
- Follow the same wiring as crystal.cs

## Reflect:
- Why is it not working?
- Can you think of another implementation to make it work?

Next we will cover the singleton pattern.
