using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

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
    public bool IsDone
    {
        get => _isDone;
        set
        {
            _isDone = value;
            _isDoneToggle.isOn = value;
            photonView.RPC(nameof(RPCSyncIsDone), RpcTarget.Others, value);
        }
    }

    static Player _localPlayer = null;
    public static Player LocalPlayer
    {
        get
        {
            if (_localPlayer == null)
            {
                var players = FindObjectsOfType<Player>();
                _localPlayer = players.First(p => p.IsMe());
            }
            return _localPlayer;
        }
    }

    Piece _selected;

    void Start()
    {
        gameObject.SetActive(IsMe());
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

    [PunRPC]
    private void RPCSyncIsDone(bool isDone)
    {
        _isDone = isDone;
        _isDoneToggle.isOn = isDone;
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
            Vector3 zeroPoint = ray.origin + ray.direction * ray.origin.y / ray.direction.y;
            return new HitInfo(zeroPoint);
        }
    }
}
