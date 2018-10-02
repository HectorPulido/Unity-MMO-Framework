using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using CielaSpike;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

using MMO.Client;
using MMO.Models;

namespace MMO.Client
{	
	public class Client : MonoBehaviour 
	{
		public string ipAdress;
		public int port;

		Dictionary<string, Action<MMOMessage>> Callbacks = new Dictionary<string, Action<MMOMessage>>();
		public void Connect(Action<MMOMessage> onConnect, Action<MMOMessage> onDisconnect)
		{		
			AddCallback("OnConnect", onConnect);
			AddCallback("OnDisconnect", onDisconnect);

			stayConnected = true;
			this.StartCoroutineAsync(StartServer());
			recievedDataCallback += (message) =>
			{
				try
				{
					MMOMessage recivedMessage = JsonConvert.DeserializeObject<MMOMessage>(message);
					if(Callbacks.ContainsKey(recivedMessage.Type))
					{
						Callbacks[recivedMessage.Type].Invoke(recivedMessage);
					}
				}
				catch(Exception ex)
				{
					Debug.LogError("Error ocurred trying to read the message " +  message + " the error was " + ex);
				}

			};

		}

		public void Connect()
		{
			Connect(
				(m)=>{print("Connection Stabished");}, 
				(m)=>{print("Connection broken");});
		}

		public void Disconnect()
		{
			stayConnected = false;
		}

		public void AddCallback(string messageType, Action<MMOMessage> message)
		{
			if(Callbacks.ContainsKey(messageType))
			{
				Callbacks[messageType] += message;				
			}
			else
			{
				Callbacks.Add(messageType, message);
			}
		}

		public void DisposeCallback(string messageType)
		{
			if(Callbacks.ContainsKey(messageType))
			{
				Callbacks.Remove(messageType);
			}
			else
			{
				return;
			}
		}

		Action<string> recievedDataCallback;
		bool stayConnected;


		IPAddress ip;
		TcpClient client;
		NetworkStream networkStream;
		IEnumerator StartServer()
		{
			
			yield return Ninja.JumpBack;

			//Start the connection
			ip = IPAddress.Parse(ipAdress);
			client = new TcpClient();
			client.Connect(ip, port);
			networkStream = client.GetStream();

			//Connection stabished, lets get some data
			yield return Ninja.JumpToUnity;
			this.StartCoroutineAsync(ReceiveData(client));
			Callbacks["OnConnect"].Invoke(new MMOMessage());
			yield return Ninja.JumpBack;

			//We must stay connected
			while (stayConnected)
			{				
				yield return null;
			}

			//The connection will broke
			client.Client.Shutdown(SocketShutdown.Send);
			networkStream.Close();
			client.Close();

			yield return Ninja.JumpToUnity;
			Callbacks["OnDisconnect"].Invoke(new MMOMessage());
		}

		IEnumerator ReceiveData(TcpClient client)
		{
			NetworkStream ns = client.GetStream();
			byte[] receivedBytes = new byte[1024];
			int byte_count;
			while ((byte_count = ns.Read(receivedBytes, 0, receivedBytes.Length)) > 0)
			{
				yield return Ninja.JumpToUnity;
				recievedDataCallback(Encoding.ASCII.GetString(receivedBytes, 0, byte_count));
				yield return Ninja.JumpBack;
			}

		}
		public void SendData(string Text)
		{
			if(networkStream == null)
			{
				Debug.LogError("Client is not connected to the server");
				return;
			}

			byte[] buffer = Encoding.ASCII.GetBytes(Text);
			networkStream.Write(buffer, 0, buffer.Length);
		}
		public void SendData(string type, string sender, string reciever, string message)
		{
			var MessageToSend = new MMOMessage();
			MessageToSend.Type = type;
			MessageToSend.Sender = sender;
			MessageToSend.Reciever = reciever;
			MessageToSend.Message = message;

			string output = JsonConvert.SerializeObject(MessageToSend);
			SendData(output);
		}

	}
}

