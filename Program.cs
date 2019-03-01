using System;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace CodingArchitect.UnixDomainSocketEchoServer
{
  class Program
  {
    static void Main(string[] args)
    {
      // int port = 6789;        
      SocketServer server = new SocketServer(
        // new IPEndPoint(IPAddress.Parse("0.0.0.0"), port)
        new UnixDomainSocketEndPoint(GetRandomNonExistingFilePath())
      );
      try
      {
        server.Run();
        Console.WriteLine("Press any key to quit");
        Console.ReadKey();    
      }
      finally
      {
          server.Close();
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
