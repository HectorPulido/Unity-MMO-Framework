using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using MMO.Client;
using MMO.Models;

namespace MMO.Client.Examples
{
	public class MultiplayerClient : Client 
	{
		public GameObject playerPrefab;

		Dictionary<string, Transform> otherPlayers = new Dictionary<string, Transform>();
		public Dictionary<string, Transform> OtherPlayers
		{
			get
			{
				return otherPlayers;
			}
		}

		Transform player;
		public Transform Player 
		{
			get
			{
				return player;
			}
		}

		string id = "";
		public string Id 
		{
			get
			{
				return id;
			}
		}

		public static MultiplayerClient singleton;

		void Awake()
		{
			if(singleton != null)
			{
				Destroy(gameObject);
				return;
			}
			singleton = this;


			Connect((m)=>
			{
				print("Connection Stabished");
			}, 
			(m)=>
			{
				print("Connection broken");
			});

			AddCallback("Disconnection", PlayerDisconnected);
			AddCallback("OnMovement", OnMovement);

			AddCallback("Error", (M) => 
			{
				Debug.LogError("Error from the server: " + M.Message);			
			});
			AddCallback("Alert", (M) => 
			{
				Debug.LogWarning("Alert from the server: " + M.Message);		
			});
			AddCallback("ConnectionAproved", (M) => 
			{
				id = M.Reciever;
				Debug.LogWarning("Connection Aproved, you'r in " + id);
				//Create the player	
				CreateAPlayer(M, true);		
			});

		}

		public void OnMovement(MMOMessage M)
		{
			if(M.Sender == id.ToString())
			{
				return;
			}			
			else
			{
				if(!otherPlayers.ContainsKey(M.Sender))
				{
					//Create and move
					CreateAPlayer(M, false);
				}
				var t = JsonConvert.DeserializeObject<MMOTransform>(M.Message);
				otherPlayers[M.Sender].SendMessage(nameof(MultiplayerPlayer.GetUpdatePosition), t);
			}
		}

		public void PlayerDisconnected(MMOMessage M)
		{
			if(otherPlayers.ContainsKey(M.Sender))
			{
				Destroy(otherPlayers[M.Sender].gameObject);
				otherPlayers.Remove(M.Sender);
			}
		}

		public void CreateAPlayer(MMOMessage M, bool isLocal)
		{
			player = Instantiate(playerPrefab).transform;
			player.SendMessage(isLocal?nameof(MultiplayerPlayer.SetLocal):nameof(MultiplayerPlayer.SetNonLocal), SendMessageOptions.DontRequireReceiver);							
			if(!isLocal)
				otherPlayers.Add(M.Sender, player);
		}


	}

}

