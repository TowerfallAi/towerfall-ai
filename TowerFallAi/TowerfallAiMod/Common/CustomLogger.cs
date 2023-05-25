using System;
using System.IO;
using System.Text;
using System.Threading;

namespace TowerfallAi.Common {
  public class CustomLogger {
    static object l = new object();
    string path;

    int batchSize;

    public CustomLogger(string path, int batchSize = 0) {
      this.path = path;
      Directory.CreateDirectory(Path.GetDirectoryName(path));
      if (!File.Exists(path)) {
        var f = File.Create(path);
        f.Close();
      }

      this.batchSize = batchSize;
    }

    StringBuilder buffer = new StringBuilder();

    public void Flush() {
      lock (l) {
        if (buffer.Length > 0) {
          string m = buffer.ToString();
          using (var f = File.Open(path, FileMode.Append)) {
            using (var w = new StreamWriter(f)) {
              w.Write(m);
            }
          }

          Console.Write(m);

          buffer.Clear();
        }
      }
    }

    public void Log(string message, string level) {
      string m = "{0} {1} {2}".Format(level, Thread.CurrentThread.ManagedThreadId, message);
      WriteLine(m);
    }

    public void WriteLine(string message) {
      lock (l) {
        buffer.Append(DateTime.Now.ToString("hh:mm:ss.ffff"));
        buffer.Append(" ");
        buffer.AppendLine(message);

        if (buffer.Length > batchSize) {
          message = buffer.ToString();
          using (var f = File.Open(path, FileMode.Append)) {
            using (var w = new StreamWriter(f)) {
              w.Write(message);
            }
          }

          Console.Write(message);

          buffer.Clear();
        }
      }
    }

    public void Error(string message) {
      Log(message, "ERROR");
    }

    public void Info(string message) {
      Log(message, "INFO ");
    }
  }
}
