using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using TowerfallAi.Api;
using TowerfallAi.Common;

namespace TowerfallAi.Data {
  [DataContract]
  public class Message {
    public class Type {
      // Game actions sent by the bot.
      public const string Actions = "actions";

      // Reset the game with a new config.
      public const string Config = "config";

      // Reset the current level with defined entities.
      public const string Reset = "reset";
      
      public const string Join = "join";

      public const string Result = "result";
    }

    [DataMember(EmitDefaultValue = true)]
    public string type;

    [DataMember(EmitDefaultValue = false)]
    public string actions;

    [DataMember(EmitDefaultValue = false)]
    public MatchConfig config;

    [DataMember(EmitDefaultValue = false)]
    public State state;

    [DataMember(EmitDefaultValue = false)]
    public Vec2 pos;

    [DataMember(EmitDefaultValue = false)]
    public List<DrawInstruction> draws;

    [DataMember(EmitDefaultValue = false)]
    public List<JObject> entities;

    [DataMember(EmitDefaultValue = true)]
    public bool success;

    [DataMember(EmitDefaultValue = false)]
    public string message;

    [DataMember(EmitDefaultValue = false)]
    public int id;
  }

  public class DrawInstruction {
    public string type;
    public Vec2 start;
    public Vec2 end;
    public float[] color;
    public float thick;
    public void Draw() {
      if (type == "line") {
        float scale = 3;
        Monocle.Draw.Line(new Vector2(start.x * scale, (240 - start.y) * scale), new Vector2(end.x * scale, (240 - end.y) * scale), 
          new Color(color[0], color[1], color[2]), thick);
      } else {
        throw new Exception("Type {0} not supported.".Format(type));
      }
    }
  }
}
