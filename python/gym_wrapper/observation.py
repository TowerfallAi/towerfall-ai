from abc import ABC, abstractmethod
from typing import Any, Dict, List, Mapping, Optional

from common.entity import Entity
from gym import Space


class Observation(ABC):
  '''
  Base class for observations.
  '''
  @abstractmethod
  def extend_obs_space(self, obs_space_dict: Mapping[str, Space]):
    '''Adds the new definitions to observations to obs_space.'''
    raise NotImplementedError()

  @abstractmethod
  def post_reset(self, state_scenario: Mapping[str, Any], player: Optional[Entity], entities: List[Entity], obs_dict: Dict[str, Any]):
    '''Hook for a gym reset call. Adds observations to obs_dict.'''
    raise NotImplementedError

  @abstractmethod
  def post_step(self, player: Optional[Entity], entities: List[Entity], actions: str, obs_dict: Mapping[str, Any]):
    '''Hook for a gym step call. Adds observations to obs_dict.'''
    raise NotImplementedError
