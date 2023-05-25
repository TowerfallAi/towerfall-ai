using System.Collections.Generic;

namespace TowerfallAi.Api {
  public class StateArcher : StateEntity {
    public int playerIndex;
    public bool shield;
    public bool wing;
    public List<string> arrows;
    public bool dead;
    public bool onGround;
    public bool onWall;
    public Vec2 aimDirection;
    public string team;
    public bool dodgeCooldown;
  }
}
