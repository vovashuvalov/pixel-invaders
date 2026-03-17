# Pixel Invaders

`Pixel Invaders` is a C# 2D arcade shooter built with MonoGame DesktopGL. You pilot a starfighter, clear waves of pixel aliens, collect power-ups, and survive five themed levels that end with boss fights.

## Build and Run

```bash
export PATH="$HOME/.dotnet:$PATH"
dotnet restore
dotnet build GalacticCoopShooter.sln
dotnet run --project src/GalacticCoopShooter/GalacticCoopShooter.csproj
```

Shortcut scripts:

```bash
./scripts/build-game.sh
./scripts/run-game.sh
```

## Open on Mac

If the project is already on your Mac, open it from Terminal:

```bash
cd /Users/vladimirshuvalov/Documents/galactic-coop-shooter
open .
```

Open it in an editor:

```bash
cd /Users/vladimirshuvalov/Documents/galactic-coop-shooter
open -a "Visual Studio Code" .
```

Or open the solution in Rider:

```bash
cd /Users/vladimirshuvalov/Documents/galactic-coop-shooter
open -a "Rider" GalacticCoopShooter.sln
```

Then run the game from the same folder:

```bash
export PATH="$HOME/.dotnet:$PATH"
dotnet run --project src/GalacticCoopShooter/GalacticCoopShooter.csproj
```

## Controls

- Move: `WASD` or `Arrow Keys`
- Fire: hold `Space` or `Left Mouse Button`
- Pause: `Esc`
- Menu navigation: `WASD` or `Arrow Keys`, confirm with `Enter` or `Space`

## Game Overview

- 5 levels: `Earth Station`, `Moon`, `Ring World`, `Blue Giant`, `Red Front`
- 3 regular waves per level plus a boss encounter
- 5 enemy types:
  - `Green` drifter
  - `Red` diver
  - `Blue` laser shooter
  - `Yellow` miner
  - `Boss` crab alien
- 4 power-ups:
  - `Extra Life`
  - `Triple Shot` for 10 seconds
  - `Bomb` that clears the screen and heavily damages bosses
  - `Shield` for 5 seconds

## Project Structure

- `GalacticCoopShooter.sln` - solution entry point
- `src/GalacticCoopShooter/ShooterGame.cs` - MonoGame bootstrap and shared background rendering
- `src/GalacticCoopShooter/Core` - config, input manager, and state machine interfaces
- `src/GalacticCoopShooter/Gameplay/LevelDefinition.cs` - campaign level metadata and theme data
- `src/GalacticCoopShooter/Gameplay/WaveManager.cs` - level progression, wave spawning, and boss progression
- `src/GalacticCoopShooter/Gameplay/CollisionManager.cs` - AABB collision resolution and power-up drops
- `src/GalacticCoopShooter/Entities` - player, enemies, power-ups, and projectile types
- `src/GalacticCoopShooter/States` - menu, play, pause, and game-over states
- `src/GalacticCoopShooter/Rendering` - pixel font and primitive drawing helpers
- `playtest/PLAYTEST_CHECKLIST.md` - manual verification checklist

## Notes

- Visuals are generated from primitive shapes only.
- The game is fully CLI-runnable with `dotnet build` and `dotnet run`.
