from typing import Any, Dict

from common.constants import DASH, DOWN, JUMP, LEFT, RIGHT, SHOOT, UP
from gym import spaces
from numpy.typing import NDArray


class Actions:
  '''
  Does conversion between gym actions and Towerfall API actions.
  Allows enabling or disabling certain actions.
  '''
  def __init__(self,
      can_jump=True,
      can_dash=True,
      can_shoot=True):

    self.can_jump = can_jump
    self.can_dash = can_dash
    self.can_shoot = can_shoot

    actions = [3,3]
    self.action_map: Dict[str, Any] = {} # maps key to index
    if can_jump:
      self.action_map[JUMP] = len(actions)
      actions.append(2)
    if can_dash:
      self.action_map[DASH] = len(actions)
      actions.append(2)
    if can_shoot:
      self.action_map[SHOOT] = len(actions)
      actions.append(2)
    self.action_space = spaces.MultiDiscrete(actions)

  def to_serialized_actions(self, actions: NDArray) -> str:
    '''
    Converts a list of actions to a the serialized string of pressed buttons that the game expects.
    '''
    actions_str = ''
    if actions[0] == 0:
      actions_str += LEFT
    elif actions[0] == 2:
      actions_str += RIGHT
    if actions[1] == 0:
      actions_str += DOWN
    elif actions[1] == 2:
      actions_str += UP

    if self.can_jump and actions[self.action_map[JUMP]]:
      actions_str += JUMP
    if self.can_dash and actions[self.action_map[DASH]]:
      actions_str += DASH
    if self.can_shoot and actions[self.action_map[SHOOT]]:
      actions_str += SHOOT
    return actions_str