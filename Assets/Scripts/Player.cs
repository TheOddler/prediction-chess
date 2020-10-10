
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(AudioListener))]
public class Player : MonoBehaviourPun
{
    struct HitInfo
    {
        public Vector3 position;
        public Piece piece;

        public HitInfo(Vector3 position, Piece piece = null)
        {
            this.position = position;
            this.piece = piece;
        }
    }

    const int MAX_MOVES = 3;
    const int MAX_PREDICTIONS = 3;

    [SerializeField]
    ChessColor _color;
    public ChessColor Color => _color;

    [SerializeField]
    Toggle _imDoneToggle;
    [SerializeField]
    Toggle _globalIsDoneToggle;

    bool _isDoneX = false;
    bool IsDone
    {
        get => _isDoneX;
        set
        {
            _isDoneX = value;
            _globalIsDoneToggle.isOn = value;
            if (IsMe()) _imDoneToggle.isOn = value;
        }
    }

    public static Player LocalPlayer { get; private set; }
    public static Player RemotePlayer { get; private set; }
    public Player OtherPlayer => LocalPlayer == this ? RemotePlayer : LocalPlayer;

    public IEnumerable<Piece> Pieces => Piece.All.OfColor(Color);

    public bool TurnIsLegal =>
        Pieces.Count(p => p.Move != null) <= MAX_MOVES
        && OtherPlayer.Pieces.Count(p => p.Prediction != null) <= MAX_PREDICTIONS
        && Pieces.All(p => p.MoveIsLegal());

    Piece _selected;

    void Awake()
    {
        IsDone = _isDoneX; // To update the toggles

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

    // Update is called once per frame
    void Update()
    {
        if (!IsDone)
        {
            if (Input.GetMouseButtonDown(0))
            {
                var hit = GetHit();
                _selected = hit.piece;
            }
            else if (Input.GetMouseButton(0) && _selected != null)
            {
                var hit = GetHit();
                _selected.SetMoveOrPrediction(hit.position);
            }
        }
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

        HandleIsDone(isDone);
        photonView.RPC(nameof(RPCSyncIsDone), RpcTarget.Others, isDone);
    }

    private void HandleIsDone(bool isDone)
    {
        IsDone = isDone;

        if (IsDone && (OtherPlayer.IsDone || PhotonNetwork.OfflineMode))
        {
            GameManager.Instance.ResolveBattle();
        }
    }

    [PunRPC]
    private void RPCSyncIsDone(bool isDone)
    {
        HandleIsDone(isDone);
    }

    HitInfo GetHit()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit info;
        if (Physics.Raycast(ray, out info))
        {
            return new HitInfo(info.point, info.transform.GetComponent<Piece>());
        }
        else
        {
            Vector3 zeroPoint = ray.origin - ray.direction * ray.origin.y / ray.direction.y;
            return new HitInfo(zeroPoint);
        }
    }
}
