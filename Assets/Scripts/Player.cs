using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
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

    Piece _selected;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var hit = GetHit();
            _selected = hit.piece;
        }
        else if (Input.GetMouseButton(0) && _selected != null)
        {
            var hit = GetHit();
            if (hit.piece == null)
            {
                _selected.SetMove(hit.position);
            }
            else
            {
                _selected.RemoveMove();
            }
        }
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
