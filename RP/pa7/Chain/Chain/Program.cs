using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

class WaveAlgorithm
{
    static void Main(string[] args)
    {
        int listeningPort = int.Parse(args[0]);
        string nextHost = args[1];
        int nextPort = int.Parse(args[2]);
        bool isInitiator = args.Length > 3 && args[3] == "true";

        IPAddress ipAddress = IPAddress.Any;

        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, listeningPort);

        Socket listenerSocket = new Socket(
            ipAddress.AddressFamily,
            SocketType.Stream,
            ProtocolType.Tcp);

        try
        {
            listenerSocket.Bind(localEndPoint);
            listenerSocket.Listen(10);

            int localX = int.Parse(Console.ReadLine());
            int receivedY;

            if (isInitiator)
            {
                SendValue(nextHost, nextPort, localX);
            }

            Thread.Sleep(500);
            Socket handlerSocket = listenerSocket.Accept();
            byte[] buffer = new byte[1024];
            int bytesReceived = handlerSocket.Receive(buffer);
            receivedY = int.Parse(Encoding.UTF8.GetString(buffer, 0, bytesReceived));

            localX = Math.Max(localX, receivedY);
            SendValue(nextHost, nextPort, localX);
            Thread.Sleep(500);

            if (isInitiator)
            {
                Console.WriteLine(localX);
                handlerSocket.Shutdown(SocketShutdown.Both);
                handlerSocket.Close();
            }
            else
            {
                handlerSocket = listenerSocket.Accept();
                bytesReceived = handlerSocket.Receive(buffer);
                localX = int.Parse(Encoding.UTF8.GetString(buffer, 0, bytesReceived));

                SendValue(nextHost, nextPort, localX);
                Thread.Sleep(500);

                Console.WriteLine(localX);
                handlerSocket.Shutdown(SocketShutdown.Both);
                handlerSocket.Close();
            }

            listenerSocket.Close();
            
           
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }

    }

    static void SendValue(string host, int port, int value)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(value.ToString());

        IPAddress ipAddress = IPAddress.Any;

        Socket senderSocket = new Socket(
            ipAddress.AddressFamily,
            SocketType.Stream,
            ProtocolType.Tcp);

        try
        {
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse(host), port);

            senderSocket.Connect(localEndPoint);
            senderSocket.Send(buffer);
            senderSocket.Shutdown(SocketShutdown.Both);
            senderSocket.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }
}