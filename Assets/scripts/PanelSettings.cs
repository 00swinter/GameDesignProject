using UnityEngine;

public class PanelSettings : MonoBehaviour
{
    [SerializeField] public Vector3 cameraCenter;
    [SerializeField] public float cameraSpeed = 30;
    [SerializeField] public float cameraZoom = 15;
    [SerializeField] public float cameraZoomSpeed = 5;
    [SerializeField] public float limit_ZoomIn = 4;
    [SerializeField] public float limit_ZoomOut = 25;
    [SerializeField] public float relativeRotation = 0;
    [SerializeField] public float limit_Left = -100;
    [SerializeField] public float limit_Right = 100;
    [SerializeField] public float limit_Top = 50;
    [SerializeField] public float limit_Down = -50;
}
