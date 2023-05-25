using Patcher;
using TowerFall;
using TowerfallAi.Api;
using TowerfallAi.Common;

namespace TowerfallAi.Mod {
  [Patch("TowerFall.TreasureChest")]
  public static class ModTreasureChest {
    public static StateEntity GetState(this TreasureChest ent) {
      if (ent.State == TreasureChest.States.WaitingToAppear) return null;

      var aiState = new StateChest { type = Types.Chest };
      ExtEntity.SetAiState(ent, aiState);
      aiState.state = ent.State.ToString().FirstLower();
      aiState.chestType = ent.type.ToString().FirstLower();

      return aiState;
    }
  }
}
