using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TowerfallAi.Common;

namespace TowerfallAi.Core {
  /// <summary>
  /// Implements read/write over sockets.
  /// </summary>
  public class RemoteConnection : IDisposable {
    private const int maxMessageSize = 1 << 16;
    private const int headerSize = 2;

    private Socket socket;
    private byte[] headerBuffer = new byte[headerSize];
    private byte[] bodyBuffer = new byte[maxMessageSize];
    private int bytesToReceive = 0;
    private AsyncQueue<string> messages = new AsyncQueue<string>();

    private TaskCompletionSource<bool> disconnectionTcs = new TaskCompletionSource<bool>();

    public RemoteConnection(Socket socket) {

      this.socket = socket;
      TaskEx.Run(() => { 
        try {
          while (true) {
            int bytesRead = socket.Receive(headerBuffer, 0, headerSize, SocketFlags.None);
            if (bytesRead == 0) {
              messages.EnqueueException(new SocketException((int)SocketError.ConnectionReset));
              return;
            }

            bytesToReceive = headerBuffer[0] << 8 | headerBuffer[1];
            bytesRead = socket.Receive(bodyBuffer, 0, bytesToReceive, SocketFlags.None);
            if (bytesRead == 0) {
              messages.EnqueueException(new SocketException((int)SocketError.ConnectionReset));
              return;
            }

            string response = Encoding.ASCII.GetString(bodyBuffer, 0, bytesToReceive);
            messages.Enqueue(response);
          }
        } finally {
          disconnectionTcs.SetResult(true);
        }
      });
    }

    public Task WaitDisconnectAsync() {
      return disconnectionTcs.Task;
    }

    public bool IsAlive() {
      return disconnectionTcs.Task.IsAlive();
    }

    private void ReceiveHeaderCallback(IAsyncResult result) {
      var bytesRead = socket.EndReceive(result);
      if (bytesRead == 0) {
        messages.EnqueueException(new SocketException((int)SocketError.ConnectionReset));
        return;
      }

      if (bytesRead != 2) {
        Logger.Error($"Received unexpected number of bytes. bytesRead: {bytesRead}");
        Dispose();
        return;
      }

      bytesToReceive = headerBuffer[0] << 8 | headerBuffer[1];
      if (bytesToReceive > maxMessageSize) {
        Logger.Error($"Message exceeds limit: {maxMessageSize}.");
        Dispose();
        return;
      }

      socket.BeginReceive(bodyBuffer, 0, bytesToReceive, SocketFlags.None, ReceiveBodyCallback, null);
    }

    private void ReceiveBodyCallback(IAsyncResult result) {
      var bytesRead = socket.EndReceive(result);
      if (bytesRead != bytesToReceive) {
        Logger.Error($"Received unexpected number of bytes. bytesRead: {bytesRead}. bytesToReceive: {bytesToReceive}");
        Dispose();
        return;
      }

      string response = Encoding.ASCII.GetString(bodyBuffer, 0, bytesRead);
      messages.Enqueue(response);
      bytesToReceive = 0;
      socket.BeginReceive(headerBuffer, 0, headerSize, SocketFlags.None, ReceiveHeaderCallback, null);
    }


    public async Task<string> ReadAsync(TimeSpan timeout = default, CancellationToken cancellationToken = default) {
      string message = await messages.DequeueAsync(timeout, cancellationToken);
      return message;
    }

    public void Write(string text) {
      byte[] payload = Encoding.ASCII.GetBytes(text);
      int size = payload.Length;
      if (size > maxMessageSize) {
        throw new Exception("Message exceeds limit: {0}.".Format(maxMessageSize));
      }
      byte[] header = new Byte[2];
      header[0] = (byte)(size >> 8);
      header[1] = (byte)(size & 0x00FF);
      socket.Send(header);
      socket.Send(payload);
    }

    public void Dispose() {
      if (socket != null && socket.IsBound) {
        socket.Shutdown(SocketShutdown.Both);
        socket.Close();
        socket.Dispose();
        socket = null;
      }
    }
  }
}
