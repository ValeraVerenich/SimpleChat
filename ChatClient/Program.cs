using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class Client
{
    private static Socket clientSocket;
    private static bool running = true;
    private static string virtualIP;

    static void Main(string[] args)
    {
        while (true)
        {
            try
            {
                Console.Write("Введите IP клиента: ");
                virtualIP = Console.ReadLine();

                
                if (!IPAddress.TryParse(virtualIP, out _))
                {
                    Console.WriteLine("Некорректный формат IP-адреса. Попробуйте снова.");
                    continue;
                }

                int port;
                while (true)
                {
                    Console.Write("Введите порт сервера: ");
                    string portInput = Console.ReadLine();

                    
                    if (!int.TryParse(portInput, out port) || port < 1 || port > 65535)
                    {
                        Console.WriteLine("Порт должен быть числом от 1 до 65535. Попробуйте снова.");
                        continue;
                    }
                    break;
                }

                string serverIP = "127.0.0.1";
                ConnectToServer(serverIP, port);
                break; 
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                Console.WriteLine("Попробуйте снова.\n");
            }
        }
    }

    private static void ConnectToServer(string ip, int port)
    {
        try
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPAddress localIP = IPAddress.Parse(virtualIP);
            clientSocket.Bind(new IPEndPoint(localIP, 0));

            Console.WriteLine($"Подключение к серверу {ip}:{port} с локального адреса {virtualIP}...");
            clientSocket.Connect(new IPEndPoint(IPAddress.Parse(ip), port));
            Console.WriteLine("Успешное подключение!");

            Thread receiveThread = new Thread(ReceiveMessages);
            receiveThread.IsBackground = true;
            receiveThread.Start();

            while (running)
            {
                string message = Console.ReadLine();
                if (message.ToLower() == "/exit")
                {
                    running = false;
                    break;
                }
                SendMessage(message);
            }
        }
        catch (SocketException sex)
        {
            Console.WriteLine($"Ошибка подключения: {sex.SocketErrorCode} - {sex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка подключения: {ex.Message}");
        }
        finally
        {
            clientSocket?.Shutdown(SocketShutdown.Both);
            clientSocket?.Close();
            Console.WriteLine("Клиент отключен. Нажмите любую клавишу...");
            Console.ReadKey();
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
                    Console.WriteLine($"Получено: {message}");
                }
            }
            catch (SocketException) when (!running)
            {
            }
            catch (Exception)
            {
                if (running)
                {
                    Console.WriteLine("Сервер отключился!");
                    running = false;
                }
                break;
            }
        }
    }
}