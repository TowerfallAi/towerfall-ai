from typing import Any, Dict

from .base_env import TowerfallEnv
from .observation import Observation


class Objective(Observation):
  '''
  Overite this to define an objective by calculating reward and done.
  '''
  def __init__(self):
    self.done: bool
    self.reward: float
    self.env: TowerfallEnv

  def get_reset_entities(self) -> list[Dict[str, Any]]:
    '''Specifies how the environment needs to be reset.'''
    return []
