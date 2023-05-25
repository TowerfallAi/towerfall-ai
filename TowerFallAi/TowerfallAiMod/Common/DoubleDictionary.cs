using System.Collections.Generic;

namespace TowerfallAi.Common {
  public class DoubleDictionary<A, B> {
    private Dictionary<A, B> first = new Dictionary<A, B>();
    private Dictionary<B, A> second = new Dictionary<B, A>();

    public void Add(A a, B b) {
      first.Add(a, b);
      second.Add(b, a);
    }

    public B GetB(A a) {
      return first[a];
    }
    
    public A GetA(B b) {
      return second[b];
    }
  }
}
