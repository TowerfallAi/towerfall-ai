from gym_wrapper import (KillEnemyObjective, PlayerObservation,
                         TowerfallBlankEnv)
from towerfall import Towerfall

'''
Test the gym wrapper with one agent performing random actions.
'''

def main():
  n_episodes = 1000
  env = create_env()
  env.reset()
  for _ in range(n_episodes):
    _, _, done, _ = env.step(env.action_space.sample())
    if done:
      env.reset()

def create_env() -> TowerfallBlankEnv:
  towerfall = Towerfall(
    verbose=1,
    config=dict(
      mode='sandbox',
      level='2',
      fps=999,
      agents=[dict(type='remote')]))
  env = TowerfallBlankEnv(
    towerfall=towerfall,
    observations= [PlayerObservation()],
    objective=KillEnemyObjective(
      enemy_count=3,
      episode_max_len=60*10),
    verbose=1)
  return env


if __name__ == '__main__':
  main()