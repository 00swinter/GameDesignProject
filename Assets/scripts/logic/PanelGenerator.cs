using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class PanelGenerator
{
    // 6 hex directions
    private static readonly Vector2[] directions = new Vector2[] {
        new Vector2(0.5f,  0.86603f),  // TR
        new Vector2(1f,     0f      ),  //  R
        new Vector2(0.5f, -0.86603f),   // BR
        new Vector2(-0.5f,-0.86603f),   // BL
        new Vector2(-1f,    0f      ),  //  L
        new Vector2(-0.5f,  0.86603f)   // TL
    };

    /// <summary>
    /// Generates a random hex‐cluster of panels, sticks bridges between neighbors,
    /// and recenters the “middle” panel at local (0,0).
    /// </summary>
    /// <param name="minPanels">minimum hex count</param>
    /// <param name="maxPanels">maximum hex count (inclusive)</param>
    /// <param name="panelPrefab">prefab for each hex tile</param>
    /// <param name="bridgePrefab">prefab for each stick (should lie along +X)</param>
    /// <param name="overlayDefines">data for coloring/type</param>
    public static GameObject Generate(
        int minPanels,
        int maxPanels,
        GameObject panelPrefab,
        GameObject bridgePrefab,
        OverlayDefine[] overlayDefines
    )
    {
        // 1) build your random shape in axial coords
        int hexCount = UnityEngine.Random.Range(minPanels, maxPanels + 1);
        var tiles = new HashSet<Vector2> { Vector2.zero };
        var walk = new List<Vector2> { Vector2.zero };

        while (tiles.Count < hexCount)
        {
            // pick a random existing tile
            Vector2 baseTile = walk[UnityEngine.Random.Range(0, walk.Count)];
            // pick one of the 6 directions
            Vector2 step = directions[UnityEngine.Random.Range(0, directions.Length)];
            Vector2 next = baseTile + step;

            if (tiles.Add(next))
                walk.Add(next);
        }

        // 2) make parent container
        GameObject blockGO = new GameObject("HexCluster");

        // 3) instantiate panels
        foreach (var pos in tiles)
        {
            var go = UnityEngine.Object.Instantiate(panelPrefab, blockGO.transform);
            go.transform.localPosition = pos;

            // assign random overlay
            int idx = UnityEngine.Random.Range(0, overlayDefines.Length);
            var def = overlayDefines[idx];
            var ps = go.GetComponent<PanelSettings>();
            ps.ptype = def.name;
            ps.color = def.color;
            ps.sprite = def.panelSprite;
        }

        // 4) stick‐bridges: only use first 3 directions to avoid duplicates
        for (int i = 0; i < 3; i++)
        {
            Vector2 dir = directions[i];
            foreach (var tile in tiles)
            {
                var nbr = tile + dir;
                if (!tiles.Contains(nbr)) continue;

                // midpoint in local‐space
                Vector2 mid = (tile + nbr) * 0.5f;
                var bridge = UnityEngine.Object.Instantiate(bridgePrefab, blockGO.transform);
                bridge.transform.localPosition = new Vector3(mid.x, mid.y, 0.1f);

                // rotate so +X of prefab points from tile→nbr
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                bridge.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
            }
        }

        // 5) find the “middle” tile (closest to the centroid)
        Vector2 centroid = tiles.Aggregate(Vector2.zero, (sum, v) => sum + v)
                          / tiles.Count;
        Vector2 centerTile = tiles
            .OrderBy(v => (v - centroid).sqrMagnitude)
            .First();

        // 6) recenter all children so that centerTile → (0,0)
        foreach (Transform child in blockGO.transform)
        {
            child.localPosition -= (Vector3)centerTile;
        }

        return blockGO;
    }
}
