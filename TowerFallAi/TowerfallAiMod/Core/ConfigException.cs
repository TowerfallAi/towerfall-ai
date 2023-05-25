using System;

namespace TowerfallAi.Core {
  public class ConfigException : Exception {
    public ConfigException(string message) : base(message) { }
  }
}
