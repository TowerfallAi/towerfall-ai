using Patcher;
using TowerFall;
using TowerfallAi.Api;

namespace TowerfallAi.Mod {
  [Patch("TowerFall.KingReaper/ReaperCrystal")]
  public static class ModReaperCrystal {
    public static StateEntity GetState(this KingReaper.ReaperCrystal ent) {
      var aiState = new StateEntity { type = "kingReaperCrystal" };
      ExtEntity.SetAiState(ent, aiState);
      aiState.canHurt = true;
      return aiState;
    }
  }
}
