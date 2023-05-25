namespace TowerfallAi.Api {
  public class StateEntity : State {
    public Vec2 pos;
    public Vec2 vel;
    public Vec2 size;
    public int id;
    public string state;
    public bool isEnemy;
    public bool canHurt;
    public bool canBounceOn;
    public bool isDead;
    public int facing;

    public StateEntity ReCenter() {
      pos.x += size.x / 2;
      pos.y -= size.y / 2;
      return this;
    }
  }
}
