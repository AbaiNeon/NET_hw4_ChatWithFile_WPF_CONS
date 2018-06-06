using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace server
{
    class Program
    {
        public static List<Client> clients = new List<Client>();

        static void Main(string[] args)
        {
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            int port = 3535;
            IPEndPoint endPoint = new IPEndPoint(ip, port);

            TcpListener server = new TcpListener(endPoint);
            server.Start();

            while (true)
            {
                //прием сообщения
                TcpClient tcpClient = server.AcceptTcpClient();
                NetworkStream stream = tcpClient.GetStream();

                Task task = new Task(() => Proccess(stream));
                task.Start();
            }
        }

        private static void Proccess(NetworkStream stream)
        {
            byte[] data = new byte[1024];
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            do
            {
                bytes = stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Default.GetString(data, 0, bytes));
            }
            while (stream.DataAvailable);

            string json = builder.ToString();
            string json2 = "";
            Message message = null;

            if (json.Contains("&image#"))
            {
                //string imageString = json.Split(new[] { "&image#" }, StringSplitOptions.None)[1];

                json2 = json.Split(new[] { "&image#" }, StringSplitOptions.None)[0];

                message = JsonConvert.DeserializeObject<Message>(json2);
            }
            else
            {
                message = JsonConvert.DeserializeObject<Message>(json);
            }
           
            Client client = clients.FirstOrDefault(c => c.Name == message.From);
            if (client == null)
            {
                clients.Add(new Client() { Name = message.From, Stream = stream });
            }

            //отправка ответа
            BroadcastMessage(json);
        }

        private static void BroadcastMessage(string json)
        {
            foreach (var item in clients)
            {
                item.Stream.Write(Encoding.Default.GetBytes(json), 0, Encoding.Default.GetBytes(json).Length);
            }
        }
    }
}
