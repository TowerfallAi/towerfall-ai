using Patcher;
using TowerFall;
using TowerfallAi.Api;
using TowerfallAi.Common;

namespace TowerfallAi.Mod {
  [Patch("TowerFall.CrackedPlatform")]
  public static class ModCrackedPlatform {
    public static StateEntity GetState(this CrackedPlatform ent) {
      var state = new StateEntity { type = Types.CrackedPlatform };
      ExtEntity.SetAiState(ent, state);
      state.state = ent.state.ToString().FirstLower();
      return state;
    }
  }
}
