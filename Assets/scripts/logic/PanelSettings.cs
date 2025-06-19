using NUnit.Framework.Constraints;
using UnityEngine;

public class PanelSettings : MonoBehaviour
{
    [SerializeField] public string ptype; //(same as the overlay Name)
    [SerializeField] public Color color;

    [SerializeField] public Sprite sprite;
    void Start()
    {
        SpriteRenderer sr = transform.GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            sr.sprite = sprite;
        }
        else
        {
            sr.color = color;
        }
        /*
        switch (ptype)
        {//"red", "blue", "green", "yellow", "purple", "orange"
            case "red":
                sr.color = Color.red;
                break;
            case "blue":
                sr.color = Color.blue;
                break;
            case "green":
                sr.color = Color.green;
                break;
            case "yellow":
                sr.color = Color.yellow;
                break;
            case "purple":
                sr.color = Color.purple;
                break;
            case "orange":
                sr.color = Color.orange;
                break;
        }
        */
    }
}
