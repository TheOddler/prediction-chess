using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
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
        _move = new BoardPosition(worldPos);
        UpdateLineRenderer();
    }

    public void RemoveMove()
    {
        _move = null;
        UpdateLineRenderer();
    }

    void UpdateLineRenderer()
    {
        if (_move == null)
        {
            _lineRenderer.SetPositions(new[] { Vector3.zero, Vector3.zero });
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
