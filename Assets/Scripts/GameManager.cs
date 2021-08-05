using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using UnityEngine.Assertions;
using System.Linq;

public class GameManager : MonoBehaviourPunCallbacks
{
    static GameManager _instance;
    public static GameManager Instance => _instance;

    void Awake()
    {
        Assert.IsNull(_instance);
        _instance = this;
    }

    void Start()
    {
        RequestDataSync();
    }

    public void RequestDataSync()
    {
        photonView.RPC(nameof(RPCRequestDataSync), RpcTarget.MasterClient); // Master client is considered to have the truth
    }

    [PunRPC]
    private void RPCRequestDataSync()
    {
        Debug.Log("RPCRequestDataSync");
        foreach (var netObject in GetComponents<ISyncsData>())
        {
            netObject.PushData();
            Debug.Log("PushData");
        }
    }

    public void HandleBothPlayersDone()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            ResolveTurn();
        }
    }

    private void ResolveTurn()
    {
        Debug.Log("ResolveBattle()");

        var pieces = Piece.AllAlive.ToArray();
        int nbPieces = pieces.Length;

        foreach (var piece in Piece.AllDead)
        {
            // Do this before the battles, so none of the newly died pieces get this
            piece.ResolveTurnDead();
        }

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

        foreach (var piece in Piece.AllAlive)
        {
            piece.ResolveTurnSurvived();
        }

        // Reset players
        Player.LocalPlayer.SetIsDone(false);
        Player.RemotePlayer.SetIsDone(false);
    }

    private void Fight(Piece first, Piece second, Vector3 fightPosition, bool fightingHalfway)
    {
        if (first.Power == second.Power)
        {
            first.ResolveTurnDied(fightPosition, fightingHalfway);
            second.ResolveTurnDied(fightPosition, fightingHalfway);
        }
        else if (first.Power > second.Power)
        {
            second.ResolveTurnDied(fightPosition, fightingHalfway);
        }
        else
        {
            first.ResolveTurnDied(fightPosition, fightingHalfway);
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
