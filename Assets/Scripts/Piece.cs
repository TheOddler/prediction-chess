using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    [SerializeField]
    ChessColor _color;
    public ChessColor Color => _color;

    BoardPosition _pos;
    BoardPosition? _move = null;

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

    public void SetMove(Vector3 worldPos)
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

    void UpdateLineRenderer()
    {
        if (_move == null)
        {
            var hidden = new Vector3(0, -1, 0);
            _lineRenderer.SetPositions(new[] { hidden, hidden });
        }
        else
        {
            Vector3 pos = _pos.worldPosition;
            Vector3 move = ((BoardPosition)_move).worldPosition;
            pos.y = move.y = 0.2f;
            _lineRenderer.SetPositions(new[] { pos, move });
        }
    }
}
