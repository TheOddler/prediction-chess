using Photon.Pun;
using UnityEngine;

public class InterfaceHelpers : MonoBehaviour
{
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void SetLocalPlayerDone(bool done)
    {
        Player.LocalPlayer.IsDone = done;
    }
}