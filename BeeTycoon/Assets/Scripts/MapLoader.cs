using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapLoader : MonoBehaviour
{
    public int mapWidth = 20;
    public int mapHeight = 20;
    private int foliageDensityMin = 5;
    private int foliageDensityMax = 12;

    private GameObject[,] tiles;

    [SerializeField]
    private GameObject grassTile;
    [SerializeField]
    private GameObject outOfBoundsTile;
    [SerializeField]
    private GameObject outOfBoundsTree;
    [SerializeField]
    private GameObject grass;
    [SerializeField]
    private GameObject mushroom;

    void Start()
    {
        tiles = new GameObject[mapWidth, mapHeight];
        GeneratePlot();
    }

    void Update()
    {
        
    }

    private void GeneratePlot()
    {
        for (int i = 0; i < mapHeight; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                if (i >= mapWidth / 4f && i < 3 * (mapWidth / 4f)
                    && j >= mapHeight / 4f && j < 3 * (mapHeight / 4f))
                    Instantiate(grassTile, new Vector3(i * 2, 0, j * 2), Quaternion.identity);
                else
                {
                    Instantiate(outOfBoundsTile, new Vector3(i * 2, 0, j * 2), Quaternion.identity);

                    //Generate Trees
                    Vector3 rotation = new Vector3(0, Random.Range(0, 360), 0);
                    Vector3 offset = new Vector3(Random.Range(-0.2f, 0.2f), 0, Random.Range(-0.2f, 0.2f));
                    Instantiate(outOfBoundsTree, new Vector3(i * 2, 0, j * 2) + offset, Quaternion.Euler(rotation));
                }
            }
        }

        GenerateFoliage();
    }

    private void GenerateFoliage()
    {
        for (int i = 0; i < mapHeight; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                int foliageDensity = Random.Range(foliageDensityMin, foliageDensityMax);
                for (int k = 0; k < foliageDensity; k++)
                {
                    Vector3 rotation = new Vector3(0, Random.Range(0, 360), 0);
                    Vector3 offset = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
                    Instantiate(grass, new Vector3(i * 2, 0, j * 2) + offset, Quaternion.Euler(rotation));
                }

                if (Random.Range(0, 5) == 0)
                {
                    Vector3 rotation = new Vector3(0, Random.Range(0, 360), 0);
                    Vector3 offset = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
                    Instantiate(mushroom, new Vector3(i * 2, 0, j * 2) + offset, Quaternion.Euler(rotation));
                }
            }
        }
    }
}
