using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Monocle;
using Newtonsoft.Json.Linq;
using TowerFall;
using TowerfallAi.Api;
using TowerfallAi.Common;

namespace TowerfallAi.Core {
  public static class EntityCreator {
    private static Dictionary<string, Func<JObject, Entity>> creatorFuncs = new Dictionary<string, Func<JObject, Entity>> {
      { "bat", CreateBat },
      { "batBomb", CreateBat },
      { "batSuperBomb", CreateBat },
      { "bird", CreateBat },
      { "slime", CreateSlime },
      { "birdman", CreateBirdman },
      { "cultist", CreateCultist },
      { "evilCrystal", CreateEvilCrystal },
      { "exploder", CreateExploder },
      { "flamingSkull", CreateFlamingSkull },
      { "ghost", CreateGhost },
      { "mole", CreateMole },
      { "worm", CreateWorm },
    };

    public static Player CreatePlayer(JObject e, int playerIndex, Allegiance team) {
      var state = e.ToObject<StateEntity>();
      Vector2 pos = GetPos(state);
      var player = new Player(
        playerIndex,
        pos,
        team,
        team,
        PlayerInventory.Default,
        Player.HatStates.Normal,
        false,
        false,
        false);
      Logger.Info($"Create archer {playerIndex} at pos {pos}");
      return player;
    }

    public static Entity CreateEntity(JObject e) {
      JToken typeToken = e.GetValue("type");
      if (typeToken == null) throw new ArgumentException("Missing 'type' from reset entity.");
      string type = typeToken.Value<string>();
      Func<JObject, Entity> creatorFunc;
      if (creatorFuncs.TryGetValue(type, out creatorFunc)) {
        return creatorFunc(e);
      }
      
      throw new Exception($"Unsupported entity type to be created: {type}");
    }

    public static Bat CreateBat(JObject e) {
      var state = e.ToObject<StateEntity>();
      return new Bat(
        GetPos(state) + new Vector2(0, 2),
        GetFacing(state),
        ConversionTypes.BatTypes.GetA(state.type));
    }

    public static Slime CreateSlime(JObject e) {
      var state = e.ToObject<StateSubType>();
      return new Slime(
        GetPos(state) + new Vector2(0, 5),
        GetFacing(state),
        state.subType == null ? Slime.SlimeColors.Green : ConversionTypes.SlimeTypes.GetA(state.subType));
    }

    public static Birdman CreateBirdman(JObject e) {
      var state = e.ToObject<StateEntity>();
      return new Birdman(GetPos(state), GetFacing(state), darkWorld: false);
    }

    public static Cultist CreateCultist(JObject e) {
      var state = e.ToObject<StateSubType>();
      return new Cultist(
        GetPos(state) + new Vector2(0, 8),
        GetFacing(state),
        state.subType == null ? Cultist.CultistTypes.Normal : ConversionTypes.CultistTypes.GetA(state.subType));
    }

    public static EvilCrystal CreateEvilCrystal(JObject e) {
      var state = e.ToObject<StateSubType>();
      Vector2 pos = GetPos(state);
      return new EvilCrystal(
        pos,
        GetFacing(state),
        state.subType == null ? EvilCrystal.CrystalColors.Red : ConversionTypes.CrystalTypes.GetA(state.subType),
        new Vector2[] { pos + new Vector2(50 * state.facing, 0) });
    }

    public static Exploder CreateExploder(JObject e) {
      var state = e.ToObject<StateEntity>();
      Vector2 pos = GetPos(state);
      return new Exploder(
        pos,
        GetFacing(state),
        new Vector2[] { pos + new Vector2(50 * state.facing, 0) });
    }

    public static FlamingSkull CreateFlamingSkull(JObject e) {
      var state = e.ToObject<StateEntity>();
      return new FlamingSkull(GetPos(state), GetFacing(state));
    }

    public static Ghost CreateGhost(JObject e) {
      var state = e.ToObject<StateSubType>();
      Vector2 pos = GetPos(state);
      return new Ghost(
        pos, 
        GetFacing(state),
        new Vector2[] { pos + new Vector2(50 * state.facing, 0) },
        state.subType == null ? Ghost.GhostTypes.Blue : ConversionTypes.GhostTypes.GetA(state.subType));
    }

    public static Mole CreateMole(JObject e) {
      var state = e.ToObject<StateEntity>();
      return new Mole(GetPos(state), GetFacing(state));
    }

    public static Worm CreateWorm(JObject e) {
      var state = e.ToObject<StateSubType>();
      return new Worm(GetPos(state));
    }

    private static Facing GetFacing(StateEntity e) {
      return e.facing > 0 ? Facing.Right : Facing.Left;
    }

    private static Vector2 GetPos(StateEntity e) {
      return new Vector2(e.pos.x, 240 - e.pos.y);
    }
  }
}
