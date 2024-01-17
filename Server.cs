using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using SetDz3;

namespace SetDz3
{
    internal class Server
    {
        private static bool exitRequested = false;
        private static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public static async Task AcceptMsg()
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, 0);
            UdpClient udpClient = new UdpClient(16874);
            Console.WriteLine("Сервер ожидает сообщения. Для завершения нажмите клавишу...");

            // Запустим задачу для ожидания нажатия клавиши
            Task exitTask = Task.Run(() =>
            {
                Console.ReadKey();
                exitRequested = true;
                cancellationTokenSource.Cancel();
            });

            while (!exitRequested)
            {
                try
                {
                    var data = await udpClient.ReceiveAsync();
                    string data1 = Encoding.UTF8.GetString(data.Buffer);

                    CancellationToken token = cancellationTokenSource.Token;
                    await Task.Run(async () =>
                    {
                        if (token.IsCancellationRequested)
                        {
                            return;
                        }

                        Message msg = Message.FromJson(data1);
                        Console.WriteLine(msg.ToString());
                        Message responseMsg = new Message("Server", "Message accept on serv!");
                        string responseMsgJs = responseMsg.ToJson();
                        byte[] responseData = Encoding.UTF8.GetBytes(responseMsgJs);
                        await udpClient.SendAsync(responseData, responseData.Length, ep);
                    }, token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
            // Дождитесь завершения задачи по нажатию клавиши
            exitTask.Wait();
        }
    }
}



