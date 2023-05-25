import logging


class NoLevelFormatter(logging.Formatter):
  def format(self, record):
    return record.getMessage()

def default_logging():
  logging.basicConfig(level=logging.INFO)
  logging.getLogger().handlers[0].setFormatter(NoLevelFormatter())