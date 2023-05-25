import logging
from typing import List, Optional, Tuple

from gym import spaces
from towerfall import Towerfall

from .actions import Actions
from .base_env import TowerfallEnv
from .objective import Objective
from .observation import Observation


class TowerfallBlankEnv(TowerfallEnv):
  '''
  A modular implementation of TowerfallEnv that can be customized with the addition of observations and an objective.
  '''
  def __init__(self,
      towerfall: Towerfall,
      observations: List[Observation],
      objective: Objective,
      actions: Optional[Actions]=None,
      record_path: Optional[str]=None,
      verbose: int = 0):
    super().__init__(towerfall, actions, record_path, verbose)
    obs_space = {}
    self.components = list(observations)
    self.components.append(objective)
    self.objective = objective
    self.objective.env = self
    for obs in self.components:
      obs.extend_obs_space(obs_space)
    self.observation_space = spaces.Dict(obs_space)
    logging.info('Action space: %s', str(self.action_space))
    logging.info('Observation space: %s', str(self.observation_space))

  def _send_reset(self):
    reset_entities = self.objective.get_reset_entities()
    self.towerfall.send_reset(reset_entities)

  def _post_reset(self) -> dict:
    obs_dict = {}
    for obs in self.components:
      obs.post_reset(self.state_scenario, self.me, self.entities, obs_dict)
    return obs_dict

  def _post_step(self) -> Tuple[object, float, bool, object]:
    obs_dict = {}
    for obs in self.components:
      obs.post_step(self.me, self.entities, self.actions_str, obs_dict)
    return obs_dict, self.objective.reward, self.objective.done, {}
