using UnityEngine;
using System.Collections.Generic;

public class snap : MonoBehaviour
{
    public Dictionary<string, OverlayData> overlayMap = new Dictionary<string, OverlayData>();
    [SerializeField] public int x;
    [SerializeField] public int y;





    private SpriteRenderer overlaySprite;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        overlaySprite = transform.Find("Overlay").GetComponent<SpriteRenderer>();
        

        string snapName = gameObject.name;
        string[] snapNameSplit = snapName.Split("_");
        x = int.Parse(snapNameSplit[1]);
        y = int.Parse(snapNameSplit[2]);
    }


    public void setOverlay(string name)
    {
        if (!overlayMap.ContainsKey(name))
        {
            overlaySprite.color = new Color(1f, 1f, 1f, 0f);
        }
        else
        {
            Color c = overlayMap[name].color;
            Color w = Color.white;
            Color n = Color.Lerp(w, c, overlayMap[name].value);
            overlaySprite.color = new Color(n.r, n.g, n.b, 0.5f);
        }
    }

    public void createOverlay(OverlayData data)
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
