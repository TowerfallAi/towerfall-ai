using Patcher;
using TowerFall;
using TowerfallAi.Api;

namespace TowerfallAi.Mod {
  [Patch("TowerFall.KingReaper/ReaperBomb")]
  public static class ModReaperBomb{
    public static StateEntity GetState(this KingReaper.ReaperBomb ent) {
      var aiState = new StateEntity { type = "kingReaperBomb" };
      ExtEntity.SetAiState(ent, aiState);
      aiState.canHurt = true;
      return aiState;
    }
  }
}
