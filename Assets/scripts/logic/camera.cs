using UnityEngine;
using System.Linq;
using Unity.VisualScripting;
using TMPro;

public class camera : MonoBehaviour
{



    [SerializeField] private LayerMask cityBlockLayerMask;
    [SerializeField] private LayerMask solarPanelLayerMask;
    [SerializeField] private LayerMask gridSnapLayerMask;

    [SerializeField] private LayerMask trayLayerMask;

    [SerializeField] public GameObject pointsTextUi;
	[SerializeField] public GameObject pointsTextUiHouse;
	[SerializeField] public GameObject pointsTextUiShop;
	[SerializeField] public float gesamtPoints;

    [SerializeField] public GameObject HouseScreen;
    [SerializeField] public GameObject ShopScreen;

    [SerializeField] private GameObject selectedCityBlock;
    [SerializeField] private overlayManager selectedOverlayManager;
    [SerializeField] private cityBlockData selectedCityBlockData;

    [SerializeField] private int gameSeed;

    [SerializeField] private cameraSettings currentCameraSettings;

    [SerializeField] public OverlayDefine[] overlayDefines;

    [SerializeField] private bool allowControl = true;

    private Camera _cam;
    private GameObject _dragged;
    private bool draggedWasOnGrid;
    private Quaternion _draggedRotation;
    private Vector3 _offset;
    private Vector3 _startPos;
    private Quaternion _startRot;

    [SerializeField] private GameObject snapTo = null;
    [SerializeField] private Vector3 cursorPosition = Vector3.zero;


    void Awake()
    {
        _cam = this.gameObject.GetComponent<Camera>();

        UnityEngine.Random.InitState(gameSeed);

        //find all OverlayManager
        overlayManager[] allOverlayManager = FindObjectsByType<overlayManager>(FindObjectsSortMode.None);
        foreach (overlayManager om in allOverlayManager)
        {
            om.gameSeed = gameSeed;
            om.overlayDefines = overlayDefines;
        }

        PanelManager[] allPanelManagers = FindObjectsByType<PanelManager>(FindObjectsSortMode.None);
        foreach (PanelManager pm in allPanelManagers)
        {
            pm.overlayDefines = overlayDefines;
        }
        ScreenToTP1();

	}

    void Update()
    {
        //Debug.Log(IsPointerOverTray());

        //select what camera settings to use
        if (selectedCityBlock != null)
        {// make faster maybe
            currentCameraSettings = selectedCityBlock.GetComponent<cameraSettings>();
        }
        else
        {
            currentCameraSettings = this.GetComponent<cameraSettings>();
        }

        if (allowControl)
        {
            playerControllLogic();
        }
        else
        {
            automaticControllLogic();
        }
    }

    void playerControllLogic()
    {
        if (selectedCityBlock != null)
        {
            //place blocks
            blockPlacingLogic();
            overlayControllLogic();
            trayMovingLogic();
        }
        else
        {
            bigMapInteractionLogic();
        }

        playerCameraLogic();
    }

    void blockPlacingLogic()
    {
        // ——— Mouse Down: begin drag ———
        if (Input.GetMouseButtonDown(0) && _dragged == null)
        {
            Vector2 wp = _cam.ScreenToWorldPoint(Input.mousePosition);
            //click another cityblock
            // 2a) Raycast at that point, but only against blockLayerMask
            //    - distance = 0 forces a “point check”
            RaycastHit2D hitCity = Physics2D.Raycast(
                wp,
                Vector2.zero,
                0f,
                cityBlockLayerMask
            );

            if (hitCity.collider != null && !IsPointerOverTray())
            {
                if (hitCity.collider.transform.parent.transform != selectedCityBlock.transform)
                {
                    //clicked a different cityblock
                    deselectCityBlock();
                    SelectObject(hitCity.collider.gameObject);
                    allowControl = false;
                    return;
                }
            }


            //click blocks
            RaycastHit2D hit = Physics2D.Raycast(wp, Vector2.zero, Mathf.Infinity, solarPanelLayerMask);
            if (hit.collider != null)
            {
                _dragged = hit.collider.gameObject.transform.parent.gameObject;

                _dragged.GetComponent<PanelData>().isDragged = true;
                if (_dragged.transform.parent.transform == selectedCityBlockData.OnGrid.transform)
                {
                    draggedWasOnGrid = true;
                }
                else
                {
                    draggedWasOnGrid = false;
                }

                _startPos = _dragged.transform.position;
                _startRot = _dragged.transform.rotation;

                //get the snap below
                setSnapTo();

                if (_dragged.transform.parent == selectedCityBlockData.OnGrid.transform)
                {
                    _draggedRotation = _dragged.transform.rotation * Quaternion.Inverse(snapTo.transform.rotation);
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
                Vector3 lerpPos = QuadraticEaseIn(_dragged.transform.position, snapPos, 12f * Time.deltaTime);
                _dragged.transform.position = new Vector3(lerpPos.x, lerpPos.y, -5);


                Quaternion fromRot = _dragged.transform.rotation;
                Quaternion toRot = snapTo.transform.rotation * _draggedRotation;


                Quaternion lerpRot = Quaternion.Lerp(fromRot, toRot, 25f * Time.deltaTime);

                _dragged.transform.rotation = lerpRot;
            }
            else
            {

                Vector3 lerpPos = Vector3.Lerp(_dragged.transform.position, cursorPosition + _offset, 6f * Time.deltaTime);
                _dragged.transform.position = new Vector3(lerpPos.x, lerpPos.y, -5);


                Quaternion fromRot = _dragged.transform.rotation;
                Quaternion toRot = Quaternion.identity * _draggedRotation;


                Quaternion lerpRot = Quaternion.Lerp(fromRot, toRot, 12f * Time.deltaTime);

                _dragged.transform.rotation = lerpRot;
            }


            updatePoints();
        }

        // ——— Mouse Up: drop ———
        if (Input.GetMouseButtonUp(0) && _dragged != null)
        {
            _dragged.GetComponent<PanelData>().isDragged = false;
            if (snapTo == null)
            {
                //to tray
                _dragged.transform.SetParent(selectedCityBlockData.tray.transform);
                _dragged.transform.rotation = _draggedRotation;


                //_dragged.transform.position = new Vector3(cursorPosition.x, cursorPosition.y, -1);

            }
            else
            {
                if (IsPointerOverTray())
                {
                    //to tray
                    _dragged.transform.SetParent(selectedCityBlockData.tray.transform);
                    _dragged.transform.rotation = _draggedRotation;
                }
                else
                {

                    //to snap
                    _dragged.transform.position = new Vector3(snapTo.transform.position.x, snapTo.transform.position.y, -1);
                    _dragged.transform.rotation = snapTo.transform.rotation * _draggedRotation;

                    if (blockIsColliding())
                    {
                        if (draggedWasOnGrid)
                        {
                            _dragged.transform.position = _startPos;
                            _dragged.transform.rotation = _startRot;
                        }
                        else
                        {
                            _dragged.transform.rotation = _startRot;
                            _dragged.transform.SetParent(selectedCityBlockData.tray.transform);
                        }
                    }
                    else
                    {
                        _dragged.transform.SetParent(selectedCityBlockData.OnGrid.transform);
                    }

                }


            }
            //Debug.Log(blockIsColliding());
            _dragged = null;
            updatePoints();
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

		//------- F to leave Cityblock View -------
		if (Input.GetKeyDown(KeyCode.F))
        {
            deselectCityBlock();
        }

	}

    void overlayControllLogic()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            selectedOverlayManager.setOverlay("none");
            selectedCityBlockData.lastUsedOverlay = "none";
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            string overlayName = selectedOverlayManager.overlayDefines[0].name;
            selectedOverlayManager.setOverlay(overlayName);
            selectedCityBlockData.lastUsedOverlay = overlayName;

        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            string overlayName = selectedOverlayManager.overlayDefines[1].name;
            selectedOverlayManager.setOverlay(overlayName);
            selectedCityBlockData.lastUsedOverlay = overlayName;
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            string overlayName = selectedOverlayManager.overlayDefines[2].name;
            selectedOverlayManager.setOverlay(overlayName);
            selectedCityBlockData.lastUsedOverlay = overlayName;
        }
    }

    void trayMovingLogic()
    {
        //scroll tray
        if (IsPointerOverTray() | true)
        {
            float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
            selectedCityBlockData.tray.GetComponent<TrayManager>().scrollProgress += scrollDelta * 3;
        }
    }
    void bigMapInteractionLogic()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // 1) Mouse → world point
            Vector2 worldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            Debug.Log("BIG INTERACTION HIT");
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

    void playerCameraLogic()
    {
        // Read input axes and mousewheel
        float h = Input.GetAxisRaw("Horizontal");   //a,d
        float v = Input.GetAxisRaw("Vertical");     //w,s
        float scrollDelta = Input.GetAxis("Mouse ScrollWheel");



        Vector3 newCamPos = transform.position;

        //transform to origin
        newCamPos = newCamPos - currentCameraSettings.cameraCenter;

        //undo rotation
        newCamPos = Rotate(newCamPos, -currentCameraSettings.relativeRotation);

        //delta
        Vector3 delta = new Vector3(h, v, 0f) * currentCameraSettings.cameraSpeed * Time.deltaTime;

        //add delte
        newCamPos = newCamPos + delta;

        //clamp

        newCamPos = new Vector3(
                        Mathf.Clamp(newCamPos.x, currentCameraSettings.limit_Left, currentCameraSettings.limit_Right),
                        Mathf.Clamp(newCamPos.y, currentCameraSettings.limit_Down, currentCameraSettings.limit_Top),
                        -10
                        );

        //rotate back
        newCamPos = Rotate(newCamPos, currentCameraSettings.relativeRotation);

        //set z  to -10
        newCamPos.z = -10;

        //transform back
        newCamPos = newCamPos + currentCameraSettings.cameraCenter;

        //apply
        transform.position = newCamPos;


        //calculate zoom
        float currentZoom = _cam.orthographicSize;
        float newZoom = currentZoom;
        if (!IsPointerOverTray())
        {
            newZoom = currentZoom - (scrollDelta * (currentCameraSettings.cameraZoomSpeed * (currentZoom / 50)));
        }

        //clamp zoom
        newZoom = Mathf.Clamp(newZoom, currentCameraSettings.limit_ZoomIn, currentCameraSettings.limit_ZoomOut);

        // Apply zoom

        _cam.orthographicSize = newZoom;
    }

    void automaticControllLogic()
    {
        int conditions = 0;
        //auto movement

        //lerp position
        Vector2 camPos = this.transform.position;
        Vector2 selectedPos = currentCameraSettings.cameraCenter;
        Vector2 lerpPos = Vector2.Lerp(camPos, selectedPos, 12f * Time.deltaTime);
        this.transform.position = new Vector3(lerpPos.x, lerpPos.y, -10);

        //check pos
        float dist = (selectedPos - lerpPos).magnitude;
        if (dist <= 0.1f)
        {
            conditions++;
        }


        //lerp rotation
        Quaternion fromRot = this.transform.rotation;
        Quaternion toRot = Quaternion.Euler(0f, 0f, currentCameraSettings.relativeRotation);
        Quaternion lerpRot = Quaternion.Lerp(fromRot, toRot, 12f * Time.deltaTime);
        this.transform.rotation = lerpRot;

        //check rot
        float angle = Quaternion.Angle(toRot, lerpRot);
        if (angle <= 0.1f)
        {
            conditions++;
        }

        //lerp Zoom

        float fromZoom = _cam.orthographicSize;
        float toZoom = currentCameraSettings.cameraZoom;
        float lerpZoom = Mathf.Lerp(fromZoom, toZoom, 12f * Time.deltaTime);
        _cam.orthographicSize = lerpZoom;

        //check zoom
        float zoomDif = toZoom - lerpZoom;
        if (Mathf.Abs(zoomDif) <= 0.1f)
        {
            conditions++;
        }



        if (conditions == 3)
        {
            allowControl = true;

            //set stuff so the values are exact
            this.transform.position = new Vector3(selectedPos.x, selectedPos.y, -10);
            this.transform.rotation = toRot;
            _cam.orthographicSize = toZoom;
        }
    }

    bool blockIsColliding()
    {

        Physics2D.SyncTransforms();


        Rigidbody2D rb = _dragged.GetComponent<Rigidbody2D>();
        bool blocks = rb.IsTouchingLayers(solarPanelLayerMask);
        bool walls = false;


        // loop over every child (and the root itself, if it has children)
        foreach (Transform part in _dragged.transform)
        {
            if (part.GetComponent<PanelSettings>() == null)
                continue;

            Vector2 worldPoint = part.position;

            // check for any collider on the grid‐snap layer at this exact point
            Collider2D hit = Physics2D.OverlapCircle(worldPoint, 0.01f, gridSnapLayerMask);
            Debug.Log(hit);
            // if there’s no collider, or it isn’t a Snap, we’re off the grid
            if (hit == null || hit.GetComponent<snap>() == null)
            {
                walls = true;
            }
        }

        bool cityBlock = false;
        //Debug.Log();
        //Debug.Log();

        if (snapTo.transform.parent.parent.transform != selectedCityBlock.transform)
        {
            cityBlock = true;
        }




        return walls || blocks || cityBlock;
    }

    void setSnapTo()
    {
        cursorPosition = _cam.ScreenToWorldPoint(Input.mousePosition);

        Collider2D[] hits = Physics2D.OverlapCircleAll(cursorPosition, 0.5f, gridSnapLayerMask);
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

    void updatePoints()
    {
        //loop all snappings
        snap[] snapScripts = selectedCityBlock.transform
            .Find("HexGridRoot")
            .Cast<Transform>()
            .Select(t => t.GetComponent<snap>()).Where(c => c != null)
            .ToArray();

        //Physics2D.SyncTransforms();

        float punkte = 0f;

        foreach (snap s in snapScripts)
        {
            //find panel above
            float value = 0f;
            RaycastHit2D hit = Physics2D.Raycast(s.transform.position, Vector2.zero, Mathf.Infinity, solarPanelLayerMask);
            if (hit)
            {
                PanelSettings ps = hit.collider.GetComponent<PanelSettings>();
                if (ps.ptype == "normal")
                {
                    //calc normal
                    int overlayCount = s.overlayMap.ToArray().Length;
                    float sum = 0;
                    foreach (var kvp in s.overlayMap.ToArray())
                    {
                        OverlayData o = kvp.Value;
                        sum += o.value;
                    }

                    value = 10 * (1 - (sum / overlayCount));
                }
                else
                {
                    //the rest
                    OverlayData od = s.overlayMap[ps.ptype];
                    if (od.value > 0)
                    {
                        value = od.value * 10;
                    }
                    else
                    {
                        value = -od.value * 10;
                    }
                }

            }


            //calc points
            if (value > 0)
            {
                punkte += value;
            }


        }

        //set in cityblockdata
        selectedCityBlockData.points = punkte;

        // update points ui
        pointsTextUi.GetComponent<TextMeshProUGUI>().text = Mathf.CeilToInt(punkte).ToString() + " Points in this block";

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
        return Vector3.Lerp(start, end, t * 1 / (dist * dist));
        // or: return start + (end - start) * quad;
    }

    private void SelectObject(GameObject selected)
    {
        GameObject sel = selected.transform.parent.gameObject;
        cityBlockData cbd = sel.GetComponent<cityBlockData>();
        if (!cbd.isUnlocked)
        {
            return;
        }

        selectedCityBlock = sel;

        //set overlayManager to use
        selectedOverlayManager = selectedCityBlock.GetComponent<overlayManager>();

        //set cityBlockData to use
        selectedCityBlockData = cbd;

        //set last used overlay
        selectedOverlayManager.setOverlay(selectedCityBlockData.lastUsedOverlay);

        //set tray to aktive
        selectedCityBlockData.tray.GetComponent<TrayManager>().isActive = true;

        updatePoints();
    }

    public void deselectCityBlock()
    {
        if (selectedCityBlock == null)
        {
            return;
        }
        //set tray to inactive
        selectedCityBlockData.tray.GetComponent<TrayManager>().isActive = false;
        //set overlay to none and maybe reduce opacity of all things in there
        selectedOverlayManager.setOverlay("none");
        selectedCityBlock = null;
        allowControl = false;

        //update points ui
        var allCBD = FindObjectsByType<cityBlockData>(FindObjectsSortMode.None);
        float points = 0;
        foreach (var cbd in allCBD)
        {
            points += cbd.points;
        }
        gesamtPoints = points;
        pointsTextUi.GetComponent<TextMeshProUGUI>().text = Mathf.CeilToInt(points).ToString() + " Points Total";
		pointsTextUiHouse.GetComponent<TextMeshProUGUI>().text = Mathf.CeilToInt(points).ToString() + " Points";
		pointsTextUiShop.GetComponent<TextMeshProUGUI>().text = Mathf.CeilToInt(points).ToString() + " Points";


	}

    private bool IsPointerOverTray()
    {
        Vector2 wp = _cam.ScreenToWorldPoint(Input.mousePosition);
        // returns true if the point overlaps the tray’s BoxCollider2D
        return Physics2D.OverlapPoint(wp, trayLayerMask) != null;
    }

    public void ScreenToTP1()
    {
		deselectCityBlock();
		GameObject TP1 = GameObject.FindGameObjectWithTag("Theaterplatz 1");
        GameObject child = TP1.transform.Find("SpriteAndCollider").gameObject;
		SelectObject(child);
        HouseScreen.SetActive(false);
        ShopScreen.SetActive(false);
    }
	public void ScreenToTP7()
	{
		deselectCityBlock(); 
        GameObject TP7 = GameObject.FindGameObjectWithTag("Theaterplatz 7");
		GameObject child = TP7.transform.Find("SpriteAndCollider").gameObject;
		SelectObject(child);
		HouseScreen.SetActive(false);
		ShopScreen.SetActive(false);
	}
	public void ScreenToSM2()
	{
		deselectCityBlock(); 
        GameObject SM2 = GameObject.FindGameObjectWithTag("Salzmarkt 2");
		GameObject child = SM2.transform.Find("SpriteAndCollider").gameObject;
		SelectObject(child);
		HouseScreen.SetActive(false);
		ShopScreen.SetActive(false);
	}
	public void ScreenToSM4()
	{
		deselectCityBlock(); 
        GameObject SM4 = GameObject.FindGameObjectWithTag("Salzmarkt 4");
		GameObject child = SM4.transform.Find("SpriteAndCollider").gameObject;
		SelectObject(child);
		HouseScreen.SetActive(false);
		ShopScreen.SetActive(false);
	}
	public void ScreenToKJG5()
	{
		deselectCityBlock(); 
        GameObject KJG5 = GameObject.FindGameObjectWithTag("Johannisgasse 5");
		GameObject child = KJG5.transform.Find("SpriteAndCollider").gameObject;
		SelectObject(child);
		HouseScreen.SetActive(false);
		ShopScreen.SetActive(false);
	}

	public void ScreenToKJG6()
	{
		deselectCityBlock(); 
        GameObject KJG6 = GameObject.FindGameObjectWithTag("Johannisgasse 6");
		GameObject child = KJG6.transform.Find("SpriteAndCollider").gameObject;
		SelectObject(child);
		HouseScreen.SetActive(false);
		ShopScreen.SetActive(false);
	}

	public void ScreenToSG24()
	{
		deselectCityBlock(); 
        GameObject SG24 = GameObject.FindGameObjectWithTag("Spitalgasse 24");
		GameObject child = SG24.transform.Find("SpriteAndCollider").gameObject;
		SelectObject(child);
		HouseScreen.SetActive(false);
		ShopScreen.SetActive(false);
	}

	public void ScreenToKJG8()
	{
		deselectCityBlock(); 
        GameObject KJG8 = GameObject.FindGameObjectWithTag("Johannisgasse 8");
		GameObject child = KJG8.transform.Find("SpriteAndCollider").gameObject;
		SelectObject(child);
		HouseScreen.SetActive(false);
		ShopScreen.SetActive(false);
	}
}
