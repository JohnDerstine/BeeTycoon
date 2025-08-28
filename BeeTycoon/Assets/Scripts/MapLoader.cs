using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MapLoader : MonoBehaviour
{
    [SerializeField]
    public List<GameObject> flowerList;

    [SerializeField]
    public GameController game;

    [SerializeField]
    private ResourcePopup popUp;

    [SerializeField]
    UIDocument document;

    [SerializeField]
    private List<Material> leafMats = new List<Material>();
    [SerializeField]
    private List<Material> grassMats = new List<Material>();
    [SerializeField]
    private List<Material> tileMats = new List<Material>();
    [SerializeField]
    private List<Material> tuftMats = new List<Material>();

    private Label nectarLabel;
    private Label nectarPlus;
    private VisualElement nectarIcon;

    private int spawnChance = 4;
    public int mapWidth = 6;
    public int mapHeight = 6;
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

    private int totalAmountGained;
    private bool cloverCalced;
    private bool alfalfaCalced;
    private bool buckwheatCalced;
    private bool goldenrodCalced;
    private bool fireweedCalced;

    private List<GameObject> trees = new List<GameObject>();
    private List<GameObject> oobTiles = new List<GameObject>();
    private List<GameObject> tileList = new List<GameObject>();
    private List<GameObject> tufts = new List<GameObject>();

    void Awake()
    {
        tiles = new Tile[mapWidth, mapHeight];
        //GeneratePlot(false);
    }

    public void GameStart(bool fromSave)
    {
        nectarLabel = document.rootVisualElement.Q<Label>("NectarLabel");
        nectarPlus = document.rootVisualElement.Q<Label>("NectarPlus");
        nectarIcon = document.rootVisualElement.Q<VisualElement>("NectarIcon");
        SetNectarVisibility(false);

        tiles = new Tile[mapWidth, mapHeight];
        GeneratePlot(fromSave);

        var values = System.Enum.GetValues(typeof(FlowerType));
        foreach (var v in values)
        {
            FlowerType fType = (FlowerType)v;
            nectarGains.Add(fType, 0);
        }
    }

    private void GeneratePlot(bool fromSave)
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
                tiles[i, j].x = i;
                tiles[i, j].y = j;
                tileList.Add(temp);
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

        GenerateFoliage();
        GenerateBorder();
        if (!fromSave)
            GenerateFlowers();
        else
            SaveSystem.Load();
    }

    private void GenerateBorder()
    {
        //spawn border tiles along edge of map for out-of-bounds area
        for (int i = 0; i < mapWidth; i += 2) // * 2 to mapWidth results in double the amount of border tiles
        {
            for (int x = 0; x < mapWidth + i + 2; x++)
            {
                Vector3 offset = new Vector3(Random.Range(-0.2f, 0.2f), 0, Random.Range(-0.2f, 0.2f));
                int rotatationRand = Random.Range(0, 360);
                oobTiles.Add(Instantiate(outOfBoundsTile, new Vector3(mapWidth * 2 - x * 2 + i, 0, -2 - i), Quaternion.identity));
                trees.Add(Instantiate(outOfBoundsTree, new Vector3(mapWidth * 2 - x * 2 + i, 0, -2 - i) + offset, Quaternion.Euler(0, rotatationRand, 0)));
                oobTiles.Add(Instantiate(outOfBoundsTile, new Vector3(mapWidth * 2 - x * 2 + i, 0, mapHeight * 2 + i), Quaternion.identity));
                trees.Add(Instantiate(outOfBoundsTree, new Vector3(mapWidth * 2 - x * 2 + i, 0, mapHeight * 2 + i) + offset, Quaternion.Euler(0, rotatationRand, 0)));
            }
            for (int y = 0; y < mapHeight + i + 2; y++)
            {
                Vector3 offset = new Vector3(Random.Range(-0.2f, 0.2f), 0, Random.Range(-0.2f, 0.2f));
                int rotatationRand = Random.Range(0, 360);
                oobTiles.Add(Instantiate(outOfBoundsTile, new Vector3(-2 - i, 0, mapHeight * 2 - y * 2 + i), Quaternion.identity));
                trees.Add(Instantiate(outOfBoundsTree, new Vector3(-2 - i, 0, mapHeight * 2 - y * 2 + i) + offset, Quaternion.Euler(0, rotatationRand, 0)));
                oobTiles.Add(Instantiate(outOfBoundsTile, new Vector3(mapWidth * 2 + i, 0, mapHeight * 2 - y * 2 + i), Quaternion.identity));
                trees.Add(Instantiate(outOfBoundsTree, new Vector3(mapWidth * 2 + i, 0, mapHeight * 2 - y * 2 + i) + offset, Quaternion.Euler(0, rotatationRand, 0)));
            }
        }
        SeasonRecolor();
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
                    tufts.Add(Instantiate(grass, new Vector3(i * 2, 0, j * 2) + offset, Quaternion.Euler(rotation)));
                }

                //if (Random.Range(0, 5) == 0)
                //{
                //    Vector3 rotation = new Vector3(0, Random.Range(0, 360), 0);
                //    Vector3 offset = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
                //    Instantiate(mushroom, new Vector3(i * 2, 0, j * 2) + offset, Quaternion.Euler(rotation));
                //}
            }
        }
    }

    public void SeasonRecolor()
    {
        int startingIndex = 0;
        int grassIndex = 0;
        switch (game.Season)
        {
            case "winter":
                startingIndex = 9;
                grassIndex = 3;
                break;
            case "spring":
                startingIndex = 0;
                grassIndex = 0;
                break;
            case "summer":
                startingIndex = 3;
                grassIndex = 1;
                break;
            case "fall":
                startingIndex = 6;
                grassIndex = 2;
                break;
        }

        Material[] matArr = outOfBoundsTree.GetComponent<MeshRenderer>().sharedMaterials;

        foreach (GameObject tree in trees)
        {
            int rand = Random.Range(0, 3);
            matArr[1] = leafMats[startingIndex + rand];
            matArr[3] = leafMats[startingIndex + rand];
            matArr[5] = leafMats[startingIndex + rand];
            tree.GetComponent<MeshRenderer>().materials = matArr;
        }

        Material[] oobTileMaterial = new Material[1];
        oobTileMaterial[0] = grassMats[grassIndex];
        foreach (GameObject tile in oobTiles)
        {
            tile.GetComponent<MeshRenderer>().materials = oobTileMaterial;
        }

        Material[] tileMaterial = new Material[1];
        tileMaterial[0] = tileMats[grassIndex];
        foreach (GameObject tile in tileList)
        {
            tile.GetComponent<MeshRenderer>().materials = tileMaterial;
        }

        Material[] tuftMaterial = new Material[1];
        tuftMaterial[0] = tuftMats[grassIndex];
        foreach (GameObject tuft in tufts)
        {
            tuft.GetComponent<MeshRenderer>().materials = tuftMaterial;
        }
    }

    public void GenerateFlowers()
    {
        var values = System.Enum.GetValues(typeof(FlowerType));
        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                if (Random.Range(0, spawnChance) == 0)
                {
                    FlowerType rand = (FlowerType)Random.Range(2, values.Length);
                    tiles[i, j].Flower = rand;
                }
            }
        }
    }

    public void ClearFlowers()
    {
        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                tiles[i, j].Flower = FlowerType.Empty;
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

    private void SetNectarVisibility(bool visible)
    {
        nectarIcon.visible = visible;
        nectarLabel.visible = visible;
        nectarPlus.visible = visible;
    }

    public IEnumerator GetNectarGains()
    {
        //Reset all values to 0
        ResetNectarGains();

        SetNectarVisibility(true);

        StartCoroutine(GetCloverValue());
        yield return new WaitWhile(() => !cloverCalced);
        cloverCalced = false;

        StartCoroutine(GetAlfalfaValue());
        yield return new WaitWhile(() => !alfalfaCalced);
        alfalfaCalced = false;

        StartCoroutine(GetBuckwheatValue());
        yield return new WaitWhile(() => !buckwheatCalced);
        buckwheatCalced = false;

        StartCoroutine(GetFireweedValue());
        yield return new WaitWhile(() => !fireweedCalced);
        fireweedCalced = false;

        StartCoroutine(GetGoldenrodValue());
        yield return new WaitWhile(() => !goldenrodCalced);
        goldenrodCalced = false;

        game.nectarCollectingFinished = true;
        SetNectarVisibility(false);
    }

    private void ResetNectarGains()
    {
        totalAmountGained = 0;
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
                    StartCoroutine(tiles[i, j].Animate(FlowerType.Clover, 1, duration, true));
                    int gain = (adjTiles.Count + diagTiles.Count) * 5;
                    popUp.DisplayPopup(tiles[i,j].transform.position, gain, duration);
                    totalAmountGained += gain;
                    nectarLabel.text = totalAmountGained.ToString();

                    //Animate related flowers
                    foreach (Tile t in adjTiles)
                        StartCoroutine(t.Animate(FlowerType.Clover, 0.3f, duration, false));
                    foreach (Tile t in diagTiles)
                        StartCoroutine(t.Animate(FlowerType.Clover, 0.3f, duration, false));

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
        nectarGains[FlowerType.Clover] = count * 5;
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

                    StartCoroutine(tiles[i, j].Animate(FlowerType.Alfalfa, 1, duration, true));
                    int gain = diagTiles.Count * 7;
                    popUp.DisplayPopup(tiles[i, j].transform.position, gain, duration);
                    totalAmountGained += gain;
                    nectarLabel.text = totalAmountGained.ToString();

                    //Animate related flowers
                    foreach (Tile t in diagTiles)
                        StartCoroutine(t.Animate(FlowerType.Alfalfa, 0.3f, duration, false));

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
                    StartCoroutine(tiles[i, j].Animate(FlowerType.Buckwheat, 1, duration, true));
                    int gain = 10;
                    popUp.DisplayPopup(tiles[i, j].transform.position, gain, duration);
                    totalAmountGained += gain;
                    nectarLabel.text = totalAmountGained.ToString();

                    yield return new WaitWhile(() => !tiles[i, j].completed);
                    tiles[i, j].completed = false;
                    animsPlayed++;
                }
                duration = DurationCalc(duration, animsPlayed);
            }
        }
        nectarGains[FlowerType.Buckwheat] = count;
        buckwheatCalced = true;
    }

    private IEnumerator GetFireweedValue()
    {
        int count = 0;
        int animsPlayed = 0;
        float duration = 0.05f;
        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                if (tiles[i, j].Flower == FlowerType.Fireweed)
                {
                    count += 5;
                    StartCoroutine(tiles[i, j].Animate(FlowerType.Fireweed, 1, duration, true));
                    int gain = 5;
                    popUp.DisplayPopup(tiles[i, j].transform.position, gain, duration);
                    totalAmountGained += gain;
                    nectarLabel.text = totalAmountGained.ToString();

                    yield return new WaitWhile(() => !tiles[i, j].completed);
                    tiles[i, j].completed = false;
                    animsPlayed++;
                }
                duration = DurationCalc(duration, animsPlayed);
            }
        }
        nectarGains[FlowerType.Fireweed] = count;
        fireweedCalced = true;
    }

    private IEnumerator GetGoldenrodValue()
    {
        int count = 0;
        int animsPlayed = 0;
        float duration = 0.05f;
        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                if (tiles[i, j].Flower == FlowerType.Goldenrod)
                {
                    count += 100;
                    StartCoroutine(tiles[i, j].Animate(FlowerType.Goldenrod, 1, duration, true));
                    int gain = 100;
                    popUp.DisplayPopup(tiles[i, j].transform.position, gain, duration);
                    totalAmountGained += gain;
                    nectarLabel.text = totalAmountGained.ToString();

                    yield return new WaitWhile(() => !tiles[i, j].completed);
                    tiles[i, j].completed = false;
                    animsPlayed++;
                }
                duration = DurationCalc(duration, animsPlayed);
            }
        }
        nectarGains[FlowerType.Goldenrod] = count;
        goldenrodCalced = true;
    }

    private void ConvertAdjacentFlowers(List<Tile> validTiles, FlowerType fType, int chance)
    {
        foreach (Tile t in validTiles)
        {
            if (t.x + 1 < mapWidth && tiles[t.x + 1, t.y].Flower != FlowerType.Empty && tiles[t.x + 1, t.y].Flower != FlowerType.Fireweed && !tiles[t.x + 1, t.y].HasHive) //Fired weed exception
                if (Random.Range(0, chance) == 0)
                    tiles[t.x + 1, t.y].Flower = fType;
            if (t.x - 1 >= 0 && tiles[t.x - 1, t.y].Flower != FlowerType.Empty && tiles[t.x - 1, t.y].Flower != FlowerType.Fireweed && !tiles[t.x - 1, t.y].HasHive)
                if (Random.Range(0, chance) == 0)
                    tiles[t.x - 1, t.y].Flower = fType;
            if (t.y + 1 < mapHeight && tiles[t.x, t.y + 1].Flower != FlowerType.Empty && tiles[t.x, t.y + 1].Flower != FlowerType.Fireweed && !tiles[t.x, t.y + 1].HasHive)
                if (Random.Range(0, chance) == 0)
                    tiles[t.x, t.y + 1].Flower = fType;
            if (t.y - 1 >= 0 && tiles[t.x, t.y - 1].Flower != FlowerType.Empty && tiles[t.x, t.y - 1].Flower != FlowerType.Fireweed && !tiles[t.x, t.y - 1].HasHive)
                if (Random.Range(0, chance) == 0)
                    tiles[t.x, t.y - 1].Flower = fType;
        }
    }

    private void SpreadToAdjacentTiles(List<Tile> validTiles, FlowerType fType, int chance)
    {
        foreach (Tile t in validTiles)
        {
            if (t.x + 1 < mapWidth && tiles[t.x + 1, t.y].Flower == FlowerType.Empty && !tiles[t.x + 1, t.y].HasHive)
                if (Random.Range(0, chance) == 0)
                    tiles[t.x + 1, t.y].Flower = fType;
            if (t.x - 1 >= 0 && tiles[t.x - 1, t.y].Flower == FlowerType.Empty && !tiles[t.x - 1, t.y].HasHive)
                if (Random.Range(0, chance) == 0)
                    tiles[t.x - 1, t.y].Flower = fType;
            if (t.y + 1 < mapHeight && tiles[t.x, t.y + 1].Flower == FlowerType.Empty && !tiles[t.x, t.y + 1].HasHive)
                if (Random.Range(0, chance) == 0)
                    tiles[t.x, t.y + 1].Flower = fType;
            if (t.y - 1 >= 0 && tiles[t.x, t.y - 1].Flower == FlowerType.Empty && !tiles[t.x, t.y - 1].HasHive)
                if (Random.Range(0, chance) == 0)
                    tiles[t.x, t.y - 1].Flower = fType;
        }
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
        List<Tile> validTiles = new List<Tile>();
        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                if (tiles[i, j].Flower == FlowerType.Fireweed)
                {
                    if (!validTiles.Contains(tiles[i, j]))
                        validTiles.Add(tiles[i, j]);
                    if (GetAdjacentFlowers(FlowerType.Empty, i, j).Count >= 3)
                        tiles[i, j].Flower = FlowerType.Empty;
                }
            }
        }
        ConvertAdjacentFlowers(validTiles, FlowerType.Fireweed, 1);
    }

    private void AdvanceBuckwheat()
    {
        List<Tile> validTiles = new List<Tile>();
        for (int i = 0; i < mapWidth; i++)
            for (int j = 0; j < mapHeight; j++)
                if (tiles[i, j].Flower == FlowerType.Buckwheat)
                        validTiles.Add(tiles[i, j]);
        SpreadToAdjacentTiles(validTiles, FlowerType.Buckwheat, 2);
    }

    public void IncreaseMapSize()
    {
        mapWidth++;
        mapHeight++;
        FlowerType[,] flowers = new FlowerType[mapWidth, mapHeight];
        for (int i = 0; i < mapWidth - 1; i++)
        {
            for (int j = 0; j < mapHeight - 1; j++)
            {
                flowers[i, j] = tiles[i, j].Flower;
            }
        }

        ClearAllTiles();
        tiles = new Tile[mapWidth, mapHeight];
        GeneratePlot(false);
        GenerateBorder();
        GameObject.Find("GridRenderer").GetComponent<GridRenderer>().Reload();
        GameObject.Find("PlayerController").GetComponent<PlayerController>().CenterCamera();
        ClearFlowers();

        for (int i = 0; i < mapWidth - 1; i++)
        {
            for (int j = 0; j < mapHeight - 1; j++)
            {
                tiles[i, j].Flower = flowers[i, j];
            }
        }
    }

    private void ClearAllTiles()
    {
        for (int i = 0; i < mapWidth - 1; i++)
        {
            for (int j = 0; j < mapHeight - 1; j++)
            {
                tiles[i, j].Flower = FlowerType.Empty;
                Destroy(tiles[i, j]);
            }
        }

        foreach (GameObject tile in oobTiles)
            Destroy(tile);
        foreach (GameObject tree in trees)
            Destroy(tree);
        foreach (GameObject tuft in tufts)
            Destroy(tuft);

        trees.Clear();
        oobTiles.Clear();
        tufts.Clear();
        tileList.Clear();
    }

    public void Save(ref MapSaveData data)
    {
        List<FlowerType> flowerData = new List<FlowerType>();
        //loop through each tile, calling it's save function
        for (int i = 0; i < mapWidth; i++)
            for (int j = 0; j < mapHeight; j++)
                flowerData.Add(tiles[i, j].Flower);
        data.flowerData = flowerData;
        data.width = mapWidth;
        data.height = mapHeight;
    }

    public void Load(MapSaveData data)
    {
        //loop through each tile, calling it's load function
        int count = 0;
        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                tiles[i, j].Flower = data.flowerData[count];
                count++;
            }
        }

        mapWidth = data.width;
        mapHeight = data.height;
    }
}

[System.Serializable]
public struct MapSaveData
{
    public List<FlowerType> flowerData;
    public int width;
    public int height;
}