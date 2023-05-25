using Patcher;
using TowerFall;
using TowerfallAi.Api;

namespace TowerfallAi.Mod {
  [Patch("TowerFall.Cultist")]
  public static class ModCultist {
    public static StateEntity GetState(this Cultist ent) {
      var aiState = new StateEntity();

      aiState.type = ConversionTypes.CultistTypes.GetB(ent.type);
      ExtEntity.SetAiState(ent, aiState);
      return aiState;
    }
  }
}
