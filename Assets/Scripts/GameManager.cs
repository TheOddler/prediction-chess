using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using UnityEngine.Assertions;
using System.Linq;
using System.Collections.Generic;

public class GameManager : MonoBehaviourPunCallbacks
{
    static GameManager _instance;
    public static GameManager Instance => _instance;

    void Awake()
    {
        Assert.IsNull(_instance);
        _instance = this;

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

    public void ResolveBattle()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("ResolveBattle()");

            var pieces = Piece.All.ToArray();
            int nbPieces = pieces.Length;

            for (int i = 0; i < pieces.Length - 1; ++i)
            {
                for (int j = i + 1; j < pieces.Length; ++j)
                {
                    Piece first = pieces[i];
                    Piece second = pieces[j];

                    // Do fighting if needed
                    if (PiecesWillEndInSamePosition(first, second, out var fightBoardPosition))
                    {
                        Fight(first, second, fightBoardPosition.worldPosition, false);
                    }

                    if (PiecesWillSwapPosition(first, second))
                    {
                        if (first.Move == first.Prediction || second.Move == second.Prediction)
                        {
                            Fight(first, second, (first.Position.worldPosition + second.Position.worldPosition) / 2, true);
                        }
                    }
                }
            }

            foreach (var piece in pieces)
            {
                // Move the pieces
                if (piece.Move != null && !piece.IsDead) piece.SetPos((BoardPosition)piece.Move);

                // Reset
                piece.ResetMove();
                piece.ResetPrediction();
            }

            // Reset players
            Player.LocalPlayer.SetIsDone(false);
            Player.RemotePlayer.SetIsDone(false);
        }
    }

    private void Fight(Piece first, Piece second, Vector3 fightPosition, bool fightingHalfway)
    {
        if (first.Power == second.Power)
        {
            first.Die(fightPosition, fightingHalfway);
            second.Die(fightPosition, fightingHalfway);
        }
        else if (first.Power > second.Power)
        {
            second.Die(fightPosition, fightingHalfway);
        }
        else
        {
            first.Die(fightPosition, fightingHalfway);
        }
    }

    private bool PiecesWillEndInSamePosition(Piece first, Piece second, out BoardPosition finalPositon)
    {
        // They should fight if they end up in the same position
        BoardPosition finalPositionFirst = first.Move ?? first.Position;
        BoardPosition finalPositionSecond = second.Move ?? second.Position;
        finalPositon = finalPositionFirst;
        return finalPositionFirst == finalPositionSecond;
    }

    private bool PiecesWillSwapPosition(Piece first, Piece second)
    {
        return first.Move == second.Position && second.Move == first.Position;
    }
}
