using Patcher;
using TowerFall;
using TowerfallAi.Api;

namespace TowerfallAi.Mod {
  [Patch("TowerFall.ProximityBlock")]
  public static class ModProximityBlock {
    public static StateEntity GetState(this ProximityBlock ent) {
      var  aiState = new StateProximityBlock { type = Types.ProximityBlock };
      
      ExtEntity.SetAiState(ent, aiState);
      aiState.collidable = ent.Collidable;

      return aiState;
    }
  }
}
