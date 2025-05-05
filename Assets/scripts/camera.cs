using System;
using UnityEngine;
using UnityEngine.Events;

public class camera : MonoBehaviour
{



    [SerializeField] private LayerMask cityBlockLayerMask;
    [SerializeField] private LayerMask solarPanelLayerMask;
    [SerializeField] private LayerMask gridSnapLayerMask;
    [SerializeField] private GameObject selectedCityBlock;

    [SerializeField] private bool allowControl = true;

    [SerializeField] private Vector2 cameraVelocity;
    [SerializeField] private float cameraZoom;


    private Camera _cam;
    private GameObject _dragged;
    private Vector3 _offset;

    [SerializeField] private GameObject snapTo = null;
    [SerializeField] private Vector3 cursorPosition = Vector3.zero;


    void Awake()
    {
        _cam = this.gameObject.GetComponent<Camera>();
    }

    public void deselectCityBlock()
    {
        selectedCityBlock = null;
    }

    void Update()
    {
        if (allowControl)
        {
            //player controll
            if(selectedCityBlock != null)
            {
                //place blocks
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
                    cursorPosition = _cam.ScreenToWorldPoint(Input.mousePosition);

                    Collider2D[] hits = Physics2D.OverlapCircleAll(cursorPosition, 1, gridSnapLayerMask);
                    if(hits.Length == 0)
                    {
                        snapTo = null;
                    }
                    float distance = 1000;
                    for (int i = 0; i < hits.Length; i++)
                    {
                        float dist = (cursorPosition - hits[i].transform.position).magnitude;
                        if (distance > dist)
                        {
                            distance = dist;
                            snapTo = hits[i].gameObject;
                        }
                    }
                    Debug.Log(distance);
                    if (snapTo != null)
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

                        Vector3 lerpPos = Vector3.Lerp(_dragged.transform.position, cursorPosition + _offset, 0.05f);
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
                    if(snapTo == null)
                    {
                        //to cursor
                        _dragged.transform.position = new Vector3(cursorPosition.x, cursorPosition.y, _dragged.transform.position.z);
                    }else{
                        //to snap
                        _dragged.transform.position = snapTo.transform.position;
                    }
                    
                    _dragged = null;
                }
            }
            else
            {
                //city interaction
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
            }


            // player move camera

            //CAMERA
            // Read input axes (WASD or arrow keys by default)
            float h = Input.GetAxisRaw("Horizontal"); // A/D or ←/→
            float v = Input.GetAxisRaw("Vertical");   // W/S or ↑/↓

            // Build movement vector
            Vector3 delta = new Vector3(h, v, 0f) * 6 * Time.deltaTime;

            // Apply to camera’s position
            transform.position += delta;

        }
        else
        {
            //auto movement
            Vector2 camPos = this.transform.position;
            Vector2 selectedPos = selectedCityBlock.transform.position;
            Vector2 lerpPos = Vector2.Lerp(camPos, selectedPos, 0.02f);
            this.transform.position = new Vector3(lerpPos.x, lerpPos.y, -10);
            float dist = (selectedPos - lerpPos).magnitude;
            if (dist <= 0.3f)
            {
                allowControl = true;
            }
        }
    }

    public static Vector3 QuadraticEaseIn(Vector3 start, Vector3 end, float t)
    {

        float dist = (start - end).magnitude;
        dist = Mathf.Clamp(dist, 0, 0.8f);
        return Vector3.Lerp(start, end, t * 1/(dist*dist));
        // or: return start + (end - start) * quad;
    }

    private void SelectObject(GameObject selected)
    {
        selectedCityBlock = selected;
        allowControl = false;
    }
}
