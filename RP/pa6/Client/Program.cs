using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Client;

class Program
{
    public static void StartClient(string host, int port, string message)
    {
        try
        {
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

            Socket sender = new Socket(
                ipAddress.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            try
            {
                sender.Connect(remoteEP);
                Console.WriteLine($"Удалённый адрес подключения сокета: {sender.RemoteEndPoint}");

                byte[] msg = Encoding.UTF8.GetBytes(message + "<EOF>");
                int bytesSent = sender.Send(msg);

                byte[] buf = new byte[1024];
                string data = "";
                while (true)
                {
                    int bytesRec = sender.Receive(buf);
                    data += Encoding.UTF8.GetString(buf, 0, bytesRec);
                    if (data.IndexOf("<EOF>") > -1)
                    {
                        data = data.Replace("<EOF>", "");
                        break;
                    }
                    else
                    {
                        throw new ArgumentException("Передано неправильное сообщение");
                    }
                }

                Console.WriteLine($"Ответ: {data}");

                sender.Shutdown(SocketShutdown.Both);
                sender.Close();
            }
            catch (ArgumentNullException ane)
            {
                Console.WriteLine($"ArgumentNullException: {ane.Message}");
                Console.WriteLine($"Стек вызовов: {ane.StackTrace}");
            }
            catch (SocketException se)
            {
                Console.WriteLine($"SocketException: {se.Message}");
                Console.WriteLine($"Стек вызовов: {se.StackTrace}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Unexpected exception: {e.Message}");
                Console.WriteLine($"Стек вызовов: {e.StackTrace}");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Исключение: {e.Message}");
            Console.WriteLine($"Стек вызовов: {e.StackTrace}");
        }
    }



    static void Main(string[] args)
    {
        var host = args[0];
        int port = Int32.Parse(args[1]);
        string message = args[2];

        StartClient(host, port, message);
    }
}