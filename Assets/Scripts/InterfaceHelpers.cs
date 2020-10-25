using Photon.Pun;
using UnityEngine;

public class InterfaceHelpers : MonoBehaviour
{
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void SetLocalPlayerDone(bool isDone)
    {
        Player.LocalPlayer.SetIsDone(isDone);
    }

    public void ResetTurn()
    {
        Player.LocalPlayer.ResetTurn();
    }
}