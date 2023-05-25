using System;
using System.Xml;
using Microsoft.Xna.Framework;
using Monocle;
using Patcher;
using TowerFall;
using TowerfallAi.Core;

namespace TowerfallAi.Mod {
  [Patch]
  public class ModLevel : Level {
    Action originalUpdate;

		Action originalHandlePausing;

    public ModLevel(Session session, XmlElement xml) : base(session, xml) {
      var ptr = typeof(Level).GetMethod("$original_Update").MethodHandle.GetFunctionPointer();
      originalUpdate = (Action)Activator.CreateInstance(typeof(Action), this, ptr);

			ptr = typeof(Level).GetMethod("$original_HandlePausing").MethodHandle.GetFunctionPointer();
      originalHandlePausing = (Action)Activator.CreateInstance(typeof(Action), this, ptr);
		}

		public override void HandlePausing() {
      // Avoid pausing when no human is playing and the screen goes out of focus.
			if (AiMod.Enabled && !AiMod.IsHumanPlaying()) {
        return;
      }

      originalHandlePausing();
    }

		public override void Update() {
      if (AiMod.Enabled) {
        Agents.RefreshInputFromAgents(this);
      }

      originalUpdate();
    }
  }
}
