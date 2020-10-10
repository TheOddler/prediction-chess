using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

public abstract class Piece : MonoBehaviourPun
{
    [SerializeField]
    ChessColor _color;
    public ChessColor Color => _color;

    public BoardPosition Position { get; protected set; }
    public BoardPosition? Move { get; protected set; } = null;
    public BoardPosition? Prediction { get; protected set; } = null;

    public bool IsDead { get; protected set; } = false;

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

    public static IEnumerable<Piece> All => FindObjectsOfType<Piece>().Where(p => !p.IsDead);
    public IEnumerable<Piece> Others => All.Where(p => p != this);
    public IEnumerable<Piece> Friends => Others.OfColor(Color);
    public IEnumerable<Piece> Enemies => Others.OfColor(Color.Invert());

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
    protected void RPCSyncPos(BoardPosition pos)
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
        if (move == Position) move = null;
        if (move != Move) photonView.RPC(nameof(RPCSyncMove), RpcTarget.All, move);
    }

    public void ResetMove()
    {
        SetMove(transform.position);
    }

    [PunRPC]
    protected void RPCSyncMove(BoardPosition? move)
    {
        var prevMove = Move;
        Move = move;

        if (prevMove != null)
        {
            foreach (var friend in Friends.MovingTo((BoardPosition)prevMove))
            {
                friend.UpdateLineRenderer();
            }
        }

        UpdateLineRenderer();

        if (move != null)
        {
            var friends = Friends.MovingTo((BoardPosition)move);
            foreach (var friend in friends)
            {
                friend.UpdateLineRenderer();
            }
        }
    }

    public void SetPrediction(Vector3 worldPos)
    {
        BoardPosition? prediction = new BoardPosition(worldPos);
        if (prediction == Position) prediction = null;
        if (prediction != Prediction) photonView.RPC(nameof(RPCSyncPrediction), RpcTarget.All, prediction);
    }

    public void ResetPrediction()
    {
        SetPrediction(transform.position);
    }

    [PunRPC]
    protected void RPCSyncPrediction(BoardPosition? prediction)
    {
        Prediction = prediction;
        UpdateLineRenderer();
    }

    public void Die()
    {
        photonView.RPC(nameof(RPCSyncIsDead), RpcTarget.All, true);
    }

    [PunRPC]
    protected void RPCSyncIsDead(bool isDead)
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

            _lineRenderer.startColor = UnityEngine.Color.clear;
            if (MoveIsLegal())
            {
                _lineRenderer.endColor = UnityEngine.Color.green;
            }
            else
            {
                _lineRenderer.endColor = UnityEngine.Color.red;
            }
        }
        else if (Prediction != null && !IsMine())
        {
            Vector3 pos = Position.worldPosition;
            Vector3 prediction = ((BoardPosition)Prediction).worldPosition;
            pos.y = prediction.y = 0.2f;
            _lineRenderer.SetPositions(new[] { pos, prediction });

            _lineRenderer.startColor = UnityEngine.Color.clear;
            if (PredictionIsLegal())
            {
                _lineRenderer.endColor = UnityEngine.Color.green;
            }
            else
            {
                _lineRenderer.endColor = UnityEngine.Color.red;
            }
        }
        else
        {
            var hidden = new Vector3(0, -1, 0);
            _lineRenderer.SetPositions(new[] { hidden, hidden });
        }
    }

    public virtual bool MoveIsLegal()
    {
        if (Move == null) return true;

        BoardPosition move = (BoardPosition)Move;
        bool destinationOk = DestinationIsLegal(move);
        return destinationOk && !Friends.AnyMovingTo(move);
    }

    public virtual bool PredictionIsLegal()
    {
        if (Prediction == null) return true;
        return DestinationIsLegal((BoardPosition)Prediction);
    }

    protected virtual bool DestinationIsLegal(BoardPosition destination)
    {
        return CalculateLegalDestinations().Contains(destination);
    }

    public abstract HashSet<BoardPosition> CalculateLegalDestinations();
}
