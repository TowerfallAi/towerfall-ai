using Patcher;
using TowerFall;
using TowerfallAi.Api;
using TowerfallAi.Common;

namespace TowerfallAi.Mod {
  [Patch("TowerFall.Slime")]
  public static class ModSlime {
    public static StateEntity GetState(this Slime ent) {
      var aiState = new StateSubType { type = "slime" };
      ExtEntity.SetAiState(ent, aiState);
      aiState.subType = ConversionTypes.SlimeTypes.GetB((Slime.SlimeColors)Util.GetPrivateFieldValue("slimeColor", ent));
      return aiState;
    }
  }
}
