using Patcher;
using TowerFall;
using TowerfallAi.Api;
using TowerfallAi.Common;

namespace TowerfallAi.Mod {
  [Patch("TowerFall.ShiftBlock")]
  public static class ModShiftBlock {
    public static StateEntity GetState(this ShiftBlock ent) {
      var aiState = new StateShiftBlock { type = Types.ShiftBlock };
      ExtEntity.SetAiState(ent, aiState);
      aiState.startPosition = new Vec2 {
        x = ent.startPosition.X,
        y = ent.startPosition.Y
      };

      aiState.endPosition = new Vec2 {
        x = ent.node.X,
        y = ent.node.Y
      };

      aiState.state = ((ShiftBlock.States)Util.GetPrivateFieldValue("state", ent)).ToString().FirstLower();
      return aiState;
    }
  }
}
