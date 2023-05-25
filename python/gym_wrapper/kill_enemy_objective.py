import random
from typing import Any, Dict, List, Optional

import numpy as np
from common.constants import HH, HW
from common.entity import Entity, Vec2
from gym import Space, spaces

from .objective import Objective


class KillEnemyObjective(Objective):
  '''
  Specifies observation and rewards associated with killing slimes.
  '''
  def __init__(self,
      enemy_type:str = 'slime',
      enemy_count: int = 1,
      min_distance: float = 50,
      max_distance: float = 100,
      bounty = 5,
      episode_max_len: int=60*2):
    super().__init__()
    self.enemy_type = enemy_type
    self.enemy_count = enemy_count
    self.target_ids = []
    self.min_distance = min_distance
    self.max_distance = max_distance
    self.bounty = bounty
    self.episode_max_len = episode_max_len
    self.episode_len = 0
    self.obs_space = spaces.Box(low=-1, high = 1, shape=(3*self.enemy_count,), dtype=np.float32)

  def extend_obs_space(self, obs_space_dict: Dict[str, Space]):
    if 'targets' in obs_space_dict:
      raise Exception('Observation space already has \'target\'')
    obs_space_dict['targets'] = self.obs_space

  def get_reset_entities(self) -> Optional[List[Dict[str, Any]]]:
    p = Vec2(160, 110)
    entities: List[Dict[str, Any]] = [dict(type='archer', pos=p.dict())]
    for i in range(self.enemy_count):
      sign = random.randint(0, 1)*2 - 1
      d = random.uniform(self.min_distance, self.max_distance) * sign
      enemy = dict(
        type=self.enemy_type,
        pos=(p + Vec2(d, -5)).dict(),
        facing=-sign)
      entities.append(enemy)
    return entities

  def post_reset(self, state_scenario: Dict[str, Any], player: Optional[Entity], entities: List[Entity], obs_dict: Dict[str, Any]):
    assert player
    targets = list(e for e in entities if e['type'] == self.enemy_type)
    assert len(targets) > 0, 'No targets found'
    self.target_ids = [t['id'] for t in targets]
    self.n_targets_prev = len(self.target_ids)

    self.done = False
    self.episode_len = 0
    self._update_obs(player, targets, obs_dict)


  def post_step(self, player: Optional[Entity], entities: List[Entity], actions: str, obs_dict: Dict[str, Any]):
    targets = list(e for e in entities if e['id'] in self.target_ids)
    self._update_reward(player, targets)
    self.episode_len += 1
    self._update_obs(player, targets, obs_dict)

  def _update_reward(self, player: Optional[Entity], targets: List[Entity]):
    '''
    Updates the reward and checks if the episode is done.
    '''
    self.reward = 0
    if not player or self.episode_len >= self.episode_max_len:
      self.done = True
      self.reward -= self.bounty / 5

    if len(targets) < self.n_targets_prev:
      self.reward = self.bounty * (self.n_targets_prev - len(targets))

    self.n_targets_prev = len(targets)
    if self.n_targets_prev == 0:
      self.done = True

  def limit(self, x: float, a: float, b: float) -> float:
    return x+b-a if x < a else x-b+a if x > b else x

  def _update_obs(self, player: Optional[Entity], targets: List[Entity], obs_dict: Dict[str, Any]):
    obs_target = np.zeros((3*self.enemy_count,), dtype=np.float32)
    if not player:
      obs_dict['targets'] = obs_target
      return

    target_by_dist = []
    for i, target in enumerate(targets):
      target_by_dist.append(((target.p - player.p).length(), target))

    target_by_dist.sort(key=lambda x: x[0])
    for i, (_, target) in enumerate(target_by_dist):
      obs_target[i*3] = 1
      obs_target[i*3 + 1] = self.limit(target.p.x - player.p.x / HW, -1, 1)
      obs_target[i*3 + 2] = self.limit(target.p.y - player.p.y / HH, -1, 1)
    obs_dict['targets'] = obs_target
