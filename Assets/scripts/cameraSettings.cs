using UnityEngine;

public class cameraSettings : MonoBehaviour
{
    [SerializeField] public Vector3 cameraCenter;
    [SerializeField] public float cameraZoom;
    [SerializeField] public float cameraSpeed;
    [SerializeField] public float limit_ZoomIn;
    [SerializeField] public float limit_ZoomOut;
    [SerializeField] public float relativeRotation;
    [SerializeField] public float limit_Left;
    [SerializeField] public float limit_Right;
    [SerializeField] public float limit_Top;
    [SerializeField] public float limit_Down;
}
