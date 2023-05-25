using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TowerFall;
using TowerfallAi.Common;
using TowerfallAi.Data;
using TowerfallAiMod.Core;

namespace TowerfallAi.Core {
  public abstract class AgentConnection : KeyboardInput, IDisposable {
    public int index;
    InputState prevInputState;
    public InputState InputState;

    public AgentConnection(int index) : base(KeyboardConfigs.Configs[index], index) {
      this.index = index;
      this.Config = KeyboardConfigs.Configs[index];
    }

    protected InputState GetCopy(InputState inputState) {
      return new InputState {
        AimAxis = inputState.AimAxis,
        ArrowsPressed = inputState.ArrowsPressed,
        DodgeCheck = inputState.DodgeCheck,
        DodgePressed = inputState.DodgePressed,
        JumpCheck = inputState.JumpCheck,
        JumpPressed = inputState.JumpPressed,
        MoveX = inputState.MoveX,
        MoveY = inputState.MoveY,
        ShootCheck = inputState.ShootCheck,
        ShootPressed = inputState.ShootPressed
      };
    }

    public override InputState GetState() {
      return GetCopy(this.InputState);
    }

    protected void ProcessResponse(string response) {
      if (response == null || response.Length == 0) return;

      this.InputState.AimAxis.X = 0;
      this.InputState.MoveX = 0;
      this.InputState.AimAxis.Y = 0;
      this.InputState.MoveY = 0;

      var responseChars = new HashSet<char>();
      foreach (char c in response) {
        responseChars.Add(c);
      }

      foreach(char c in responseChars) {
        ProcessAction(c);
      }
    }

    protected void ProcessAction(char c) {
      switch (c) {
        case 'j':
          this.InputState.JumpCheck = true;
          this.InputState.JumpPressed = !this.prevInputState.JumpCheck;
          break;
        case 's':
          this.InputState.ShootCheck = true;
          this.InputState.ShootPressed = !this.prevInputState.ShootCheck;
          break;
        case 'z':
          this.InputState.DodgeCheck = true;
          this.InputState.DodgePressed = !this.prevInputState.DodgeCheck;
          break;
        case 'a':
          this.InputState.ArrowsPressed = true;
          break;
        case 'u':
          this.InputState.AimAxis.Y = -1;
          this.InputState.MoveY = -1;
          break;
        case 'd':
          this.InputState.AimAxis.Y += 1;
          this.InputState.MoveY += 1;
          break;
        case 'l':
          this.InputState.AimAxis.X -= 1;
          this.InputState.MoveX -= 1;
          break;
        case 'r':
          this.InputState.MoveX += 1;
          this.InputState.AimAxis.X += 1;
          break;
      }
    }

    public void UpdateGameInput(string response) {
      this.InputState = new InputState();
      ProcessResponse(response);
      this.prevInputState = GetCopy(this.InputState);
    }

    public abstract void Send(string message, int frame);

    public abstract Task<Message> ReceiveAsync(TimeSpan timeout = default, CancellationToken cancellationToken = default);

    public abstract void Dispose();

    protected void Disconnect(string message) {
      Logger.Error(message);
      this.Dispose();
      throw new Exception("Disconnected");
    }
  }
}
