using System.Reflection;
using Monocle;
using Patcher;
using TowerFall;
using TowerfallAi.Api;
using TowerfallAi.Common;

namespace TowerfallAi.Mod {
  [Patch("TowerFall.Birdman")]
  public static class ModBirdman {
    public static StateEntity GetState(this Birdman ent) {
      var aiState = new StateEntity { type = "birdman" };

      if ((Counter)Util.GetFieldValue("attackCooldown", typeof(Birdman), ent, BindingFlags.NonPublic | BindingFlags.Instance) 
          && !(bool)Util.GetFieldValue("canArrowAttack", typeof(Birdman), ent, BindingFlags.NonPublic | BindingFlags.Instance)) {
        aiState.state = "resting";
      } else {
        switch (ent.State) {
          case Birdman.ST_IDLE:
            aiState.state = "idle";
            break;
          case Birdman.ST_ATTACK:
            aiState.state = "attack";
            break;
        }
      }
      
      ExtEntity.SetAiState(ent, aiState);
      return aiState;
    }
  }
}
