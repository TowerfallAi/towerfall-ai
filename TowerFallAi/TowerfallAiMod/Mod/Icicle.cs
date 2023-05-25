using Patcher;
using TowerFall;
using TowerfallAi.Api;

namespace TowerfallAi.Mod {
  [Patch("TowerFall.Icicle")]
  public static class ModIcicle {
    public static StateEntity GetState(this Icicle ent) {
      var aiState = new StateFalling { type = Types.Icicle };
      ExtEntity.SetAiState(ent, aiState);
      aiState.falling = ent.falling;
      return aiState;
    }
  }
}
