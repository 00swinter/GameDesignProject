using System;
using UnityEngine;
using UnityEngine.Events;

public class camera : MonoBehaviour
{



    [SerializeField] private Collider2D[] myHits;

    [SerializeField] private LayerMask cityBlockLayerMask;
    [SerializeField] private LayerMask solarPanelLayerMask;
    [SerializeField] private LayerMask gridSnapLayerMask;
    [SerializeField] private GameObject selectedBlock;

    [SerializeField] private bool allowControl = true;
    [SerializeField] private bool placeMode = true;


    private Camera _cam;
    private GameObject _dragged;
    private Vector3 _offset;


    void Awake()
    {
        _cam = this.gameObject.GetComponent<Camera>();
    }

    void Update()
    {
        if (placeMode)
        {
            // ——— Mouse Down: begin drag ———
            if (Input.GetMouseButtonDown(0) && _dragged == null)
            {
                Vector2 wp = _cam.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(wp, Vector2.zero, Mathf.Infinity, solarPanelLayerMask);
                if (hit.collider != null)
                {
                    _dragged = hit.collider.gameObject.transform.parent.gameObject;
                    // store offset so you don't snap the pivot
                    _offset = _dragged.transform.position - new Vector3(wp.x, wp.y, _dragged.transform.position.z);
                }
            }

            // ——— Mouse Hold: move object ———
            if (Input.GetMouseButton(0) && _dragged != null)
            {
                Vector3 wp3 = _cam.ScreenToWorldPoint(Input.mousePosition);

                Collider2D[] hits = Physics2D.OverlapCircleAll(wp3, 1, gridSnapLayerMask);
                Debug.Log(hits);
                myHits = hits;
                GameObject snapTo = null;
                float distance = 1000;
                for(int i=0; i<hits.Length; i++)
                {
                    Vector2 wp2 = wp3;
                    float dist = (wp3 - hits[i].transform.position).magnitude;
                    if (distance > dist)
                    {
                        distance = dist;
                        snapTo = hits[i].gameObject;
                    }
                }
                Debug.Log(distance);
                if(snapTo != null)
                {
                    Vector3 snapPos = snapTo.transform.position;
                    Vector3 lerpPos = QuadraticEaseIn(_dragged.transform.position, snapPos, 0.01f);
                    _dragged.transform.position = new Vector3(lerpPos.x, lerpPos.y, _dragged.transform.position.z);


                    Quaternion fromRot = _dragged.transform.rotation;
                    Quaternion toRot = snapTo.transform.rotation;


                    Quaternion lerpRot = Quaternion.Lerp(fromRot, toRot, 0.01f);

                    _dragged.transform.rotation = lerpRot;
                }
                else
                {

                    Vector3 lerpPos = Vector3.Lerp(_dragged.transform.position, wp3 + _offset, 0.05f);
                    _dragged.transform.position = new Vector3(lerpPos.x, lerpPos.y, _dragged.transform.position.z);


                    Quaternion fromRot = _dragged.transform.rotation;
                    Quaternion toRot = Quaternion.identity;


                    Quaternion lerpRot = Quaternion.Lerp(fromRot, toRot, 0.05f);

                    _dragged.transform.rotation = lerpRot;
                }



            }

            // ——— Mouse Up: drop ———
            if (Input.GetMouseButtonUp(0) && _dragged != null)
            {
                _dragged = null;
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                // 1) Mouse → world point
                Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                // 2a) Raycast at that point, but only against blockLayerMask
                //    - distance = 0 forces a “point check”
                RaycastHit2D hit = Physics2D.Raycast(
                    worldPoint,
                    Vector2.zero,
                    0f,
                    cityBlockLayerMask
                );

                if (hit.collider != null)
                {
                    SelectObject(hit.collider.gameObject);
                }
            }

            //move cam to new selected block
            if (!allowControl)
            {
                Vector2 camPos = this.transform.position;
                Vector2 selectedPos = selectedBlock.transform.position;
                Vector2 lerpPos = Vector2.Lerp(camPos, selectedPos, 0.02f);
                this.transform.position = new Vector3(lerpPos.x, lerpPos.y, -10);
                float dist = (selectedPos - lerpPos).magnitude;
                if (dist <= 0.5f)
                {
                    allowControl = true;
                }
            }
        }
        


        
    }

    public static Vector3 QuadraticEaseIn(Vector3 start, Vector3 end, float t)
    {

        float dist = (start - end).magnitude;
        dist = Mathf.Clamp(dist, 0, 1.1f);
        return Vector3.Lerp(start, end, t * 1/(dist*dist));
        // or: return start + (end - start) * quad;
    }

    private void SelectObject(GameObject selected)
    {
        selectedBlock = selected;
        allowControl = false;
    }
}
