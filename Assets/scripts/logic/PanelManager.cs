using System;
using UnityEngine;

public class PanelManager : MonoBehaviour
{
    public GameObject panelPrefab;
    public GameObject bridgePrefab;
    public int numBlocks;

    public OverlayDefine[] overlayDefines;

    public GameObject BlocksGameObject;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        for (int i = 0; i < numBlocks; i++)
        {
            GameObject block = PanelGenerator.Generate(3, 6, panelPrefab, bridgePrefab, overlayDefines);
            block.transform.SetParent(BlocksGameObject.transform);

            Vector2 ranVec = UnityEngine.Random.insideUnitCircle * 4;


            block.transform.localPosition = ranVec;
            Rigidbody2D rb = block.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.useFullKinematicContacts = true;

            PanelData pd = block.AddComponent<PanelData>();
            pd.isDragged = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
