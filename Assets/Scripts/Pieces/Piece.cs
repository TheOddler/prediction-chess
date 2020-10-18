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

    BoardPosition _position;
    BoardPosition _previousPosition;
    public BoardPosition Position => _position;

    BoardPosition? _move = null;
    public BoardPosition? Move
    {
        get => _move;
        private set
        {
            var prev = _move;
            _move = value;
            UpdateOtherPiecesLinerenderersWhereNeeded(value, prev);
            UpdateLineRenderers();
        }
    }

    BoardPosition? _prediction = null;
    public BoardPosition? Prediction
    {
        get => _prediction;
        private set
        {
            _prediction = value;
            UpdateLineRenderers();
        }
    }

    public bool IsDead { get; private set; } = false;

    [SerializeField]
    LineRenderer _turnRenderer;
    [SerializeField]
    LineRenderer _previousTurnRenderer;

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
        Assert.IsNotNull(_turnRenderer);
        Assert.IsNotNull(_previousTurnRenderer);

        var position = new BoardPosition(transform.position);
        transform.position = position.worldPosition;

        _previousPosition = position;
        _position = position;
        UpdateLineRenderers();
    }

    IEnumerator Animate(Vector3 startPosition, Vector3 movePosition, bool died, bool diedHalfway)
    {
        float startTime = Time.time;
        float moveAnimEndTime = startTime + ANIM_MOVE_TIME / (died && diedHalfway ? 2f : 1f);

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
            float xScaleSign = Mathf.Sign(transform.localScale.x);
            while (Time.time < deathAnimEndTime)
            {
                float passedTime = Time.time - moveAnimEndTime;
                float progress = passedTime / ANIM_DIE_TIME;

                float scale = 1 - progress * 0.5f;
                transform.localScale = new Vector3(scale * xScaleSign, scale, scale);
                transform.position = Vector3.Lerp(startPosition, movePosition + transform.right * 0.3f, progress);

                yield return null;
            }
        }
    }

    public bool IsMine()
    {
        return Player.LocalNetworkPlayerColor() == _color;
    }

    public bool SetMoveOrPrediction(BoardPosition? position)
    {
        if (IsMine())
        {
            return SetMove(position);
        }
        else
        {
            return SetPrediction(position);
        }
    }

    public bool SetMove(BoardPosition? move)
    {
        if (move == Position)
        {
            move = null;
        }

        if (move != Move)
        {
            photonView.RPC(nameof(RPCSyncMove), RpcTarget.All, move);
            return true;
        }

        return false;
    }

    [PunRPC]
    protected void RPCSyncMove(BoardPosition? move)
    {
        Move = move;
    }

    public bool SetPrediction(BoardPosition? prediction)
    {
        if (prediction == Position)
        {
            prediction = null;
        }

        if (prediction != Prediction)
        {
            photonView.RPC(nameof(RPCSyncPrediction), RpcTarget.All, prediction);
            return true;
        }

        return false;
    }

    [PunRPC]
    protected void RPCSyncPrediction(BoardPosition? prediction)
    {
        Prediction = prediction;
    }

    public void ResolveTurn()
    {
        photonView.RPC(nameof(RPCResolveTurn), RpcTarget.All);
    }

    [PunRPC]
    protected void RPCResolveTurn()
    {
        _previousPosition = Position;
        if (Move != null) _position = (BoardPosition)Move;

        StartCoroutine(Animate(_previousPosition.worldPosition, _position.worldPosition, false, false));

        UpdateLineRenderers();

        SetMove(null);
        SetPrediction(null);
    }

    public void Die(Vector3 positionToDieAt, bool diedHalfway)
    {
        photonView.RPC(nameof(RPCDie), RpcTarget.All, positionToDieAt, diedHalfway);
    }

    [PunRPC]
    protected void RPCDie(Vector3 positionToDieAt, bool diedHalfway)
    {
        IsDead = true;
        StartCoroutine(Animate(Position.worldPosition, positionToDieAt, true, diedHalfway));
    }

    void UpdateLineRenderers()
    {
        Vector3 prevPos = _previousPosition.worldPosition;
        Vector3 pos = _position.worldPosition;
        Vector3 nextPos = pos; // Default
        bool isLegal = false;

        if (Move != null && IsMine())
        {
            nextPos = ((BoardPosition)Move).worldPosition;
            isLegal = MoveIsLegal();
        }
        else if (Prediction != null && !IsMine())
        {
            nextPos = ((BoardPosition)Prediction).worldPosition;
            isLegal = PredictionIsLegal();
        }

        prevPos.y = pos.y = nextPos.y = 0.2f;
        _turnRenderer.SetPositions(new[] { pos, nextPos });
        _turnRenderer.endColor = isLegal ? UnityEngine.Color.green : UnityEngine.Color.red;

        _previousTurnRenderer.SetPositions(new[] { prevPos, pos });
    }

    void UpdateOtherPiecesLinerenderersWhereNeeded(BoardPosition? move, BoardPosition? prevMove)
    {
        if (prevMove != null)
        {
            foreach (var friend in Friends.MovingTo((BoardPosition)prevMove))
            {
                friend.UpdateLineRenderers();
            }
        }

        if (move != null)
        {
            var friends = Friends.MovingTo((BoardPosition)move);
            foreach (var friend in friends)
            {
                friend.UpdateLineRenderers();
            }
        }
    }

    public virtual bool MoveIsLegal()
    {
        if (Move == null)
        {
            return true;
        }

        BoardPosition move = (BoardPosition)Move;
        bool destinationOk = DestinationIsLegal(move);
        return destinationOk && !Friends.AnyMovingTo(move);
    }

    public virtual bool PredictionIsLegal()
    {
        if (Prediction == null)
        {
            return true;
        }

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

        BoardPosition? maybeNext = Position.Add(x, y);
        for (int i = 0; i < maxDistance && maybeNext != null; ++i)
        {
            BoardPosition checking = (BoardPosition)maybeNext;
            if (Friends.AtPosition(checking) == null)
            {
                legalDestinations.Add(checking);
            }
            else
            {
                break;
            }

            if (Enemies.AtPosition(checking) != null)
            {
                break;
            }

            maybeNext = checking.Add(x, y);
        }

        return legalDestinations;
    }
}
