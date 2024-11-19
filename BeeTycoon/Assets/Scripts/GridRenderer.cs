using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridRenderer : MonoBehaviour
{
    private List<GameObject> lines = new List<GameObject>(); // A list of all lines
    private float targetWidth = 0f; // The target width of all lines
    private float lineWidth = 0f; // The set width of all lines

    [SerializeField]
    GameObject lineObject;
    //[SerializeField]
    //PlayerController player;
    //[SerializeField]
    //TowerMenu menu;
    [SerializeField]
    MapLoader loader;

    // Start is called before the first frame update
    void Start()
    {
        int x = -1; int z = -1; // Position variables

        // FOR JOHN: IDK how to get the grid width and height dynamically but you can do that here
        for (int i = 0; i <= loader.mapWidth; i++)
        {
            for (int j = 0; j <= loader.mapHeight; j++)
            {
                lines.Add(createLine(lineObject, x + i * 2, z, x + i * 2, z + loader.mapHeight * 2, lineWidth)); // Vertical lines
                lines.Add(createLine(lineObject, x, z + j * 2, x + loader.mapWidth * 2, z + j * 2, lineWidth)); // Horizontal lines
            }
        }
    }

    static GameObject createLine(GameObject lineObject, float x1, float z1, float x2, float z2, float y)
    {
        GameObject line = Instantiate(lineObject, new Vector3(0, 0, 0), Quaternion.identity);
        line.GetComponent<LineRenderer>().SetPosition(0, new Vector3(x1, y, z1));
        line.GetComponent<LineRenderer>().SetPosition(1, new Vector3(x2, y, z2));

        return (line);
    }

    // Update is called once per frame
    void Update()
    {
        //targetWidth = !menu.Active ? 0f : 0.1f;
        targetWidth = 0.2f;
        lineWidth += (targetWidth - lineWidth) / 8;

        foreach (GameObject line in lines)
        {
            line.GetComponent<LineRenderer>().startWidth = lineWidth;
            line.GetComponent<LineRenderer>().endWidth = lineWidth;
        }
    }
}
