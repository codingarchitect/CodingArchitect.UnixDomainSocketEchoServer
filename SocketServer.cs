using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CodingArchitect.UnixDomainSocketEchoServer
{
  public class SocketServer 
  {
    private const int BufferSize = 4096;
    private readonly EndPoint address;
    private readonly Func<string, string> requestProcessor;

    private Socket listener;
    public SocketServer(
      EndPoint address,
      Func<string, string> requestProcessor = null)
    {
      this.address = address == null ? 
        new UnixDomainSocketEndPoint(GetRandomNonExistingFilePath()) :
        address;
      this.requestProcessor = requestProcessor == null ? Echo : requestProcessor;
    }

    public async void Run()
    {
      var protocolType = address.AddressFamily == AddressFamily.InterNetwork ?
        ProtocolType.Tcp : ProtocolType.Unspecified;
      listener = new Socket(address.AddressFamily, SocketType.Stream, protocolType);
      listener.Bind(address);
      listener.Listen(100);
      Console.WriteLine("Service is now running on '{0}'", address);
      
      while (true) {
        try {
          var client = await listener.AcceptAsync();
          ClientLoop(client);
        }
        catch (Exception ex) {
          Console.WriteLine(ex.Message);
        }
      }
    }

    private async void ClientLoop(Socket client)
    {
      var buffer = new byte[BufferSize];
      string clientEndPoint = client.RemoteEndPoint.ToString();
      Console.WriteLine("Received connection request from {0}", clientEndPoint);
      try 
      {
        using(NetworkStream networkStream = new NetworkStream(client))
        {
          var bufferString = new StringBuilder();
          int byteCount = 0;
          bool hasEndOfTransmissionMarker = false;
          do
          {          
            byteCount = await networkStream.ReadAsync(buffer, 0, buffer.Length);
            if (byteCount > 0)
            {
              hasEndOfTransmissionMarker = buffer[byteCount-1] == 0;
              if (hasEndOfTransmissionMarker)
                bufferString.Append(Encoding.UTF8.GetString(buffer, 0, byteCount-1));
              else
                bufferString.Append(Encoding.UTF8.GetString(buffer, 0, byteCount));  
            }
          }
          while ((hasEndOfTransmissionMarker == false) && (byteCount > 0));
          Console.WriteLine("[Server] Client {0} wrote {1}", clientEndPoint, bufferString);
          var response = requestProcessor(bufferString.ToString());
          await networkStream.WriteAsync(Encoding.UTF8.GetBytes(response + "\0"), 0, response.Length + 1);
        }
        client.Close();
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
        if (client.Connected)
          client.Close();
      }
    }
    public static string Echo(string request)
    {
      return request;
    }

    public void Close()
    {
      try 
      {
        if (listener.Connected)
        {
          listener.Shutdown(SocketShutdown.Both);
        }
        listener.Close();
      }
      catch(Exception ex)
      {
        Console.WriteLine("[Server] Unable to close socket '{0}'", ex);
      }
    }

    private static string GetRandomNonExistingFilePath()
    {
      string result;
      do
      {
        result = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".sock");
      }
      while (File.Exists(result));
      return result;
    }
  }
}
