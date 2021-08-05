using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using UnityEngine.Assertions;
using System.Linq;

public class NetworkManager : MonoBehaviourPunCallbacks
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

    void OnApplicationQuit()
    {
        PhotonNetwork.Disconnect();
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }
}
