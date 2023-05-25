from typing import Any, Dict, List, Mapping

from common.logging_options import default_logging
from common.test import assert_initial_state
from towerfall import Towerfall

from agents import SimpleAgent

default_logging()

'''
Test reset for all supported entities.
'''

_TEST_ENTITIES = [
  dict(type='bat', facing=-1, pos=dict(x=220, y=160)),
  dict(type='batBomb', facing=-1, pos=dict(x=220, y=160)),
  dict(type='batSuperBomb', facing=-1, pos=dict(x=220, y=160)),
  dict(type='bird', facing=-1, pos=dict(x=220, y=160)),
  dict(type='slime', subType="blue", facing=-1, pos=dict(x=220, y=110)),
  dict(type='slime', subType="green", facing=-1, pos=dict(x=220, y=110)),
  dict(type='slime', subType="red", facing=-1, pos=dict(x=220, y=110)),
  dict(type='birdman', facing=-1, pos=dict(x=220, y=160)),
  dict(type='cultist', subtype='boss', facing=-1, pos=dict(x=220, y=120)),
  dict(type='cultist', subtype='normal', facing=-1, pos=dict(x=220, y=120)),
  dict(type='cultist', subtype='scythe', facing=-1, pos=dict(x=220, y=120)),
  dict(type='evilCrystal', subtype='blue', facing=-1, pos=dict(x=220, y=180)),
  dict(type='evilCrystal', subtype='green', facing=-1, pos=dict(x=220, y=180)),
  dict(type='evilCrystal', subtype='pink', facing=-1, pos=dict(x=220, y=180)),
  dict(type='evilCrystal', subtype='red', facing=-1, pos=dict(x=220, y=180)),
  dict(type='exploder', facing=-1, pos=dict(x=220, y=180)),
  dict(type='flamingSkull', facing=-1, pos=dict(x=220, y=180)),
  dict(type='ghost', subType='blue', facing=-1, pos=dict(x=220, y=180)),
  dict(type='ghost', subType='fire', facing=-1, pos=dict(x=220, y=180)),
  dict(type='ghost', subType='green', facing=-1, pos=dict(x=220, y=180)),
  dict(type='ghost', subType='greenFire', facing=-1, pos=dict(x=220, y=180)),
  dict(type='mole', facing=-1, pos=dict(x=220, y=120)),
  dict(type='worm', pos=dict(x=220, y=120)),
]


def main():
  # Creates or reuse a Towerfall game.
  towerfall = Towerfall(
    verbose = 1,
    config = dict(
      mode='sandbox',
      level='2',
      fps=999,
      agentTimeout='00:00:02',
      solids=[[0] * 32]*14 + [[1]*32] + [[0] * 32]*9,
      agents=[dict(type='remote')]
    )
  )

  connection = towerfall.join(timeout=10, verbose=1)
  agent = SimpleAgent(connection)

  n_frames = 180
  for i in range(0, len(_TEST_ENTITIES)):
    reset_entities = send_reset(towerfall, i)
    if i > 0:
      # We still need to ack the last frame, or the agent will get disconnected.
      game_state = connection.read_json()
      connection.send_json(dict(type='actions', actions='', id=game_state['id']))
    asserted = False
    for _ in range(n_frames):
      # Read the state of the game then replies with an action.
      game_state = connection.read_json()
      if not asserted and game_state['type'] == 'update':
        assert_initial_state(reset_entities, game_state['entities'])
        asserted = True
      agent.act(game_state)


def send_reset(towerfall: Towerfall, i: int) -> List[Dict[str, Any]]:
  reset_entities = get_reset_entities(i)
  towerfall.send_reset(reset_entities)
  return reset_entities


def get_reset_entities(i: int) -> List[Dict[str, Any]]:
  return [dict(type='archer', pos=dict(x=120, y=110)), _TEST_ENTITIES[i]]


if __name__ == '__main__':
  main()