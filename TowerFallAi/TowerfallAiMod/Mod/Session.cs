using System;
using Patcher;
using TowerFall;
using TowerfallAi.Common;
using TowerfallAi.Core;

namespace TowerfallAi.Mod {
  [Patch]
  public class ModSession : Session {
    Action originalOnLevelLoadFinish;

    public ModSession(MatchSettings matchSettings) : base(matchSettings) { 
      originalOnLevelLoadFinish = Util.GetAction("$original_OnLevelLoadFinish", typeof(Session), this);
    }

    public override void OnLevelLoadFinish() {
      originalOnLevelLoadFinish();

      if (AiMod.Enabled) {
        Agents.NotifyLevelLoad(this.CurrentLevel);
      }
    }
  }
}
