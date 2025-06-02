using UnityEngine;
using System.Collections.Generic;

public class snap : MonoBehaviour
{
    [SerializeField] public Dictionary<string, OverlayData> overlayMap;
    [SerializeField] public int x;
    [SerializeField] public int y;




    private SpriteRenderer overlaySprite;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        overlaySprite = transform.Find("Overlay").GetComponent<SpriteRenderer>();

        string snapName = gameObject.name;
        string[] snapNameSplit = snapName.Split("_");
        x = int.Parse(snapNameSplit[1]);
        y = int.Parse(snapNameSplit[2]);

        overlayMap = new Dictionary<string, OverlayData>();

        /*overlays = new overlayData[2];

        overlays[0] = new overlayData
        {
            overlayName = "Green",
            value = 0.5f,
            color = new Color(0f, 1f, 0f, 0.5f)
        };

        overlays[1] = new overlayData
        {
            overlayName = "Red",
            value = 1,
            color = new Color(1f, 0f, 0f, 0.5f)
        };*/
    }


    public void setOverlay(string name)
    {
        Debug.Log("setting " + name);

        if(!overlayMap.ContainsKey(name))
        {
            overlaySprite.color = new Color(1f, 1f, 1f, 0f);
        }
        else
        {
            Color c = overlayMap[name].color;
            Color w = Color.white;
            Color n = Color.Lerp(w, c, overlayMap[name].value);
            overlaySprite.color = new Color(n.r, n.g, n.b, 1f);
            Debug.Log(overlayMap[name].value);
        }
    }

    public void initOverlay(OverlayData data)
    {
        overlayMap.Add(data.overlayName, data);
    }


}



[System.Serializable]
public struct OverlayData
{
    public string overlayName;
    public float value;
    public Color color;

    public OverlayData(string name, float value, Color color)
    {
        this.overlayName = name;
        this.value = value;
        this.color = color;
    }
}
