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
    private readonly int port;
    private readonly IPAddress address;
    private readonly Func<string, string> requestProcessor;
    public SocketServer(
      int port,
      IPAddress address = null,
      Func<string, string> requestProcessor = null)
    {
      this.port = port;
      this.address = address == null ? IPAddress.Parse("0.0.0.0") : address;
      this.requestProcessor = requestProcessor == null ? Echo : requestProcessor;
    }

    public async void Run()
    {
      Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
      listener.Bind(new IPEndPoint(address, port));
      listener.Listen(100);
      Console.Write("Service is now running");
      Console.WriteLine(" on port " + this.port);
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
          while (true) 
          {
            var byteCount = await networkStream.ReadAsync(buffer, 0, buffer.Length);
            if (byteCount > 0)
            {
              var request = Encoding.UTF8.GetString(buffer, 0, byteCount);
              Console.WriteLine("[Server] Client {0} wrote {1}", clientEndPoint, request);
              var response = requestProcessor(request);
              await networkStream.WriteAsync(Encoding.UTF8.GetBytes(response), 0, response.Length);
            }
            else
              break; // Client closed connection
          }
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
  }
}
