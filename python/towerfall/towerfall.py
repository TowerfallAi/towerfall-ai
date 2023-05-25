import json
import logging
import os
import random
import signal
import time
from io import TextIOWrapper
from typing import Any, Callable, Dict, List, Mapping, Optional

import psutil
from psutil import Popen

from .connection import Connection

_DEFAULT_STEAM_PATH_WINDOWS = 'C:/Program Files (x86)/Steam/steamapps/common/TowerFall'

class TowerfallError(Exception):
  pass

class Towerfall:
  '''
  Creates or reuses a Towerfall game process.

  params towerfall_path: The parent path where Towerfall.exe is located.
  params timeout: The timeout for the management API (Config, Reset).
  params verbose: The verbosity level. 0: no logging, 1: much logging.
  '''
  def __init__(self,
      config: Mapping[str, Any] = {},
      towerfall_path: str = _DEFAULT_STEAM_PATH_WINDOWS,
      timeout: float = 2,
      verbose: int = 0):
    self.config: Mapping[str, Any] = config
    self.towerfall_path = towerfall_path
    self.towerfall_path_exe = os.path.join(self.towerfall_path, 'TowerFall.exe')
    self.pool_name = 'default'
    self.pool_path = os.path.join(self.towerfall_path, 'aimod', 'pools', self.pool_name)
    self.timeout = timeout
    self.verbose = verbose
    tries = 0
    while True:
      self.port = self._attain_game_port()

      try:
        self.open_connection = Connection(self.port, timeout=timeout, verbose=verbose)
        self.send_config(config)
        break
      except TowerfallError:
        if tries > 3:
          raise TowerfallError('Could not config a Towerfall process.')
        tries += 1

  def join(self, timeout: float = 2, verbose: int = 0) -> Connection:
    '''
    Joins a towerfall game.

    params timeout: Timeout in seconds to wait for a response. The same timeout will be used on calls to get the observations.

    returns: A connection to a Towerfall game. This should be used by the agent to interact with the game.
    '''
    connection = Connection(self.port, timeout=timeout, verbose=verbose)
    connection.send_json(dict(type='join'))
    response = connection.read_json()
    if response['type'] != 'result':
      raise TowerfallError(f'Unexpected response type: {response["type"]}')
    if not response['success']:
      raise TowerfallError(f'Failed to join the game. Port: {self.port}, Response: {response["message"]}')
    self._try_log(logging.info, f'Successfully joined the game. Port: {self.port}')
    return connection

  def send_reset(self, entities: Optional[List[Dict[str, Any]]] = None):
    '''
    Sends a game reset. This will recreate the entities in the game in the same scenario. To change the scenario, use send_config.

    params entities: The entities to reset. If None, the entities specified in the last reset will be used.
    '''

    response = self.send_request_json(dict(type='reset', entities=entities))
    if response['type'] != 'result':
      raise TowerfallError(f'Unexpected response type: {response["type"]}')
    if not response['success']:
      raise TowerfallError(f'Failed to reset the game. Port: {self.port}, Response: {response["message"]}')
    self._try_log(logging.info, f'Successfully reset the game. Port: {self.port}')

  def send_config(self, config = None):
    '''
    Sends a game configuration. This will restart the session of the game in the specified scenario and specified number of agents.

    params config: The configuration to send. If None, the configuration specified in the last config will be used.
    '''
    if config:
      self.config = config
    else:
      config = self.config

    response = self.send_request_json(dict(type='config', config=config))
    if response['type'] != 'result':
      raise TowerfallError(f'Unexpected response type: {response["type"]}')
    if not response['success']:
      raise TowerfallError(f'Failed to configure the game. Port: {self.port}, Response: {response["message"]}')
    self.config = config

  def send_request_json(self, obj: Mapping[str, Any]):
    self.open_connection.send_json(obj)
    return self.open_connection.read_json()

  @classmethod
  def close_all(cls):
    '''
    Closes all Towerfall processes.
    '''
    logging.info('Closing all TowerFall.exe processes...')
    for process in psutil.process_iter(attrs=['pid', 'name']):
      # logging.info(f'Checking process {process.pid} {process.name()}')
      if process.name() != 'TowerFall.exe':
        continue
      try:
        logging.info(f'Killing process {process.pid}...')
        os.kill(process.pid, signal.SIGTERM)
      except Exception as ex:
        logging.error(f'Failed to kill process {process.pid}: {ex}')
        continue

  def close(self):
    '''
    Close the management connection. This will free the Towerfall process to be used by other clients.
    '''
    self.open_connection.close()

  def _attain_game_port(self) -> int:
    metadata = self._find_compatible_metadata()

    if not metadata:
      self._try_log(logging.info, f'Starting new process from {self.towerfall_path_exe}.')
      pargs = [self.towerfall_path_exe]
      Popen(pargs, cwd=self.towerfall_path)

    tries = 0
    self._try_log(logging.info, f'Waiting for available process.')
    while not metadata and tries < 10:
      time.sleep(2)
      metadata = self._find_compatible_metadata()
      tries += 1
    if not metadata:
      raise TowerfallError('Could not find or create a Towerfall process.')

    return metadata['port']

  def _find_compatible_metadata(self) -> Optional[Mapping[str, Any]]:
    if not os.path.exists(self.pool_path):
      return None
    dirs = list(os.listdir(self.pool_path))
    random.shuffle(dirs)
    for file_name in os.listdir(self.pool_path):
      try:
        pid = int(file_name)
        psutil.Process(pid)
      except (ValueError, psutil.NoSuchProcess):
        os.remove(os.path.join(self.pool_path, file_name))
        continue
      try:
        with open(os.path.join(self.pool_path, file_name), 'r') as file:
          try:
            metadata = Towerfall._load_metadata(file)
          except (ValueError, json.JSONDecodeError, FileNotFoundError) as ex:
            self._try_log(logging.warning, f'Invalid metadata file {file_name}. Exception: {ex}')
            continue
          return metadata
      except PermissionError as ex:
        self._try_log(logging.warning, f'Failed to access {file_name}. Exception: {ex}')
        continue
    return None

  @staticmethod
  def _load_metadata(file: TextIOWrapper) -> Mapping[str, Any]:
    metadata = json.load(file)
    if 'port' not in metadata:
      raise ValueError('Port not found in metadata.')
    try:
      metadata['port'] = int(metadata['port'])
    except ValueError:
      raise ValueError(f'Port is not an integer. Port: {metadata["port"]}')

    if 'fastrun' not in metadata:
      metadata['fastrun'] = False
    if 'nographics' not in metadata:
      metadata['nographics'] = False
    return metadata

  def _try_log(self, log_fn: Callable[[str], None], message: str):
    if self.verbose > 0:
      log_fn(message)

