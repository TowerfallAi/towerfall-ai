using System;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework.Graphics;

namespace TowerfallAi.Common {
  public static class Util {
    public static string PathCombine(params string[] paths) {
      return string.Join("/", paths);
    }

    public static string[] GetPathSegments(string path) {
      string[] segments = path.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
      return segments;
    }

    public static string GetDirectoryPath(string path) {
      int i = Math.Max(path.LastIndexOf('/'), path.LastIndexOf('\\'));

      return path.Substring(0, i);
    }

    public static void CreateDirectory(string path) {
      string[] segments = path.Split(new string[] { "/", "\\" }, StringSplitOptions.None);
      path = null;

      foreach (string segment in segments) {
        if (path == null) {
          path = segment;
        } else {
          path = PathCombine(path, segment);
        }

        if (!Directory.Exists(path)) {
          if (string.IsNullOrEmpty(path)) continue;
          Directory.CreateDirectory(path);
        }
      }
    }

    public static void DeleteFolderIfExists(string path) {
      if (Directory.Exists(path)) {
        Directory.Delete(path, true);
      }
    }

    public static void DeleteFileIfExists(string path) {
      if (File.Exists(path)) {
        File.Delete(path);
      }
    }

    public static void ParseExecute(string p, out string program, out string args) {
      var space = new char[] { ' ' };
      p = p.Trim(space);

      bool closeQuotes = false;
      for (int i = 0; i < p.Length; i++) {
        if (p[i] == ' ') {
          if (!closeQuotes) {
            program = p.Substring(0, i);
            program = program.Trim(space);
            args = p.Substring(i + 1, p.Length - i - 1);
            args = args.Trim(space);
            return;
          }
        } else if (p[i] == '\"') {
          closeQuotes = !closeQuotes;
        }
      }

      program = p;
      args = null;
    }

    public static Func<T> GetFunction<T>(string name, object inst) {
      var ptr = inst.GetType().GetMethod(name).MethodHandle.GetFunctionPointer();
      return (Func<T>)Activator.CreateInstance(typeof(Func<T>), inst, ptr);
    }

    public static Action GetAction(string name, object inst) {
      var ptr = inst.GetType().GetMethod(name).MethodHandle.GetFunctionPointer();
      return (Action)Activator.CreateInstance(typeof(Action), inst, ptr);
    }

    public static Action GetAction(string name, Type type, object inst, BindingFlags bindingFlags) {
      MethodInfo method = type.GetMethod(name, bindingFlags);
      if (method == null) {
        throw new Exception("Type {0} doesnt have method {1}".Format(type, name));
      }
      var ptr = method.MethodHandle.GetFunctionPointer();
      return (Action)Activator.CreateInstance(typeof(Action), inst, ptr);
    }

    public static Action GetAction(string name, Type type, object inst) {
      MethodInfo method = type.GetMethod(name);
      if (method == null) {
        throw new Exception("Type {0} doesnt have method {1}".Format(type, name));
      }
      var ptr = method.MethodHandle.GetFunctionPointer();
      return (Action)Activator.CreateInstance(typeof(Action), inst, ptr);
    }

    public static object GetFieldValue(string name, Type type, object inst, BindingFlags bindingFlags) {
      FieldInfo info = type.GetField(name, bindingFlags);
      if (info == null) {
        throw new Exception("Type {0} doesnt have field {1}".Format(type, name));
      }

      return info.GetValue(inst);
    }

    public static object GetPrivateFieldValue(string name, object inst) {
      return GetFieldValue(name, inst.GetType(), inst, BindingFlags.NonPublic | BindingFlags.Instance);
    }

    public static void GetData(RenderTarget2D renderTarget, byte[] buffer) {
      renderTarget.GetData(buffer);
    }
  }
}
