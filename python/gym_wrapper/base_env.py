from abc import ABC, abstractmethod
from typing import Any, Dict, List, Optional, Tuple

from common.entity import Entity, to_entities
from gym import Env
from numpy.typing import NDArray
from towerfall import Towerfall

from .actions import Actions


class TowerfallEnv(Env, ABC):
  '''
  Interacts with the Towerfall.exe process to create an interface with the agent that follows the gym API.
  Inherit from this class to choose the appropriate observations and reward functions.

  params towerfall: The Towerfall instance to connect to.
  params actions: The actions that the agent can take. If None, the default actions are used.
  params record_path: The path to record the game to. If None, no recording is done.
  params verbose: The verbosity level. 0: no logging, 1: much logging.
  '''
  def __init__(self,
      towerfall: Towerfall,
      actions: Optional[Actions] = None,
      record_path: Optional[str] = None,
      verbose: int = 0):
    self.towerfall = towerfall
    self.verbose = verbose
    self.connection = self.towerfall.join(timeout=5)
    self.connection.record_path = record_path
    if actions:
      self.actions = actions
    else:
      self.actions = Actions()
    self.action_space = self.actions.action_space
    self._draw_elems = []
    self.is_init_sent = False

  def _send_reset(self):
    '''
    Sends the reset instruction to the game. Overwrite this to change the starting conditions.
    '''
    self.towerfall.send_reset()

  @abstractmethod
  def _post_reset(self) -> Tuple[NDArray, dict]:
    '''
    Hook for a gym reset call. Subclass should populate and return the same as a reset in gym API.

    Returns:
      A tuple of (observation, info)
    '''
    raise NotImplementedError

  @abstractmethod
  def _post_step(self) -> Tuple[NDArray, float, bool, dict]:
    '''
    Hook for a gym step call. Subclass should populate this to return the same as a step in gym API.

    Returns:
      A tuple of (observation, reward, done, info)
    '''
    raise NotImplementedError

  def draws(self, draw_elem):
    '''
    Draws an element on the screen. This is useful for debugging.
    '''
    self._draw_elems.append(draw_elem)

  def reset(self) -> Tuple[NDArray, dict]:
    '''
    Gym reset. This is called by the agent to reset the environment.
    '''

    self._send_reset()
    if not self.is_init_sent:
      state_init = self.connection.read_json()
      assert state_init['type'] == 'init', state_init['type']
      self.index = state_init['index']
      self.connection.send_json(dict(type='result', success=True))

      self.state_scenario = self.connection.read_json()
      assert self.state_scenario['type'] == 'scenario', self.state_scenario['type']
      self.connection.send_json(dict(type='result', success=True))
      self.is_init_sent = True
    else:
      self.connection.send_json(dict(type='actions', actions="", id=self.state_update['id']))

    self.frame = 0
    self.state_update = self.connection.read_json()
    assert self.state_update['type'] == 'update', self.state_update['type']
    self.entities = to_entities(self.state_update['entities'])
    self.me = self._get_own_archer(self.entities)

    return self._post_reset()

  def step(self, actions: NDArray) -> Tuple[NDArray, float, bool, object]:
    '''
    Gym step. This is called by the agent to take an action in the environment.
    '''
    actions_str = self.actions.to_serialized_actions(actions)

    resp: Dict[str, Any] = dict(
      type='actions',
      actions=actions_str,
      id=self.state_update['id']
    )
    if self._draw_elems:
      resp['draws'] = self._draw_elems
    self.connection.send_json(resp)
    self._draw_elems.clear()
    self.state_update = self.connection.read_json()
    self.actions_str = actions_str
    assert self.state_update['type'] == 'update'
    self.entities = to_entities(self.state_update['entities'])
    self.me = self._get_own_archer(self.entities)
    return self._post_step()

  def _get_own_archer(self, entities: List[Entity]) -> Optional[Entity]:
    '''
    Iterates over all entities to find the archer that matches the index specified in init.
    '''
    for e in entities:
      if e.type == 'archer':
        if e['playerIndex'] == self.index:
          return e
    return None

  def render(self, mode='human'):
    '''
    This is a no-op since the game is rendered independenly by MonoGame/XNA.
    '''
    pass