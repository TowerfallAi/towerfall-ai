using System;
using TowerFall;
using TowerfallAi.Common;
using TowerfallAi.Data;

namespace TowerfallAi.Core {
  public static class AgentConfigExtension {
    private static string[] archerColors = new string[] {
      "green",
      "blue",
      "pink",
      "orange",
      "white",
      "yellow",
      "teal",
      "purple",
      "red",
    };

    public static Allegiance GetTeam(this AgentConfig agentConfig) {
      if (agentConfig.team == null || agentConfig.team == "neutral") return Allegiance.Neutral;
      if (agentConfig.team == "red") return Allegiance.Red;
      if (agentConfig.team == "blue") return Allegiance.Blue;
      
      throw new Exception("Invalid team '{0}'".Format(agentConfig.team));
    }

    public static string GetTeam(Allegiance team) {
      if (team == Allegiance.Red) return "red";
      if (team == Allegiance.Blue) return "blue";
      if (team == Allegiance.Neutral) return "neutral"; ;

      throw new Exception("Invalid team '{0}'".Format(team));
    }

    public static int GetArcherIndex(this AgentConfig agentConfig) {
      if (string.IsNullOrWhiteSpace(agentConfig.archer)) {
        return AiMod.Random.Next(0, 8);
      }

      var parts = agentConfig.archer.Split('-');
      if (parts.Length < 1 || parts.Length > 2) {
        throw new Exception("Invalid archer '{0}'".Format(agentConfig.archer));
      }

      int archerIndex;
      if (int.TryParse(parts[0], out archerIndex)) {
        if (archerIndex < 0 || archerIndex >= archerColors.Length) {
          throw new Exception("Invalid archer '{0}'".Format(agentConfig.archer));
        }

        return archerIndex;
      }

      for (int i = 0; i < archerColors.Length; i++) {
        if (archerColors[i] == parts[0]) {
          return i;
        }
      }

      throw new Exception("Invalid archer '{0}'".Format(agentConfig.archer));
    }

    public static ArcherData.ArcherTypes GetArcherType(this AgentConfig agentConfig) {
      if (string.IsNullOrWhiteSpace(agentConfig.archer)) {
        return ArcherData.ArcherTypes.Normal;
      }

      var parts = agentConfig.archer.Split('-');
      if (parts.Length != 2) {
        return ArcherData.ArcherTypes.Normal;
      }

      if (parts[1] == "alt") {
        return ArcherData.ArcherTypes.Alt;
      }

      if (parts[1] == "secret") {
        return ArcherData.ArcherTypes.Secret;
      }

      throw new Exception("Invalid archer '{0}'".Format(agentConfig.archer));
    }
  }
}
