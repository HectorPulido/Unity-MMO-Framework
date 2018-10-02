using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

using MMO.Client;
using MMO.Models;

namespace MMO.Client.Examples
{
	public class MultiplayerPlayerAnimator : MonoBehaviour 
	{
		Animator animator;
		MultiplayerPlayer multiplayerPlayer;
		public string[] intsVariablesToSerialize;
		public string[] boolsVariablesToSerialize;
		public string[] floatsVariablesToSerialize;
		//Triggers are little bit different ...
		public float SendRate = 0.1f;

		void Awake () 
		{
			animator = GetComponent<Animator>();
			multiplayerPlayer = GetComponent<MultiplayerPlayer>();
		}
		void Setup()
		{
			if(multiplayerPlayer.IsLocal)
			{
				InvokeRepeating(nameof(SendAnimations), 0, SendRate);
			}
			else
			{
				animator.applyRootMotion = false;
				MultiplayerClient.singleton.AddCallback("OnAnimation", GetAnimations);
			}
		}

		void GetAnimations(MMOMessage M)
		{
			if(multiplayerPlayer.IsLocal)
				return;
			if(!MultiplayerClient.singleton.OtherPlayers.ContainsKey(M.Sender))
				return;
			if(MultiplayerClient.singleton.OtherPlayers[M.Sender] != transform)
				return;

			Dictionary<string, string>[] Variables = JsonConvert.DeserializeObject<Dictionary<string, string>[]>(M.Message);
			
			Dictionary<string, string> intsVariables = Variables[0];
			Dictionary<string, string> boolsVariables = Variables[1];
			Dictionary<string, string> floatsVariables = Variables[2];

			foreach(var i in intsVariables.Keys)
			{
				animator.SetInteger(i, int.Parse(intsVariables[i]));
			}
			foreach(var b in boolsVariables.Keys)
			{
				animator.SetBool(b, boolsVariables[b] == "1" ? true : false);
			}
			foreach(var f in floatsVariables.Keys)
			{
				animator.SetFloat(f, float.Parse(floatsVariables[f]));
			}	
		}

		string lastSerializedVariables;
		void SendAnimations()
		{
			if(!multiplayerPlayer.IsLocal)
				return;

			Dictionary<string, string> intsVariables = new Dictionary<string, string>();
			Dictionary<string, string> boolsVariables = new Dictionary<string, string>();
			Dictionary<string, string> floatsVariables = new Dictionary<string, string>();

			foreach(var i in intsVariablesToSerialize)
			{
				intsVariables.Add(i, animator.GetInteger(i).ToString());
			}
			foreach(var b in boolsVariablesToSerialize)
			{
				boolsVariables.Add(b, animator.GetBool(b) ? "1": "0");
			}
			foreach(var f in floatsVariablesToSerialize)
			{
				floatsVariables.Add(f, animator.GetFloat(f).ToString("F4"));
			}			

			Dictionary<string, string>[] Variables = new Dictionary<string, string>[3];
			Variables[0] = intsVariables;
			Variables[1] = boolsVariables;
			Variables[2] = floatsVariables;

			string serializedVariables = JsonConvert.SerializeObject(Variables);
			if(serializedVariables == lastSerializedVariables)
				return;

			MultiplayerClient.singleton.SendData("OnAnimation", "", "All", serializedVariables);
			lastSerializedVariables = serializedVariables;

		}
		
	}
}