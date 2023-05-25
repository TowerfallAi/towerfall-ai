using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using TowerfallAi.Common;

namespace TowerfallAi.Core {
  public class Server {
    Socket listener;
    Task serverTask;

    public Action<Socket> onConnection;
    public Action onClose;

    public Server(Action<Socket> onConnection, Action onClose) {
      this.onConnection = onConnection;
      this.onClose = onClose;
    }

    public bool IsAlive() {
      return serverTask.IsAlive();
    }

    public int Start(string ip = "127.0.0.1", int port = 0) {
      if (serverTask != null && serverTask.IsAlive()) {
        throw new InvalidOperationException("Server task is still alive");
      }

      SocketPermission permission = new SocketPermission(
        NetworkAccess.Accept,
        TransportType.Tcp,
        "",
        SocketPermission.AllPorts);
      permission.Demand();

      Logger.Info("Permissions created.");

      IPEndPoint ipEndPoint = GetIpEndpoint(ip, port);
      listener = new Socket(
        ipEndPoint.AddressFamily,
        SocketType.Stream,
        ProtocolType.Tcp);

      Logger.Info("Server socket created.");
      listener.Bind(ipEndPoint);
      Logger.Info("Listener binded.");
      port = ((IPEndPoint)listener.LocalEndPoint).Port;
      Logger.Info("Server started on port {0}".Format(port));

      listener.Listen(10);
      Logger.Info("Starting server thread.");
      serverTask = TaskEx.Run(() => {
        try {
          Logger.Info("Server thread started.");
          while (true) {
            Logger.Info("Server Accepting.");
            onConnection(listener.Accept());
          }
        } catch(Exception ex) {
          Logger.Error(ex.ToString());
          throw;
        }
      });

      return port;
    }

    private IPEndPoint GetIpEndpoint(string ip, int port) {
      return new IPEndPoint(IPAddress.Parse(ip), port);
    }
  }
}
