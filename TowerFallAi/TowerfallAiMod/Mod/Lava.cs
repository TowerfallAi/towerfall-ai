using TowerfallAi.Api;
using Patcher;
using TowerFall;

namespace TowerfallAi.Mod {
  [Patch("TowerFall.Lava")]
  public static class ModLava {
    public static StateEntity GetState(this Lava ent) {
      var aiState =  new StateLava { type = Types.Lava };
      ExtEntity.SetAiState(ent, aiState);
      aiState.height = ent.Collider.Top;
      return aiState;
    }
  }
}
