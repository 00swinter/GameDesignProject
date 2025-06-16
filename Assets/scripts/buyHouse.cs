using UnityEngine;

public class buyHouseButton : MonoBehaviour
{
    public int money;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void buyHouse()
    {
        if (money >= 250)
        {
            money -= 250;
            
        }
    }
}
