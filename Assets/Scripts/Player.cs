
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

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

    [SerializeField]
    ChessColor _color;
    public ChessColor Color => _color;

    [SerializeField]
    Toggle _isDoneToggle;
    bool _isDone = false;

    public static Player LocalPlayer { get; private set; }
    public static Player RemotePlayer { get; private set; }
    public Player OtherPlayer => LocalPlayer == this ? RemotePlayer : LocalPlayer;

    Piece _selected;

    void Awake()
    {
        Assert.IsNotNull(_isDoneToggle);
        Assert.AreEqual(_isDone, _isDoneToggle.isOn);

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
        if (!_isDone)
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
        HandleIsDone(isDone);
        photonView.RPC(nameof(RPCSyncIsDone), RpcTarget.Others, isDone);
    }

    private void HandleIsDone(bool isDone)
    {
        _isDone = isDone;
        _isDoneToggle.isOn = isDone;

        if (_isDone && OtherPlayer._isDone)
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
