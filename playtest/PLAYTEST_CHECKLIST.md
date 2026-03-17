# Pixel Invaders Playtest Checklist

## Startup

- [ ] `dotnet run --project src/GalacticCoopShooter/GalacticCoopShooter.csproj` launches the game
- [ ] Main menu shows `Start`, `Controls`, and `Quit`
- [ ] Controls overlay mentions keyboard and mouse fire input

## Controls and Core Feel

- [ ] Ship moves with both `WASD` and arrow keys
- [ ] Ship stays inside screen bounds
- [ ] Holding `Space` fires continuously
- [ ] Holding left mouse button also fires continuously
- [ ] `Esc` pauses and resumes correctly

## Campaign Flow

- [ ] Level 1 starts in `Earth Station`
- [ ] Each level contains 3 regular waves before the boss
- [ ] Boss appears at the end of every level
- [ ] Clearing the final boss shows the victory screen

## Enemies

- [ ] Green enemies drift straight down
- [ ] Red enemies dive toward the player lane
- [ ] Blue enemies shoot lasers
- [ ] Yellow enemies leave mines
- [ ] Boss crab fires multi-shot attacks and mines

## Power-ups

- [ ] `Extra Life` increases lives up to the cap
- [ ] `Triple Shot` lasts 10 seconds
- [ ] `Bomb` clears current enemies and projectiles
- [ ] `Shield` lasts 5 seconds and blocks damage
- [ ] Active timed power-ups appear on the HUD

## Combat and States

- [ ] Score increases when enemies are destroyed
- [ ] Enemy projectiles damage the player
- [ ] Reaching zero lives opens the game over screen
- [ ] Pause menu supports `Resume`, `Restart`, and `Main Menu`
