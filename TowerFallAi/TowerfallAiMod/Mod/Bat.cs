using Patcher;
using TowerFall;
using TowerfallAi.Api;

namespace TowerfallAi.Mod {
  [Patch("TowerFall.Bat")]
  public static class ModBat {
    public static StateEntity GetState(this Bat ent) {
      var aiState = new StateEntity();

      aiState.type = ConversionTypes.BatTypes.GetB(ent.batType);
      ExtEntity.SetAiState(ent, aiState);
      return aiState;
    }
  }
}
