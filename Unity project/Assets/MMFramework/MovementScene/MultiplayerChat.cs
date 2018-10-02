using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using MMO.Client;
using MMO.Models;

namespace MMO.Client.Examples
{
	public class MultiplayerChat : MonoBehaviour 
	{
		public GameObject gameobjectChat;
		public Text textChat;
		public Button buttonSend;
		public InputField inputText;

		bool connected = false;
		void Start () 
		{
			MultiplayerClient.singleton.AddCallback("OnDisconnect", (M) =>
			{	
				gameobjectChat.SetActive(false);

				buttonSend.onClick.RemoveAllListeners();

				connected = false;
			});
			MultiplayerClient.singleton.AddCallback("ConnectionAproved", (M) =>
			{
				gameobjectChat.SetActive(true);

				buttonSend.onClick.AddListener(SendChatMessage);

				connected = true;
			});
			MultiplayerClient.singleton.AddCallback("Error", (M) => 
			{
				textChat.text += $"\n<color=#ff0000ff>{M.Message}</color>";	
			});
			MultiplayerClient.singleton.AddCallback("Alert", (M) => 
			{
				textChat.text += $"\n<color=#ffa500ff>{M.Message}</color>";		
			});
			MultiplayerClient.singleton.AddCallback("Chat", (M) =>
			{
				textChat.text += $"\n>>{M.Sender}:{M.Message}";
			});
		}
		void SendChatMessage () 
		{
			if(!connected)
				return;
			if(inputText.text.Length <= 1)
				return;
			
			MultiplayerClient.singleton.SendData("Chat", "", "All", inputText.text);
			inputText.text = "";
		}
	}
}