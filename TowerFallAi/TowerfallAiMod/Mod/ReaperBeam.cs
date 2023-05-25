using Microsoft.Xna.Framework;
using Patcher;
using TowerFall;
using TowerfallAi.Api;
using TowerfallAi.Common;

namespace TowerfallAi.Mod {
  [Patch("TowerFall.KingReaper/ReaperBeam")]
  public static class ModReaperBeam {
    public static StateEntity GetState(this KingReaper.ReaperBeam ent) {
      var aiState = new StateReaperBeam { type = "kingReaperBeam" };
      ExtEntity.SetAiState(ent, aiState);
      aiState.canHurt = ent.Collidable;
      Vector2 normal = (Vector2)Util.GetPrivateFieldValue("normal", ent);
      aiState.dir = new Vec2 {
        x = normal.X,
        y = normal.Y
      };
      aiState.width = 8;
      return aiState;
    }
  }
}
