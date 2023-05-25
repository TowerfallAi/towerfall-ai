from typing import Any, Dict, List, Mapping, Optional, Sequence

import numpy as np
from common.constants import DASH, JUMP, SHOOT
from common.entity import Entity
from gym import Space, spaces

from .observation import Observation


class PlayerObservation(Observation):
  '''
  Observation for the player state.
  '''
  def __init__(self, exclude: Optional[Sequence[str]] = None):
    self.exclude = exclude

  def extend_obs_space(self, obs_space_dict: Dict[str, Space]):
    def try_add_obs(key, value):
      if self.exclude and key in self.exclude:
        return
      if key in obs_space_dict.keys():
        raise Exception(f'Observation space already has {key}')
      obs_space_dict[key] = value

    try_add_obs('prev_jump', spaces.Discrete(2))
    try_add_obs('prev_dash', spaces.Discrete(2))
    try_add_obs('prev_shoot', spaces.Discrete(2))

    try_add_obs('dodgeCooldown', spaces.Discrete(2))
    try_add_obs('dodging', spaces.Discrete(2))
    try_add_obs('facing', spaces.Discrete(2))
    try_add_obs('onGround', spaces.Discrete(2))
    try_add_obs('onWall', spaces.Discrete(2))
    try_add_obs('vel', spaces.Box(low=-2, high=2, shape=(2,), dtype=np.float32))

  def post_reset(self, state_scenario: Mapping[str, Any], player: Optional[Entity], entities: List[Entity], obs_dict: Dict[str, Any]):
    self._extend_obs(player, '', obs_dict)

  def post_step(self, player: Optional[Entity], entities: List[Entity], actions: str, obs_dict: Dict[str, Any]):
    self._extend_obs(player, actions, obs_dict)

  def _extend_obs(self, player: Optional[Entity], actions: str, obs_dict: Dict[str, Any]):
    def try_add_obs(key, value):
      if self.exclude and key in self.exclude:
        return
      obs_dict[key] = value

    try_add_obs('prev_jump', int(JUMP in actions))
    try_add_obs('prev_dash', int(DASH in actions))
    try_add_obs('prev_shoot', int(SHOOT in actions))
    if not player:
      try_add_obs('dodgeCooldown', 0)
      try_add_obs('dodging', 0)
      try_add_obs('facing', 0)
      try_add_obs('onGround', 0)
      try_add_obs('onWall', 0)
      try_add_obs('vel', np.zeros(2))
      return

    try_add_obs('dodgeCooldown', int(player['dodgeCooldown']))
    try_add_obs('dodging', int(player['state']=='dodging'))
    try_add_obs('facing', (player['facing'] + 1) // 2) # -1,1 -> 0,1
    try_add_obs('onGround', int(player['onGround']))
    try_add_obs('onWall', int(player['onWall']))
    try_add_obs('vel', np.clip(player.v.numpy() / 5, -2, 2))
