const ASSETS = "../Assets/Art";
const MAX_FIGHTS = 6;

const CHARACTERS = [
  { id: "bob", name: "Bob", hp: 30, attack: 5, armor: 32, speed: 5, image: `${ASSETS}/Characters/Bob.png`, style: "Steady and adaptable." },
  { id: "garry", name: "Garry", hp: 42, attack: 5, armor: 40, speed: 3, image: `${ASSETS}/Characters/Garry.png`, style: "A stubborn wall of muscle." },
  { id: "kyle", name: "Kyle", hp: 28, attack: 6, armor: 33, speed: 33, image: `${ASSETS}/Characters/Kyle.png`, style: "Quick enough to strike first." },
  { id: "steve", name: "Steve", hp: 45, attack: 4, armor: 67, speed: 16, image: `${ASSETS}/Characters/steve_placeholder.png`, style: "The toughest of the bunch." },
];

const ENEMIES = [
  { name: "Rat Thief", hp: 18, attack: 5, armor: 8, speed: 17, image: `${ASSETS}/Enemies/RAT_One.png` },
  { name: "Slime", hp: 26, attack: 4, armor: 20, speed: 5, image: `${ASSETS}/Enemies/slime.png` },
  { name: "Goblin Fighter", hp: 28, attack: 6, armor: 17, speed: 44, image: `${ASSETS}/Enemies/Goblin_sword.png` },
  { name: "Goblin Sorcerer", hp: 33, attack: 7, armor: 16, speed: 12, image: `${ASSETS}/Enemies/mage_golbin.png` },
  { name: "Human Thug", hp: 38, attack: 8, armor: 25, speed: 24, image: `${ASSETS}/Enemies/human warrior.png` },
  { name: "Ogre Brute", hp: 46, attack: 10, armor: 30, speed: 3, image: `${ASSETS}/Enemies/Ogre club.png` },
  { name: "Dwarf Warrior", hp: 50, attack: 9, armor: 26, speed: 31, image: `${ASSETS}/Enemies/Dwarf axe.png` },
  { name: "Fire Elemental", hp: 55, attack: 10, armor: 22, speed: 28, image: `${ASSETS}/Enemies/Blobl_monster.png` },
];

const BOSSES = [
  { name: "Rat King", hp: 82, attack: 10, armor: 34, speed: 30, image: `${ASSETS}/Enemies/RAT_One.png` },
  { name: "Goblin King", hp: 96, attack: 12, armor: 38, speed: 34, image: `${ASSETS}/Enemies/Goblin_sword.png` },
];

const UPGRADES = [
  { id: "power", name: "Sharpened Edge", kind: "Might", icon: "⚔", description: "+2 Attack. End the fight before it begins.", apply: hero => { hero.attack += 2; } },
  { id: "armor", name: "Iron Hide", kind: "Guard", icon: "⬡", description: "+12 Armor. Turn heavy blows aside.", apply: hero => { hero.armor += 12; } },
  { id: "vitality", name: "Vitality", kind: "Vitality", icon: "♥", description: "+8 Max HP and restore 8 HP now.", apply: hero => { hero.maxHp += 8; hero.hp = Math.min(hero.maxHp, hero.hp + 8); } },
  { id: "quickness", name: "Quickstep", kind: "Tempo", icon: "➶", description: "+12 Speed. The fastest fighter acts first.", apply: hero => { hero.speed += 12; } },
  { id: "twin-strike", name: "Twin Strike", kind: "Technique", icon: "⚔", description: "Every third attack lands twice.", apply: hero => { hero.doubleStrike = true; } },
  { id: "fireball", name: "Fireball", kind: "Ability", icon: "✦", description: "Every second turn, blast your foe for 5 extra damage.", apply: hero => { hero.fireball = true; } },
  { id: "lifesteal", name: "Blood Drinker", kind: "Passive", icon: "♨", description: "Heal for 25% of the damage you deal.", apply: hero => { hero.lifesteal += 0.25; } },
  { id: "thorns", name: "Thorn Armor", kind: "Passive", icon: "❋", description: "Return 3 damage whenever you are hit.", apply: hero => { hero.thorns += 3; } },
  { id: "regeneration", name: "Regeneration", kind: "Passive", icon: "✚", description: "Recover 2 HP at the start of each of your turns.", apply: hero => { hero.regeneration += 2; } },
  { id: "poison", name: "Venomous Edge", kind: "Passive", icon: "☠", description: "Your attacks poison the enemy for 2 damage each turn.", apply: hero => { hero.poison += 2; } },
];

const game = document.querySelector("#game");
const state = {
  screen: "title",
  selectedCharacter: CHARACTERS[0],
  hero: null,
  enemy: null,
  fight: 1,
  wins: 0,
  log: [],
  choices: [],
  battleRunning: false,
  battleToken: 0,
  result: null,
  endedBy: null,
};

function esc(value) {
  return String(value).replace(/[&<>"']/g, char => ({
    "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;",
  })[char]);
}

function header(note = "A roguelike auto-battler") {
  return `<header class="topbar"><div class="brand"><span class="brand-mark">✦</span>THREE CHOICES</div><div class="topbar-note">${note}</div></header>`;
}

function render() {
  const screens = {
    title: renderTitle,
    select: renderCharacterSelect,
    battle: renderBattle,
    draft: renderDraft,
    ending: renderEnding,
  };
  game.innerHTML = `${header(state.screen === "battle" ? `THE WILDS · FIGHT ${state.fight} / ${MAX_FIGHTS}` : "A roguelike auto-battler")}${screens[state.screen]()}`;
  bindEvents();
}

function renderTitle() {
  return `<section class="screen title-screen">
    <p class="eyebrow">A tiny browser edition</p>
    <h1 class="hero-title">Three<br>Choices</h1>
    <div class="title-rule"></div>
    <p class="tagline">Choose your champion. Build your legend. See how far you can make it through the wilds.</p>
    <div class="ornament" aria-hidden="true">✦ &nbsp; ⚔ &nbsp; ✦</div>
    <button class="button" data-action="begin">Begin your run</button>
    <p class="small-note">One hero · three upgrades · one last boss</p>
  </section>`;
}

function renderCharacterSelect() {
  const cards = CHARACTERS.map(character => `<button class="character-card ${state.selectedCharacter.id === character.id ? "selected" : ""}" data-character="${character.id}" aria-pressed="${state.selectedCharacter.id === character.id}">
      <div class="portrait-wrap"><img src="${character.image}" alt="${esc(character.name)}" onerror="this.style.opacity='.15'"></div>
      <h3>${esc(character.name)}</h3>
      <p class="character-description">${character.style}</p>
      <div class="stat-line"><span>HEALTH</span><strong>${character.hp}</strong></div>
      <div class="stat-line"><span>ATTACK</span><strong>${character.attack}</strong></div>
      <div class="stat-line"><span>ARMOR</span><strong>${character.armor}</strong></div>
      <div class="stat-line"><span>SPEED</span><strong>${character.speed}</strong></div>
    </button>`).join("");
  return `<section class="screen select-screen">
    <p class="eyebrow">Choose your champion</p>
    <h2 class="section-heading">Who enters the wilds?</h2>
    <p class="section-copy">Every hero starts with different strengths. Your choices shape the rest.</p>
    <div class="character-grid">${cards}</div>
    <div class="selection-actions"><button class="button" data-action="start-run">Enter the wilds</button></div>
  </section>`;
}

function hpBar(unit, enemy = false) {
  const percent = Math.max(0, Math.min(100, unit.hp / unit.maxHp * 100));
  return `<div class="health-meta"><span>${Math.max(0, unit.hp)} / ${unit.maxHp} HP</span><span>${Math.round(percent)}%</span></div>
    <div class="health-track" role="progressbar" aria-label="${enemy ? "Enemy" : "Hero"} health" aria-valuenow="${Math.max(0, unit.hp)}" aria-valuemin="0" aria-valuemax="${unit.maxHp}"><div class="health-fill" style="width:${percent}%"></div></div>`;
}

function renderBattle() {
  const hero = state.hero;
  const enemy = state.enemy;
  const logs = state.log.slice(-6).reverse().map(entry => `<li class="${entry.type || ""}">${entry.text}</li>`).join("");
  const tags = hero.upgrades.length
    ? hero.upgrades.map(upgrade => `<span class="build-tag">${esc(upgrade.name)}</span>`).join("")
    : `<span class="empty-build">No upgrades yet. Earn your first after this fight.</span>`;
  const action = state.battleRunning
    ? `<p class="battle-status">The battle is unfolding…</p>`
    : state.result === "win"
      ? `<p class="battle-status">Victory. The wilds offer a new choice.</p><button class="button" data-action="draft">Choose your upgrade</button>`
      : state.result === "loss"
        ? `<p class="battle-status">Your run ends here.</p><button class="button secondary" data-action="restart">Start a new run</button>`
        : `<p class="battle-status">Your champion battles automatically.</p><button class="button" data-action="fight">Begin fight ${state.fight}</button>`;

  return `<section class="screen battle-screen">
    <div class="battle-topline"><span class="fight-count">${state.fight === MAX_FIGHTS ? "FINAL BOSS" : `FIGHT ${state.fight} / ${MAX_FIGHTS}`}</span><span>${state.wins} ${state.wins === 1 ? "victory" : "victories"}</span></div>
    <div class="arena">
      <div class="unit-card ${state.attacking === "hero" ? "attacking" : ""}">
        <h2 class="unit-name">${esc(hero.name)}</h2>
        <img class="unit-image" src="${hero.image}" alt="${esc(hero.name)}" onerror="this.style.opacity='.15'">
        ${hpBar(hero)}
        <div class="unit-stats"><span>ATK <b>${hero.attack}</b></span><span>ARM <b>${hero.armor}</b></span><span>SPD <b>${hero.speed}</b></span></div>
      </div>
      <div class="versus" aria-hidden="true"><span>VS</span></div>
      <div class="unit-card enemy ${state.attacking === "enemy" ? "attacking" : ""}">
        <h2 class="unit-name">${esc(enemy.name)}${enemy.boss ? `<span class="boss-label">BOSS</span>` : ""}</h2>
        <img class="unit-image" src="${enemy.image}" alt="${esc(enemy.name)}" onerror="this.style.opacity='.15'">
        ${hpBar(enemy, true)}
        <div class="unit-stats"><span>ATK <b>${enemy.attack}</b></span><span>ARM <b>${enemy.armor}</b></span><span>SPD <b>${enemy.speed}</b></span></div>
      </div>
    </div>
    <div class="combat-layout">
      <section class="combat-log" aria-label="Combat log"><h3 class="panel-label">Battle log</h3><ul class="log-entries">${logs || "<li>The arena falls quiet.</li>"}</ul></section>
      <section class="run-build"><h3 class="panel-label">Your build · ${hero.upgrades.length} ${hero.upgrades.length === 1 ? "upgrade" : "upgrades"}</h3><div class="build-tags">${tags}</div></section>
    </div>
    <div class="battle-actions">${action}</div>
  </section>`;
}

function renderDraft() {
  const cards = state.choices.map((upgrade, index) => `<button class="upgrade-card" data-upgrade="${index}">
      <span class="upgrade-icon" aria-hidden="true">${upgrade.icon}</span>
      <div class="upgrade-kind">${upgrade.kind}</div>
      <h3>${upgrade.name}</h3>
      <p>${upgrade.description}</p>
    </button>`).join("");
  return `<section class="screen draft-screen">
    <p class="eyebrow">Fight ${state.fight} won · ${state.wins} ${state.wins === 1 ? "victory" : "victories"}</p>
    <h2 class="section-heading">Choose your next edge</h2>
    <p class="section-copy">Three paths forward. Pick one upgrade to carry into the next fight.</p>
    <div class="upgrade-grid">${cards}</div>
    <p class="recovery-note">The breather restores 5 HP before your next battle.</p>
    <button class="button secondary" data-action="skip-upgrade">Skip this choice</button>
  </section>`;
}

function renderEnding() {
  const victory = state.endedBy === "victory";
  const bossName = state.enemy.name;
  return `<section class="screen ending-screen ${victory ? "victory" : "defeat"}">
    <div class="ending-mark" aria-hidden="true">${victory ? "✦" : "☠"}</div>
    <p class="eyebrow">${victory ? "The wilds are conquered" : "Run ended"}</p>
    <h2 class="section-heading">${victory ? "A legend is born." : `${esc(state.hero.name)} has fallen.`}</h2>
    <p class="tagline">${victory ? `${esc(state.hero.name)} defeated ${bossName} and made it out alive.` : "The wilds claim another challenger. Every run begins with a new choice."}</p>
    <div class="run-summary">Fights won: <strong>${state.wins} / ${MAX_FIGHTS}</strong><br>Upgrades chosen: <strong>${state.hero.upgrades.length}</strong></div>
    <button class="button" data-action="restart">Play again</button>
  </section>`;
}

function bindEvents() {
  game.querySelector('[data-action="begin"]')?.addEventListener("click", () => { state.screen = "select"; render(); });
  game.querySelectorAll("[data-character]").forEach(card => card.addEventListener("click", () => {
    state.selectedCharacter = CHARACTERS.find(character => character.id === card.dataset.character);
    render();
  }));
  game.querySelector('[data-action="start-run"]')?.addEventListener("click", startRun);
  game.querySelector('[data-action="fight"]')?.addEventListener("click", startBattle);
  game.querySelector('[data-action="draft"]')?.addEventListener("click", showDraft);
  game.querySelector('[data-action="restart"]')?.addEventListener("click", () => { state.screen = "select"; render(); });
  game.querySelectorAll("[data-upgrade]").forEach(card => card.addEventListener("click", () => chooseUpgrade(Number(card.dataset.upgrade))));
  game.querySelector('[data-action="skip-upgrade"]')?.addEventListener("click", () => proceedAfterDraft(null));
}

function startRun() {
  const selected = state.selectedCharacter;
  state.hero = {
    ...selected,
    maxHp: selected.hp,
    hp: selected.hp,
    attack: selected.attack,
    armor: selected.armor,
    speed: selected.speed,
    upgrades: [],
    doubleStrike: false,
    fireball: false,
    lifesteal: 0,
    thorns: 0,
    regeneration: 0,
    poison: 0,
  };
  state.fight = 1;
  state.wins = 0;
  state.result = null;
  state.endedBy = null;
  state.log = [{ text: `${selected.name} enters the wilds.`, type: "" }];
  prepareEnemy();
  state.screen = "battle";
  render();
}

function prepareEnemy() {
  const boss = state.fight === MAX_FIGHTS;
  const pool = boss ? BOSSES : ENEMIES;
  const base = boss
    ? pool[Math.floor(Math.random() * pool.length)]
    : pool[Math.floor(Math.random() * Math.min(pool.length, 2 + state.fight))];
  const scaling = 1 + (state.fight - 1) * 0.12;
  state.enemy = {
    ...base,
    boss,
    maxHp: Math.round(base.hp * scaling),
    hp: Math.round(base.hp * scaling),
    attack: Math.max(1, Math.round(base.attack * (1 + (state.fight - 1) * 0.08))),
    poisonStacks: 0,
  };
}

function startBattle() {
  if (state.battleRunning || state.result) return;
  state.battleRunning = true;
  state.battleToken += 1;
  state.turn = 0;
  state.heroAttackCount = 0;
  state.actor = state.hero.speed >= state.enemy.speed ? "hero" : "enemy";
  state.log = [{ text: `${state.enemy.boss ? "The boss" : "A"} ${state.enemy.name} approaches.`, type: "" }];
  addLog(`${state.actor === "hero" ? state.hero.name : state.enemy.name} is quicker and takes the first turn.`);
  render();
  scheduleTurn(state.battleToken);
}

function scheduleTurn(token) {
  window.setTimeout(() => {
    if (token !== state.battleToken || !state.battleRunning) return;
    resolveTurn();
  }, 620);
}

function resolveTurn() {
  if (state.hero.hp <= 0 || state.enemy.hp <= 0 || state.turn >= 32) {
    finishBattle(state.hero.hp > 0 && state.enemy.hp <= 0);
    return;
  }
  state.turn += 1;
  const heroActs = state.actor === "hero";
  const actor = heroActs ? state.hero : state.enemy;
  const target = heroActs ? state.enemy : state.hero;
  state.attacking = state.actor;
  if (heroActs) state.heroAttackCount += 1;

  if (heroActs && actor.regeneration > 0 && actor.hp < actor.maxHp) {
    const healed = Math.min(actor.regeneration, actor.maxHp - actor.hp);
    actor.hp += healed;
    addLog(`${actor.name} regenerates ${healed} HP.`, "heal");
  }
  if (heroActs && state.enemy.poisonStacks > 0) {
    const poisonDamage = Math.min(state.enemy.hp, state.enemy.poisonStacks);
    state.enemy.hp -= poisonDamage;
    addLog(`Poison deals ${poisonDamage} damage to ${state.enemy.name}.`, "damage");
    if (state.enemy.hp <= 0) {
      render();
      window.setTimeout(() => finishBattle(true), 500);
      return;
    }
  }

  let damage = Math.max(1, Math.ceil(actor.attack * 100 / (100 + target.armor)));
  if (heroActs && actor.hp <= actor.maxHp / 2) damage = Math.ceil(damage * 1.2);
  if (heroActs && actor.fireball && state.heroAttackCount % 2 === 0) {
    damage += 5;
    addLog(`${actor.name} casts Fireball!`);
  }
  const hits = heroActs && actor.doubleStrike && state.heroAttackCount % 3 === 0 ? 2 : 1;
  let dealt = 0;
  for (let hit = 0; hit < hits && target.hp > 0; hit += 1) {
    const actual = Math.min(target.hp, damage);
    target.hp -= actual;
    dealt += actual;
    addLog(`${actor.name} hits ${target.name} for ${actual}.`, "damage");
  }
  if (heroActs && actor.poison > 0 && target.hp > 0) {
    target.poisonStacks += actor.poison;
    addLog(`${target.name} is poisoned (${target.poisonStacks} per turn).`);
  }
  if (heroActs && actor.lifesteal > 0 && dealt > 0) {
    const healed = Math.min(actor.maxHp - actor.hp, Math.floor(dealt * actor.lifesteal));
    if (healed > 0) {
      actor.hp += healed;
      addLog(`${actor.name} drains ${healed} HP.`, "heal");
    }
  }
  if (!heroActs && target.thorns > 0 && actor.hp > 0) {
    const reflected = Math.min(target.thorns, actor.hp);
    actor.hp -= reflected;
    addLog(`${actor.name} takes ${reflected} thorn damage from ${target.name}.`, "damage");
  }
  state.actor = heroActs ? "enemy" : "hero";
  render();
  scheduleTurn(state.battleToken);
}

function finishBattle(won) {
  state.battleRunning = false;
  state.attacking = null;
  state.result = won ? "win" : "loss";
  if (won) {
    state.wins += 1;
    addLog(`${state.hero.name} wins the fight!`);
    if (state.fight === MAX_FIGHTS) {
      state.endedBy = "victory";
      state.screen = "ending";
    }
  } else {
    addLog(`${state.hero.name} falls in battle.`);
    state.endedBy = "defeat";
    state.screen = "ending";
  }
  render();
}

function addLog(text, type = "") {
  state.log.push({ text, type });
  if (state.log.length > 18) state.log.shift();
}

function showDraft() {
  const available = UPGRADES.filter(upgrade => !state.hero.upgrades.some(owned => owned.id === upgrade.id));
  state.choices = shuffle(available).slice(0, 3);
  state.screen = "draft";
  render();
}

function shuffle(items) {
  const shuffled = [...items];
  for (let index = shuffled.length - 1; index > 0; index -= 1) {
    const swapIndex = Math.floor(Math.random() * (index + 1));
    [shuffled[index], shuffled[swapIndex]] = [shuffled[swapIndex], shuffled[index]];
  }
  return shuffled;
}

function chooseUpgrade(index) {
  proceedAfterDraft(state.choices[index]);
}

function proceedAfterDraft(upgrade) {
  if (upgrade) {
    upgrade.apply(state.hero);
    state.hero.upgrades.push(upgrade);
    addLog(`${state.hero.name} chose ${upgrade.name}.`);
  }
  const previousHp = state.hero.hp;
  state.hero.hp = Math.min(state.hero.maxHp, state.hero.hp + 5);
  const restoredHp = state.hero.hp - previousHp;
  state.fight += 1;
  state.result = null;
  state.log = [{ text: `A short rest restores ${restoredHp} HP.`, type: "heal" }, ...state.log.slice(-4)];
  prepareEnemy();
  state.screen = "battle";
  render();
}

render();
