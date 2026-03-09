# Three Choices — Web Edition

A standalone vanilla JavaScript recreation of the Three Choices auto-battler, playable in any browser with zero dependencies.

## How to Play

Open `index.html` in a browser (or serve this directory with any static file server).

```bash
# Example: Python's built-in server
cd WebEdition
python3 -m http.server 8080
# Open http://localhost:8080
```

## Features

- **4 playable characters** with original pixel art and stats from ScriptableObjects (Bob, Garry, Kyle, Steve)
- **Full combat system** — lunge attacks, projectile animations, floating damage numbers, hit reactions, death animations, health bars
- **Upgrade draft system** — 18 upgrades across 4 rarities (Common → Epic), including passives, abilities, and stat boosts
- **Status effects** — Poison, Bleed, Burn with stacking and duration
- **Passives** — Lifesteal, Thorns, Rage, Execute, Double Strike, Critical Chance, Death Shield
- **Abilities** — Fireball (with Burn DoT), Arcane Missiles
- **4 enemy tiers** scaling with fight progress, from Slimes and Goblins to Dragons and Titans
- **Boss fights** every 10 rounds with artifact rewards (Gorloc the Destroyer, Iron Colossus, Shadow Lord)
- **High score leaderboard** — localStorage-backed, top 10, with name entry and medals
- **Speed control** — Normal / Fast / Turbo
- **Combat log** with color-coded entries

## Art Assets

References the existing art from `Assets/Art/` via relative paths. No art files are duplicated.

## Technical Notes

- Single HTML file, ~40KB, no external dependencies
- All game data extracted from the Unity ScriptableObjects (character stats, enemy definitions, boss definitions)
- Combat engine mirrors the C# implementation: armor mitigation formula, speed-based turn order, status effect ticking, damage pipeline
- Security headers included in the custom server script (CSP, X-Frame-Options, etc.)

## Credits

Based on [Three Choices](https://github.com/edvinklaebo/Three-Choices) by Edvin Klæbo and contributors.
