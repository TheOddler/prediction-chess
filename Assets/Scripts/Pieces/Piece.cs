using Photon.Pun;
using UnityEngine;

public class Piece : MonoBehaviourPun
{
    [SerializeField]
    ChessColor _color;
    public ChessColor Color => _color;

    public BoardPosition Position { get; private set; }
    public BoardPosition? Move { get; private set; } = null;
    public BoardPosition? Prediction { get; private set; } = null;

    public bool IsDead { get; private set; } = false;

    LineRenderer _lineRenderer;

    public int Power
    {
        get
        {
            int power = 0; // Default power of 0
            if (Move != null)
            {
                power += 1; // Add one power when moving, you prepared for battle since you were moving, so you're stronger
            }
            if (Move == Prediction)
            {
                power -= 2; // Remove one power when correctly predicted, the opponent predicted your move, so you're weaker
            }
            return power;
        }
    }

    void Awake()
    {
        Position = new BoardPosition(transform.position);
        transform.position = Position.worldPosition;

        _lineRenderer = GetComponent<LineRenderer>();
    }

    void Start()
    {
        UpdateLineRenderer();
    }

    void Update()
    {
        if (IsDead)
        {
            transform.Translate(0, -Time.deltaTime, 0);
        }
    }

    public bool IsMine()
    {
        return Player.LocalNetworkPlayerColor() == _color;
    }

    public void SetPos(BoardPosition pos)
    {
        if (pos != Position)
        {
            photonView.RPC(nameof(RPCSyncPos), RpcTarget.All, pos);
        }
    }

    [PunRPC]
    private void RPCSyncPos(BoardPosition pos)
    {
        transform.position = pos.worldPosition;
        Position = pos;
        UpdateLineRenderer();
    }

    public void SetMoveOrPrediction(Vector3 worldPos)
    {
        if (IsMine())
        {
            SetMove(worldPos);
        }
        else
        {
            SetPrediction(worldPos);
        }
    }

    public void SetMove(Vector3 worldPos)
    {
        BoardPosition? move = new BoardPosition(worldPos);
        if (move != Move)
        {
            if (move == Position)
            {
                move = null;
            }

            photonView.RPC(nameof(RPCSyncMove), RpcTarget.All, move);
        }
    }

    public void ResetMove()
    {
        SetMove(transform.position);
    }

    [PunRPC]
    private void RPCSyncMove(BoardPosition? move)
    {
        Move = move;
        UpdateLineRenderer();
    }

    public void SetPrediction(Vector3 worldPos)
    {
        BoardPosition? prediction = new BoardPosition(worldPos);
        if (prediction != Prediction)
        {
            if (prediction == Position)
            {
                prediction = null;
            }

            photonView.RPC(nameof(RPCSyncPrediction), RpcTarget.All, prediction);
        }
    }

    public void ResetPrediction()
    {
        SetPrediction(transform.position);
    }

    [PunRPC]
    private void RPCSyncPrediction(BoardPosition? prediction)
    {
        Prediction = prediction;
        UpdateLineRenderer();
    }

    public void Die()
    {
        photonView.RPC(nameof(RPCSyncIsDead), RpcTarget.All, true);
    }

    [PunRPC]
    private void RPCSyncIsDead(bool isDead)
    {
        IsDead = isDead;
    }

    void UpdateLineRenderer()
    {
        if (Move != null && IsMine())
        {
            Vector3 pos = Position.worldPosition;
            Vector3 move = ((BoardPosition)Move).worldPosition;
            pos.y = move.y = 0.2f;
            _lineRenderer.SetPositions(new[] { pos, move });
        }
        else if (Prediction != null && !IsMine())
        {
            Vector3 pos = Position.worldPosition;
            Vector3 prediction = ((BoardPosition)Prediction).worldPosition;
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
