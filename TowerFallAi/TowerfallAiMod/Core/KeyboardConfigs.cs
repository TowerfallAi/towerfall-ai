using Microsoft.Xna.Framework.Input;
using TowerFall;

namespace TowerfallAiMod.Core {
  public static class KeyboardConfigs {
    public static KeyboardConfig[] Configs = new KeyboardConfig[4];

    static KeyboardConfigs() {
      for (int i = 0; i < Configs.Length; i++) {
        Configs[i] = KeyboardConfig.GetDefault();
      }

      int j = 1;
      Configs[j].Down = new Keys[] { Keys.K };
      Configs[j].Up = new Keys[] { Keys.I };
      Configs[j].Left = new Keys[] { Keys.J };
      Configs[j].Right = new Keys[] { Keys.L };
      Configs[j].Jump = new Keys[] { Keys.U };
      Configs[j].Shoot = new Keys[] { Keys.O };
      Configs[j].Dodge = new Keys[] { Keys.NumPad9 };

      j++;
      Configs[j].Down = new Keys[] { Keys.H };
      Configs[j].Up = new Keys[] { Keys.Y };
      Configs[j].Left = new Keys[] { Keys.G };
      Configs[j].Right = new Keys[] { Keys.J };
      Configs[j].Jump = new Keys[] { Keys.Y };
      Configs[j].Shoot = new Keys[] { Keys.I };
      Configs[j].Dodge = new Keys[] { Keys.NumPad6 };

      j++;
      Configs[j].Down = new Keys[] { Keys.W };
      Configs[j].Up = new Keys[] { Keys.S };
      Configs[j].Left = new Keys[] { Keys.A };
      Configs[j].Right = new Keys[] { Keys.F };
      Configs[j].Jump = new Keys[] { Keys.Q };
      Configs[j].Shoot = new Keys[] { Keys.E };
      Configs[j].Dodge = new Keys[] { Keys.NumPad3 };
    }
  }
}
