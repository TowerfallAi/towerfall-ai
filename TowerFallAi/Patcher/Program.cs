using System;
using System.Reflection;
using CommandLine;

namespace Patcher {
  public class Program {
    [Verb("makebaseimage", HelpText = "Creates a base image to be referenced by the mod.")]
    public class MakeBaseImageOptions {
      [Option('t', "target", Required = true, HelpText = "The file to be patched.")]
      public string Target { get; set; }
    }

    [Verb("patch", HelpText = "Patches the binary with the mod changes.")]
    public class PatchOptions {
      [Option('t', "target", Required = false, HelpText = "The file to be patched.")]
      public string Target { get; set; }
    }

    public static int Main(string[] args) {
      Patcher patcher = new Patcher();
      if (args.Length == 1 && args[0] == "--version") {
        Assembly assembly = Assembly.GetExecutingAssembly();
        Version version = assembly.GetName().Version;
        Console.WriteLine(version);
      }

      if (args.Length == 0) {
        patcher.MakeBaseImage();
        patcher.Patch();
        return 0;
      }

      Parser.Default.ParseArguments<MakeBaseImageOptions, PatchOptions>(args)
          .MapResult(
        (MakeBaseImageOptions o) => {
          patcher.MakeBaseImage(o.Target);
          return 0;
        },
        (PatchOptions o) => {
          patcher.Patch(o.Target);
          return 0;
        },
        e => 1); ;
      if (args.Length == 0) {
        Console.WriteLine("Usage: Patcher.exe makeBaseImage | patch <patch DLLs>*");
        return -1;
      }

      return 0;
    }
  }
}
