using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapLoader : MonoBehaviour
{
    [SerializeField]
    public List<GameObject> flowerList;

    [SerializeField]
    public GameController game;

    [SerializeField]
    private ResourcePopup popUp;

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

    private bool cloverCalced;
    private bool alfalfaCalced;

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
                tiles[i, j].map = this;
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
                if (Random.Range(0, 3) == 0) //3
                {
                    int rand = Random.Range(2, 3); //2 - 7
                    tiles[i, j].Flower = (FlowerType)rand;
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

    public IEnumerator GetNectarGains()
    {
        //Reset all values to 0
        ResetNectarGains();

        StartCoroutine(GetCloverValue());
        yield return new WaitWhile(() => !cloverCalced);
        cloverCalced = false;

        StartCoroutine(GetAlfalfaValue());
        yield return new WaitWhile(() => !alfalfaCalced);
        alfalfaCalced = false;

        Debug.Log("Alfalfa calced");

        GetBuckwheatValue();
        GetFireweedValue();
        GetGoldenrodValue();

        game.nectarCollectingFinished = true;
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

    private IEnumerator GetCloverValue()
    {
        int count = 0;
        int animsPlayed = 0;
        float duration = 0.05f;
        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {

                if (tiles[i, j].Flower == FlowerType.Clover)
                {
                    List<Tile> adjTiles = GetAdjacentFlowers(FlowerType.Clover, i, j);
                    List<Tile> diagTiles = GetDiagonalFlowers(FlowerType.Clover, i, j);

                    count += adjTiles.Count;
                    count += diagTiles.Count;
                    //Print tempCount to the screen above tile.
                    //Animate flower
                    StartCoroutine(tiles[i, j].Animate(FlowerType.Clover, 1, duration));
                    popUp.DisplayPopup(tiles[i,j].transform.position, (adjTiles.Count + diagTiles.Count) * 2, duration);

                    //Animate related flowers
                    foreach (Tile t in adjTiles)
                        StartCoroutine(t.Animate(FlowerType.Clover, 0.3f, duration));
                    foreach (Tile t in diagTiles)
                        StartCoroutine(t.Animate(FlowerType.Clover, 0.3f, duration));

                    yield return new WaitWhile(() => !tiles[i, j].completed);
                    tiles[i, j].completed = false;
                    foreach (Tile t in adjTiles)
                        t.completed = false;
                    foreach (Tile t in diagTiles)
                        t.completed = false;
                    animsPlayed++;
                }
                duration = DurationCalc(duration, animsPlayed);
            }
        }
        nectarGains[FlowerType.Clover] = count * 2;
        cloverCalced = true;
    }

    private IEnumerator GetAlfalfaValue()
    {
        int count = 0;
        int animsPlayed = 0;
        float duration = 0.05f;
        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                if (tiles[i, j].Flower == FlowerType.Alfalfa)
                {
                    List<Tile> diagTiles = GetDiagonalFlowers(FlowerType.Alfalfa, i, j);
                    count += diagTiles.Count;

                    StartCoroutine(tiles[i, j].Animate(FlowerType.Alfalfa, 1, duration));
                    popUp.DisplayPopup(tiles[i, j].transform.position, diagTiles.Count * 7, duration);

                    //Animate related flowers
                    foreach (Tile t in diagTiles)
                        StartCoroutine(t.Animate(FlowerType.Alfalfa, 0.3f, duration));

                    yield return new WaitWhile(() => !tiles[i, j].completed);
                    tiles[i, j].completed = false;
                    foreach (Tile t in diagTiles)
                        t.completed = false;
                    animsPlayed++;
                }
                duration = DurationCalc(duration, animsPlayed);
            }
        }
        nectarGains[FlowerType.Alfalfa] = count * 7;
        alfalfaCalced = true;
    }

    private IEnumerator GetBuckwheatValue()
    {
        int count = 0;
        int animsPlayed = 0;
        float duration = 0.05f;
        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                if (tiles[i, j].Flower == FlowerType.Buckwheat)
                {
                    count += 10;
                    StartCoroutine(tiles[i, j].Animate(FlowerType.Buckwheat, 1, duration));
                    popUp.DisplayPopup(tiles[i, j].transform.position, 10, duration);

                    yield return new WaitWhile(() => !tiles[i, j].completed);
                    tiles[i, j].completed = false;
                    animsPlayed++;
                }
                duration = DurationCalc(duration, animsPlayed);
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

    private List<Tile> GetAdjacentFlowers(FlowerType fType, int i, int j)
    {
        List<Tile> adjTiles = new List<Tile>();
        if (i + 1 < mapWidth && tiles[i + 1, j].Flower == fType)
            adjTiles.Add(tiles[i + 1, j]);
        if (i - 1 >= 0 && tiles[i - 1, j].Flower == fType)
            adjTiles.Add(tiles[i - 1, j]);
        if (j + 1 < mapHeight && tiles[i, j + 1].Flower == fType)
            adjTiles.Add(tiles[i, j + 1]);
        if (j - 1 >= 0 && tiles[i, j - 1].Flower == fType)
            adjTiles.Add(tiles[i, j - 1]);
        return adjTiles;
    }

    private List<Tile> GetDiagonalFlowers(FlowerType fType, int i, int j)
    {
        List<Tile> diagTiles = new List<Tile>();
        if (i + 1 < mapWidth && j + 1 < mapHeight && tiles[i + 1, j + 1].Flower == fType)
            diagTiles.Add(tiles[i + 1, j + 1]);
        if (i + 1 < mapWidth && j - 1 >= 0 && tiles[i + 1, j - 1].Flower == fType)
            diagTiles.Add(tiles[i + 1, j - 1]);
        if (i - 1 >= 0 && j + 1 < mapHeight && tiles[i - 1, j + 1].Flower == fType)
            diagTiles.Add(tiles[i - 1, j + 1]);
        if (i - 1 >= 0 && j - 1 >= 0 && tiles[i - 1, j - 1].Flower == fType)
            diagTiles.Add(tiles[i - 1, j - 1]);
        return diagTiles;
    }

    private float DurationCalc(float duration, int animsPlayed)
    {
        float newDuration = duration;
        if (animsPlayed > 3)
        {
            if (duration >= 0.01f)
                newDuration = duration * Mathf.Pow(1 - 0.05f, 1.1f);
            else
                newDuration = 0.01f;
        }

        return newDuration;
    }

    public void AdvanceFlowerStates()
    {
        AdvanceFireweed();
        AdvanceBuckwheat();

        game.flowerAdvanceFinished = true;
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
                    if (GetAdjacentFlowers(FlowerType.Empty, i, j).Count >= 3)
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
