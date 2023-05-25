using Patcher;
using TowerFall;
using TowerfallAi.Api;

namespace TowerfallAi.Mod {
  [Patch("TowerFall.Spikeball")]
  public static class ModSpikeball {
    public static StateEntity GetState(this Spikeball ent) {
      var aiState = new StateSpikeBall { type = Types.SpikeBall };

      ExtEntity.SetAiState(ent, aiState);
      aiState.center = new Vec2 {
        x = ent.pivot.X,
        y = ent.pivot.Y
      };

      aiState.radius = ent.radius;

      return aiState;
    }
  }
}
