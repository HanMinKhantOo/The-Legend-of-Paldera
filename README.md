
# The Legend of Paldera

A 2D pixel-art survival RPG developed using Unity and C#.

Team Members
- Phone Khant Aung (6712096)
- Thar Htet Zaw (6712149)
- Han Min Khant Oo (6712157)

## 1. About the Game

The Legend of Paldera is a top-down survival role-playing game set on a mysterious island filled with ancient ruins, magical creatures, valuable resources, and unexplored territories.

Players must survive by collecting resources, crafting equipment, fighting enemies, completing quests, and exploring the island.

The game combines survival mechanics, resource management, crafting, combat, and RPG exploration.

### Story

A young boy fascinated by science and time travel attempts to build a machine capable of travelling through time.

However, an unexpected malfunction causes his experiment to go terribly wrong.

When he awakens, he finds himself in a mysterious land known as Paldera, far away from the world he once knew.

Stranded on an unfamiliar island, he must learn to survive, discover the secrets of Paldera, and find a way home.

The planned opening sequence introduces this story before the character awakens beneath the ancient Yggdrasil tree.

---

## 2. Game Information

| Category | Details |
|---|---|
| Game Title | The Legend of Paldera |
| Genre | 2D Survival RPG |
| Game Engine | Unity 6 |
| Unity Editor Version | 6000.5.3f1 |
| Programming Language | C# |
| Graphics | 2D Pixel Art |
| Perspective | Top-Down |
| Game Mode | Single Player |
| Development Status | In Development |
| Platforms | Windows and macOS (Unity development) |

---

## 3. Installation and Setup

### Requirements

To open and run the project, install:

- Unity Hub
- Unity Editor 6000.5.3f1
- Git (if cloning through Terminal)

The project uses Unity's Universal Render Pipeline, 2D tools, and Input System.

### Running the Game on macOS

1. Download or clone this repository.
2. Open Unity Hub.
3. Select Add Project.
4. Locate the downloaded `The-Legend-of-Paldera` folder.
5. Open the project using the required Unity Editor version.
6. Allow Unity to import all project assets.
7. Open `Assets/Scenes/MainMenu.unity`.
8. Press the Play button at the top of Unity.
9. Select New Game from the main menu.

### Clone Using macOS Terminal

Open Terminal and enter:

```bash
git clone -b Game-setup https://github.com/HanMinKhantOo/The-Legend-of-Paldera.git
```

This downloads the Game-setup development branch.

**Important:** The latest gameplay development is on the `Game-setup` branch, not necessarily the default `main` branch.

---

## 4. Gameplay Controls

The game is designed to use standard keyboard inputs and a mouse or MacBook trackpad.

No Windows-specific keyboard keys are required for normal gameplay.

### Movement

| Key | Action |
|---|---|
| W | Move Up |
| A | Move Left |
| S | Move Down |
| D | Move Right |
| W + A | Move Diagonally Up-Left |
| W + D | Move Diagonally Up-Right |
| S + A | Move Diagonally Down-Left |
| S + D | Move Diagonally Down-Right |

The character can move in eight directions while using four-directional character animations.

The character automatically faces the last movement direction when standing still.

### Combat and Interaction

| Key | Action |
|---|---|
| Space | Punch or attack |
| E | Interact with nearby NPCs |
| E | Open or close NPC dialogue |
| E | Interact with ruins or altar objects |

To attack, stand near the target and face it before pressing Space.

Attack detection occurs in front of the character.

### Inventory and Crafting

| Key | Action |
|---|---|
| Tab | Open or close the menu/inventory interface |
| C | Open or close the crafting interface |
| Left Click | Select an inventory item or crafting material |
| Drag and Drop | Rearrange items in inventory |
| Double Click Food | Consume one food item |

### Hotbar

| Key | Action |
|---|---|
| 1 | Select Hotbar Slot 1 |
| 2 | Select Hotbar Slot 2 |
| 3 | Select Hotbar Slot 3 |
| 4 | Select Hotbar Slot 4 |
| 5 | Select Hotbar Slot 5 |
| 6 | Select Hotbar Slot 6 |
| 7 | Select Hotbar Slot 7 |
| 8 | Select Hotbar Slot 8 |

Currently, the first four hotbar slots are associated with:

| Slot | Item |
|---|---|
| 1 | Wooden Sword |
| 2 | Wooden Pickaxe |
| 3 | Wooden Axe |
| 4 | Food |
| 5–8 | Reserved for future items |

Items must be available in the player's inventory before they appear in their corresponding hotbar slots.

### MacBook Notes

- Use the regular number keys at the top of the keyboard.
- Press `Tab` directly to open the menu.
- Press `C` directly to open crafting.
- Use the `Space` bar to attack.
- Mouse clicks can also be performed using a MacBook trackpad.
- Click inside Unity's Game window if the keyboard does not respond.

No Command (⌘) key combination is required for the gameplay controls listed above.

---

## 5. Main Gameplay Mechanics

### Exploration

Players can explore Paldera and discover different environments containing trees, resources, NPCs, enemies, and ruins.

The game includes procedurally generated environmental content alongside manually placed game objects.

Different regions contain different resources and gameplay opportunities.

### Resource Gathering

Players can collect resources scattered throughout the world.

Examples include:

- Branches
- Logs
- Stones
- Food
- Other resource pickups

Walking into a collectible item adds it to the inventory, provided there is available inventory space.

Resources can be used for crafting or quest completion.

### Tree Cutting

Trees can be attacked to obtain logs.

The current tree system uses an eight-hit default health value.

When a tree is destroyed, it drops a random number of logs.

| Resource | Drop Amount |
|---|---|
| Tree | 3–5 Logs |

Players can collect these logs by walking over them.

Logs are used to craft planks and other wooden equipment.

### Stone Mining

The stone mining system is currently under development.

The intended mining mechanic is:

1. Equip a Wooden Pickaxe.
2. Approach a stone boulder.
3. Face the boulder.
4. Press Space to attack.
5. Break the boulder after eight successful hits.
6. Collect five stone pickups.

| Resource | Required Equipment | Hits | Drops |
|---|---|---|---|
| Stone Boulder | Wooden Pickaxe | 8 | 5 Stones |

**Development note:** The stone health and pickup scripts have been created, but reliable mining interaction is still being debugged. This feature should not yet be considered fully functional.

---

## 6. Inventory System

The inventory stores resources, food, equipment, and quest-related items.

Features include:

- Item collection
- Item stacking
- Item quantities
- Inventory slot management
- Drag-and-drop item rearrangement
- Item consumption
- Crafting material storage

The inventory controller currently creates 40 inventory slots by default.

Inventory items have a configurable maximum stack size, with 99 as the default value.

Examples of inventory items include:

| Item | Purpose |
|---|---|
| Branch | Collectible resource |
| Log | Crafting material |
| Plank | Crafting material |
| Stick | Crafting material |
| Stone | Resource for future crafting |
| Food | Restores hunger |
| Wooden Sword | Combat equipment |
| Wooden Pickaxe | Mining equipment |
| Wooden Axe | Woodcutting equipment |
| Colored Gems | Quest and altar items |

---

## 7. Crafting System

Players can convert collected resources into useful materials and equipment.

Press `C` to open the crafting interface.

The crafting system uses a 3 × 3 crafting grid.

### How to Craft

1. Collect the required resources.
2. Press C to open the crafting interface.
3. Select a material from the available inventory.
4. Click the desired position in the crafting grid.
5. Repeat until the recipe is complete.
6. Click the crafting output to create the item.

Placing an ingredient into the crafting grid removes one unit from the inventory.

Clicking an occupied crafting slot returns its ingredient to the inventory.

The completed item is added to the player's inventory.

### Crafting Symbols

| Symbol | Material |
|---|---|
| P | Wooden Plank |
| S | Stick |
| - | Empty Slot |

### Recipe 1: Wooden Planks

**Ingredients:** 1 Log

**Output:** 2 Wooden Planks

Place one log into the crafting grid.

### Recipe 2: Sticks

**Ingredients:** 1 Wooden Plank

**Output:** 4 Sticks

Place one wooden plank into the crafting grid.

### Recipe 3: Wooden Sword

**Ingredients:**
- 2 Wooden Planks
- 1 Stick

**Output:** 1 Wooden Sword

Crafting pattern:

```text
[ - ] [ P ] [ - ]
[ - ] [ P ] [ - ]
[ - ] [ S ] [ - ]
```

### Recipe 4: Wooden Pickaxe

**Ingredients:**
- 3 Wooden Planks
- 2 Sticks

**Output:** 1 Wooden Pickaxe

Crafting pattern:

```text
[ P ] [ P ] [ P ]
[ - ] [ S ] [ - ]
[ - ] [ S ] [ - ]
```

### Recipe 5: Wooden Axe

**Ingredients:**
- 3 Wooden Planks
- 2 Sticks

**Output:** 1 Wooden Axe

Crafting pattern:

```text
[ P ] [ P ] [ - ]
[ P ] [ S ] [ - ]
[ - ] [ S ] [ - ]
```

The wooden axe also supports a mirrored crafting pattern:

```text
[ - ] [ P ] [ P ]
[ - ] [ S ] [ P ]
[ - ] [ S ] [ - ]
```

---

## 8. Combat System

The game features directional combat.

Players can attack enemies while facing up, down, left, or right.

Press Space to perform an attack.

The combat system uses an attack hitbox positioned in front of the player.

Current combat features include:

- Directional attacks
- Punch animations
- Wooden weapon animations
- Enemy health
- Enemy damage
- Enemy death
- Equipment-specific animation controllers

Examples of enemies include magical wolves and other creatures encountered throughout Paldera.

### Weapon Types

| Weapon | Gameplay Role |
|---|---|
| Bare Hands | Basic attacks |
| Wooden Sword | Combat |
| Wooden Pickaxe | Mining |
| Wooden Axe | Woodcutting |

---

## 9. Survival System

The player has two primary survival attributes:

### Health (HP)

Health represents the player's remaining life.

Taking damage reduces health.

When health reaches zero, the character dies.

### Hunger

Hunger gradually decreases over time.

Players must consume food to restore hunger.

When hunger reaches zero, the player begins losing health.

The player can also regenerate health gradually while sufficiently well-fed.

### Eating Food

To eat:

1. Open the inventory using Tab.
2. Locate a food item.
3. Double-click the food.
4. The player consumes one item.

Food restores hunger and may also restore HP, depending on the item's configuration.

---

## 10. Death and Respawning

When the player's health reaches zero, a death screen appears.

The screen displays the cause of death.

The player can select Respawn to return to the game.

The current respawn system restores:

- Health
- Hunger
- Player movement and attack controls

The respawn system attempts to return the player to their saved position.

---

## 11. NPC and Dialogue System

Paldera contains NPCs that players can interact with.

To speak with an NPC:

1. Approach the NPC.
2. Press E.
3. Read the dialogue.
4. Press E again to close the conversation.

NPCs can provide dialogue, quest information, and rewards.

Some NPCs stop moving and turn toward the player during conversations.

---

## 12. Quest System

Players can complete quests assigned by NPCs.

The current quest system supports two main objective types:

### Enemy Hunting

Defeat a required number of a specified enemy.

### Resource Gathering

Collect a required quantity of a specified resource.

### Quest Progression

1. Talk to a quest-giving NPC.
2. Receive the quest objective.
3. Complete the required task.
4. Return to the NPC.
5. Receive the quest reward.

The quest interface displays active objectives and progress.

Completed objectives can display a completion message before disappearing from the quest panel after reward collection.

Quest rewards can include colored gems.

---

## 13. Ancient Ruins and the Golem Altar

Paldera contains ancient ruins and mysterious structures.

Players can interact with certain objects by approaching them and pressing E.

One of the game's planned progression objectives involves collecting four colored gems:

- Red Gem
- Green Gem
- Blue Gem
- Yellow Gem

The altar system checks whether the player possesses the required gems.

When all required gems are available, the altar can activate and summon a golem.

This system uses the game's existing inventory, interaction, and enemy systems.

---

## 14. Saving and Loading

The project includes a JSON-based save system.

The current SaveController supports:

- Saving the player's position
- Loading the player's saved position
- Deleting save data

Save files are stored in Unity's application persistent data directory.

**Important:** Full inventory, quest, equipment, and world-state persistence is not yet implemented.

The Continue button currently loads the gameplay scene in the same way as New Game, while SaveController handles loading the saved player position when available.

---

## 15. Current Development Progress

### Implemented or Partially Implemented

- Main menu
- Player movement
- Four-directional character animations
- Basic combat
- Wooden weapon animations
- Stone weapon animation assets
- Inventory interface
- Hotbar
- Resource pickups
- Wooden crafting recipes
- Tree destruction and log drops
- NPC interaction
- Dialogue system
- Quest tracking
- Health and hunger
- Food consumption
- Death and respawn
- Basic saving and loading
- Procedural environment generation
- Opening story/loading sequence
- Initial spawn beneath Yggdrasil
- Expanded survival gameplay
- Additional enemy encounters
- More quests and exploration content
- Full game-state persistence
- Additional game balancing and bug fixes

This is an active development project. Some features may be incomplete or behave differently from their intended final design.

---

## 16. Suggested Gameplay Demonstration

For a first-time player or project evaluator:

1. Open the game and select New Game.
2. Use WASD to move around the environment.
3. Approach a fallen branch and collect it.
4. Open the inventory with Tab.
5. Find a tree and attack it using Space.
6. Collect the dropped logs.
7. Open crafting with C.
8. Convert logs into planks.
9. Convert planks into sticks.
10. Craft a wooden sword, pickaxe, or axe using the recipes above.
11. Select equipment through the number keys.
12. Explore the map and interact with NPCs using E.
13. Complete available quests and collect rewards.
14. Monitor the health and hunger bars while surviving.

The stone mining and stone crafting features are still undergoing development and should not be relied upon for the current gameplay demonstration.

---
## 17. Credit to Owner for Main Menu & Background Musics

Main Menu: https://opengameart.org/content/rpgvillageexplorationmusic

Gameplay Background Music: https://opengameart.org/content/rpg-the-secret-within-the-woods

---

## 18. Development Repository

GitHub Repository:

https://github.com/HanMinKhantOo/The-Legend-of-Paldera

Development Branch:

https://github.com/HanMinKhantOo/The-Legend-of-Paldera/tree/Game-setup

The Legend of Paldera is an ongoing game design and development project.
