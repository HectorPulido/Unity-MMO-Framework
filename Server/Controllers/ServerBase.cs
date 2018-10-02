using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using Newtonsoft.Json;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

using MMO.Models;

namespace MMO.Server
{
    public class ServerBase
    {

        protected Dictionary<int, TcpClient> clientsList = new Dictionary<int, TcpClient>();
        protected MongoClient clientDb;
        protected IMongoDatabase database;
        protected Dictionary<string, IMongoCollection<BsonDocument>> collections = new Dictionary<string, IMongoCollection<BsonDocument>>();



        public void Start(string mongoUri, string databaseName, string[] collectionsNames, int port)
        {
            int count = 1;

            clientDb = new MongoClient(mongoUri);
            database = clientDb.GetDatabase(databaseName);

            foreach(var col in collectionsNames)
            {
                collections.Add(col, database.GetCollection<BsonDocument>(col));
            }

            TcpListener ServerSocket = new TcpListener(IPAddress.Any, port);
            ServerSocket.Start();
            Console.WriteLine("Server started!");

            while (true)
            {
                TcpClient client = ServerSocket.AcceptTcpClient();
                clientsList.Add(count, client);
                Console.WriteLine("Someone connected!!");

                Thread t = new Thread(ClientManagement);
                t.Start(count);
                count++;
            }
        }
        protected virtual void ClientManagement(object o)
        {
            
        }
        

        protected void BroadcastMessage(string data)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(data + Environment.NewLine);

            foreach (TcpClient c in clientsList.Values)
            {
                SendMessage(buffer, c);
            }
        }

        protected void SendMessage(byte[] buffer, TcpClient c)
        {
            try
            {
                NetworkStream stream = c.GetStream();
                stream.Write(buffer, 0, buffer.Length);
            }
            catch
            {
                Console.WriteLine($"Message could not be sent");
            }

        }
        protected void SendMessage(string data, TcpClient c)
        {
            try
            {
                byte[] buffer = Encoding.ASCII.GetBytes(data + Environment.NewLine);
                SendMessage(buffer, c);
            }
            catch
            {
                Console.WriteLine($"Message could not be sent");
            }

        }
        protected void SendMessage(string type, string sender, string reciever, string message, TcpClient c)
        {
            var m = new MMOMessage();
            m.Sender = sender;
            m.Type = type;
            m.Reciever = reciever;
            m.Message = message;
            SendMessage(JsonConvert.SerializeObject(m), c);
        }
        protected void BroadcastMessage(string type, string sender, string reciever, string message)
        {
            var m = new MMOMessage();
            m.Sender = sender;
            m.Type = type;
            m.Reciever = reciever;
            m.Message = message;
            BroadcastMessage(JsonConvert.SerializeObject(m));
        }
    }

}
