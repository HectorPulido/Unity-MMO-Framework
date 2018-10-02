using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

using MMO.Client;
using MMO.Models;

namespace MMO.Client.Examples
{
	public class MultiplayerLogin : MonoBehaviour {

		public InputField inputPassword;
		public InputField inputNickname;
		public Button buttonLogin;
		public Button buttonRegister;

		public GameObject objectLogin;
		public Text textAlert;

		void Start () 
		{
			//ButtonsLogic
			buttonLogin.onClick.AddListener(Login);
			buttonRegister.onClick.AddListener(Register);

			//Callbacks

			MultiplayerClient.singleton.AddCallback("OnDisconnect", (M) =>
			{	
				objectLogin.SetActive(true);
				textAlert.text = $"<color=#ff0000ff>Disconnected from the server</color>";
			});
			MultiplayerClient.singleton.AddCallback("ConnectionAproved", (M) =>
			{
				objectLogin.SetActive(false);
			});
			MultiplayerClient.singleton.AddCallback("Error", (M) => 
			{
				textAlert.text = $"<color=#ff0000ff>{M.Message}</color>";		
			});
			MultiplayerClient.singleton.AddCallback("Alert", (M) => 
			{
				textAlert.text = $"<color=#ffa500ff>{M.Message}</color>";		
			});
		}
		
		public void Login()
		{
			var LoginInfo = new MMOLogin();
			LoginInfo.nickname = inputNickname.text;
			LoginInfo.password = inputPassword.text;

			MultiplayerClient.singleton.SendData("Login", "", "Server", JsonConvert.SerializeObject(LoginInfo));

		}
		public void Register()
		{
			var LoginInfo = new MMOLogin();
			LoginInfo.nickname = inputNickname.text;
			LoginInfo.password = inputPassword.text;

			MultiplayerClient.singleton.SendData("Register", "", "Server", JsonConvert.SerializeObject(LoginInfo));

		}
	}
}