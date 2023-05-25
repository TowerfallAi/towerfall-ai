using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TowerfallAi.Data;

namespace TowerfallAi.Core {
  public class AgentConnectionRemote : AgentConnection {
    public RemoteConnection Connection { get; set; }

    public AgentConnectionRemote(int index) : base(index) { }

    public override void Send(string message, int frame) {
      Connection.Write(message);
    }

    public override async Task<Message> ReceiveAsync(TimeSpan timeout = default, CancellationToken cancellationToken = default) {
      return JsonConvert.DeserializeObject<Message>(await Connection.ReadAsync(timeout, cancellationToken));
    }

    public override void Dispose() {
      if (Connection != null) {
        Connection.Dispose();
        Connection = null;
      }
    }
  }
}
