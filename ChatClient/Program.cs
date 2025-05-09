using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Client
{
    private static Socket clientSocket;
    private static bool running = true;

    static void Main(string[] args)
    {
        Console.Write("Введите IP-адрес сервера: ");
        string ip = Console.ReadLine();
        Console.Write("Введите порт сервера: ");
        int port = int.Parse(Console.ReadLine());

        ConnectToServer(ip, port);
    }

    private static void ConnectToServer(string ip, int port)
    {
        try
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSocket.Connect(new IPEndPoint(IPAddress.Parse(ip), port));
            Console.WriteLine("Вы подключились к серверу!");

            Thread receiveThread = new Thread(ReceiveMessages);
            receiveThread.Start();


            while (running)
            {
                string message = Console.ReadLine();
                SendMessage(message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка подключения: {ex.Message}");
        }
        finally
        {
            clientSocket?.Close();
        }
    }

    private static void SendMessage(string message)
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            clientSocket.Send(data);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка отправки: {ex.Message}");
            running = false;
        }
    }

    private static void ReceiveMessages()
    {
        byte[] buffer = new byte[1024];
        while (running)
        {
            try
            {
                int bytesReceived = clientSocket.Receive(buffer);
                if (bytesReceived > 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesReceived);
                    Console.WriteLine($"Сообщение от другого клиента: {message}");
                }
            }
            catch (Exception)
            {
                if (running)
                {
                    Console.WriteLine("Потеряно соединение с сервером");
                    running = false;
                }
                break;
            }
        }
    }
}