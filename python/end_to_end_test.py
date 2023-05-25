import logging
import random
import time
from typing import Any, Dict, List

from common.logging_options import default_logging
from common.test import assert_initial_state
from towerfall import Connection, Towerfall

default_logging()

_VERBOSE = 1
_TIMEOUT = 10


'''
Runs the game with different reset configurations.
'''

def main():
  Towerfall.close_all()
  try:
    agent_count = 2
    towerfall = get_process(agent_count)
    run_test(towerfall, agent_count)

    agent_count = 1
    towerfall.send_config(get_config(agent_count))
    run_test(towerfall, agent_count)

    agent_count = 3
    towerfall.send_config(get_config(agent_count))
    run_test(towerfall, agent_count)

    logging.info('Test closing towerfall ...')
    towerfall.close()
    logging.info('Test closing towerfall successful.')

    agent_count = 4
    towerfall = get_process(agent_count)
    run_test(towerfall, agent_count)
  finally:
    Towerfall.close_all()


def get_random_actions() -> str:
  s = ''
  p = 0.1
  keys = ['u', 'd', 'l', 'r', 'j', 'z', 's']
  for key in keys:
    if random.random() < p:
      s += key
  return s


def get_config(agent_count: int) -> dict[str, Any]:
  return dict(
    mode='sandbox',
    level='2',
    fps=999,
    solids=[[0] * 32]*14 + [[1]*32] + [[0] * 32]*9,
    agents=[dict(type='remote', team='blue')]*agent_count)


def get_process(agent_count: int) -> Towerfall:
  return Towerfall(
    verbose=_VERBOSE,
    config=get_config(agent_count))


def join(towerfall: Towerfall, agent_count: int) -> list[Connection]:
  connections = []
  for _ in range(agent_count):
    conn = towerfall.join(timeout=_TIMEOUT)
    conn.log_cap = 100
    connections.append(conn)
  return connections


def reset(towerfall: Towerfall, agent_count: int) -> list[dict]:
  y = 120
  entities: List[Dict[str, Any]] = [dict(type='archer', pos=dict(x=0, y=y)) for i in range(agent_count)]
  entities.append(dict(type='slime', facing=-1, pos=dict(x=0, y=y)))
  for i, entity in enumerate(entities):
    entity['pos']['x'] = 10 + i * (300 / len(entities))
  towerfall.send_reset(entities)
  return entities


def receive_init(connections: List[Connection]):
  if _VERBOSE >= 1:
    logging.info('receive_init')
  for connection in connections:
    # init
    state_init = connection.read_json()
    assert state_init['type'] == 'init', state_init['type']
    connection.send_json(dict(type='result', success=True))

  for connection in connections:
    # scenario
    state_scenario = connection.read_json()
    assert state_scenario['type'] == 'scenario', state_scenario['type']
    connection.send_json(dict(type='result', success=True))


def receive_update(connections: List[Connection], expected_entities: List[dict], n_frames: int):
  now = time.time()
  for _ in range(n_frames):
    for i_con, connection in enumerate(connections):
      # update
      state_update = connection.read_json()
      assert state_update['type'] == 'update', state_update['type']
      if state_update['id'] == 0 and i_con == 0:
        assert_initial_state(expected_entities, state_update['entities'])
      connection.send_json(dict(type='actions', actions=get_random_actions(), id=state_update['id']))
  dt = time.time() - now
  logging.info(f'fps: {n_frames/dt:.2f}')


def run_many_resets(towerfall: Towerfall, agent_count: int, reset_count: int):
  connections = join(towerfall, agent_count)
  entities = reset(towerfall, agent_count)

  receive_init(connections)
  receive_update(connections, entities, n_frames=200)

  for _ in range(reset_count):
    entities = reset(towerfall, agent_count)
    receive_update(connections, entities, n_frames=20)


def run_test(towerfall, agent_count):
  reset_count = 5
  logging.info(f'Test with {agent_count} agents ...')
  run_many_resets(towerfall, agent_count, reset_count)
  logging.info(f'Test with {agent_count} agents successful.')


if __name__ == '__main__':
  main()
