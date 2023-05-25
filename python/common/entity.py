from __future__ import annotations

import sys
from math import sqrt
from typing import Any, Dict, List, Tuple

import numpy as np
from numpy.typing import NDArray


class Entity:
  def __init__(self, e: Dict[str, Any]):
    self.p: Vec2 = vec2_from_dict(e['pos'])
    self.v: Vec2 = vec2_from_dict(e['vel'])
    self.s: Vec2 = vec2_from_dict(e['size'])
    self.isEnemy: bool = e['isEnemy']
    self.type: str = e['type']
    self.e: Any = e

  def __getitem__(self, key):
    return self.e[key]

  def bot_left(self) -> Vec2:
    return Vec2(self.p.x - self.s.x / 2, self.p.y - self.s.y / 2)

  def top_right(self) -> Vec2:
    return Vec2(self.p.x + self.s.x / 2, self.p.y + self.s.y / 2)


class Vec2:
  def __init__(self, x: float, y: float):
    self.x: float = x
    self.y: float = y

  def __str__(self):
    return 'Vec2({}, {})'.format(self.x, self.y)

  def __hash__(self):
    return hash((self.x, self.y))

  def __add__(self, o):
    return Vec2(self.x + o.x, self.y + o.y)

  def __sub__(self, o):
    return Vec2(self.x - o.x, self.y - o.y)

  def __neg__(self):
    return Vec2(-self.x, -self.y)

  def __mul__(self, o):
    if isinstance(o, float) or isinstance(o, int):
      return Vec2(self.x * o, self.y * o)
    raise NotImplementedError()

  def __truediv__(self, o):
    if isinstance(o, float) or isinstance(o, int):
      return Vec2(self.x / o, self.y / o)
    raise NotImplementedError()

  def __eq__(self, other):
    if isinstance(other, Vec2):
      if self.x != other.x:
        return False
      if self.y != other.y:
        return False
      return True
    return NotImplemented

  def tupleint(self) -> Tuple[int, int]:
    return int(self.x), int(self.y)

  def numpy(self) -> NDArray[np.float32]:
    return np.array([self.x, self.y], dtype=np.float32)

  def dict(self) -> dict[str, float]:
    return dict(x=self.x, y=self.y)

  def set_length(self, l: float):
    d = self.length()
    self.x *= l/d
    self.y *= l/d

  def length(self):
    return sqrt(self.x**2 + self.y**2)

  def copy(self):
    return Vec2(self.x, self.y)

  def add(self, v: Vec2):
    self.x += v.x
    self.y += v.y

  def sub(self, v: Vec2):
    self.x -= v.x
    self.y -= v.y

  def mul(self, f: float):
    self.x *= f
    self.y *= f

  def div(self, f: float):
    self.x /= f
    self.y /= f


def vec2_from_dict(p: Dict[str, Any]) -> Vec2:
  try:
    return Vec2(p['x'], p['y'])
  except KeyError:
    sys.stderr.write(str(p))
    raise


def to_entities(entities: List[Dict[str, Any]]) -> List[Entity]:
  result = []
  for e in entities:
    result.append(Entity(e))

  return result
