# Galactic Coop Shooter

A complete C# 2D arcade shooter built with MonoGame (DesktopGL). You pilot a ship, clear enemy chicken formations, dodge eggs, and survive escalating waves.

## Build and Run

```bash
export PATH="$HOME/.dotnet:$PATH"
dotnet restore
dotnet build GalacticCoopShooter.sln
dotnet run --project src/GalacticCoopShooter/GalacticCoopShooter.csproj
```

Shortcuts:

```bash
./scripts/build-game.sh
./scripts/run-game.sh
```

## Controls

- Move: `A` / `D` or `Left` / `Right`
- Shoot: `Space`
- Pause: `Esc`
- Menu Select: `Enter` (and `Up` / `Down` or `W` / `S`)

## Gameplay Summary

- Player starts with 3 lives (heart HUD).
- Enemy chickens spawn in invader-style grids and move as a group.
- Enemies drop eggs with randomized cooldowns and probability.
- 3 waves minimum, each increasing speed/fire pressure and enemy mix.
- Wave 2+ introduces tougher chickens (2 hit) and wave 3 adds rapid shooters.
- Power-ups drop from defeated enemies:
  - Rapid Fire (temporary)
  - Double Shot (temporary)
- States included:
  - Main Menu (Start / Controls / Quit)
  - Pause (Resume / Restart / Main Menu)
  - Game Over / Victory (Final score + restart/menu)

## Project Structure

- `GalacticCoopShooter.sln`: solution file.
- `src/GalacticCoopShooter/ShooterGame.cs`: MonoGame bootstrap + background rendering.
- `src/GalacticCoopShooter/Core`: config, input manager, state machine interfaces.
- `src/GalacticCoopShooter/Entities`: player, enemies, projectiles, power-ups.
- `src/GalacticCoopShooter/Gameplay`: wave spawning/movement and collision resolution.
- `src/GalacticCoopShooter/States`: menu, play, pause, game-over states.
- `src/GalacticCoopShooter/Rendering`: primitive shape + pixel text renderers.
- `scripts`: build/run helper scripts.
- `playtest/PLAYTEST_CHECKLIST.md`: test pass checklist to validate game behavior.
