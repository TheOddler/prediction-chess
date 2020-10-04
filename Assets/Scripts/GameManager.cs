using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviourPunCallbacks
{
    void Awake()
    {
        CustomTypeSerialization.RegisterTypes();

        if (Application.isEditor && SceneManager.GetActiveScene().buildIndex != 0 && !PhotonNetwork.IsConnected)
        {
            PhotonNetwork.OfflineMode = true;
            PhotonNetwork.JoinRandomRoom();
        }
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }
}
