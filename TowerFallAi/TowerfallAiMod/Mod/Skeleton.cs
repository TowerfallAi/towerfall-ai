using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;
using Patcher;
using TowerFall;
using TowerfallAi.Api;
using TowerfallAi.Common;

namespace TowerfallAi.Mod {
  [Patch("TowerFall.Skeleton")]
  public static class ModSkeleton{
    public static StateEntity GetState(this Skeleton ent) {
      var aiState = new StateArcher() { type = "archer" };

      ExtEntity.SetAiState(ent, aiState);
      aiState.playerIndex = -1;
      PlayerShield shield = ((PlayerShield)Util.GetPrivateFieldValue("shield", ent));
      aiState.shield = shield != null && shield.Visible;
      PlayerWings wings = ((PlayerWings)Util.GetPrivateFieldValue("wings", ent));
      aiState.wing = wings != null && wings.Visible;
      aiState.arrows = new List<string>();
      List<ArrowTypes> arrows = ent.Arrows.Arrows;
      for (int i = 0; i < arrows.Count; i++) {
        aiState.arrows.Add(arrows[i].ToString().FirstLower());
      }
      aiState.canHurt = ent.CanHurt;
      aiState.dead = ent.IsDead();
      aiState.facing = (int)ent.Facing;
      aiState.onGround = (bool)Util.GetPrivateFieldValue("onGround", ent);
      aiState.onWall = false;

      Vector2 aim = Calc.AngleToVector(ent.AimingAngle, 1);
      aiState.aimDirection = new Vec2 {
        x = aim.X,
        y = -aim.Y
      };
      aiState.team = "neutral";
      aiState.isEnemy = true;

      return aiState;
    }
  }
}
