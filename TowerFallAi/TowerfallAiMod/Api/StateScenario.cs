namespace TowerfallAi.Api {
  public class StateScenario : State {
    public StateScenario() {
      type = "scenario";
    }

    public int[,] grid;
    public float cellSize = 10;
  }
}
