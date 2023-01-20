﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


namespace Com.CrazyD.FPSProto
{
    public class Launcher : MonoBehaviourPunCallbacks
    {

        public void Awake()
        {

            PhotonNetwork.AutomaticallySyncScene = true;
            

        }

         void Start()
        {
            Connect();
        }

        public override void OnConnectedToMaster()
        {
            Debug.Log("Connected!");
            Join();

            base.OnConnectedToMaster();
        }

        public override void OnJoinedRoom()
        {
            StartGame();

            base.OnJoinedRoom();
        }

        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Create();

            base.OnJoinRandomFailed(returnCode, message);
        }


        public void Connect ()
        {
            Debug.Log("Trying To Connect...");
            PhotonNetwork.GameVersion = "0.0.0";
            PhotonNetwork.ConnectUsingSettings();
           

        }

        public void Join ()
        {
            PhotonNetwork.JoinRandomRoom();
        }

        public void Create ()
        {
            PhotonNetwork.CreateRoom("");
        }

        public void StartGame()
        {
            if(PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                PhotonNetwork.LoadLevel(1);

            }
        }
    }
}
