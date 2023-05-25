
A mod that enables AI to play the game [TowerFall Ascension](http://www.towerfall-game.com). Get started with code examples in python to create your AI to play with (or against).

# Installing

Only tested on Windows at the moment.

* Purchase the game [TowerFall Ascension](https://store.steampowered.com/app/251470/TowerFall_Ascension/) on steam.

* Locate `TowerFall.exe` in the game directory. To find it, go to Steam, right-click the game and navigate to `Properties > Local Files > Browse...`

* Download the mod from releases (TODO add release link), unzip all its contents in the game directory. Make sure they are in the same directory as `TowerFall.exe`.

* Make sure the game is not running, then execute `Patcher.exe`. This will overwrite `TowerFall.exe`.

* Include `--aimod` in `Properties > General > LAUNCH OPTIONS`. Whenever you want to disable the mod and play the game normally, just remove the flag.

* If you run the game now, you should see a **purple screen**. That is expected. The game is waiting for agents to connect and configure a match, which you will learn how to do in the sessions below.

## Uninstalling
To uninstall this mod, right-click on the game and go to `Properties > Local Files > Verify integrity of game files...` This will download the unmodified file from Steam.

**Note:** The mod is uninstalled whenever the game is updated. To reinstall it, delete the `Patcher` directory and re-run the patch command.

# Getting started.

Open a terminal and check that python is installed:

```shell
python --version
```

**Note:** If you are new to python, it is recommended installing it using a package manager, like [Miniconda](https://docs.conda.io/en/latest/miniconda.html) or another of your choice.

Install the required packages:

```shell
pip install -r python/requirements.txt
```

Run the 1v1 versus example:

```shell
python python/run_simple_agent_versus.py
```

You should expect an output similar to this (it will look slightly different if the game is already opened):

```shell
Starting new process from C:/Program Files (x86)/Steam/steamapps/common/TowerFall/TowerFall.exe.
Waiting for available process.
```

After a few seconds, the game window will open and the match will start. Take a moment to enjoy watching the two little archers battle each other.

**Troubleshoot:** If it fails, but the game window is open, run the command again. Sometimes the game takes longer to start, which causes a timeout.

Notice that when the match ends, you can select rematch, but all other options will lead you to a **purple screen**. That is expected, and it means the game is waiting for a new connection. For now, this mod does not support navigating through UIs of the game.

**Note:** You can still edit game options (like sound volume) by editing `tf_savedata` file manually.

A match can be programmatically started in many configurations. Give each one of the other examples a try to see a few options:
- `run_simple_agent_quest.py`
- `run_simple_agent_sandbox.py`
- `run_simple_agent_versus_2v2.py`
- `run_simple_agent_versus_human.py`

## Code example walkthrough

Now that you saw it working, lets walk through one of the examples and learn how to use the API. Inspect the contents of [python/run_simple_agent_versus.py](TODO include link).

First step is creating an instance of `Towerfall`. It is a client that talks to the game (which itself acts as a server). The key part here is the `config`. That is how you select the `mode`, the `level`, number of agents, etc.

```python
towerfall = Towerfall(
  verbose = 1,
  config = dict(
    mode='versus',
    level='3',
    fps=60,
    agentTimeout='00:00:02',
    agents=[
      dict(type='remote', archer='green-alt', team='blue'),
      dict(type='remote', archer='blue-alt', team='red')],
  )
)
```

You can tinker with this config a little. Change one of the agent types from `remote` to `human`. Or increase the `fps`, so the game runs faster (quite useful when developing). The [Config Reference](#config-reference) explains all parameters.

`Towerfall` gives you a management API. To actually play the game, it needs to create a connection for each agent using `towerfall.join()`. First, count how many connections are needed (exclude the human ones). Then, call `join` to create a connection that the agent will use:

```python
connections = []
agents = []
remote_agents = sum(1 for agent in towerfall.config['agents'] if agent['type'] != 'human')
for i in range(remote_agents):
  connections.append(towerfall.join(timeout=10, verbose=1))
  agents.append(SimpleAgent(connections[i]))
```

Now you can use the connection to read the state and send actions to the game in a loop:

```python
while True:
  # Read the state of the game then replies with an action.
  for connection, agent in zip(connections, agents):
    game_state = connection.read_json()
    agent.act(game_state)
```

**Note:** The server communicates with each connection independently, so you **have** to read the game_state from all of them (if you don't, the socket buffer can become full over time and the connection will eventually fail).

`SimpleAgent` is a minimalist implementation of an agent that shows how to read the state, make a decision, and reply with actions. The code in [python/agents/simple_agents.py](TODO include link) is extensively explained with comments, so take some time to read it thoroughly. It is a good place to start by making small improvements, like:

* Shoot in more directions.
* Dodge arrows.
* Shoot only when there is no wall in between. Check [scenario](TODO add link) message.

Once you are comfortable with the basics, read the [API](#api) section for more information.

# Config reference

```jsonc
{
  "mode": "versus", // Possible values:
      // versus
      // quest
      // sandbox
  "level": 2, // Level selection. [1-12]
  "skipWaves": 0, // Amount of waves to skip in quest mode.
  "agents": [
    {
      "type": "remote", // Possible values:
          // remote: create slots for agents to connect to.
          // human: input is taken from normal input, like keyboard and gamepads.
      "archer": "blue",  // The selected archer. Possible values:
          // green
          // blue
          // pink
          // orange
          // white
          // yellow
          // teal
          // purple
          // red
          //
          // Each one accepts an alternative version, example: blue-alt
          // Some accept <color>-secret.
      "team": "red", // The team for versus mode.
          // blue
          // neutral
          // red
    },
  ]
}
```

# API

## Messages

The agent listens to messages from the server and replies accordingly in a loop for as long as the match goes on. This is a reference to the meaning of each message and how the agent is supposed to reply to them.

### `init`

The first message sent when a match starts.

```jsonc
{
  "type": "init", // Identifies the agent.
  "index": 0, // [0-3] Use this to identify your archer in 'update'.
  "version": "0.1.1" // The version of the mod.
}
```

Reply with `result`:

```jsonc
{
  "type": "result",
  "success": true
}
```

### `scenario`

Since the scenario does not change much, it is sent only once in the beginning of a round, right after `init`.

```jsonc
{
  "type": "scenario", // Shows what the scenario grid looks like.
  "grid": [
    [0, 1, 1, 1, 0, ...],
    [0, 1, 1, 0, 1, ...],
    ...
  ], // 32 x 24 grid. 1 Means there is a block in that position blocking movement. 0 means empty. The first index of the grid corresponds to X coordinate, second index to Y coordinate.
  "cellSize": 10, // Each cell in the grid has dimensions (cellSize x cellSize). This value is unlikely to ever change, but it needs to be documented, so here it is.
}
```

Reply with `result`:

```jsonc
{
  "type": "result",
  "success": true
}
```

`update`

Every frame, an `update` is sent to indicate the current state of the game:
```jsonc
{
  "type": "update", // A normal update of the game state.
  "entities": [ // The list of entities, like players, arrows, enemies, etc.
    <Entity>,
    <Entity>,
    ...
  ]
}
```

For every `update`, reply with `actions`, containing the set of buttons to be pressed:

```jsonc
{
  "type": "actions",
  "actions": "jr" // The actions chosen by the agent. Each letter represents a pressed button. This example means that 'jump' and 'right' are pressed, and no other button is pressed. Mapping of all buttons:
      // r = right
      // l = left
      // u = up
      // d = down
      // j = jump
      // z = dash
      // s = shoot
      //
      // Changing arrows is not supported.
}
```

**Note:** The archer will not fire an arrow unless the `shoot` button is released. So in order to shoot, you have to include `'s'` in one frame and not include it in the next.
Same for dashing or jumping. If you keep `'j'` in all frames the archer will jump only once, or if you keep `'z'` in all frames, it will dash only once. They behave the same as if you were pressing the buttons in a real controller.

## Entities

Here is the reference of the entity types. Before diving on each one, know that every entity includes these properties:

### `<Entity>`
```jsonc
{
  "id": 0, // An id to keep track of an entity over many frames.
  "pos": {"x": 0.0, "y": 0.0}, // 2D vector indicating the position of its center.
  "vel": {"x": 0.0, "y": 0.0}, // 2D vector indicating its velocity.
  "size": {"x": 0.0, "y": 0.0}, // 2D vector indicating its size.
  "isEnemy": true, // True if it is an enemy, false otherwise. Enemies refer to the monsters in quest mode. Players are not marked as enemies.
  "canHurt": true, // True if players die if they touch this entity. False if it is safe to touch.
  "canBounceOn": true, // True if it can get killed by bouncing on it. False if it doesn't die that way.
  "isDead": false, // True if entity is dead, false otherwise. Dead entities are sent because they might still attack when dead.
  "facing": 1, // -1 if it is facing left, +1 if it is facing right.
}
```

There are several types of entities. The most important ones make the top of this list:

### `<Archer>`

This can be either the archer that player/agent controls, or the enemy archers that appear in quest mode.
```jsonc
{
  "type": "archer",
  ... // All properties from <Entity>
  "playerIndex": 0, // [0-3]. To identify yourself, match the value from 'init'.
  "team": "red", // Only used in team versus.
      // blue
      // neutral
      // red
  "aimDirection": {"x":1, "y":1}, // The direction of the aim as a 2D vector.
  "arrows": ["normal", "bomb", ...], // List of arrows that the archer is holding.
  "canHurt": true, // true if the archer can stomp on others.
  "dead": false, // true if dead. Useful when reviving teammates.
  "dodgeCooldown": 0, // Indicates how long until archer can dodge again. 0 if it can dodge now, more than 0 otherwise.
  "onGround": true, // true if stepping on the ground, false otherwise. Archer needs to be 'onGround':true to be able to jump.
  "onWall": false, // true if leaning on a wall, false otherwise. Archer needs to be leaning on wall to wall jump. They can also delay falling when holding the wall.
  "shield": false, // true if archer has a shield, false otherwise.
  "wings": false, // true if archer has wings, false otherwise.
  "state": "normal", // Possible archer states:
      // dodging
      // ducking
      // dying
      // frozen: Archer can't move. Usually in an intro scene, before round starts.
      // ledgeGrab
      // normal
}
```

### `<Arrow>`
```jsonc
{
  "type": "arrow",
  ... // All properties from <Entity>
  "arrowType": "normal", // Possible arrow types:
      // bolt
      // bomb
      // bramble
      // drill
      // feather
      // laser
      // normal
      // prism
      // superBomb
      // toy
      // trigger
  "state": "shooting", // Possible states
      // buried: TODO
      // drilling: Specific for drill arrows when they go are digging through walls.
      // galling: TODO
      // gravity: TODO
      // layingOnGround: It can be picked up
      // shooting: Arrow is mid air.
      // stuck: It can be picked up
  "timeLeft": 0 // Specific to bomb arrows. It is the time until they explode.
}
```

### `<Item>`
```jsonc
{
  "type": "item",
  ... // All properties from <Entity>
  "itemType": "arrowBolt", // Possible item types:
      // arrowBolt
      // arrowBomb
      // arrowBramble
      // arrowDrill
      // arrowFeather
      // arrowLaser
      // arrowNormal
      // arrowPrism
      // arrowSuperBomb
      // arrowToy
      // arrowTrigger
      // bomb
      // mirror
      // orbChaos
      // orbDark
      // orbLava
      // orbSpace
      // orbTime
      // shield
      // wings
}
```

### `<Chest>`
```jsonc
{
  "type": "chest",
  ... // All properties from <Entity>
  "state": "closed", // Possible chest states:
      // appearing
      // closed
      // opened
      // opening
  "chestType": "normal", // Possible chest types:
      // autoOpen
      // bottomless
      // large
      // normal
      // special
}
```

Now the list of less remarkable entities by alphabetic order.

### `<Birdman>`
```jsonc
{
  "type": "birdman",
  ... // All properties from <Entity>
  "state": "idle", // Possible birdman states:
      // idle: Flying around
      // attack: Performing an attack
      // resting: Can't attack
}
```

### `<CrackedPlatform>`
```jsonc
{
  "type": "crackedPlatform",
  ... // All properties from <Entity>
  "state": "closed", // Possible chest states:
      // dead
      // respawning
      // shaking
      // shifting
      // waiting
}
```

### `<CrackedWall>`
```jsonc
{
  "type": "crackedPlatform",
  ... // All properties from <Entity>
  "count": 5.0, // Count until it breaks. Breaks at 0.
}
```

### `<EvilCrystal>`
```jsonc
{
  "type": "evilCrystal",
  ... // All properties from <Entity>
  "subType": "blue", // Possible subTypes:
      // blue
      // green
      // pink
      // red
}
```

### `<Hat>`
```jsonc
{
  "type": "hat",
  ... // All properties from <Entity>
  "playerIndex": 0, // The index of the owner of the hat
}
```

### `<FloorMiasma>`
```jsonc
// That little fire that appears from the floor.
{
  "type": "floorMiasma",
  ... // All properties from <Entity>
  "bottom": 0.0, // The height of the floor miasma.
}
```

### `<Ghost>`
```jsonc
{
  "type": "ghost",
  ... // All properties from <Entity>
  "subType": "blue", // Possible subTypes:
      // blue
      // green
      // fire
      // greenFire
}
```

### `<Icicle>`
```jsonc
{
  "type": "icicle",
  ... // All properties from <Entity>
  "falling": true, // True if the icicle is falling, false otherwise.
}
```

### `<KingReaper>`
```jsonc
{
  "type": "kingReaper",
  ... // All properties from <Entity>
  "hasShield": true, // True if shield is activated, false otherwise.
}
```

### `<KingReaperBeam>`
```jsonc
{
  "type": "kingReaper",
  ... // All properties from <Entity>
  "dir": {"x": 0.0, "y": 1.0}, // Direction of the beam. There are actually two beams starting from 'pos' and shooting to 'dir' and -'dir'.
  "width": 8.0, // Width of the beam.
}
```

### `<Lantern>`
```jsonc
{
  "type": "lantern",
  ... // All properties from <Entity>
  "falling": true, // True if the lantern is falling, false otherwise.
}
```

### `<Lava>`
```jsonc
{
  "type": "lava",
  ... // All properties from <Entity>
  "bottom": 0.0, // The height of the lava.
}
```

### `<Miasma>`
```jsonc
// That purple fog that comes fromm the sides.
{
  "type": "miasma",
  ... // All properties from <Entity>
  "left": 0.0, // The left most safe area.
  "right": 0.0, // The right most safe area.
}
```

### `<Orb>`
```jsonc
{
  "type": "orb",
  ... // All properties from <Entity>
  "falling": true, // True if the orb is falling, false otherwise.
}
```

### `<ProximityBlock>`
```jsonc
// Block that let things pass when a player is close.
{
  "type": "proximityBlock",
  ... // All properties from <Entity>
  "collidable": true, // True if things collide with it, false otherwise.
}
```

### `<ShiftBlock>`
```jsonc
// Block that moves to crush the player.
{
  "type": "shiftBlock",
  ... // All properties from <Entity>
  "startPosition": {"x": 0.0, "y": 0.0},
  "endPosition": {"x": 0.0, "y": 0.0},
  "state": "waiting" // possible states:
      // moving
      // shaking
      // waiting
}
```

### `<Slime>`
```jsonc
{
  "type": "slime",
  ... // All properties from <Entity>
  "subType": "green", // Possible subTypes:
      // blue
      // green
      // red
}
```

### `<SpikeBall>`
```jsonc
{
  "type": "spikeBall",
  ... // All properties from <Entity>
  "center": {"x": 0.0, "y": 0.0},
  "radius": 1.0,
}
```

### `<SwitchBlock>`
```jsonc
// Those red and blue blocks
{
  "type": "switchBlock",
  ... // All properties from <Entity>
  "collidable": true, // True if things collide with it, false otherwise.
  "warning": true, // True if it is about to switch.
}
```

### All entity types

The complete list of entity types. A lot of them only has the regular `<Entity>` schema and is not documented above.

```
amaranthBoss
amaranthShot
arrow
bat
batBomb
batSuperBomb
bird
birdman
brambles
cataclysmBlade
cataclysmBlock
cataclysmBullet
cataclysmEye
cataclysmMissile
cataclysmShieldOrb
chain
chest
crackedPlatform
crackedWall
crown
crumbleBlock
crumbleWall
cultist
cyclopsEye
cyclopsFist
cyclopsPlatform
cyclopsShot
dreadEye
dreadFlower
dreadTentacle
dummy
enemyAttack
evilCrystal
exploder
explosion
fakeWall
flamingSkull
floorMiasma
ghost
ghostPlatform
graniteBlock
hat
hotCoals
ice
icicle
jumpPad
kingReaper
kingReaperBeam
kingReaperBomb
kingReaperCrystal
lantern
laserArrow
lava
loopPlatform
miasma
mirrorPickup
mole
moonGlassBlock
movingPlatform
mud
orb
player
playerCorpse
portal
prism
prismArrow
proximityBlock
purpleArcherPortal
sensorBlock
shiftBlock
shockCircle
slime
spikeball
switchBlock
technoMage
technoMissile
tornado
worm
```

# Communication Protocol

The game (server) and the agent (client) exchange messages over socket in the following protocol:
* `2 bytes` for the size `N` of the body (unsigned, big-endian).
* `N bytes` for the body, an `ascii` encoded JSON object. It contains the property `type`, which defines the set of other properties in the message.

See [Connection](TODO add link) class for implementation details in python.

## Management API

Exchange starts with the client opening a new socket and sending a `config` message:

```jsonc
{
  "type": "config",
  "config": {
    "mode": "quest",
    "level": "2",
    "fps": 60,
    "agents": [
      {
        "type": "remote"
      }
    ]
  }
}
```

Server replies with a `result`, containing either success...

```jsonc
{
  "type": "result",
  "success": true
}
```

...or maybe an error, if something goes wrong:

```jsonc
{
  "type": "result",
  "success": false,
  "message": "Some reason why it failed."
}
```

**Note:** This socket can be either closed, or kept opened for further management API calls.

On success, one slot will be opened for every `remote` agent defined in config. Each of them opens a new socket and sends a `join` message:

```jsonc
{
  "type": "join"
}
```

Server again replies with a `result`, containing either success...

```jsonc
{
  "type": "result",
  "success": true,
  "message": "Game will start once all agents join."
}
```

...or maybe an error:

```jsonc
{
  "type": "result",
  "success": false,
  "message": "No open slot to join."
}
```

After successful `join`, this connection needs to be kept open to do all the communication for the entire match. The client waits for the game messages and reply accordingly, as explained in the [API](#api)

Management API also supports a `reset` message that can be used to choose the initial configuration of the entities in `sandbox` mode (trying to set the entities in any other mode doesn't take any effect).

```jsonc
{
  "type": "reset",
  "entities": [
    {
      "type": "bat",
      "facing": -1,
      "pos": { "x": 220, "y": 160 }
    },
  ]
}
```

See [reset_test.py](TODO add link) for a full list of entities supported in reset.

# Limitations

There are a few improvements that are tricky, but in theory possible. They might be implemented in the future:

* Seamless integration with the game: no purple screens, enable AI in Archer selection screen.
* Vectorized environments. Steam does not let multiple instances of a game run at the same time. But it would be possible to have one instance run multiple matches. This would be very useful for Reinforcement Learning approaches.

Some other things cannot be implemented:
* Selecting Dark World DLC in config. Since it is sold separately, this project can't offer support for that.

# Acknowledgements

[Extremely OK Games](https://exok.com), for making this awesome, lovely game.

The developers of [Bartizan](https://github.com/Kha/Bartizan#installation), from where I borrowed the basic code for patching.
