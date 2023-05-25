using TowerfallAi.Core;

namespace TowerfallAi.Api {
  public class StateInit : State {
    public StateInit() {
      type = "init";
      version = AiMod.Version;
    }

    // The version of the mod.
    public string version;

    // The index of the player
    public int index;
  }
}
