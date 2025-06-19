using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;

public class overlayManager : MonoBehaviour
{
    //the scripts
    [SerializeField] snap[] snapScripts;
    [SerializeField] public int gameSeed;

    [SerializeField] public OverlayDefine[] overlayDefines;


    [Header("Grid Settings")]
    [SerializeField] public int gridSizeX;
    [SerializeField] public int gridSizeY;


    void Start()
    {

        //find all snapScripts in the cityblock
        snapScripts = transform
            .Find("HexGridRoot")
            .Cast<Transform>()
            .Select(t => t.GetComponent<snap>()).Where(c => c != null)
            .ToArray();

        //create the noiseTexture in the overlay defines
        for (int i = 0; i < overlayDefines.Length; i++)
        {
            var od = overlayDefines[i];
            od.nt = new NoiseTexture(gridSizeX * 2, gridSizeY * 2, 5f, gameSeed + (UnityEngine.Random.Range(0, 1000)));
            overlayDefines[i] = od;
        }

        //init the snapping point values

        foreach (OverlayDefine od in overlayDefines)
        {
            foreach (snap s in snapScripts)
            {
                float x = s.x;
                float y = s.y;
                float steps = 5f;
                float value = 1 - od.nt.SampleUV(x / gridSizeX, y / gridSizeY);

                float roundedValue = Mathf.Round(value * steps) / steps;

                float clampedValue = (roundedValue >= 0.6f ? roundedValue : 0f);
                OverlayData data = new OverlayData(od.name, clampedValue, od.color);
                s.createOverlay(data);
            }
        }
    }
    public void setOverlay(string name)
    {
        foreach (snap s in snapScripts)
        {
            s.setOverlay(name);
        }
    }
}

[System.Serializable]
public struct OverlayDefine
{
    public NoiseTexture nt;
    public int seedOffset;
    public string name;
    public Color color;
    public int gridSizeX;
    public int gridSizeY;
    public Sprite panelSprite;
}
