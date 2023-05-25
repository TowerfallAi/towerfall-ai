using TowerFall;
using TowerfallAi.Common;

namespace TowerfallAi.Api {
  public class ConversionTypes {
    public static DoubleDictionary<Bat.BatType, string> BatTypes = new DoubleDictionary<Bat.BatType, string>();
    public static DoubleDictionary<Slime.SlimeColors, string> SlimeTypes = new DoubleDictionary<Slime.SlimeColors, string>();
    public static DoubleDictionary<Cultist.CultistTypes, string> CultistTypes = new DoubleDictionary<Cultist.CultistTypes, string>();
    public static DoubleDictionary<EvilCrystal.CrystalColors, string> CrystalTypes = new DoubleDictionary<EvilCrystal.CrystalColors, string>();
    public static DoubleDictionary<Ghost.GhostTypes, string> GhostTypes = new DoubleDictionary<Ghost.GhostTypes, string>();

    static ConversionTypes() {
      BatTypes.Add(Bat.BatType.Eye, "bat");
      BatTypes.Add(Bat.BatType.Bomb, "batBomb");
      BatTypes.Add(Bat.BatType.SuperBomb, "batSuperBomb");
      BatTypes.Add(Bat.BatType.Bird, "bird");

      SlimeTypes.Add(Slime.SlimeColors.Blue, "blue");
      SlimeTypes.Add(Slime.SlimeColors.Green, "green");
      SlimeTypes.Add(Slime.SlimeColors.Red, "red");

      CultistTypes.Add(Cultist.CultistTypes.Boss, "boss");
      CultistTypes.Add(Cultist.CultistTypes.Normal, "normal");
      CultistTypes.Add(Cultist.CultistTypes.Scythe, "scythe");

      CrystalTypes.Add(EvilCrystal.CrystalColors.Blue, "blue");
      CrystalTypes.Add(EvilCrystal.CrystalColors.Green, "green");
      CrystalTypes.Add(EvilCrystal.CrystalColors.Pink, "pink");
      CrystalTypes.Add(EvilCrystal.CrystalColors.Red, "red");

      GhostTypes.Add(Ghost.GhostTypes.Blue, "blue");
      GhostTypes.Add(Ghost.GhostTypes.Fire, "fire");
      GhostTypes.Add(Ghost.GhostTypes.Green, "green");
      GhostTypes.Add(Ghost.GhostTypes.GreenFire, "greenFire");
    }
  }

  public class Types {
    public const string Arrow = "arrow";
    public const string Bramble = "bramble";
    public const string CrackedPlatform = "crackedPlatform";
    public const string CrackedWall = "crackedWall";
    public const string Crown = "crown";
    public const string Hat = "hat";
    public const string EvilCrystal = "evilCrystal";
    public const string ExpandingPlatform = "expandingPlatform";
    public const string ExpandingTrigger = "expandingTrigger";
    public const string Explosion = "explosion";
    public const string FakeWall = "fakeWall";
    public const string FloorMiasma = "floorMiasma";
    public const string GraniteBlock = "graniteBlock";
    public const string HotCoal = "hotCoal";
    public const string Ice = "ice";
    public const string Icicle = "icicle";
    public const string JumpPad = "jumpPad";
    public const string Lantern = "lantern";
    public const string Lava = "lava";
    public const string Miasma = "miasma";
    public const string MoonGlassBlock = "moonGlassBlock";
    public const string MovingPlatform = "movingPlatform";
    public const string Orb = "orb";
    public const string ProximityBlock = "proximityBlock";
    public const string Item = "item";
    public const string ShiftBlock = "shiftBlock";
    public const string SpikeBall = "spikeBall";
    public const string SwitchBlock = "switchBlock";
    public const string Chest = "chest";
  }

  public class TypesItems {
    public const string Bomb = "bomb";
    public const string Mirror = "mirror";
    public const string Shield = "shield";
    public const string Wings = "wings";

    public const string OrbDark = "orbDark";
    public const string OrbTime = "orbTime";
    public const string OrbLava = "orbLava";
    public const string OrbSpace = "orbSpace";
    public const string OrbChaos = "orbChaos";
  }
}
