using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Piece : MonoBehaviourPun
{
    [SerializeField]
    ChessColor _color;
    public ChessColor Color => _color;

    BoardPosition _pos;
    BoardPosition? _move = null;
    BoardPosition? _prediction = null;

    LineRenderer _lineRenderer;

    void Awake()
    {
        _pos = new BoardPosition(transform.position);
        transform.position = _pos.worldPosition;

        _lineRenderer = GetComponent<LineRenderer>();
    }

    void Start()
    {
        UpdateLineRenderer();
    }

    public bool IsMine()
    {
        return Player.LocalNetworkPlayerColor() == _color;
    }

    public void SetMoveOrPrediction(Vector3 worldPos)
    {
        if (IsMine())
        {
            photonView.RPC("SetMoveRpc", RpcTarget.All, worldPos);
        }
        else
        {
            photonView.RPC("SetPredictionRpc", RpcTarget.All, worldPos);
        }
    }

    [PunRPC]
    private void SetMoveRpc(Vector3 worldPos)
    {
        var move = new BoardPosition(worldPos);
        if (move == _pos)
        {
            _move = null;
        }
        else
        {
            _move = move;
        }
        UpdateLineRenderer();
    }

    [PunRPC]
    private void SetPredictionRpc(Vector3 worldPos)
    {
        var prediction = new BoardPosition(worldPos);
        if (prediction == _pos)
        {
            _prediction = null;
        }
        else
        {
            _prediction = prediction;
        }
        UpdateLineRenderer();
    }

    void UpdateLineRenderer()
    {
        if (_move != null)
        {
            Vector3 pos = _pos.worldPosition;
            Vector3 move = ((BoardPosition)_move).worldPosition;
            pos.y = move.y = 0.2f;
            _lineRenderer.SetPositions(new[] { pos, move });
        }
        else if (_prediction != null)
        {
            Vector3 pos = _pos.worldPosition;
            Vector3 prediction = ((BoardPosition)_prediction).worldPosition;
            pos.y = prediction.y = 0.2f;
            _lineRenderer.SetPositions(new[] { pos, prediction });
        }
        else
        {
            var hidden = new Vector3(0, -1, 0);
            _lineRenderer.SetPositions(new[] { hidden, hidden });
        }
    }
}
