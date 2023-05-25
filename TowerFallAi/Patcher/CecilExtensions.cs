using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace Patcher {
  static class CecilExtensions {
    public static IEnumerable<TypeDefinition> AllNestedTypes(this TypeDefinition type) {
      yield return type;
      foreach (TypeDefinition nested in type.NestedTypes) {
        foreach (TypeDefinition moreNested in AllNestedTypes(nested)) {
          yield return moreNested;
        }
      }
    }

    public static IEnumerable<TypeDefinition> AllNestedTypes(this ModuleDefinition module) {
      return module.Types.SelectMany(AllNestedTypes);
    }

    public static string Signature(this MethodReference method) {
      return string.Format("{0}({1})", method.Name, string.Join(", ", method.Parameters.Select(p => p.ParameterType)));
    }
  }
}
