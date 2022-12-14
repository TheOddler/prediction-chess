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
    const float DEATH_SIZE = 0.5f;
    const float DEATH_OFFSET = 0.3f;

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
            UpdateOtherPiecesMoveIndicatorsWhereNeeded(value, prev);
            UpdateMoveIndicators();
        }
    }

    BoardPosition? _prediction = null;
    public BoardPosition? Prediction
    {
        get => _prediction;
        private set
        {
            _prediction = value;
            UpdateMoveIndicators();
        }
    }

    public bool IsDead { get; private set; } = false;

    [SerializeField]
    LineRenderer _turnRenderer;
    [SerializeField]
    LineRenderer _previousTurnRenderer;

    MeshRenderer _meshRenderer;
    Material _noMoveMaterial;
    [SerializeField]
    Material _legalMoveMaterial;
    [SerializeField]
    Material _illegalMoveMaterial;

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

    public bool IsMine => Player.LocalNetworkPlayerColor() == _color;

    public static IEnumerable<Piece> AllAliveAndDead => FindObjectsOfType<Piece>();
    public static IEnumerable<Piece> AllAlive => AllAliveAndDead.Where(p => !p.IsDead);
    public static IEnumerable<Piece> AllDead => AllAliveAndDead.Where(p => p.IsDead);
    public IEnumerable<Piece> Others => AllAlive.Where(p => p != this);
    public IEnumerable<Piece> Friends => Others.OfColor(Color);
    public IEnumerable<Piece> Enemies => AllAlive.OfColor(Color.Invert());

    void Awake()
    {
        Assert.IsNotNull(_turnRenderer);
        Assert.IsNotNull(_previousTurnRenderer);

        _meshRenderer = GetComponent<MeshRenderer>();
        Assert.IsNotNull(_meshRenderer);

        _noMoveMaterial = _meshRenderer.sharedMaterial;
        Assert.IsNotNull(_noMoveMaterial);
        Assert.IsNotNull(_legalMoveMaterial);
        Assert.IsNotNull(_illegalMoveMaterial);

        var position = new BoardPosition(transform.position);
        transform.position = position.worldPosition;

        _previousPosition = position;
        _position = position;
        UpdateMoveIndicators();
    }

    IEnumerator Animate(Vector3 startPosition, Vector3 endPosition, bool died, bool diedHalfway)
    {
        float startTime = Time.time;
        float animMoveTime = ANIM_MOVE_TIME / (died && diedHalfway ? 2f : 1f);
        float moveAnimEndTime = startTime + animMoveTime;

        while (Time.time < moveAnimEndTime)
        {
            float passedTime = Time.time - startTime;
            float progress = passedTime / animMoveTime;
            transform.position = Vector3.Lerp(startPosition, endPosition, progress);

            yield return null;
        }

        transform.position = endPosition;

        if (died)
        {
            float deathAnimEndTime = moveAnimEndTime + ANIM_DIE_TIME;
            float xScaleSign = Mathf.Sign(transform.localScale.x);
            while (Time.time < deathAnimEndTime)
            {
                float passedTime = Time.time - moveAnimEndTime;
                float progress = passedTime / ANIM_DIE_TIME;

                float scale = 1 - progress * (1 - DEATH_SIZE);
                transform.localScale = new Vector3(scale * xScaleSign, scale, scale);
                transform.position = Vector3.Lerp(endPosition, endPosition + transform.right * DEATH_OFFSET, progress);

                yield return null;
            }
        }
    }

    public bool SetMoveOrPrediction(BoardPosition? position)
    {
        if (IsMine)
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

    public void ResolveTurnSurvived()
    {
        photonView.RPC(nameof(RPCResolveTurnSurvived), RpcTarget.All);
    }

    [PunRPC]
    protected void RPCResolveTurnSurvived()
    {
        ResolveTurnCommon();
        StartCoroutine(Animate(_previousPosition.worldPosition, _position.worldPosition, false, false));
    }

    public void ResolveTurnDied(Vector3 positionToDieAt, bool diedHalfway)
    {
        photonView.RPC(nameof(RPCResolveTurnDied), RpcTarget.All, positionToDieAt, diedHalfway);
    }

    [PunRPC]
    protected void RPCResolveTurnDied(Vector3 positionToDieAt, bool diedHalfway)
    {
        IsDead = true;
        ResolveTurnCommon();
        StartCoroutine(Animate(_previousPosition.worldPosition, positionToDieAt, true, diedHalfway));
    }

    private void ResolveTurnCommon()
    {
        _previousPosition = Position;
        if (Move != null)
        {
            _position = (BoardPosition)Move;
        }

        UpdateMoveIndicators();

        SetMove(null);
        SetPrediction(null);
    }

    public void ResolveTurnDead()
    {
        photonView.RPC(nameof(RPCResolveTurnDead), RpcTarget.All);
    }

    [PunRPC]
    protected void RPCResolveTurnDead()
    {
        Destroy(gameObject);
    }

    void UpdateMoveIndicators()
    {
        Vector3 prevPos = _previousPosition.worldPosition;
        Vector3 pos = _position.worldPosition;
        Vector3 nextPos = pos; // Default
        bool? isLegal = null;

        if (Move != null && IsMine)
        {
            nextPos = ((BoardPosition)Move).worldPosition;
            isLegal = MoveIsLegal();
        }
        else if (Prediction != null && !IsMine)
        {
            nextPos = ((BoardPosition)Prediction).worldPosition;
            isLegal = PredictionIsLegal();
        }

        prevPos.y = pos.y = nextPos.y = 0.2f;
        _turnRenderer.SetPositions(new[] { pos, nextPos });
        _turnRenderer.endColor = isLegal != false ? UnityEngine.Color.green : UnityEngine.Color.red;

        _previousTurnRenderer.SetPositions(new[] { prevPos, pos });

        _meshRenderer.sharedMaterial = isLegal == null ? _noMoveMaterial : isLegal == true ? _legalMoveMaterial : _illegalMoveMaterial;
    }

    void UpdateOtherPiecesMoveIndicatorsWhereNeeded(BoardPosition? move, BoardPosition? prevMove)
    {
        if (prevMove != null)
        {
            foreach (var friend in Friends.MovingTo((BoardPosition)prevMove))
            {
                friend.UpdateMoveIndicators();
            }
        }

        if (move != null)
        {
            var friends = Friends.MovingTo((BoardPosition)move);
            foreach (var friend in friends)
            {
                friend.UpdateMoveIndicators();
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
