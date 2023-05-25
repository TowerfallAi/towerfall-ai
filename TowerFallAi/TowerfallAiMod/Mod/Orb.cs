using Patcher;
using TowerFall;
using TowerfallAi.Api;

namespace TowerfallAi.Mod {
  [Patch("TowerFall.Orb")]
  public static class ModOrb{
    public static StateEntity GetState(this Orb ent) {
      var aiState = new StateFalling { type = Types.Orb };
      ExtEntity.SetAiState(ent, aiState);
      aiState.falling = ent.falling;
      return aiState;
    }
  }
}
