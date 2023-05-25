using Monocle;
using Patcher;
using TowerFall;
using TowerfallAi.Api;
using TowerfallAi.Common;

namespace TowerfallAi.Mod {
  [Patch("TowerFall.KingReaper")]
  public static class ModKingReaper {
    public static StateEntity GetState(this KingReaper ent) {
      var aiState = new StateKingReaper { type = "kingReaper" };
      ExtEntity.SetAiState(ent, aiState);
      aiState.shield = (Counter)Util.GetPrivateFieldValue("shieldCounter", ent);
      return aiState;
    }
  }
}
