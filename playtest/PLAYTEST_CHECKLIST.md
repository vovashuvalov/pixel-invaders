# Playtest Checklist

Use this as the testing place for manual verification while running the game.

## Startup

- [ ] Game launches from CLI (`dotnet run ...`) without crashes.
- [ ] Main menu appears with Start / Controls / Quit.

## Controls

- [ ] Player moves left/right with both Arrow keys and A/D.
- [ ] Player cannot leave screen bounds.
- [ ] Space shoots with cooldown.

## Core Combat

- [ ] Bullets destroy enemies and increase score.
- [ ] Enemies sweep left/right and drop when hitting edges.
- [ ] Eggs damage player and reduce lives.
- [ ] Hearts display exactly current lives.

## Waves and Difficulty

- [ ] Wave banner appears at wave start.
- [ ] Wave 2 introduces tougher enemies.
- [ ] Wave 3 introduces rapid shooter enemies.
- [ ] Enemy pressure increases each wave.

## Power-ups

- [ ] Power-up drops can appear from defeated enemies.
- [ ] Rapid Fire temporarily shortens fire cooldown.
- [ ] Double Shot temporarily fires two bullets.
- [ ] Active power-up timers show on HUD.

## States

- [ ] Esc opens pause overlay with Resume / Restart / Main Menu.
- [ ] Resume returns to same run.
- [ ] Restart resets a fresh run.
- [ ] Losing all lives opens game over screen.
- [ ] Clearing final wave shows victory screen.
