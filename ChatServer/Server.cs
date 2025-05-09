using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Server
{
    private static readonly List<Socket> clientSockets = new List<Socket>();
    private static Socket serverSocket;

    static void Main(string[] args)
    {
        Console.Write("Введите IP-адрес для сервера: ");
        string ip = Console.ReadLine();
        Console.Write("Введите порт: ");
        int port = int.Parse(Console.ReadLine());

        SetupServer(ip, port);
    }

    private static void SetupServer(string ip, int port)
    {
        try
        {
            IPAddress ipAddress = IPAddress.Parse(ip);
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                serverSocket.Bind(new IPEndPoint(ipAddress, port));
                serverSocket.Listen(10);
                Console.WriteLine($"Сервер запущен на {ip}:{port}");
            }
            catch (SocketException ex)
            {
                Console.WriteLine($"Ошибка: Порт {port} уже используется или недоступен. {ex.Message}");
                return;
            }

            Thread acceptThread = new Thread(AcceptClients);
            acceptThread.Start();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка настройки сервера: {ex.Message}");
        }
    }

    private static void AcceptClients()
    {
        while (true)
        {
            try
            {
                Socket client = serverSocket.Accept();
                clientSockets.Add(client);
                Console.WriteLine("Новый клиент подключился!");

                Thread clientThread = new Thread(() => HandleClient(client));
                clientThread.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка принятия клиента: {ex.Message}");
            }
        }
    }

    private static void HandleClient(Socket clientSocket)
    {
        byte[] buffer = new byte[1024];
        while (true)
        {
            try
            {
                int bytesReceived = clientSocket.Receive(buffer);
                if (bytesReceived == 0) break;

                string message = Encoding.UTF8.GetString(buffer, 0, bytesReceived);
                Console.WriteLine($"Получено: {message}");

                SendMessage(message, clientSocket);
            }
            catch (Exception)
            {
                clientSockets.Remove(clientSocket);
                clientSocket.Close();
                Console.WriteLine("Клиент отключился");
                break;
            }
        }
    }

    private static void SendMessage(string message, Socket sender)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        foreach (Socket client in clientSockets)
        {
            if (client != sender)
            {
                try
                {
                    client.Send(data);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка отправки: {ex.Message}");
                }
            }
        }
    }
}