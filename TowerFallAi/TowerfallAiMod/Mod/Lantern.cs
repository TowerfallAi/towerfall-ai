using Patcher;
using TowerFall;
using TowerfallAi.Api;

namespace TowerfallAi.Mod {
  [Patch("TowerFall.Lantern")]
  public static class ModLantern {
    public static StateEntity GetState(this Lantern ent) {
      var aiState = new StateFalling { type = Types.Lantern };
      ExtEntity.SetAiState(ent, aiState);
      aiState.falling = ent.falling;
      return aiState;
    }
  }
}
