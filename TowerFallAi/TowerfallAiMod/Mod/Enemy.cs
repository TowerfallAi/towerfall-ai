using TowerFall;
using TowerfallAi.Common;
using System.Reflection;

namespace TowerfallAi.Mod {
  public static class ExtEnemy {
    public static bool IsDead(this Enemy e) {      
      return (bool)Util.GetFieldValue("dead", typeof(Enemy), e, BindingFlags.Instance | BindingFlags.NonPublic);
    }
  }
}
