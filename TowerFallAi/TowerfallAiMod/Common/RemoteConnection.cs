//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Net.Sockets;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace Common {
//  public class RemoteConnection : IDisposable {
//    Socket listener;
//    Socket senderSocket;
//    List<string> inputLines = new List<string>();
//    Semaphore mutexInput = new Semaphore(0, int.MaxValue);

//    Exception receiveException;

//    public bool IsConnected {
//      get {
//        if (this.senderSocket == null) return false;

//        return this.senderSocket.Connected;
//      }
//    }

//    public void StartListening(string ip, int port) {
//      SocketPermission permission = new SocketPermission(
//          NetworkAccess.Accept,
//          TransportType.Tcp,
//          "",
//          SocketPermission.AllPorts);

//      permission.Demand();

//      IPEndPoint ipEndPoint = GetIpEndpoint(ip, port);

//      listener = new Socket(
//          ipEndPoint.AddressFamily,
//          SocketType.Stream,
//          ProtocolType.Tcp);

//      listener.Bind(ipEndPoint);

//      listener.Listen(1);

//      AsyncCallback aCallback = new AsyncCallback(AcceptCallback);
//      listener.BeginAccept(aCallback, listener);
//    }

//    public void Disconnect() {
//      if (listener != null) {
//        listener.Close();
//        listener.Dispose();
//        listener = null;
//      }

//      if (senderSocket != null) {
//        senderSocket.Close();
//        senderSocket.Dispose();
//        senderSocket = null;
//      }
//    }

//    private IPEndPoint GetIpEndpoint(string ip, int port) {
//      return new IPEndPoint(IPAddress.Parse(ip), port);
//    }

//    public void Connect(string ip, int port) {
//      SocketPermission permission = new SocketPermission(
//              NetworkAccess.Connect,
//              TransportType.Tcp,
//              "",
//              SocketPermission.AllPorts);

//      permission.Demand();

//      IPEndPoint ipEndPoint = GetIpEndpoint(ip, port);

//      senderSocket = new Socket(
//          ipEndPoint.AddressFamily,
//          SocketType.Stream,
//          ProtocolType.Tcp);

//      senderSocket.NoDelay = false;
//      senderSocket.Connect(ipEndPoint);

//      byte[] buffer = new byte[1024];
//      object[] obj = new object[2];
//      obj[0] = buffer;
//      obj[1] = senderSocket;

//      senderSocket.BeginReceive(
//          buffer,
//          0,
//          buffer.Length,
//          SocketFlags.None,
//          new AsyncCallback(ReceiveCallback),
//          obj);
//    }

//    public void AcceptCallback(IAsyncResult ar) {
//      byte[] buffer = new byte[1024];
//      listener = (Socket)ar.AsyncState;
//      senderSocket = listener.EndAccept(ar);
//      senderSocket.NoDelay = false;

//      object[] obj = new object[2];
//      obj[0] = buffer;
//      obj[1] = senderSocket;

//      senderSocket.BeginReceive(
//          buffer,
//          0,
//          buffer.Length,
//          SocketFlags.None,
//          new AsyncCallback(ReceiveCallback),
//          obj);
//    }

//    public Task<string> ReadLine() {
//      //return Task.Run(() => {
//      //    mutexInput.WaitOne();

//      //    if (receiveException != null) {
//      //        throw new Exception("Failed to receive", receiveException);
//      //    }

//      //    lock (inputLines) {
//      //        string content = inputLines.First();
//      //        inputLines.RemoveAt(0);
//      //        return content;
//      //    }
//      //});
//      return null;
//    }

//    public async Task<T> ReadAs<T>() {
//      //string content = await this.ReadLine();
//      //T obj = JsonConvert.DeserializeObject<T>(content);
//      //return obj;
//      return default(T);
//    }

//    public bool HasData() {
//      return inputLines.Count > 0;
//    }

//    public void Write(string text) {
//      byte[] byteData = Encoding.Unicode.GetBytes(text);
//      senderSocket.Send(byteData);
//    }

//    public void WriteLine(string text) {
//      byte[] byteData = Encoding.Unicode.GetBytes(text + '\n');
//      senderSocket.Send(byteData);
//    }

//    public void ReceiveCallback(IAsyncResult ar) {
//      try {
//        object[] obj = new object[2];
//        obj = (object[])ar.AsyncState;

//        byte[] buffer = (byte[])obj[0];

//        var handler = (Socket)obj[1];

//        StringBuilder content = new StringBuilder();

//        int bytesRead = handler.EndReceive(ar);

//        bool keepReading = bytesRead > 0;

//        while (keepReading) {
//          string partialMessage = Encoding.Unicode.GetString(buffer, 0,
//              bytesRead);
//          keepReading = partialMessage[partialMessage.Length - 1] != '\n';

//          int start = 0;
//          int i = partialMessage.IndexOf('\n');
//          int length = partialMessage.Length;
//          while (i < partialMessage.Length && i > -1) {
//            lock (inputLines) {
//              content.Append(partialMessage.Substring(start, i - start));
//              inputLines.Add(content.ToString());
//              content.Clear();
//              mutexInput.Release(1);
//              //Console.WriteLine(mutexInput.Release(1));
//            }

//            length -= i - start + 1;
//            start = i + 1;
//            i = partialMessage.IndexOf('\n', start);
//          }

//          if (partialMessage.Length > length) {
//            content.Append(partialMessage.Substring(start, length));
//          } else {
//            content.Append(partialMessage);
//          }

//          if (keepReading) {
//            bytesRead = senderSocket.Receive(
//                buffer,
//                0,
//                buffer.Length,
//                SocketFlags.None);
//          }
//        }

//        senderSocket.BeginReceive(
//            buffer,
//            0,
//            buffer.Length,
//            SocketFlags.None,
//            new AsyncCallback(ReceiveCallback),
//            obj);
//      } catch (Exception ex) {
//        receiveException = ex;
//        mutexInput.Release(1);
//      }
//    }

//    public void Dispose() {
//      if (senderSocket != null) {
//        senderSocket.Close();
//        senderSocket.Dispose();
//        senderSocket = null;
//      }

//      if (listener != null) {
//        listener.Close();
//        listener.Dispose();
//        listener = null;
//      }
//    }
//  }
//}
