using UnityEngine;
using System.Linq;
using System;

public class overlayManager : MonoBehaviour
{
    //the scripts
    [SerializeField] snap[] snapScripts;
    
    void Start()
    {
        snapScripts = transform
            .Find("HexGridRoot")
            .Cast<Transform>()
            .Select(t => t.GetComponent<snap>()).Where(c => c != null)
            .ToArray();

        NoiseTexture nt = new NoiseTexture(2500, 2500, 50f, 2232);

        foreach (snap s in snapScripts)
        {
            float x = s.x;
            float y = s.y;
            Debug.Log(x);
            Debug.Log(y);
            float steps = 5f;
            float value = (1 - nt.SampleUV(x / 25, y / 15));

            float roundedValue = Mathf.Round(value * steps) / steps;

            float clampedValue = (roundedValue >= 0.6f ? roundedValue : 0f);

            OverlayData data = new OverlayData("test", clampedValue, Color.green);;
            s.initOverlay(data);
        }
        SpriteRenderer sp = transform.GetComponent<SpriteRenderer>();
        sp.sprite = nt.CreateSprite();
        runSetOverlay();
    }


    
    public void setOverlay(string name)
    {
        foreach (snap s in snapScripts)
        {
            s.setOverlay(name);
        }
    }

    [ContextMenu("run setOverlay")]
    void runSetOverlay()
    {
        setOverlay("test");
    }
}
