using Patcher;
using TowerFall;
using TowerfallAi.Api;

namespace TowerfallAi.Mod {
  [Patch("TowerFall.DefaultHat")]
  public static class ModDefaultHat{
    public static StateEntity GetState(this DefaultHat ent) {
      var aiState = new StateHat { type = Types.Hat };
     
      aiState.playerIndex = ent.PlayerIndex;
      ExtEntity.SetAiState(ent, aiState);
      return aiState;
    }
  }
}
