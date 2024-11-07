using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server;

class Program
{
    private static List<string> Messages = new List<string>();

    public static void StartListening(int port)
    {
        IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

        Socket listener = new Socket(
            ipAddress.AddressFamily,
            SocketType.Stream,
            ProtocolType.Tcp);

        try
        {
            listener.Bind(localEndPoint);
            Console.WriteLine($"Сервер слушает на {localEndPoint}");

            listener.Listen(10);
            Console.WriteLine("Ожидание соединения...");

            while (true)
            {
                Socket handler = listener.Accept();
                Console.WriteLine($"Принято соединение от {handler.RemoteEndPoint}");

                Console.WriteLine("Получение данных...");
                byte[] buf = new byte[1024];
                string data = "";
                while (true)
                {
                    int bytesRec = handler.Receive(buf);
                    data += Encoding.UTF8.GetString(buf, 0, bytesRec);
                    if (data.IndexOf("<EOF>") > -1)
                    {
                        break;
                    }
                    else
                    {
                        throw new ArgumentException("Передано неправильное сообщение");
                    }
                }
                Messages.Add(data);

                data = data.Replace("<EOF>", "");
                Console.WriteLine("Полученный текст: {0}", data);

                var result = String.Join('\n', Messages.ToArray());
                byte[] msg = Encoding.UTF8.GetBytes(result);

                handler.Send(msg);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
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
        Console.WriteLine("Запуск сервера...");
        int port = Int32.Parse(args[0]);
        StartListening(port);

        Console.WriteLine("\nНажмите ENTER чтобы выйти...");
        Console.Read();
    }
}