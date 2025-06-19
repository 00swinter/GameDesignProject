using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class TrayManager : MonoBehaviour
{
    [Header("Alignment Settings")]
    [Tooltip("Direction (in local space) along which children will be laid out")]
    [SerializeField] private Vector2 alignDirection = Vector2.up;

    [Tooltip("Distance between each child")]
    [SerializeField] private float spacing = 1f;

    [Tooltip("How fast children slide into place (higher = snappier)")]
    [SerializeField] private float lerpSpeed = 5f;

    [Header("Tray View Bounds")]
    [Tooltip("Minimum local-axis position (items won’t go above this)")]
    [SerializeField] private float minVisible = 0f;

    [Tooltip("Maximum local-axis position (items won’t go below this)")]
    [SerializeField] private float maxVisible = 5f;  // in local units along alignDirection

    [Header("Scrolling")]
    [Tooltip("0 = top of tray; positive = scroll down")]
    [SerializeField, Min(0f)] public float scrollProgress = 0f;



    [Header("State")]
    [Tooltip("When true: tray follows camera. When false: tray flies out to inactiveOffset then sleeps.")]
    [SerializeField] public bool isActive = false;

    [Header("Offsets")]
    [Tooltip("Where the tray sits when active (camera-local)")]
    [SerializeField] private Vector3 activeOffset   = new Vector3(-2f, 0f, 0f);

    [Tooltip("Where the tray sits when inactive (world-space)")]
    [SerializeField] private Vector3 inactiveOffset = new Vector3(-10f, 0f, 0f);

    [Header("Animation")]
    [Tooltip("Speed of the tray movement (higher = faster)")]
    [SerializeField] private float BodyLerpSpeed = 5f;

    [Tooltip("How close before we consider “arrived”")]
    [SerializeField] private float arrivalThreshold = 0.05f;

    private Camera   _cam;
    private bool     sleep;

    void OnEnable()
    {
        _cam = Camera.main;
        if (_cam == null) Debug.LogError("TrayManager: no Camera.main!");
        // snap to start
        if (isActive)
            SnapToActive();
        else
        {
            SnapToInactive();
            sleep = true;
        }
    }

    void LateUpdate()
    {
        if (isActive)
        {
            // — ACTIVE: always follow camera + offset, never sleep —
            sleep = false;

            Vector3 targetPos = _cam.transform.TransformPoint(activeOffset);
            targetPos.z = 0;
            Quaternion targetRot = _cam.transform.rotation;

            transform.position = Vector3.Lerp(transform.position, targetPos, BodyLerpSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, BodyLerpSpeed * Time.deltaTime);
            TrayUpdate();
            return;
        }

        // — INACTIVE: if already slept after arrival, do nothing —
        if (sleep) return;

        // Fly out toward the fixed world-space inactiveOffset
        Vector3 worldTarget = inactiveOffset;
        transform.position = Vector3.Lerp(transform.position, worldTarget, BodyLerpSpeed * Time.deltaTime);
        // (Keep whatever rotation you had; no further rotation)

        // Once close enough, go to sleep
        if (Vector3.Distance(transform.position, worldTarget) < arrivalThreshold)
        {
            transform.position = worldTarget;  // snap exactly
            sleep = true;
        }
    }

    private void SnapToActive()
    {
        transform.position = _cam.transform.TransformPoint(activeOffset);
        transform.rotation = _cam.transform.rotation;
    }

    private void SnapToInactive()
    {
        transform.position = inactiveOffset;
        // leave rotation as-is
    }

    /// <summary>Call this to open/close the tray at runtime.</summary>
    public void SetActive(bool on)
    {
        isActive = on;
        if (on)
        {
            // wake up immediately and snap start
            sleep = false;
            SnapToActive();
        }
        else
        {
            // wake up for fly-out
            sleep = false;
        }
    }

    void TrayUpdate()
    {
        /*
        Vector3 camLocalOffset = new Vector3(activeOffset, 0f, 0f);

        // 2) Transform that into world‐space:
        Camera _cam = Camera.main;
        Vector3 worldPos = _cam.transform.TransformPoint(camLocalOffset);
        worldPos.z = -5;

        // 3) Apply position and match rotation:
        var t = transform;
        t.position = worldPos;
        t.rotation = _cam.transform.rotation;
*/



        // 1) Gather all draggable children
        var children = new List<Transform>();
        foreach (Transform t in transform)
            if (t.GetComponent<Rigidbody2D>() != null)
                children.Add(t);

        int count = children.Count;
        if (count == 0) return;

        // 2) Compute how many slots fit in view
        float viewSize = Mathf.Abs(maxVisible - minVisible);
        int visibleCount = Mathf.FloorToInt(viewSize / spacing) + 1;

        // 3) Clamp scrollProgress so you can't scroll past content
        float maxScroll = Mathf.Max(0, count - visibleCount+2);
        scrollProgress = Mathf.Clamp(scrollProgress, 0f, maxScroll);

        // 4) Sort by world-axis position (so drifted items keep order)
        Vector3 worldAxis = transform.TransformDirection(alignDirection.normalized);
        children = children
            .OrderBy(t => Vector3.Dot(t.position, worldAxis))
            .ToList();

        // 5) Compute scrolling offset in local space
        Vector3 localAxis = alignDirection.normalized;
        Vector3 scrollOffset = localAxis * (scrollProgress * spacing);

        // 6) Lerp each child into its (clamped) slot
        for (int i = 0; i < count; i++)
        {
            var child = children[i];
            if (child.GetComponent<PanelData>()?.isDragged == true)
                continue;

            // base position for slot i
            Vector3 rawTarget = localAxis * (i * spacing);
            // apply scroll
            Vector3 offsetTarget = rawTarget - scrollOffset;

            // clamp to visible window
            float proj = Vector3.Dot(offsetTarget, localAxis);
            proj = Mathf.Clamp(proj, minVisible, maxVisible);
            Vector3 clampedTarget = localAxis * proj;

            // smooth move
            child.localPosition = Vector3.Lerp(
                child.localPosition,
                clampedTarget,
                lerpSpeed * Time.deltaTime
            );
        }
    }
}