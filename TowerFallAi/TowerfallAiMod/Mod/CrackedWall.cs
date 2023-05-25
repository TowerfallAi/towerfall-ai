using Patcher;
using TowerFall;
using TowerfallAi.Api;

namespace TowerfallAi.Mod {
  [Patch("TowerFall.CrackedWall")]
  public static class ModCrackedWall{
    public static StateEntity GetState(this CrackedWall ent) {
      var state = new StateCrackedWall { type = Types.CrackedWall };
      ExtEntity.SetAiState(ent, state);
      state.count = ent.explodeCounter;
      return state;
    }
  }
}
