using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Assertions;

public abstract class Piece : MonoBehaviourPun
{
    const float ANIM_MOVE_TIME = 1.0f;
    const float ANIM_DIE_TIME = 0.5f;

    [SerializeField]
    ChessColor _color;
    public ChessColor Color => _color;

    public BoardPosition Position { get; protected set; }
    public BoardPosition? Move { get; protected set; } = null;
    public BoardPosition? Prediction { get; protected set; } = null;

    public bool IsDead { get; protected set; } = false;

    LineRenderer _lineRenderer;

    public virtual int Power
    {
        get
        {
            int power = 0; // Default power of 0
            if (Move != null)
            {
                power += 1; // Add one power when moving, you prepared for battle since you were moving, so you're stronger
            }
            if (Prediction != null && Move == Prediction)
            {
                power -= 2; // Remove one power when correctly predicted, the opponent predicted your move, so you're weaker
            }
            return power;
        }
    }

    public static IEnumerable<Piece> AllAndDying => FindObjectsOfType<Piece>();
    public static IEnumerable<Piece> All => AllAndDying.Where(p => !p.IsDead);
    public IEnumerable<Piece> Others => All.Where(p => p != this);
    public IEnumerable<Piece> Friends => Others.OfColor(Color);
    public IEnumerable<Piece> Enemies => All.OfColor(Color.Invert());

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

    IEnumerator Animate(Vector3 movePosition, bool died, bool diedHalfway)
    {
        float startTime = Time.time;
        float moveAnimEndTime = startTime + ANIM_MOVE_TIME / (died && diedHalfway ? 2f : 1f);
        Vector3 startPosition = Position.worldPosition;

        while (Time.time < moveAnimEndTime)
        {
            float passedTime = Time.time - startTime;
            transform.position = Vector3.Lerp(startPosition, movePosition, passedTime / ANIM_MOVE_TIME);

            yield return null;
        }

        transform.position = movePosition;

        if (died)
        {
            float deathAnimEndTime = moveAnimEndTime + ANIM_DIE_TIME;
            while (Time.time < deathAnimEndTime)
            {
                float passedTime = Time.time - moveAnimEndTime;

                float scale = 1 - (passedTime / ANIM_DIE_TIME);
                float blobScale = 2 - scale * scale;
                transform.localScale = new Vector3(blobScale, scale, blobScale);

                yield return null;
            }

            Destroy(gameObject);
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
        StartCoroutine(Animate(pos.worldPosition, false, false));
        Position = pos;
        UpdateLineRenderer();
    }

    public void SetMoveOrPrediction(BoardPosition? position)
    {
        if (IsMine())
        {
            SetMove(position);
        }
        else
        {
            SetPrediction(position);
        }
    }

    public void SetMove(BoardPosition? move)
    {
        if (move == Position) move = null;
        if (move != Move) photonView.RPC(nameof(RPCSyncMove), RpcTarget.All, move);
    }

    public void ResetMove()
    {
        SetMove(null);
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

    public void SetPrediction(BoardPosition? prediction)
    {
        if (prediction == Position) prediction = null;
        if (prediction != Prediction) photonView.RPC(nameof(RPCSyncPrediction), RpcTarget.All, prediction);
    }

    public void ResetPrediction()
    {
        SetPrediction(null);
    }

    [PunRPC]
    protected void RPCSyncPrediction(BoardPosition? prediction)
    {
        Prediction = prediction;
        UpdateLineRenderer();
    }

    public void Die(Vector3 positionToDieAt, bool diedHalfway)
    {
        photonView.RPC(nameof(RPCDie), RpcTarget.All, positionToDieAt, diedHalfway);
    }

    [PunRPC]
    protected void RPCDie(Vector3 positionToDieAt, bool diedHalfway)
    {
        IsDead = true;
        StartCoroutine(Animate(positionToDieAt, true, diedHalfway));
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

    protected HashSet<BoardPosition> CalculateLegalDestinationsInDirection(int x, int y, int maxDistance = int.MaxValue)
    {
        Assert.IsTrue(x.In(0, 1, -1));
        Assert.IsTrue(y.In(0, 1, -1));

        var legalDestinations = new HashSet<BoardPosition>();

        BoardPosition checking = Position.Add(x, y);
        for (int i = 0; i < maxDistance; ++i)
        {
            if (Friends.AtPosition(checking) == null) legalDestinations.Add(checking);
            else break;

            if (Enemies.AtPosition(checking) != null) break;

            var prev = checking;
            checking = prev.Add(x, y);

            if (prev == checking) break;
        }

        return legalDestinations;
    }
}
