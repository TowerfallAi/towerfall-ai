from typing import Any, Mapping

from common.logging_options import default_logging
from towerfall import Towerfall

from agents import SimpleAgent

default_logging()

'''
One agent playing a sandbox game. This mode allows resetting the
entities and the scenario in an initial state for quick experimentation.
'''

def main():
  # Creates or reuse a Towerfall game.
  towerfall = Towerfall(
    verbose = 1,
    config = dict(
      mode='sandbox',
      level='2',
      fps=60,
      agentTimeout='00:00:02',
      solids=[[0] * 32]*14 + [[1]*32] + [[0] * 32]*9,
      agents=[
        dict(type='remote', archer='white-alt')]
    )
  )

  connection = towerfall.join(timeout=10, verbose=1)

  send_reset(towerfall)
  agent = SimpleAgent(connection)
  while True:
    # Read the state of the game then replies with an action.
    game_state = connection.read_json()
    check_for_end_condition(game_state, towerfall)
    agent.act(game_state)


def send_reset(towerfall: Towerfall):
  towerfall.send_reset([
      dict(type='archer', pos=dict(x=160, y=110)),
      dict(type='slime', facing=-1, pos=dict(x=220, y=110))
    ]
  )


def check_for_end_condition(game_state: Mapping[str, Any], towerfall: Towerfall):
  # The sandbox mode will not end the game automatically. We need to explicitly request a reset.
  if game_state['type'] != 'update':
    return

  is_player_alive = False
  is_enemy_alive = False
  for state in game_state['entities']:
    if state['type'] == 'archer':
      is_player_alive = True
    if state['isEnemy']:
      is_enemy_alive = True
  if not is_player_alive or not is_enemy_alive:
    send_reset(towerfall)


if __name__ == '__main__':
  main()