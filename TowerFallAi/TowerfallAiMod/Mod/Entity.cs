using System;
using System.Collections.Generic;
using Monocle;
using Patcher;
using TowerFall;
using TowerfallAi.Api;
using TowerfallAi.Common;
using System.Reflection;
using Microsoft.Xna.Framework;

namespace TowerfallAi.Mod {
  [Patch]
  public class ModEntity : Entity {
    public override void Update() {
      try {
        base.Update();
      } catch (Exception ex) {
        throw new Exception(GetType().ToString(), ex);
      }
    }
  }

  public static class ExtEntity {
    static Dictionary<Entity, int> ids = new Dictionary<Entity, int>();

    static Dictionary<Entity, Vec2> prevPositions = new Dictionary<Entity, Vec2>();

    public static void Reset() {
      ids.Clear();
      prevPositions.Clear();
    }

    public static bool TryGetSpeed(object obj, out Vector2 speed) {
      var type = obj.GetType();
      speed = new Vector2();
      var field = type.GetField("Speed");
      if (field != null) {
        field.GetValue(obj);
      }
      return false;
    }

    static float GetDecimal(float x) {
      return x - (float)(Math.Round(x));
    }

    public static void SetAiState(Entity ent, StateEntity state) {
      if (ids.ContainsKey(ent)) {
        state.id = ids[ent];
      } else {
        state.id = ids.Count;
        ids.Add(ent, state.id);
      }

      float x;
      float y;
       
      if (ent.Collider == null) {
        x = ent.Position.X;
        y = 240 - ent.Position.Y;
      } else {
        x = ent.CenterX;
        y = 240 - ent.CenterY;
      }

      if (ent is Actor) {
        Actor actor = (Actor)ent;
        x += GetDecimal(actor.ActualPosition.X);
        y += GetDecimal(240 - actor.ActualPosition.Y);
      }

      state.pos = new Vec2 { x = x, y = y };

      Vec2 prevPos;
      Vector2 speed;

      if (TryGetSpeed(ent, out speed)) {
        state.vel = new Vec2 {
          x = speed.X,
          y = -speed.Y
        };
      } else {
        if (prevPositions.TryGetValue(ent, out prevPos)) {
          state.vel = new Vec2 {
            x = state.pos.x - prevPos.x,
            y = state.pos.y - prevPos.y,
          };
        } else {
          state.vel = new Vec2 {
            x = 0,
            y = 0,
          };
        }
        prevPositions[ent] = state.pos;
      }
      
      if (ent.Collider == null) {
        state.size = new Vec2();
      } else {
        state.size = new Vec2 { x = ent.Width, y = ent.Height };
      }

      if (ent is Enemy) {
        Enemy enemy = (Enemy)ent;
        
        state.isEnemy = true;
        state.canHurt = enemy.CanHurt;
        state.canBounceOn = enemy.CanBounceOn;
        state.isDead = enemy.IsDead();
        state.facing = (int)enemy.Facing;
        state.state = ((string[])Util.GetFieldValue("names", typeof(Enemy), enemy, BindingFlags.NonPublic | BindingFlags.Instance))[enemy.State].FirstLower();
      }
    }

    public static StateEntity GetState(Entity e) {
      return GetState(e, null);
    }

    public static StateEntity GetState(Entity ent, string type) {
      var state = new StateEntity {
        type = (type == null ? ent.GetType().Name : type).FirstLower()
      };
      SetAiState(ent, state);
      return state;
    }

    public static StateItem GetStateItem(Entity ent, string itemType) {
      var state = new StateItem {
        type = Types.Item,
        itemType = itemType
      };
      SetAiState(ent, state);
      return state;
    }

    public static StateEntity GetStateArrow(Entity ent) {
      Arrow arrow = ent as Arrow;
      var state = new StateArrow {
        type = Types.Arrow,
        arrowType = arrow.ArrowType.ToString().FirstLower(),
      };
      SetAiState(arrow, state);
      state.state = arrow.State.ToString().FirstLower();
      return state;
    }

    public static StateItem GetStateBombPickup(BombPickup ent) {
      var state = new StateItem {
        type = Types.Item,
        itemType = TypesItems.Bomb
      };
      SetAiState(ent, state);
      return state;
    }
  }
}
