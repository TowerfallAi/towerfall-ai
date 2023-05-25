using System.Collections.Generic;

namespace TowerfallAi.Api {
  public class StateUpdate : State {
    public StateUpdate() {
      type = "update";
    }
    
    public List<StateEntity> entities = new List<StateEntity>();

    public float dt;
    public int id;
  }
}
