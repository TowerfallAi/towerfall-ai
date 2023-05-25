using System;
using Monocle;
using Patcher;
using TowerFall;
using TowerfallAi.Core;

namespace TowerfallAi.Mod {
  [Patch("TowerFall.MenuInput")]
  public static class ModMenuInput {
    [Patch(".cctor")]
    static ModMenuInput() {
      if (!AiMod.Enabled) {
        // Avoid overriding menu inputs created by the mod.
        MenuInput.MenuInputs = new PlayerInput[0];
      }
      MenuInput.repeatLeftCounter = new Counter();
      MenuInput.repeatRightCounter = new Counter();
      MenuInput.repeatUpCounter = new Counter();
      MenuInput.repeatDownCounter = new Counter();
    }

    public static bool Confirm {
      get {
        // Makes the bot automatically confirm all menus.
        if (AiMod.Enabled && !AiMod.IsHumanPlaying()) return true;
        var ptr = typeof(MenuInput).GetMethod("$original_get_Confirm").MethodHandle.GetFunctionPointer();
        var orginalGetConfirm = (Func<bool>)Activator.CreateInstance(typeof(Func<bool>), null, ptr);
        return orginalGetConfirm();
      }
    }

    public static bool ConfirmOrStart {
      get {
        // Makes the bot automatically confirm all menus.
        if (AiMod.Enabled && !AiMod.IsHumanPlaying()) return true;
        var ptr = typeof(MenuInput).GetMethod("$original_get_ConfirmOrStart").MethodHandle.GetFunctionPointer();
        var orginalGetConfirm = (Func<bool>)Activator.CreateInstance(typeof(Func<bool>), null, ptr);
        return orginalGetConfirm();
      }
    }
  }
}
