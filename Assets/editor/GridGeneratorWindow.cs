using UnityEngine;
using UnityEditor;

public class GridGeneratorWindow : EditorWindow
{
    private GameObject prefabToSpawn;
    private int rows = 5;
    private int columns = 5;
    private Vector3 spacing = new Vector3(1f, 0.866f, 0f); // horizontal and vertical center-to-center distances
    private Vector3 startPosition = Vector3.zero;
    private string parentName = "HexGridRoot";
    private string childName = "snapA";

    [MenuItem("Tools/Hex Grid Generator")]
    public static void ShowWindow()
    {
        GetWindow<GridGeneratorWindow>("Hex Grid Generator");
    }

    void OnGUI()
    {
        GUILayout.Label("Hex Grid Settings", EditorStyles.boldLabel);

        prefabToSpawn = (GameObject)EditorGUILayout.ObjectField(
            "Cell Prefab", prefabToSpawn,
            typeof(GameObject), false
        );

        rows = EditorGUILayout.IntField("Rows", rows);
        columns = EditorGUILayout.IntField("Columns", columns);
        spacing = EditorGUILayout.Vector3Field("Spacing (X, Y, Z)", spacing);
        startPosition = EditorGUILayout.Vector3Field("Start Position", startPosition);
        parentName = EditorGUILayout.TextField("Parent Name", parentName);
        childName = EditorGUILayout.TextField("Child Name", childName);

        GUI.enabled = prefabToSpawn != null;
        if (GUILayout.Button("Generate Hex Grid"))
            GenerateHexGrid();
        GUI.enabled = true;
    }

    private void GenerateHexGrid()
    {
        // Create parent for grid
        GameObject parent = new GameObject(parentName);
        Undo.RegisterCreatedObjectUndo(parent, "Create Hex Grid Root");

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                // For pointy-top hex, offset every other row by half horizontal spacing
                float xOffset = (r % 2 == 0) ? 0f : spacing.x * 0.5f;

                Vector3 pos = startPosition + new Vector3(
                    c * spacing.x + xOffset,
                    r * spacing.y,
                    spacing.z
                );

                // Instantiate prefab preserving prefab link
                GameObject cell = (GameObject)PrefabUtility.InstantiatePrefab(prefabToSpawn);
                Undo.RegisterCreatedObjectUndo(cell, "Spawn Hex Cell");

                cell.name = $"{childName}_{c}_{r}";
                cell.transform.SetParent(parent.transform);
                cell.transform.position = pos;
            }
        }
    }
}
