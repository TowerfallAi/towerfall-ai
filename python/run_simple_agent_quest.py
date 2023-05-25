from agents import SimpleAgent
from common.logging_options import default_logging
from towerfall import Towerfall

default_logging()

'''
Two agents playing a normal quest game.
'''

def main():
  # Creates or reuse a Towerfall game.
  towerfall = Towerfall(
    verbose = 1,
    config = dict(
      mode='quest',
      level='2',
      fps=60,
      agentTimeout='00:00:02',
      agents=[
        dict(type='remote', archer='blue'),
        dict(type='remote', archer='green')],
    )
  )

  connections = []
  agents = []
  remote_agents = sum(1 for agent in towerfall.config['agents'] if agent['type'] != 'human')
  for i in range(remote_agents):
    connections.append(towerfall.join(timeout=10, verbose=1))
    agents.append(SimpleAgent(connections[i]))

  while True:
    # Read the state of the game then replies with an action.
    for connection, agent in zip(connections, agents):
      game_state = connection.read_json()
      agent.act(game_state)


if __name__ == '__main__':
  main()