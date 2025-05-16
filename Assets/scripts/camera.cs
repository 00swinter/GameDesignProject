using System;
using UnityEngine;
using UnityEngine.Events;

public class camera : MonoBehaviour
{



    [SerializeField] private LayerMask cityBlockLayerMask;
    [SerializeField] private LayerMask solarPanelLayerMask;
    [SerializeField] private LayerMask gridSnapLayerMask;
    [SerializeField] private GameObject selectedCityBlock;
    [SerializeField] private cameraSettings currentCameraSettings;

    [SerializeField] private bool allowControl = true;

    private Camera _cam;
    private GameObject _dragged;
    private Quaternion _draggedRotation;
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
        allowControl = false;
    }

    void Update()
    {

        //select what camera settings to use
        if (selectedCityBlock != null)
        {
            currentCameraSettings = selectedCityBlock.GetComponent<cameraSettings>();
        }
        else
        {
            currentCameraSettings = this.GetComponent<cameraSettings>();
        }

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

                        //get the snap below
                        setSnapTo();

                        if(snapTo != null)
                        {
                            _draggedRotation = _dragged.transform.rotation  * Quaternion.Inverse(snapTo.transform.rotation);
                        }
                        else
                        {
                            _draggedRotation = _dragged.transform.rotation;
                        }

                        // store offset so you don't snap the pivot
                        _offset = _dragged.transform.position - new Vector3(wp.x, wp.y, _dragged.transform.position.z);
                    }
                }

                // ——— Mouse Hold: move object ———
                if (Input.GetMouseButton(0) && _dragged != null)
                {
                    setSnapTo();

                    if (snapTo != null)
                    {
                        Vector3 snapPos = snapTo.transform.position;
                        Vector3 lerpPos = QuadraticEaseIn(_dragged.transform.position, snapPos, 0.01f);
                        _dragged.transform.position = new Vector3(lerpPos.x, lerpPos.y, _dragged.transform.position.z);


                        Quaternion fromRot = _dragged.transform.rotation;
                        Quaternion toRot = snapTo.transform.rotation * _draggedRotation;


                        Quaternion lerpRot = Quaternion.Lerp(fromRot, toRot, 0.05f);

                        _dragged.transform.rotation = lerpRot;
                    }
                    else
                    {

                        Vector3 lerpPos = Vector3.Lerp(_dragged.transform.position, cursorPosition + _offset, 0.05f);
                        _dragged.transform.position = new Vector3(lerpPos.x, lerpPos.y, _dragged.transform.position.z);


                        Quaternion fromRot = _dragged.transform.rotation;
                        Quaternion toRot = Quaternion.identity * _draggedRotation;


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
                        _dragged.transform.rotation = _draggedRotation;
                    }
                    else{
                        //to snap
                        _dragged.transform.position = snapTo.transform.position;
                        _dragged.transform.rotation = snapTo.transform.rotation * _draggedRotation;
                    }
                    
                    _dragged = null;
                }

                // -------Rotate block-------
                if (Input.GetKeyDown(KeyCode.Q))
                {
                    _draggedRotation = Quaternion.Euler(0f, 0f, _draggedRotation.eulerAngles.z + 60);
                }

                if (Input.GetKeyDown(KeyCode.E))
                {
                    _draggedRotation = Quaternion.Euler(0f, 0f, _draggedRotation.eulerAngles.z - 60); 
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
                        allowControl = false;
                    }
                }
            }


            // player move camera
            //CAMERA
            

           





            // Read input axes and mousewheel
            float h = Input.GetAxisRaw("Horizontal");   //a,d
            float v = Input.GetAxisRaw("Vertical");     //w,s
            float scrollDelta = Input.GetAxis("Mouse ScrollWheel");

            //calculate position
            Vector3 delta = new Vector3(h, v, 0f) * currentCameraSettings.cameraSpeed * Time.deltaTime;
            Vector3 deltaRotated = Rotate(delta, currentCameraSettings.relativeRotation);
            Vector3 currentCamPos = transform.position;
            Vector3 newCamPos = currentCamPos + deltaRotated;
            //clamp position
            
            // Apply position
            transform.position = newCamPos;



            //calculate zoom
            float currentZoom = _cam.orthographicSize;
            float newZoom = currentZoom - (scrollDelta * (currentCameraSettings.cameraZoomSpeed * (currentZoom /50)));
            //clamp zoom

            newZoom = Mathf.Clamp(newZoom, currentCameraSettings.limit_ZoomIn, currentCameraSettings.limit_ZoomOut);

            // Apply zoom

            _cam.orthographicSize = newZoom;



        }
        else
        {
            int conditions = 0;
            //auto movement

            //lerp position
            Vector2 camPos = this.transform.position;
            Vector2 selectedPos = currentCameraSettings.cameraCenter;
            Vector2 lerpPos = Vector2.Lerp(camPos, selectedPos, 0.02f);
            this.transform.position = new Vector3(lerpPos.x, lerpPos.y, -10);

            //lerp rotation
            Quaternion fromRot = this.transform.rotation;
            Quaternion toRot = Quaternion.Euler(0f, 0f, currentCameraSettings.relativeRotation);
            Quaternion lerpRot = Quaternion.Lerp(fromRot, toRot, 0.02f);
            this.transform.rotation = lerpRot;

            float dist = (selectedPos - lerpPos).magnitude;
            if (dist <= 0.3f)
            {
                conditions++;
            }
            float angle = Quaternion.Angle(toRot, lerpRot);

            if(angle <= 1)
            {
                conditions++;
            }

            if (conditions == 2)
            {
                allowControl = true;

                //set stuff so the values are exact
                this.transform.position = new Vector3(selectedPos.x, selectedPos.y, -10);
                this.transform.rotation = toRot;
            }


        }
    }

    void setSnapTo()
    {
        cursorPosition = _cam.ScreenToWorldPoint(Input.mousePosition);

        Collider2D[] hits = Physics2D.OverlapCircleAll(cursorPosition, 1, gridSnapLayerMask);
        if (hits.Length == 0)
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
    }

    Vector2 Rotate(Vector2 v, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);
        return new Vector2(
            v.x * cos - v.y * sin,
            v.x * sin + v.y * cos
        );
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
        selectedCityBlock = selected.transform.parent.gameObject;
    }
}
