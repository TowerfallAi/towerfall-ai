using Patcher;
using TowerFall;
using TowerfallAi.Api;

namespace TowerfallAi.Mod {
  [Patch("TowerFall.SwitchBlock")]
  public static class ModSwitchBlock {
    public static StateEntity GetState(this SwitchBlock ent) {
      var aiState = new StateSwitchBlock { type = Types.SwitchBlock };

      ExtEntity.SetAiState(ent, aiState);
      aiState.collidable = ent.Collidable;
      aiState.warning = ent.drawFlicker || ent.DrawWarning > 0;

      return aiState;
    }
  }
}
