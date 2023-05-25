using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;
using Patcher;
using TowerFall;
using TowerfallAi.Api;
using TowerfallAi.Common;
using TowerfallAi.Core;

namespace TowerfallAi.Mod {
  [Patch("TowerFall.Player")]
  public static class ModPlayer{
    public static StateEntity GetState(this Player ent) {
      var aiState = new StateArcher() { type = "archer" };

      ExtEntity.SetAiState(ent, aiState);
      aiState.playerIndex = ent.PlayerIndex;
      aiState.shield = ent.HasShield;
      aiState.wing = ent.HasWings;
      aiState.state = ent.State.ToString().FirstLower();
      aiState.arrows = new List<string>();
      List<ArrowTypes> arrows = ent.Arrows.Arrows;
      for (int i = 0; i < arrows.Count; i++) {
        aiState.arrows.Add(arrows[i].ToString().FirstLower());
      }
      aiState.canHurt = ent.CanHurt;
      aiState.dead = ent.Dead;
      aiState.facing = (int)ent.Facing;
      aiState.onGround = ent.OnGround;
      aiState.onWall = ent.CanWallJump(Facing.Left) || ent.CanWallJump(Facing.Right);
      Vector2 aim = Calc.AngleToVector(ent.AimDirection, 1);
      aiState.aimDirection = new Vec2 {
        x = aim.X,
        y = -aim.Y
      };
      aiState.team = AgentConfigExtension.GetTeam(ent.TeamColor);

      return aiState;
    }
  }
}
