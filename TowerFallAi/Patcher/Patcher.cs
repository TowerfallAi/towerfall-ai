using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Patcher {
  public class Patcher {
    private static string PatcherDir = Path.GetFullPath("Patcher");
    private static string UnsealedDir = Path.Combine(PatcherDir, "Unsealed");
    private static string OriginalDir = Path.Combine(PatcherDir, "Original");

    private static string modModulePath = Path.GetFullPath("TowerfallAiMod.dll");

    ModuleDefinition modModule;
    ModuleDefinition baseModule;

    Dictionary<string, TypeReference> catchTypes = new Dictionary<string, TypeReference>();

    private static HashSet<string> namespaces = new HashSet<string> {
      "TowerFall",
      "Monocle"
    };

    private static bool IsInNamespace(string name) {
      foreach (string ns in namespaces) {
        if (name.StartsWith(ns + ".")) {
          return true;
        }
      }
      return false;
    }

    static void CreateDirIfNotExists(string dir) {
      if (!Directory.Exists(dir)) {
        Directory.CreateDirectory(dir);
      }
    }

    static string CreateOriginalIfNotExists(string target) {
      CreateDirIfNotExists(PatcherDir);
      CreateDirIfNotExists(OriginalDir);
      string targetName = Path.GetFileName(target);
      string originalPath = Path.Combine(OriginalDir, targetName);

      if (File.Exists(originalPath)) {
        ModuleDefinition module = ModuleDefinition.ReadModule(originalPath);
        AssertOriginalModule(module, originalPath);
      } else {
        ModuleDefinition module = ModuleDefinition.ReadModule(target);
        AssertOriginalModule(module, target);
        File.Copy(target, originalPath);
      }

      return originalPath;
    }

    /// <summary>
    /// Make everything public, unsealed an virtual, so everything can be referenced.
    /// </summary>
    public void MakeBaseImage(string target = "TowerFall.exe") {
      target = Path.GetFullPath(target);
      string originalPath = CreateOriginalIfNotExists(target);
      CreateUnsealedIfNotExists(originalPath);
    }

    static string CreateUnsealedIfNotExists(string originalPath) {
      string targetName = Path.GetFileName(originalPath);
      string unsealedPath = Path.Combine(UnsealedDir, targetName);

      if (File.Exists(unsealedPath)) {
        return unsealedPath;
      }

      CreateDirIfNotExists(PatcherDir);
      CreateDirIfNotExists(UnsealedDir);

      Console.WriteLine("Creating unsealed image from {0}", originalPath);
      var module = ModuleDefinition.ReadModule(originalPath);
      foreach (var type in module.AllNestedTypes()) {
        if (!IsInNamespace(type.FullName)) {
          continue;
        }

        if (type.Name.StartsWith("<>")) {
          continue;
        }
        if (type.IsNested)
          type.IsNestedPublic = true;
        if (type.IsValueType) {
          continue;
        }

        type.IsSealed = false;
        foreach (var field in type.Fields)
          field.IsPublic = true;
        foreach (var method in type.Methods) {
          method.IsPublic = true;
          if (!method.IsConstructor && !method.IsStatic)
            method.IsVirtual = true;
        }
      }

      module.Write(unsealedPath);
      Console.WriteLine("Unsealed binary created in {0}", unsealedPath);
      return unsealedPath;
    }

    private static void PrintWithIndent(int depth, string format, params object[] args) {
      for (int i = 0; i < depth; i++) {
        Console.Write(" ");
      }

      Console.WriteLine(format, args);
    }

    private bool IsPatchType(TypeReference type, int depth) {
      if (type.Scope == modModule) {
        return type.Resolve().CustomAttributes.Any(attr => attr.AttributeType.FullName == "Patcher.PatchAttribute");
      }
      return false;
    }

    TypeReference MapToPatchedType(TypeReference modType, int depth) {
      if (modType.IsGenericParameter) {
        return modType;
      }
      if (modType.IsArray) {
        var type = MapToPatchedType(((ArrayType)modType).ElementType, depth + 1);
        return new ArrayType(type);
      }
      if (IsPatchType(modType, depth + 1)) {
        return baseModule.Types.SelectMany(CecilExtensions.AllNestedTypes).First(t => t.FullName == GetModifiedTypeFullName(modType.Resolve()));
      }
      return baseModule.ImportReference(modType);
    }

    MethodReference MapToBaseMethod(MethodReference modMethod, int depth) {
      string modMethodName = GetName(modMethod.Resolve());
      TypeReference returningType = MapToPatchedType(modMethod.ReturnType, depth + 1);
      TypeReference declaringType = MapToPatchedType(modMethod.DeclaringType, depth + 1);
      var method = new MethodReference(modMethodName, returningType, declaringType);
      method.HasThis = modMethod.HasThis;
      MapParams(modMethod, method, depth + 1);

      var modInst = modMethod as GenericInstanceMethod;
      if (modInst != null) {
        method.CallingConvention = MethodCallingConvention.Generic;
        var inst = new GenericInstanceMethod(method);
        foreach (var arg in modInst.GenericArguments) {
          inst.GenericArguments.Add(MapToPatchedType(arg, depth + 1));
        }
        method = inst;
      }
      return method;
    }

    void MapParams(MethodReference modMethod, MethodReference method, int depth) {
      foreach (var param in modMethod.Parameters)
        method.Parameters.Add(new ParameterDefinition(MapToPatchedType(param.ParameterType, depth + 1)));
    }

    MethodDefinition CloneMethod(MethodDefinition modMethod, string prefix, int depth) {
      string modMethodName = GetName(modMethod);
      var method = new MethodDefinition(prefix + modMethodName, modMethod.Attributes, MapToPatchedType(modMethod.ReturnType, depth + 1));
      MapParams(modMethod, method, depth + 1);
      foreach (var modParam in modMethod.GenericParameters) {
        // GenericParameter is not public, so using reflection here to create it. There might be a more proper way, but this seems to work.
        ConstructorInfo genericParameterCtor = typeof(GenericParameter).GetConstructor(new[] { typeof(int), typeof(GenericParameterType), typeof(ModuleDefinition) });
        var param = (GenericParameter)genericParameterCtor.Invoke(new object[] { modParam.Position, GenericParameterType.Method, modModule });
        method.GenericParameters.Add(param);
      }
      return method;
    }

    /// <summary>
    /// Adds or replaces methods implementations defined in classes containing PatchAttribute.
    /// </summary>
    public void Patch(string target = "TowerFall.exe") {
      target = Path.GetFullPath(target);
      string targetName = Path.GetFileName(target);
      string originalPath = CreateOriginalIfNotExists(target);

      if (File.Exists("FNA.dll")) {
        File.Copy("FNA.dll", Path.Combine(OriginalDir, "FNA.dll"), true);
      }

      Directory.SetCurrentDirectory(OriginalDir);
      
      baseModule = ModuleDefinition.ReadModule(originalPath);
      AssertOriginalModule(baseModule, originalPath);
      modModule = ModuleDefinition.ReadModule(modModulePath);

      foreach (TypeDefinition type in baseModule.Types.SelectMany(CecilExtensions.AllNestedTypes)) {
        foreach (var method in type.Methods) {
          if (method.Body == null) continue;
          foreach (var exHandler in method.Body.ExceptionHandlers) {
            if (exHandler.CatchType != null) {
              if (catchTypes.ContainsKey(exHandler.CatchType.FullName)) continue;
              catchTypes.Add(exHandler.CatchType.FullName, exHandler.CatchType);
            }
          }
        }
      }

      foreach (TypeDefinition modType in modModule.Types.SelectMany(CecilExtensions.AllNestedTypes)) {
        if (IsPatchType(modType, 0)) {
          ModifyPatchedType(modType);
        }
      }

      Console.WriteLine("Writing {0}", target);
      baseModule.Write(target);

      string modFinalPath = Path.Combine(GetDirectoryPath(target), Path.GetFileName(modModulePath));

      if (modModulePath != modFinalPath) {
        Console.WriteLine("Copying {0} to {1}", modModulePath, modFinalPath);
        File.Copy(modModulePath, modFinalPath, overwrite: true);
        string modModulePdbPath = modModulePath.Replace(".dll", ".pdb");
        string modPdbFinalPath = modFinalPath.Replace(".dll", ".pdb");
        Console.WriteLine("Copying {0} to {1}", modModulePdbPath, modFinalPath);
        File.Copy(modModulePdbPath, modPdbFinalPath, overwrite: true);
      }
    }

    static void AssertOriginalModule(ModuleDefinition moduleDefinition, string path) {
      TypeDefinition tfType = moduleDefinition.Types.Where(type => type.Name == "TFGame").FirstOrDefault();
      if (tfType == default) {
        throw new ArgumentException($"File is not a towerfall game executable: {path}");
      }

      FieldDefinition versionField = tfType.Fields.Where(field => field.Name == "AiModVersion").FirstOrDefault();
      if (versionField != default) {
        throw new ArgumentException($"File is already patched with version {versionField.Constant}. File: {path}");
      }
    }

    static string GetDirectoryPath(string path) {
      int i = Math.Max(path.LastIndexOf('/'), path.LastIndexOf('\\'));

      return path.Substring(0, i);
    }

    static string GetSignature(MethodDefinition method) {
      string name = GetName(method);
      if (name == method.Name) {
        return method.Signature();
      }

      string signature = method.Signature().Replace(method.Name + "(", name + "(");
      return signature;
    }

    static string GetName(MethodDefinition method) {
      CustomAttribute attr = method.CustomAttributes.SingleOrDefault(a => a.AttributeType.FullName == "Patcher.PatchAttribute");
      if (attr == null) {
        return method.Name;
      }

      foreach (CustomAttributeArgument arg in attr.ConstructorArguments) {
        return arg.Value.ToString();
      }

      return method.Name;
    }

    string GetModifiedTypeFullName(TypeDefinition type) {
      CustomAttribute attr = type.CustomAttributes.SingleOrDefault(a => a.AttributeType.FullName == "Patcher.PatchAttribute");
      if (attr == null) {
        return type.BaseType.FullName;
      }

      foreach (CustomAttributeArgument arg in attr.ConstructorArguments) {
        if (arg.Value == null) continue;
        return arg.Value.ToString();
      }

      return type.BaseType.FullName;
    }

    private void ModifyPatchedType(TypeDefinition modType) {
      int indent = 0;
      PrintWithIndent(indent, "{0}", modType);
      string modifiedTypeFullName = GetModifiedTypeFullName(modType);
      var type = baseModule.AllNestedTypes().Single(t => t.FullName == modifiedTypeFullName);
      PrintWithIndent(++indent, "Original: {0}", type);

      // Copy over fields including their custom attributes.
      foreach (var modField in modType.Fields) {
        if (modField.DeclaringType == modType) {
          var newField = new FieldDefinition(modField.Name, modField.Attributes, MapToPatchedType(modField.FieldType, indent));
          foreach (var attribute in modField.CustomAttributes) {
            newField.CustomAttributes.Add(new CustomAttribute(MapToBaseMethod(attribute.Constructor, indent), attribute.GetBlob()));
          }
          type.Fields.Add(newField);
        }
      }

      // Copy over or replace methods.
      foreach (var modMethod in modType.Methods) {
        if (modMethod.Name == ".cctor") continue;
        if (modMethod.DeclaringType == modType) {
          string modMethodName = GetName(modMethod);
          MethodDefinition original = type.Methods.SingleOrDefault(m => m.Signature() == GetSignature(modMethod));
          MethodDefinition savedMethod = null;
          if (original == null) {
            PrintWithIndent(1, "Add new method: {0}", modMethodName);
            type.Methods.Add(original = CloneMethod(modMethod, "", indent));
          } else {
            PrintWithIndent(1, "Modify method: {0}", modMethodName);
            savedMethod = CloneMethod(modMethod, "$original_", indent);
            savedMethod.Body = original.Body;
            savedMethod.IsRuntimeSpecialName = false;
            type.Methods.Add(savedMethod);
          }

          if (modMethod.Body.HasExceptionHandlers) {
            foreach (var exHandler in modMethod.Body.ExceptionHandlers) {
              if (exHandler.CatchType != null) {
                exHandler.CatchType = catchTypes[exHandler.CatchType.FullName];
              }
            }
          }

          original.Body = modMethod.Body;
          // Redirect any references in the body.
          var proc = modMethod.Body.GetILProcessor();
          foreach (Instruction instr in modMethod.Body.Instructions) {
            if (instr.Operand is MethodReference) {
              MethodReference callee = MapToBaseMethod((MethodReference)instr.Operand, indent);
              if (original != null && callee.FullName == original.FullName) {
                // Replace base calls with ones to $original
                instr.Operand = savedMethod;
              } else {
                instr.Operand = callee;
              }
            } else if (instr.Operand is FieldReference) {
              var field = (FieldReference)instr.Operand;
              instr.Operand = new FieldReference(field.Name, MapToPatchedType(field.FieldType, indent), MapToPatchedType(field.DeclaringType, indent));
            } else if (instr.Operand is TypeReference)
              instr.Operand = MapToPatchedType((TypeReference)instr.Operand, indent);
          }
          foreach (var var in modMethod.Body.Variables) {
            var.VariableType = MapToPatchedType(var.VariableType, indent);
          }
          modMethod.Body = proc.Body;
        } else {
          throw new Exception();
        }
      }
    }
  }
}
