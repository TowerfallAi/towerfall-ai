using System;
using Patcher;
using TowerFall;
using TowerfallAi.Api;
using TowerfallAi.Common;

namespace TowerfallAi.Mod {
  [Patch("TowerFall.SuperBombArrow")]
  public static class ModSuperBombArrow {
    public static StateEntity GetState(this SuperBombArrow ent) {
      var aiState = new StateArrow { type = Types.Arrow };

      ExtEntity.SetAiState(ent, aiState);
      aiState.state = ent.State.ToString().FirstLower();
      aiState.arrowType = ent.ArrowType.ToString().FirstLower();
      aiState.timeLeft = (int)Math.Ceiling(ent.explodeAlarm.FramesLeft);

      return aiState;
    }
  }
}
