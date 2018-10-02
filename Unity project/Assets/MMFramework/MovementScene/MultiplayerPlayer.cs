using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

using MMO.Client;
using MMO.Models;

namespace MMO.Client.Examples
{
	public class MultiplayerPlayer : MonoBehaviour 
	{
		public Rigidbody playerRigidbody;
		public SerializableEvent whenSetLocal;
		public SerializableEvent whenSetNoLocal;

		public float positionChangeBias;
		public float rotationChangeBias;
		public float smoothPositionRate = 10;
		public float smoothRotationRate = 1;

		public float SendRate = 0.1f;


		bool isLocal;
		public bool IsLocal
		{
			get
			{
				return isLocal;
			}
		}

		Vector3 lastPosition;
		Quaternion lastRotation;
		bool IsPrudentToMove
		{
			get
			{
				bool sw = false;

				if(Vector3.Distance(transform.position, lastPosition) > positionChangeBias)
				{
					sw = true;
				}
				if(Quaternion.Angle(transform.rotation, lastRotation) > rotationChangeBias)
				{
					sw = true;
				}

				if(sw)
				{
					lastPosition = transform.position;
					lastRotation = transform.rotation;
				}
				return sw;
			}
		}
		void Start()
		{
			playerRigidbody = GetComponent<Rigidbody>();
		}
		void Update()
		{	
			Interpolate();
		}
		public void SetLocal () 	
		{
			isLocal = true;
			whenSetLocal.Invoke();
			InvokeRepeating(nameof(SendUpdatedPosition), 0, SendRate);	
			SendMessage("Setup", SendMessageOptions.DontRequireReceiver);
		}	
		public void SetNonLocal () 	
		{
			isLocal = false;
			whenSetNoLocal.Invoke();	
			SendMessage("Setup", SendMessageOptions.DontRequireReceiver);
		}	
		
		public void SendUpdatedPosition()
		{
			if(!isLocal)
				return;
			if(!IsPrudentToMove)
				return;

			var playerTransform = MMOTransform.FromTransform(transform);			
			MultiplayerClient.singleton.SendData("OnMovement", "", "All", JsonConvert.SerializeObject(playerTransform));
		}

		Vector3 nextPosition;
		Quaternion nextRotation;
		public void GetUpdatePosition(MMOTransform t)
		{
			if(isLocal)
				return;

			nextPosition = t.GetVector3();
			nextRotation = t.GetQuaternion();
		}

		float LastInterpolationTime;
		void Interpolate()
		{
			if(isLocal)
				return;
			transform.position = Vector3.Lerp(transform.position, nextPosition, Time.deltaTime * smoothPositionRate);
			transform.rotation = Quaternion.Lerp(transform.rotation, nextRotation, Time.deltaTime * smoothRotationRate);
		}
	}
}