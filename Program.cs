using System;

namespace CodingArchitect.UnixDomainSocketEchoServer
{
  class Program
  {
    static void Main(string[] args)
    {
      int port = 6789;
      SocketServer server = new SocketServer(port);
      server.Run();
      Console.WriteLine("Press any key to quit");
      Console.ReadKey();
    }
  }
}
