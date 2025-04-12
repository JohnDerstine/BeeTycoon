using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapLoader : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> flowerList;

    public int mapWidth = 10;
    public int mapHeight = 10;
    private int foliageDensityMin = 5;
    private int foliageDensityMax = 12;

    public Tile[,] tiles;

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

    public Dictionary<FlowerType, float> nectarGains = new Dictionary<FlowerType, float>();

    void Start()
    {
        tiles = new Tile[mapWidth, mapHeight];
        GeneratePlot();

        var values = System.Enum.GetValues(typeof(FlowerType));
        foreach (var v in values)
        {
            FlowerType fType = (FlowerType)v;
            nectarGains.Add(fType, 0);
        }
    }

    void Update()
    {

    }

    private void GeneratePlot()
    {
        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                //if (i >= mapWidth / 4f && i < 3 * (mapWidth / 4f)
                //    && j >= mapHeight / 4f && j < 3 * (mapHeight / 4f))
                //{
                GameObject temp = Instantiate(grassTile, new Vector3(i * 2, 0, j * 2), Quaternion.identity);
                tiles[i, j] = temp.GetComponent<Tile>();
                tiles[i, j].Flower = (FlowerType)0;
                //}
                //else
                //{
                //    Instantiate(outOfBoundsTile, new Vector3(i * 2, 0, j * 2), Quaternion.identity);

                //    //Generate Trees
                //    Vector3 rotation = new Vector3(0, Random.Range(0, 360), 0);
                //    Vector3 offset = new Vector3(Random.Range(-0.2f, 0.2f), 0, Random.Range(-0.2f, 0.2f));
                //    Instantiate(outOfBoundsTree, new Vector3(i * 2, 0, j * 2) + offset, Quaternion.Euler(rotation));
                //}
            }
        }

        GenerateBorder();
        GenerateFoliage();
        GenerateFlowers();
    }

    private void GenerateBorder()
    {
        //spawn border tiles along edge of map for out-of-bounds area
        for (int i = 0; i < mapWidth; i += 2)
        {
            for (int x = 0; x < mapWidth + i + 2; x++)
            {
                Vector3 offset = new Vector3(Random.Range(-0.2f, 0.2f), 0, Random.Range(-0.2f, 0.2f));
                int rotatationRand = Random.Range(0, 360);
                Instantiate(outOfBoundsTile, new Vector3(mapWidth * 2 - x * 2 + i, 0, -2 - i), Quaternion.identity);
                Instantiate(outOfBoundsTree, new Vector3(mapWidth * 2 - x * 2 + i, 0, -2 - i) + offset, Quaternion.Euler(0, rotatationRand, 0));
                Instantiate(outOfBoundsTile, new Vector3(mapWidth * 2 - x * 2 + i, 0, mapHeight * 2 + i), Quaternion.identity);
                Instantiate(outOfBoundsTree, new Vector3(mapWidth * 2 - x * 2 + i, 0, mapHeight * 2 + i) + offset, Quaternion.Euler(0, rotatationRand, 0));
            }
            for (int y = 0; y < mapHeight + i + 2; y++)
            {
                Vector3 offset = new Vector3(Random.Range(-0.2f, 0.2f), 0, Random.Range(-0.2f, 0.2f));
                int rotatationRand = Random.Range(0, 360);
                Instantiate(outOfBoundsTile, new Vector3(-2 - i, 0, mapHeight * 2 - y * 2 + i), Quaternion.identity);
                Instantiate(outOfBoundsTree, new Vector3(-2 - i, 0, mapHeight * 2 - y * 2 + i) + offset, Quaternion.Euler(0, rotatationRand, 0));
                Instantiate(outOfBoundsTile, new Vector3(mapWidth * 2 + i, 0, mapHeight * 2 - y * 2 + i), Quaternion.identity);
                Instantiate(outOfBoundsTree, new Vector3(mapWidth * 2 + i, 0, mapHeight * 2 - y * 2 + i) + offset, Quaternion.Euler(0, rotatationRand, 0));
            }
        }
    }

    private void GenerateFoliage()
    {
        for (int i = 0; i < mapWidth; i++)
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

    public void GenerateFlowers()
    {
        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                if (Random.Range(0, 1) == 0) //0
                {
                    int rand = Random.Range(1, 2); //5
                    tiles[i, j].Flower = (FlowerType)rand;
                    Instantiate(flowerList[rand], tiles[i, j].transform.position, Quaternion.identity);
                }
            }
        }
    }

    public int GetFlowerCount()
    {
        int count = 0;
        for (int i = 0; i < mapWidth; i++)
            for (int j = 0; j < mapHeight; j++)
                if (tiles[i, j].Flower != FlowerType.Empty)
                    count++;
        return count;
    }

    public void GetNectarGains()
    {
        //Reset all values to 0
        ResetNectarGains();

        GetCloverValue(); //Make these methois coroutines
        GetAlfalfaValue();
        GetBuckwheatValue();
        GetFireweedValue();
        GetGoldenrodValue();
    }

    private void ResetNectarGains()
    {
        var values = System.Enum.GetValues(typeof(FlowerType));
        foreach (var v in values)
        {
            FlowerType fType = (FlowerType)v;
            nectarGains[fType] = 0;
        }
    }

    private void GetCloverValue()
    {
        int count = 0;
        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                if (tiles[i, j].Flower == FlowerType.Clover)
                {
                    count += GetAdjacentFlowers(FlowerType.Clover, i, j);
                    count += GetDiagonalFlowers(FlowerType.Clover, i, j);
                }
                //Print tempCount to the screen above tile.
                //Animate flower
            }
        }
        nectarGains[FlowerType.Clover] = count * 2;
    }

    private void GetAlfalfaValue()
    {
        int count = 0;
        for (int i = 0; i < mapWidth; i++)
            for (int j = 0; j < mapHeight; j++)
                if (tiles[i, j].Flower == FlowerType.Alfalfa)
                    count += GetAdjacentFlowers(FlowerType.Alfalfa, i, j);
        nectarGains[FlowerType.Alfalfa] = count * 5;
    }

    private void GetBuckwheatValue()
    {
        int count = 0;
        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                if (tiles[i, j].Flower == FlowerType.Buckwheat)
                {
                    count += 10;
                    ConvertAdjacentFlowers(FlowerType.Buckwheat, i, j, 2);
                }
            }
        }
        nectarGains[FlowerType.Buckwheat] = count;
    }

    private void GetFireweedValue()
    {
        int count = 0;
        for (int i = 0; i < mapWidth; i++)
            for (int j = 0; j < mapHeight; j++)
                if (tiles[i, j].Flower == FlowerType.Fireweed)
                    count += 5;
        nectarGains[FlowerType.Fireweed] = count;
    }

    private void GetGoldenrodValue()
    {
        int count = 0;
        for (int i = 0; i < mapWidth; i++)
            for (int j = 0; j < mapHeight; j++)
                if (tiles[i, j].Flower == FlowerType.Goldenrod)
                    count += 20;
        nectarGains[FlowerType.Goldenrod] = count;
    }

    private void ConvertAdjacentFlowers(FlowerType fType, int i, int j, int chance)
    {
        if (i + 1 < mapWidth && tiles[i + 1, j].Flower != FlowerType.Empty)
            if (Random.Range(0, chance) == 0)
                tiles[i + 1, j].Flower = fType;
        if (i - 1 >= 0 && tiles[i - 1, j].Flower != FlowerType.Empty)
            if (Random.Range(0, chance) == 0)
                tiles[i + 1, j].Flower = fType;
        if (j + 1 < mapHeight && tiles[i, j + 1].Flower != FlowerType.Empty)
            if (Random.Range(0, chance) == 0)
                tiles[i + 1, j].Flower = fType;
        if (j - 1 >= 0 && tiles[i, j - 1].Flower != FlowerType.Empty)
            if (Random.Range(0, chance) == 0)
                tiles[i + 1, j].Flower = fType;
    }

    private void SpreadToAdjacentTiles(FlowerType fType, int i, int j, int chance)
    {
        if (i + 1 < mapWidth && tiles[i + 1, j].Flower == FlowerType.Empty)
            if (Random.Range(0, chance) == 0)
                tiles[i + 1, j].Flower = fType;
        if (i - 1 >= 0 && tiles[i - 1, j].Flower == FlowerType.Empty)
            if (Random.Range(0, chance) == 0)
                tiles[i + 1, j].Flower = fType;
        if (j + 1 < mapHeight && tiles[i, j + 1].Flower == FlowerType.Empty)
            if (Random.Range(0, chance) == 0)
                tiles[i + 1, j].Flower = fType;
        if (j - 1 >= 0 && tiles[i, j - 1].Flower == FlowerType.Empty)
            if (Random.Range(0, chance) == 0)
                tiles[i + 1, j].Flower = fType;
    }

    private int GetAdjacentFlowers(FlowerType fType, int i, int j)
    {
        int count = 0;
        if (i + 1 < mapWidth && tiles[i + 1, j].Flower == fType)
            count++;
        if (i - 1 >= 0 && tiles[i - 1, j].Flower == fType)
            count++;
        if (j + 1 < mapHeight && tiles[i, j + 1].Flower == fType)
            count++;
        if (j - 1 >= 0 && tiles[i, j - 1].Flower == fType)
            count++;
        return count;
    }

    private int GetDiagonalFlowers(FlowerType fType, int i, int j)
    {
        int count = 0;
        if (i + 1 < mapWidth && j + 1 < mapHeight && tiles[i + 1, j + 1].Flower == fType)
            count++;
        if (i + 1 < mapWidth && j - 1 >= 0 && tiles[i + 1, j - 1].Flower == fType)
            count++;
        if (i - 1 >= 0 && j + 1 < mapHeight && tiles[i - 1, j + 1].Flower == fType)
            count++;
        if (i - 1 >= 0 && j - 1 >= 0 && tiles[i - 1, j - 1].Flower == fType)
            count++;
        return count;
    }

    public void AdvanceFlowerStates()
    {
        AdvanceFireweed();
        AdvanceBuckwheat();
    }

    private void AdvanceFireweed()
    {
        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                if (tiles[i, j].Flower == FlowerType.Fireweed)
                {
                    SpreadToAdjacentTiles(FlowerType.Fireweed, i, j, 1);
                    if (GetAdjacentFlowers(FlowerType.Empty, i, j) >= 3)
                        tiles[i, j].Flower = FlowerType.Empty;
                }
            }
        }
    }

    private void AdvanceBuckwheat()
    {
        for (int i = 0; i < mapWidth; i++)
            for (int j = 0; j < mapHeight; j++)
                if (tiles[i, j].Flower == FlowerType.Fireweed)
                    ConvertAdjacentFlowers(FlowerType.Buckwheat, i, j, 2);
    }
}
