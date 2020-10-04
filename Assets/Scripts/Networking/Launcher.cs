using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;

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
        SetInterfaceConnected(true);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Application.Quit();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        // Random join failed, maybe all are full, so let's create our own
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = _maxPlayersPerRoom });
    }

    public override void OnJoinedRoom()
    {
        SceneManager.LoadScene(1); // PhotonNetwork.LoadLevel()
    }

    private void SetInterfaceConnected(bool connected)
    {
        _controlPanel.SetActive(connected);
        _progressLabel.SetActive(!connected);
    }
}
