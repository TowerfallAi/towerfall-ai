using System;
using Microsoft.Xna.Framework;
using Patcher;
using TowerFall;
using TowerfallAi.Core;

namespace TowerfallAi.Mod {
  [Patch]
  public class ModTFGame : TFGame {
    // This allows identifying that TowerFall.exe is patched.
    public const string AiModVersion = AiMod.Version;

    Action originalInitialize;
    Action<GameTime> originalUpdate;

    [STAThread]
    public static void Main(string[] args) {
      try {
        AiMod.ParseArgs(args);
        typeof(TFGame).GetMethod("$original_Main").Invoke(null, new object[] { args });
      } catch (Exception exception) {
        TFGame.Log(exception, false);
        TFGame.OpenLog();
      }
    }

    public ModTFGame(bool noIntro) : base(noIntro) {
      var ptr = typeof(TFGame).GetMethod("$original_Initialize").MethodHandle.GetFunctionPointer();
      originalInitialize = (Action)Activator.CreateInstance(typeof(Action), this, ptr);

      ptr = typeof(TFGame).GetMethod("$original_Update").MethodHandle.GetFunctionPointer();
      originalUpdate = (Action<GameTime>)Activator.CreateInstance(typeof(Action<GameTime>), this, ptr);

      if (AiMod.Enabled) {
        this.InactiveSleepTime = new TimeSpan(0);

        if (AiMod.IsFastrun) {
          Monocle.Engine.Instance.Graphics.SynchronizeWithVerticalRetrace = false;
          this.IsFixedTimeStep = false;
        } else {
          this.IsFixedTimeStep = true;
        }
      }
    }

    public override void Initialize() {
      if (!AiMod.Enabled) {
        originalInitialize();
        return;
      }

      AiMod.PreGameInitialize();
      originalInitialize();
      AiMod.PostGameInitialize();
    }

    public override void Update(GameTime gameTime) {
      if (!AiMod.Enabled) {
        originalUpdate(gameTime);
        return;
      }

      AiMod.Update(originalUpdate);
    }

    public override void Draw(GameTime gameTime) {
      if (!AiMod.Enabled) {
        base.Draw(gameTime);
        return;
      }

      if (!AiMod.IsMatchRunning() || AiMod.NoGraphics) {
        Monocle.Engine.Instance.GraphicsDevice.SetRenderTarget(null);
        return;
      }
      base.Draw(gameTime);
      Monocle.Draw.SpriteBatch.Begin();
      Agents.Draw();
      Monocle.Draw.SpriteBatch.End();
    }
  }
}
