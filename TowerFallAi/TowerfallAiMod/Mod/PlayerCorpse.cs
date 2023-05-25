using Patcher;
using TowerFall;
using TowerfallAi.Api;

namespace TowerfallAi.Mod {
  [Patch("TowerFall.PlayerCorpse")]
  public static class ModPlayerCorpse {
    public static StateEntity GetState(this PlayerCorpse ent) {
      if (ent.PlayerIndex < 0) return null;
      var state = new StateEntity { type = "playerCorpse" };
      ExtEntity.SetAiState(ent, state);
      return state;
    }
  }
}
