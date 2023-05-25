using System;

namespace Patcher {
  public class PatchAttribute : Attribute {
    public string Name;
    public PatchAttribute(string name = null) {
      this.Name = name;
    }
  }
}
