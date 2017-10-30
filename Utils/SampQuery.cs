

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Launcher.Utils
{
  internal class SampQuery : IDisposable
  {
    private Socket sock;
    private EndPoint endPoint;
    private System.Threading.Timer timer;
    private SampUtils.E_OPCODE opcode;
    private byte[] buffer;
    private int timeoutflag;
    private Action<string[], object> callback;
    private DateTime timestamp;

    public static string GetServerAddressByDomain(string hostname)
    {
      string empty = string.Empty;
      try
      {
        return Dns.GetHostAddresses(hostname)[0].ToString();
      }
      catch
      {
        return string.Empty;
      }
    }

    public object state { get; }

    public SampQuery(object state = null)
    {
      this.state = state;
      this.sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
      this.buffer = new byte[512];
    }

    private void OnSend(IAsyncResult res)
    {
      if (Interlocked.CompareExchange(ref this.timeoutflag, 1, 0) == 1)
        return;
      if (this.timer != null)
        this.timer.Change(-1, 5000);
      this.timestamp = DateTime.Now;
      this.sock.EndSendTo(res);
      this.Receive();
    }

    private void Receive()
    {
      this.sock.BeginReceiveFrom(this.buffer, 0, this.buffer.Length, SocketFlags.None, ref this.endPoint, new AsyncCallback(this.OnReceive), (object) null);
    }

    private void OnReceive(IAsyncResult res)
    {
      if (Interlocked.CompareExchange(ref this.timeoutflag, 2, 1) == 2)
        return;
      if (this.timer != null)
        this.timer.Dispose();
      try
      {
        this.sock.EndReceiveFrom(res, ref this.endPoint);
        if (this.buffer.Length == 0)
          return;
        string[] strArray;
        if (this.opcode == SampUtils.E_OPCODE.SERVER_PING)
          strArray = new string[1]
          {
            DateTime.Now.Subtract(this.timestamp).Milliseconds.ToString()
          };
        else
          strArray = SampUtils.__parseResponse(this.buffer);
        this.callback(strArray, this.state);
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.StackTrace);
      }
      finally
      {
        this.Dispose();
      }
    }

    private void OnTimer(object obj)
    {
      if (Interlocked.CompareExchange(ref this.timeoutflag, 3, 2) == 3)
        return;
      this.Dispose();
    }

    public void Send(string hostname, int port, SampUtils.E_OPCODE opcode, Action<string[], object> callback)
    {
      IPAddress hostAddress = Dns.GetHostAddresses(hostname)[0];
      this.endPoint = (EndPoint) new IPEndPoint(hostAddress, port);
      hostname = hostAddress.ToString();
      byte[] buffer = SampUtils.__buildPacket(hostname, port, opcode);
      IAsyncResult asyncResult = this.sock.BeginSendTo(buffer, 0, buffer.Length, SocketFlags.None, this.endPoint, new AsyncCallback(this.OnSend), (object) null);
      this.opcode = opcode;
      this.callback = callback;
      if (asyncResult.IsCompleted)
        return;
      this.timer = new System.Threading.Timer(new TimerCallback(this.OnTimer), (object) null, 1000, -1);
    }

    public void Dispose()
    {
      this.buffer = (byte[]) null;
      if (this.timer != null)
        this.timer.Dispose();
      if (this.sock == null)
        return;
      this.sock.Close();
      this.sock.Dispose();
    }
  }
}
