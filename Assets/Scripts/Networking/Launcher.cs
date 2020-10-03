using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Launcher : MonoBehaviourPunCallbacks
{
    [SerializeField]
    string _gameVersion = "1";

    [SerializeField]
    byte _maxPlayersPerRoom = 2;

    [SerializeField]
    GameObject _controlPanel;

    [SerializeField]
    GameObject _progressLabel;

    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true; // Allows us to use PhotonNetwork.LoadLevel() which will load a new scene for all connected clients
    }

    void Start()
    {
        SetInterfaceConnected(PhotonNetwork.IsConnected);

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = _gameVersion;
        }
    }

    public void JoinRandomRoom()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.JoinRandomRoom();

            SetInterfaceConnected(false);
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("OnConnectedToMaster()");

        SetInterfaceConnected(true);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarningFormat(" OnDisconnected({0})", cause);

        SetInterfaceConnected(true);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogFormat("OnJoinRandomFailed({0}, {1})", returnCode, message);

        // Random join failed, maybe all are full, so let's create our own
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = _maxPlayersPerRoom });
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom()");
    }

    private void SetInterfaceConnected(bool connected)
    {
        _controlPanel.SetActive(connected);
        _progressLabel.SetActive(!connected);
    }
}
