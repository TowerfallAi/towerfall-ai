import logging
from typing import List


def assert_eq(expected, actual):
  assert expected == actual, f'{expected} != {actual}'


def assert_close(expected, actual, eps):
  assert abs(expected - actual) < eps, f'{expected} and {actual} not within {eps}'


def assert_initial_state(expected_entities: List[dict], actual_entities: List[dict]):
  actual_entities.sort(key=lambda e: e['pos']['x'])
  for i, (expected, actual) in enumerate(zip(expected_entities, actual_entities)):
    try:
      assert_eq(expected['type'], actual['type'])
      if 'subType' in expected:
        assert_eq(expected['subType'], actual['subType'])
      assert_close(expected['pos']['x'], actual['pos']['x'], 2)
      assert_close(expected['pos']['y'], actual['pos']['y'], 2)
    except Exception as ex:
      logging.error(f'Error in entity {expected["type"]}')
      raise ex