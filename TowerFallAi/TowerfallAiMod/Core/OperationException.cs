using System;

namespace TowerfallAi.Core {
  public class OperationException : Exception {
    public OperationException(string message) : base(message) { }
  }
}
