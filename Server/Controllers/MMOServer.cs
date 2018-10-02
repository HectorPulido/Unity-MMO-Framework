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
    public class MMOServer : ServerBase
    {
        protected Dictionary<string, TcpClient> loggedClientsList = new Dictionary<string, TcpClient>();

        protected override void ClientManagement(object o)
        {
            int id = (int)o;
            TcpClient client = clientsList[id];

            bool isConnected = true;

            string nickname = "";

            while (isConnected)
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024];
                try   
                {  
                    int byte_count = stream.Read(buffer, 0, buffer.Length);

                    if (byte_count == 0)
                    {
                        Console.WriteLine($"Client {id} disconnected");
                        BroadcastMessage("Disconnection", nickname, "All", "");
                        isConnected = false;

                        if(loggedClientsList.ContainsKey(nickname))
                        {
                            loggedClientsList.Remove(nickname);
                        }

                        break;
                    }

                    string data = Encoding.ASCII.GetString(buffer, 0, byte_count);
                    MMOMessage recivedMessage = JsonConvert.DeserializeObject<MMOMessage>(data);                

                    if(recivedMessage.Reciever != "Server")
                    {
                        if(!string.IsNullOrEmpty(nickname))
                        {
                            recivedMessage.Sender = nickname;
                            BroadcastMessage(JsonConvert.SerializeObject(recivedMessage));
                            Console.WriteLine(data);
                        }  
                    }
                    else
                    {
                        if(recivedMessage.Type == "Login")
                        {
                            //Comprobate
                            Console.WriteLine($"{0} is trying to start a connection");
                            MMOLogin logData = JsonConvert.DeserializeObject<MMOLogin>(recivedMessage.Message);
                            if(Login(logData.nickname, logData.password))
                            {                                                     
                                nickname = logData.nickname;
                                loggedClientsList.Add(nickname, client);
                                SendMessage("ConnectionAproved", "Server", nickname, "", client);
                            }
                            else
                            {
                                SendMessage("Error", "Server", "", "Incorrect login data", client);                     
                            }                       
                        }
                        else if (recivedMessage.Type == "Register")
                        {
                            MMOLogin logData = JsonConvert.DeserializeObject<MMOLogin>(recivedMessage.Message);
                            if(Register(logData.nickname, logData.password))
                            {
                                SendMessage("Alert", "Server", id.ToString(), "Succedfull register", client);
                            }
                            else
                            {
                                SendMessage("Error", "Server", id.ToString(), "Error ocurred on the register", client);
                            }
                        }
                    }
                }  
                catch  
                {  

                    Console.WriteLine($"Client {id} disconnected in with an error");
                    BroadcastMessage("Disconnection", nickname, "All", "");
                    isConnected = false;

                    if(loggedClientsList.ContainsKey(nickname))
                    {
                        loggedClientsList.Remove(nickname);
                    }
                    break;
                }      
            }

            clientsList.Remove(id);
            client.Client.Shutdown(SocketShutdown.Both);
            client.Close();
        }
        bool Login(string nickname, string password)
        {
            if(String.IsNullOrEmpty(nickname) || string.IsNullOrEmpty(password))
            {
                //nickname or password empty
                return false;
            }
            if(loggedClientsList.ContainsKey(nickname))
            {
                //Other user is already logged with this account
                return false;
            }


            var filter = new BsonDocument("nickname", nickname);
            var f = collections["collectionUsers"].Find(filter).ToList();
            if(f.Count > 0)
            {
                if(f[0].GetValue("password") == password)
                {
                    //CorrectLogin
                    return true;
                }
                else
                {
                    //Incorrect password
                    Console.WriteLine ($"Incorrect Password, real {f[0].GetValue("password")}, yours {password}");
                    return false;
                }
            }
            else
            {
                //Nickname doesn't exist
                Console.WriteLine ("Nickname doesn't exist " + nickname);
                return false;
            }
        }
        bool Register(string nickname, string password)
        {
            if(String.IsNullOrEmpty(nickname) || string.IsNullOrEmpty(password))
            {
            //nickname or password empty
                return false;
            }
            var filter = new BsonDocument("nickname", nickname);
            var f = collections["collectionUsers"].Find(filter).ToList();
            if(f.Count > 0)
            {
                //Could not register
                return false;
            }
            else
            {
                try
                {
                    var document = new BsonDocument
                    {
                        {"nickname", new BsonString(nickname)},
                        {"password", new BsonString(password)}
                    };
                    collections["collectionUsers"].InsertOneAsync(document);
                    return true;
                }
                catch
                {
                    //Error ocurred
                    return false;
                }
            }
        }
    }
}