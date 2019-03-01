using System;
using System.Net;

namespace CodingArchitect.UnixDomainSocketEchoServer
{
  class Program
  {
    static void Main(string[] args)
    {
      int port = 6789;
      SocketServer server = new SocketServer(
        new IPEndPoint(IPAddress.Parse("0.0.0.0"), port)
      );
      server.Run();
      Console.WriteLine("Press any key to quit");
      Console.ReadKey();
    }
  }
}
