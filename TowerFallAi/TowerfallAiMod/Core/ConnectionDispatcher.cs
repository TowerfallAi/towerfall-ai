using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TowerfallAi.Common;
using TowerfallAi.Data;

namespace TowerfallAi.Core {
  public class ConnectionDispatcher {
    private Server server;
    private List<RemoteConnection> remoteConnections;

    private object joinLock = new object();
    private object waitConnectionsLock = new object();
    private bool isWaitingConnection = false;
    private int joinCount;

    private Semaphore semConfig = new Semaphore(1, 1);

    private TaskCompletionSource<List<RemoteConnection>> remoteConnectionsTaskSource;

    private RemoteConnection openConfigConnection;

    private const string pools_dir = "pools";
    private string poolPath;

    public int Port { get; private set; }

    public ConnectionDispatcher(string poolName) {
      this.poolPath = Path.Combine(AiMod.BaseDirectory, pools_dir, poolName);
      server = new Server(OnConnection, () => { throw new Exception("Server closed"); });
      remoteConnections = new List<RemoteConnection>();
      Port = server.Start();
      RegisterInPool();
    }

    public bool IsRunning { get { return server.IsAlive(); } }

    private async Task<List<RemoteConnection>> WaitForConnectionsAsync(int count) {
      if (count < 0) throw new Exception($"Expected count > 0. Got: {count}");

      lock (waitConnectionsLock) {
        if (isWaitingConnection) throw new InvalidOperationException("Cannot have concurrent WaitForConnections calls.");
        isWaitingConnection = true;
      }

      lock (joinLock) {
        remoteConnections.Clear();
        isWaitingConnection = true;
        joinCount = count;
        remoteConnectionsTaskSource = new TaskCompletionSource<List<RemoteConnection>>();
      }

      Logger.Info($"Accepting join requests: {count}");

      // ConfigureAwait(false) is required, otherwise the main thread is blocked.
      var result = await remoteConnectionsTaskSource.Task.ConfigureAwait(false);
      isWaitingConnection = false;
      joinCount = 0;
      return result;
    }

    // The server handles one connection at a time. So it is ok for this implementation to not be thread safe.
    private void OnConnection(Socket socket) {
      RemoteConnection connection = new RemoteConnection(socket);
      try {
        Message message = JsonConvert.DeserializeObject<Message>(connection.ReadAsync().Result);
        if (message.type == Message.Type.Join) {
          HandleJoinMessage(connection);
        } else if (message.type == Message.Type.Config) {
          HandleNewConfigConnection(connection, message);
        } else if (message.type == Message.Type.Reset) {
          HandleResetMessage(connection, message);
        } else {
          Logger.Error("Message type not supported: {0}".Format(message.type));
        }
      } catch (SocketException ex) {
        Logger.Info($"Error handling connection: Exception:\n  {ex}");
      }
    }

    private void HandleJoinMessage(RemoteConnection connection) {
      lock (joinLock) {
        if (!isWaitingConnection || remoteConnections.Count >= joinCount) {
          Logger.Info("No open slot to join.");
          connection.Write(JsonConvert.SerializeObject(new Message {
            type = Message.Type.Result,
            success = false,
            message = "No open slot to join.",
          }));
          return;
        }

        remoteConnections.Add(connection);
        Logger.Info($"Join accepted: {remoteConnections.Count}/{joinCount}");
        if (remoteConnections.Count >= joinCount) {
          remoteConnectionsTaskSource.SetResult(remoteConnections);
        }

        connection.Write(JsonConvert.SerializeObject(new Message {
          type = Message.Type.Result,
          success = true,
          message = "Game will start once all agents join."
        }));
      }
    }

    private bool CheckOpenConnectionAndReply(RemoteConnection connection) {
      if (openConfigConnection != null && openConfigConnection.IsAlive()) {
        connection.Write(JsonConvert.SerializeObject(new Message {
          type = Message.Type.Result,
          success = false,
          message = "Game is currently owned by a different client.",
        }));
        return true;
      }
      return false;
    }

    private void HandleNewConfigConnection(RemoteConnection connection, Message message) {
      Logger.Info("Config received from new connection.");
      if (CheckOpenConnectionAndReply(connection)) {
        return;
      }

      HandleConfigMessage(connection, message);
      ListenToNewConfigConnection(connection);
    }

    private void ListenToNewConfigConnection(RemoteConnection connection) {
      openConfigConnection = connection;
      UnregisterFromPool();
      TaskEx.Run(async () => {
        await connection.WaitDisconnectAsync();
        RegisterInPool();
      });
      TaskEx.Run(() => {
        try {
          Logger.Info("Start listening to new config connection.");
          while (true) {
            Message message = JsonConvert.DeserializeObject<Message>(connection.ReadAsync().Result);
            if (message.type == Message.Type.Config) {
              HandleConfigMessage(connection, message);
            } else if (message.type == Message.Type.Reset) {
              HandleResetMessage(connection, message);
            } else {
              Logger.Error("Message type not supported: {0}".Format(message.type));
            }
          }
        } catch (SocketException ex) {
          Logger.Info($"Error handling connection: Exception:\n  {ex}");
        } finally {
          Logger.Info("Stopped listening to config connection.");
        }
      });
    }

    private void HandleConfigMessage(RemoteConnection connection, Message message) {
      try {
        AiMod.ValidateConfig(message.config);
        int connectionsRequired = Agents.CountRemoteConnections(message.config.agents);
        lock (joinLock) {
          if (remoteConnectionsTaskSource != null && remoteConnectionsTaskSource.Task.IsAlive()) {
            remoteConnectionsTaskSource.SetCanceled();
          }
        }

        if (!semConfig.WaitOne(1000)) {
          connection.Write(JsonConvert.SerializeObject(new Message {
            type = Message.Type.Result,
            success = false,
            message = "Unable to cancel existing reconfig request",
          }));
          return;
        }

        Logger.Info("Reconfiguration started.");
        TaskEx.Run(async () => {
          try {
            List<RemoteConnection> connections = await WaitForConnectionsAsync(connectionsRequired);
            AiMod.ReconfigOperation reconfigOperation = new AiMod.ReconfigOperation() {
              Config = message.config,
              Connections = connections
            };
            AiMod.Reconfig(reconfigOperation);
          } catch (TaskCanceledException) {
            Logger.Info("Config cancelled");
          } catch (Exception ex) {
            Logger.Error($"{ex}");
            throw;
          } finally {
            semConfig.Release();
          }
        });
        connection.Write(JsonConvert.SerializeObject(new Message {
          type = Message.Type.Result,
          success = true,
        }));
      } catch (ConfigException ex) {
        Logger.Error(ex.Message);
        connection.Write(JsonConvert.SerializeObject(new Message {
          type = Message.Type.Result,
          success = false,
          message = ex.Message,
        }));
      }
    }

    private void RegisterInPool() { 
      string metadataPath = GetMetadataPath();
      Util.CreateDirectory(poolPath);

      using (var f = File.Open(metadataPath, FileMode.OpenOrCreate)) {
        using (var w = new StreamWriter(f)) {
          Logger.Info($"Writing metadata to {metadataPath}");
          w.Write(JsonConvert.SerializeObject(new Metadata {
            port = Port,
            fastrun = AiMod.IsFastrun,
            nographics = AiMod.NoGraphics,
          }));
        }
      }
    }

    public void UnregisterFromPool() {
      string metadataPath = GetMetadataPath();
      Logger.Info($"Deleting {metadataPath}");
      File.Delete(GetMetadataPath());
    }

    private string GetMetadataPath() {
      return Path.Combine(poolPath, Process.GetCurrentProcess().Id.ToString());
    }

    private void HandleResetMessage(RemoteConnection connection, Message message) {
      AiMod.Reset(new AiMod.ResetOperation { Entities = message.entities });
      connection.Write(JsonConvert.SerializeObject(new Message {
        type = Message.Type.Result,
        success = true,
      }));
    }
  }
}
