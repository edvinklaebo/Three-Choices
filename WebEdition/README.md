# Three Choices — Web Edition

A standalone vanilla JavaScript recreation of the Three Choices auto-battler, playable in any browser with zero dependencies.

## 🎮 Play Now

If GitHub Pages is enabled, play at:
**https://edvinklaebo.github.io/Three-Choices/**

## Features

- **4 playable characters** with original pixel art and stats (Bob, Garry, Kyle, Steve)
- **Full combat system** — lunge attacks, projectile animations, floating damage numbers, hit reactions, death animations
- **18 upgrades** across 4 rarities (Common → Epic)
- **Status effects** — Poison, Bleed, Burn with stacking and duration
- **Passives** — Lifesteal, Thorns, Rage, Execute, Double Strike, Critical Chance, Death Shield
- **Abilities** — Fireball (with Burn DoT), Arcane Missiles
- **4 enemy tiers** scaling with progress, from Slimes to Dragons
- **Boss fights** every 10 rounds with artifact rewards
- **🏆 High score leaderboard** — persistent (localStorage), top 10, name entry, medals
- **Speed control** — Normal / Fast / Turbo
- **Combat log** with color-coded entries

## Run Locally

Just open `index.html` in a browser, or serve with any static file server:

```bash
cd WebEdition
python3 -m http.server 8080
# Open http://localhost:8080
```

## Deploy to GitHub Pages

### Option A: Automatic (GitHub Actions)

A workflow is included at `.github/workflows/deploy-web-edition.yml` that auto-deploys on push.

1. Go to your repo **Settings → Pages**
2. Under **Source**, select **GitHub Actions**
3. Push any change to `WebEdition/` on `main` or `develop`
4. The workflow deploys automatically
5. Your game is live at `https://<username>.github.io/Three-Choices/`

You can also trigger it manually from **Actions → Deploy Web Edition → Run workflow**.

### Option B: Manual (gh-pages branch)

```bash
# From the repo root
git subtree push --prefix WebEdition origin gh-pages
```

Then in **Settings → Pages**, set Source to **Deploy from a branch** → `gh-pages` / `/ (root)`.

### Option C: Direct Upload

1. Go to **Settings → Pages → Source → GitHub Actions**
2. Or just drop the `WebEdition/` folder on any static host (Netlify, Vercel, Cloudflare Pages, etc.)

## Technical Notes

- Single HTML file, ~40KB, no build step, no external dependencies
- All game data from the Unity ScriptableObjects (character stats, enemy definitions, boss stats)
- Combat engine mirrors the C# implementation (armor mitigation formula, speed-based turn order, status ticking, damage pipeline)
- Art assets bundled in `art/` — originals from `Assets/Art/`
- High scores stored in browser localStorage (per-device, no server needed)

## Credits

Based on [Three Choices](https://github.com/edvinklaebo/Three-Choices) by Edvin Klæbo and contributors.
Web edition by [@Wong1dev](https://github.com/Wong1dev).
