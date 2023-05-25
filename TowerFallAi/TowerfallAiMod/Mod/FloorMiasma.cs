using Patcher;
using TowerFall;
using TowerfallAi.Api;

namespace TowerfallAi.Mod {
  [Patch("TowerFall.FloorMiasma")]
  public static class ModFloorMiasma {
    public static StateEntity GetState(this FloorMiasma ent) {
      if (ent.state == FloorMiasma.States.Invisible) return null;

      var aiState = new StateEntity { type = Types.FloorMiasma };
      
      if (ent.state == FloorMiasma.States.Dangerous) {
        aiState.canHurt = true;
      }

      ExtEntity.SetAiState(ent, aiState);
      return aiState;
    }
  }
}
