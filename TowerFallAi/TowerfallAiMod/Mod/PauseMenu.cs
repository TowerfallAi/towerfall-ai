using Microsoft.Xna.Framework;
using Patcher;
using TowerFall;
using TowerfallAi.Common;
using TowerfallAi.Core;

namespace TowerfallAi.Mod {
  [Patch]
  public class ModPauseMenu : PauseMenu {
    public ModPauseMenu(Level level, Vector2 position, MenuType menuType, int controllerDisconnected = -1) : base(level, position, menuType, controllerDisconnected) { }

    public override void VersusMatchSettingsAndSave() {
      if (AiMod.Enabled) {
        AiMod.EndSession();
        return;
      }
      
      Util.GetAction("$original_VersusMatchSettingsAndSave", this)();
    }

    public override void Quit() {
      if (AiMod.Enabled) {
        AiMod.EndSession();
        return;
      }

      Util.GetAction("$original_Quit", this)();
    }

    public override void VersusMatchSettings() {
      if (AiMod.Enabled) {
        AiMod.EndSession();
        return;
      }

      Util.GetAction("$original_VersusMatchSettings", this)();
    }

    public override void VersusArcherSelect() {
      if (AiMod.Enabled) {
        AiMod.EndSession();
        return;
      }

      Util.GetAction("$original_VersusArcherSelect", this)();
    }

    public override void QuestMap() {
      if (AiMod.Enabled) {
        AiMod.EndSession();
        return;
      }

      Util.GetAction("$original_QuestMap", this)();
    }

    public override void VersusRematch() {
      if (AiMod.Enabled) {
        AiMod.Rematch();
        return;
      }

      Util.GetAction("$original_VersusRematch", this)();
    }

    public override void QuestRestart() {
      if (AiMod.Enabled) {
        AiMod.Rematch();
        return;
      }

      Util.GetAction("$original_QuestRestart", this)();
    }

    public override void QuestMapAndSave() {
      if (AiMod.Enabled) {
        AiMod.EndSession();
        return;
      }

      Util.GetAction("$original_QuestMapAndSave", this)();
    }

    public override void QuitAndSave() {
      if (AiMod.Enabled) {
        AiMod.EndSession();
        return;
      }

      Util.GetAction("$original_QuitAndSave", this)();
    }
  }
}
