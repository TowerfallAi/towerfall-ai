using Patcher;
using TowerFall;
using TowerfallAi.Api;

namespace TowerfallAi.Mod {
  [Patch("TowerFall.OrbPickup")]
  public static class ModOrbPickup{
    public static StateEntity GetState(this OrbPickup ent) {
      var aiState = new StateItem { type = Types.Item };
      ExtEntity.SetAiState(ent, aiState);
      aiState.itemType = "orb" + ent.orbType.ToString();
      return aiState;
    }
  }
}
