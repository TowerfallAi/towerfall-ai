import json
import logging
import socket
from typing import Any, Callable, Mapping

_BYTE_ORDER = 'big'
_ENCODING = 'ascii'
_LOCALHOST = '127.0.0.1'

class Connection:
  '''
  Connection to a Towerfall server. It is used to send and receive messages.

  params port: Port of the server.
  params ip: Ip address of the server.
  params timeout: Timeout for the socket.
  params verbose: Verbosity level. 0: no logging, 1: much logging.
  params log_cap: Maximum number of characters to log.
  params record_path: Path to a file to record the messages sent and received.
  '''
  def __init__(self, port: int, ip: str = _LOCALHOST, timeout: float = 0, verbose=0, log_cap=100, record_path=None):
    self.verbose = verbose
    self.log_cap = log_cap
    self.record_path = record_path
    self._socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    self._socket.connect((ip, port))
    if timeout:
      self._socket.settimeout(timeout)
    self.port = port
    self.on_close: Callable

  def __del__(self):
    self.close()

  def close(self):
    '''
    Closes the socket.
    '''
    if hasattr(self, '_socket'):
      if self.verbose > 0:
        logging.info('Closing socket')
      self._socket.close()
      del self._socket
    if hasattr(self, 'on_close'):
      self.on_close()

  def write(self, msg: str):
    '''
    Writes a new message following the game's protocol.
    '''
    size = len(msg)
    if self.verbose > 0:
      logging.info('Writing: %sB %s', size, self._cap(msg))

    self._socket.sendall(size.to_bytes(2, byteorder=_BYTE_ORDER))
    self._socket.sendall(msg.encode(_ENCODING))
    if self.record_path:
      with open(self.record_path, 'a') as file:
        file.write(msg + '\n')

  def read(self) -> str:
    '''
    Reads a message following the game's protocol.
    '''
    try:
      header: bytes = self._socket.recv(2)
      size = int.from_bytes(header, _BYTE_ORDER)
      if size == 0:
        raise ConnectionError('Connection is closed')
      payload = self._socket.recv(size)
      resp = payload.decode(_ENCODING)
      if self.verbose > 0:
        logging.info('Read: %dB %s', size, self._cap(resp))
      if self.record_path:
        with open(self.record_path, 'a') as file:
          file.write(resp + '\n')
      return resp
    except socket.timeout as ex:
      logging.error(f'Socket timeout {self._socket.getsockname()}')
      raise ex

  def read_json(self) -> Mapping[str, Any]:
    '''
    Reads a message and parses it to json.
    '''
    return json.loads(self.read())

  def send_json(self, obj: Mapping[str, Any]):
    '''
    Convert the object to json and writes it.
    '''
    self.write(json.dumps(obj))

  def _cap(self, value: str) -> str:
    return value[:self.log_cap] + '...' if len(value) > self.log_cap else value
