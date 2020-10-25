
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(AudioListener))]
public class Player : MonoBehaviourPun
{
    const int MAX_MOVES = 3;
    const int MAX_PREDICTIONS = 3;

    [SerializeField]
    ChessColor _color;
    public ChessColor Color => _color;

    [SerializeField]
    Toggle _imDoneToggle;
    [SerializeField]
    Toggle _globalIsDoneToggle;

    [SerializeField]
    TMP_Text _moveCountIndicator;
    [SerializeField]
    TMP_Text _predictionCountIndicator;

    bool _isDoneX = false;
    bool IsDone
    {
        get => _isDoneX;
        set
        {
            _isDoneX = value;
            _globalIsDoneToggle.isOn = value;
            if (IsMe()) _imDoneToggle.isOn = value;

            if (IsDone && (OtherPlayer.IsDone || PhotonNetwork.OfflineMode))
            {
                GameManager.Instance.HandleBothPlayersDone();
            }
        }
    }

    public static Player LocalPlayer { get; private set; }
    public static Player RemotePlayer { get; private set; }
    public Player OtherPlayer => LocalPlayer == this ? RemotePlayer : LocalPlayer;

    public IEnumerable<Piece> Pieces => Piece.AllAlive.OfColor(Color);
    public IEnumerable<Piece> EnemyPieces => Piece.AllAlive.OfColor(Color.Invert());

    public bool TurnIsLegal
    {
        get
        {
            return MoveCountOk
                && PredictionCountOk
                && AllMovesLegal
                && AllPredictionsLegal;
        }
    }

    public int MoveCount => Pieces.Count(p => p.Move != null);
    public int PredictionCount => OtherPlayer.Pieces.Count(p => p.Prediction != null);
    public bool MoveCountOk => MoveCount <= MAX_MOVES;
    public bool PredictionCountOk => PredictionCount <= MAX_PREDICTIONS;
    public bool AllMovesLegal => Pieces.All(p => p.MoveIsLegal());
    public bool AllPredictionsLegal => EnemyPieces.All(p => p.PredictionIsLegal());

    Piece _selected;

    void Awake()
    {
        if (IsMe())
        {
            Assert.IsNull(LocalPlayer);
            LocalPlayer = this;
        }
        else
        {
            Assert.IsNull(RemotePlayer);
            RemotePlayer = this;
        }

        enabled = IsMe();
        GetComponent<Camera>().enabled = enabled;
        GetComponent<AudioListener>().enabled = enabled;
    }

    void Start()
    {
        IsDone = _isDoneX; // To update the toggles

        UpdateMoveCountIndicator();
        UpdatePredictionCountIndicator();
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsDone)
        {
            if (Input.GetMouseButtonDown(0))
            {
                var position = GetMousePosition();
                _selected = Piece.AllAlive.AtPosition(position);
            }
            else if (Input.GetMouseButton(0) && _selected != null)
            {
                var position = GetMousePosition();
                bool changed = _selected.SetMoveOrPrediction(position);
                if (changed)
                {
                    UpdateMoveCountIndicator();
                    UpdatePredictionCountIndicator();
                }
            }
        }
    }

    public void ResetTurn()
    {
        foreach (var piece in Pieces)
        {
            piece.SetMove(null);
        }

        foreach (var piece in EnemyPieces)
        {
            piece.SetPrediction(null);
        }

        UpdateMoveCountIndicator();
        UpdatePredictionCountIndicator();
    }

    bool IsMe()
    {
        return LocalNetworkPlayerColor() == _color;
    }

    public static ChessColor LocalNetworkPlayerColor()
    {
        if (PhotonNetwork.OfflineMode) // In the editor (when not connected) we're white
        {
            return ChessColor.White;
        }

        return PhotonNetwork.IsMasterClient ? ChessColor.White : ChessColor.Black;
    }

    public void SetIsDone(bool isDone)
    {
        isDone = isDone && TurnIsLegal;

        if (isDone != IsDone)
        {
            photonView.RPC(nameof(RPCSyncIsDone), RpcTarget.All, isDone);
        }
        else
        {
            IsDone = isDone; // To force correcting the ui toggle
        }

        UpdateMoveCountIndicator();
        UpdatePredictionCountIndicator();
    }

    [PunRPC]
    private void RPCSyncIsDone(bool isDone)
    {
        IsDone = isDone;
    }

    BoardPosition? GetMousePosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 zeroPoint = ray.origin - ray.direction * ray.origin.y / ray.direction.y;
        return BoardPosition.IfInBoard(zeroPoint);
    }

    void UpdateMoveCountIndicator()
    {
        _moveCountIndicator.text = $"{MoveCount}/{MAX_MOVES}";

        _moveCountIndicator.color = MoveCountOk && AllMovesLegal ? UnityEngine.Color.white : UnityEngine.Color.red;
    }

    void UpdatePredictionCountIndicator()
    {
        _predictionCountIndicator.text = $"{PredictionCount}/{MAX_MOVES}";
        _predictionCountIndicator.color = PredictionCountOk && AllPredictionsLegal ? UnityEngine.Color.white : UnityEngine.Color.red;
    }
}
