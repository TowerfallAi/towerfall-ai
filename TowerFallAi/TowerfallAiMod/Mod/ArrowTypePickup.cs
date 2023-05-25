using Monocle;
using Patcher;
using TowerFall;
using TowerfallAi.Api;

namespace TowerfallAi.Mod {
  [Patch("TowerFall.ArrowTypePickup")]
  public static class ModArrowTypePickup {
    public static StateEntity GetState(this Entity ent) {
      var item = ent as ArrowTypePickup;
      var state = new StateItem {
        type = Types.Item,
        itemType = "arrow" + item.arrowType.ToString()
      };
      ExtEntity.SetAiState(ent, state);
      return state;
    }
  }
}
