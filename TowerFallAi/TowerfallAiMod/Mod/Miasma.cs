using Monocle;
using Patcher;
using TowerFall;
using TowerfallAi.Api;

namespace TowerfallAi.Mod {
  [Patch("TowerFall.Miasma")]
  public static class ModMiasma{
    public static StateEntity GetState(this Miasma ent) {
      var aiState = new StateMiasma { type = Types.Miasma };
      ExtEntity.SetAiState(ent, aiState);
      aiState.left = ((ColliderList)ent.Collider).colliders[0].Right;
      aiState.right = ((ColliderList)ent.Collider).colliders[1].Left;
      return aiState;
    }
  }
}
