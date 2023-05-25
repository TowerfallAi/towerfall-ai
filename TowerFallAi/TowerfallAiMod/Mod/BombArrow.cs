using System;
using Patcher;
using TowerFall;
using TowerfallAi.Api;

namespace TowerfallAi.Mod {
  [Patch("TowerFall.BombArrow")]
  public static class ModBombArrow {
    public static StateEntity GetState(this BombArrow ent) {
      var state = (StateArrow)ExtEntity.GetStateArrow(ent);
      state.timeLeft = (float)Math.Ceiling(ent.explodeAlarm.FramesLeft);
      return state;
    }
  }
}
