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
    GameObject hive;

    [SerializeField]
    private AudioClip audio;

    [SerializeField]
    private GameObject trash;

    [SerializeField]
    private List<Material> leafMats = new List<Material>();
    [SerializeField]
    private List<Material> grassMats = new List<Material>();
    [SerializeField]
    private List<Material> tileMats = new List<Material>();
    [SerializeField]
    private List<Material> tuftMats = new List<Material>();

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
    public int populatedHives;

    private int totalAmountGained;
    private int flowerAmountGained;
    private bool cloverCalced;
    private bool alfalfaCalced;
    private bool buckwheatCalced;
    private bool goldenrodCalced;
    private bool fireweedCalced;

    private List<GameObject> trees = new List<GameObject>();
    private List<GameObject> oobTiles = new List<GameObject>();
    private List<GameObject> tileList = new List<GameObject>();
    private List<GameObject> tufts = new List<GameObject>();

    const int cloverValue = 25;
    const int alfalfaValue = 30;
    const int buckwheatValue = 15;
    const int fireweedValue = 20;
    const int goldenrodValue = 50;

    AudioSource source;

    private GameObject trashObject;
    private PlayerController player;
    private HexMenu hexMenu;

    [SerializeField]
    private VisualTreeAsset nectarItem;

    [SerializeField]
    private Texture2D honeySprite;

    TemplateContainer item = null;
    VisualElement total;
    Label totalAmount;

    void Awake()
    {
        tiles = new Tile[mapWidth, mapHeight];
        source = GetComponent<AudioSource>();
        GeneratePlot(false, false);
    }

    public void GameStart(bool fromSave)
    {
        tiles = new Tile[mapWidth, mapHeight];
        GeneratePlot(fromSave, false);

        var values = System.Enum.GetValues(typeof(FlowerType));
        foreach (var v in values)
        {
            FlowerType fType = (FlowerType)v;
            nectarGains.Add(fType, 0);
        }
    }

    private void GeneratePlot(bool fromSave, bool reload)
    {
        trees.Clear();
        tufts.Clear();
        oobTiles.Clear();
        tileList.Clear();

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

        if (!fromSave && !reload && game.CurrentState != GameStates.Menu)
        {
            int randX = Random.Range(0, mapWidth);
            int randY = Random.Range(0, mapHeight);
            GameObject temp = Instantiate(hive, new Vector3(randX * 2, 0.5f, randY * 2), Quaternion.identity);
            tiles[randX, randY].Flower = FlowerType.Empty;
            Hive h = temp.GetComponent<Hive>();
            player = GameObject.Find("PlayerController").GetComponent<PlayerController>();
            hexMenu = GameObject.Find("UIDocument").GetComponent<HexMenu>();
            player.hives.Add(h);
            h.Placed = true;
            h.queen = h.GetComponent<QueenBee>();
            tiles[randX, randY].HasHive = true;
            h.hiveTile = tiles[randX, randY];
            h.x = (int)tiles[randX, randY].transform.position.x;
            h.y = (int)tiles[randX, randY].transform.position.y;

        }

        GenerateTrash();
    }

    private void GenerateTrash()
    {
        trashObject = Instantiate(trash, new Vector3(3, 0, -3), Quaternion.identity);
        ClearOverlappingTrees();
    }

    private void ClearOverlappingTrees()
    {
        List<GameObject> toRemove = new List<GameObject>();

        foreach (GameObject tree in trees)
        {
            if (tree.GetComponent<Collider>().bounds.Intersects(trashObject.GetComponent<Collider>().bounds))
                toRemove.Add(tree);
        }

        while (toRemove.Count > 0)
        {
            trees.Remove(toRemove[0]);
            Destroy(toRemove[0]);
            toRemove.RemoveAt(0);
        }
    }

    private void GenerateBorder()
    {
        //spawn border tiles along edge of map for out-of-bounds area
        for (int i = 0; i < mapWidth * 2; i += 2) // * 2 to mapWidth results in double the amount of border tiles
        {
            for (int x = 0; x < mapWidth + i + 2; x++)
            {
                Vector3 offset = new Vector3(Random.Range(-0.2f, 0.2f), 0, Random.Range(-0.2f, 0.2f));
                int rotatationRand = Random.Range(0, 360);
                oobTiles.Add(Instantiate(outOfBoundsTile, new Vector3(mapWidth * 2 - x * 2 + i, 0, -2 - i), Quaternion.identity));
                oobTiles.Add(Instantiate(outOfBoundsTile, new Vector3(mapWidth * 2 - x * 2 + i, 0, mapHeight * 2 + i), Quaternion.identity));
                trees.Add(Instantiate(outOfBoundsTree, new Vector3(mapWidth * 2 - x * 2 + i, 0, mapHeight * 2 + i) + offset, Quaternion.Euler(0, rotatationRand, 0)));
                trees.Add(Instantiate(outOfBoundsTree, new Vector3(mapWidth * 2 - x * 2 + i, 0, -2 - i) + offset, Quaternion.Euler(0, rotatationRand, 0)));
            }
            for (int y = 0; y < mapHeight + i + 2; y++)
            {
                Vector3 offset = new Vector3(Random.Range(-0.2f, 0.2f), 0, Random.Range(-0.2f, 0.2f));
                int rotatationRand = Random.Range(0, 360);
                oobTiles.Add(Instantiate(outOfBoundsTile, new Vector3(-2 - i, 0, mapHeight * 2 - y * 2 + i), Quaternion.identity));
                oobTiles.Add(Instantiate(outOfBoundsTile, new Vector3(mapWidth * 2 + i, 0, mapHeight * 2 - y * 2 + i), Quaternion.identity));
                trees.Add(Instantiate(outOfBoundsTree, new Vector3(mapWidth * 2 + i, 0, mapHeight * 2 - y * 2 + i) + offset, Quaternion.Euler(0, rotatationRand, 0)));
                trees.Add(Instantiate(outOfBoundsTree, new Vector3(-2 - i, 0, mapHeight * 2 - y * 2 + i) + offset, Quaternion.Euler(0, rotatationRand, 0)));
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
            tile.GetComponent<Tile>().currentMat = tileMats[grassIndex];
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

    public IEnumerator GetNectarGains()
    {
        source.clip = audio;
        float pitch = source.pitch;
        //Reset all values to 0
        ResetNectarGains();
        total = document.rootVisualElement.Q<VisualElement>("Total");
        totalAmount = total.Q<Label>("Amount");

        StartCoroutine(GetCloverValue());
        yield return new WaitWhile(() => !cloverCalced);
        yield return new WaitForSeconds(0.2f);
        cloverCalced = false;
        source.pitch = pitch;

        StartCoroutine(GetAlfalfaValue());
        yield return new WaitWhile(() => !alfalfaCalced);
        yield return new WaitForSeconds(0.2f);
        alfalfaCalced = false;
        source.pitch = pitch;

        StartCoroutine(GetBuckwheatValue());
        yield return new WaitWhile(() => !buckwheatCalced);
        yield return new WaitForSeconds(0.2f);
        buckwheatCalced = false;
        source.pitch = pitch;

        StartCoroutine(GetFireweedValue());
        yield return new WaitWhile(() => !fireweedCalced);
        yield return new WaitForSeconds(0.2f);
        fireweedCalced = false;
        source.pitch = pitch;

        StartCoroutine(GetGoldenrodValue());
        yield return new WaitWhile(() => !goldenrodCalced);
        yield return new WaitForSeconds(0.2f);
        goldenrodCalced = false;
        source.pitch = pitch;

        game.nectarCollectingFinished = true;
        source.pitch = pitch;

        populatedHives = 0;
        foreach (Hive h in player.hives)
            if (!h.queen.nullQueen)
                populatedHives++;
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

    private void UpdateNectarUI(int spriteIndex)
    {
        item = nectarItem.Instantiate();
        item.Q<VisualElement>("Icon").style.backgroundImage = hexMenu.allFlowerSprites[spriteIndex];
        document.rootVisualElement.Q<VisualElement>("NectarColumn").Insert(0, item);
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
                    if (item == null)
                        UpdateNectarUI(0);

                    List<Tile> adjTiles = GetAdjacentFlowers(FlowerType.Clover, i, j);
                    List<Tile> diagTiles = GetDiagonalFlowers(FlowerType.Clover, i, j);

                    count += adjTiles.Count;
                    count += diagTiles.Count;
                    //Print tempCount to the screen above tile.
                    //Animate flower
                    StartCoroutine(tiles[i, j].Animate(FlowerType.Clover, 1, duration, true, source));
                    int gain = (adjTiles.Count + diagTiles.Count) * cloverValue;
                    popUp.DisplayPopup(tiles[i,j].transform.position, gain, duration);
                    flowerAmountGained += gain;
                    totalAmountGained += gain;
                    if (flowerAmountGained > 999)
                        item.Q<Label>("Amount").style.fontSize = 48;
                    if (totalAmountGained > 999)
                        totalAmount.style.fontSize = 48;
                    totalAmount.text = totalAmountGained.ToString();
                    item.Q<Label>("Amount").text = flowerAmountGained.ToString();

                    //Animate related flowers
                    foreach (Tile t in adjTiles)
                    {
                        StartCoroutine(t.Animate(FlowerType.Clover, 0.3f, duration, false, source));
                        //source.Play();
                    }
                    foreach (Tile t in diagTiles)
                    {
                        StartCoroutine(t.Animate(FlowerType.Clover, 0.3f, duration, false, source));
                        //source.Playo();
                    }

                    yield return new WaitWhile(() => !tiles[i, j].completed);
                    tiles[i, j].completed = false;
                    foreach (Tile t in adjTiles)
                        t.completed = false;
                    foreach (Tile t in diagTiles)
                        t.completed = false;
                    animsPlayed++;
                    source.pitch += 0.25f;
                    if (source.pitch >= 3)
                        source.pitch = 3;
                }
                duration = DurationCalc(duration, animsPlayed);
            }
        }
        yield return new WaitForSeconds(duration);
        nectarGains[FlowerType.Clover] = count * cloverValue;
        flowerAmountGained = 0;
        cloverCalced = true;
        item = null;
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
                    if (item == null)
                        UpdateNectarUI(1);

                    List<Tile> diagTiles = GetDiagonalFlowers(FlowerType.Alfalfa, i, j);
                    count += diagTiles.Count;

                    StartCoroutine(tiles[i, j].Animate(FlowerType.Alfalfa, 1, duration, true, source));
                    int gain = diagTiles.Count * alfalfaValue;
                    popUp.DisplayPopup(tiles[i, j].transform.position, gain, duration);
                    flowerAmountGained += gain;
                    totalAmountGained += gain;
                    if (flowerAmountGained > 999)
                        item.Q<Label>("Amount").style.fontSize = 48;
                    if (totalAmountGained > 999)
                        totalAmount.style.fontSize = 48;
                    totalAmount.text = totalAmountGained.ToString();
                    item.Q<Label>("Amount").text = flowerAmountGained.ToString();

                    //Animate related flowers
                    foreach (Tile t in diagTiles)
                        StartCoroutine(t.Animate(FlowerType.Alfalfa, 0.3f, duration, false, source));

                    yield return new WaitWhile(() => !tiles[i, j].completed);
                    tiles[i, j].completed = false;
                    foreach (Tile t in diagTiles)
                        t.completed = false;
                    animsPlayed++;
                    source.pitch += 0.25f;
                    if (source.pitch >= 3)
                        source.pitch = 3;
                }
                duration = DurationCalc(duration, animsPlayed);
            }
        }
        nectarGains[FlowerType.Alfalfa] = count * alfalfaValue;
        flowerAmountGained = 0;
        alfalfaCalced = true;
        item = null;
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
                    if (item == null)
                        UpdateNectarUI(2);

                    count += buckwheatValue;
                    StartCoroutine(tiles[i, j].Animate(FlowerType.Buckwheat, 1, duration, true, source));
                    int gain = buckwheatValue;
                    popUp.DisplayPopup(tiles[i, j].transform.position, gain, duration);
                    flowerAmountGained += gain;
                    totalAmountGained += gain;
                    if (flowerAmountGained > 999)
                        item.Q<Label>("Amount").style.fontSize = 48;
                    if (totalAmountGained > 999)
                        totalAmount.style.fontSize = 48;
                    totalAmount.text = totalAmountGained.ToString();
                    item.Q<Label>("Amount").text = flowerAmountGained.ToString();

                    yield return new WaitWhile(() => !tiles[i, j].completed);
                    tiles[i, j].completed = false;
                    animsPlayed++;
                    source.pitch += 0.25f;
                    if (source.pitch >= 3)
                        source.pitch = 3;
                }
                duration = DurationCalc(duration, animsPlayed);
            }
        }
        nectarGains[FlowerType.Buckwheat] = count;
        flowerAmountGained = 0;
        buckwheatCalced = true;
        item = null;
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
                    if (item == null)
                        UpdateNectarUI(4);

                    count += fireweedValue;
                    StartCoroutine(tiles[i, j].Animate(FlowerType.Fireweed, 1, duration, true, source));
                    int gain = fireweedValue;
                    popUp.DisplayPopup(tiles[i, j].transform.position, gain, duration);
                    flowerAmountGained += gain;
                    totalAmountGained += gain;
                    if (flowerAmountGained > 999)
                        item.Q<Label>("Amount").style.fontSize = 48;
                    if (totalAmountGained > 999)
                        totalAmount.style.fontSize = 48;
                    totalAmount.text = totalAmountGained.ToString();
                    item.Q<Label>("Amount").text = flowerAmountGained.ToString();

                    yield return new WaitWhile(() => !tiles[i, j].completed);
                    tiles[i, j].completed = false;
                    animsPlayed++;
                    source.pitch += 0.25f;
                    if (source.pitch >= 3)
                        source.pitch = 3;
                }
                duration = DurationCalc(duration, animsPlayed);
            }
        }
        nectarGains[FlowerType.Fireweed] = count;
        flowerAmountGained = 0;
        fireweedCalced = true;
        item = null;
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
                    if (item == null)
                        UpdateNectarUI(3);

                    count += goldenrodValue;
                    StartCoroutine(tiles[i, j].Animate(FlowerType.Goldenrod, 1, duration, true, source));
                    int gain = goldenrodValue;
                    popUp.DisplayPopup(tiles[i, j].transform.position, gain, duration);
                    flowerAmountGained += gain;
                    totalAmountGained += gain;
                    if (flowerAmountGained > 999)
                        item.Q<Label>("Amount").style.fontSize = 48;
                    if (totalAmountGained > 999)
                        totalAmount.style.fontSize = 48;
                    totalAmount.text = totalAmountGained.ToString();
                    item.Q<Label>("Amount").text = flowerAmountGained.ToString();

                    yield return new WaitWhile(() => !tiles[i, j].completed);
                    tiles[i, j].completed = false;
                    animsPlayed++;
                    source.pitch += 0.25f;
                    if (source.pitch >= 3)
                        source.pitch = 3;
                }
                duration = DurationCalc(duration, animsPlayed);
            }
        }
        nectarGains[FlowerType.Goldenrod] = count;
        goldenrodCalced = true;
        item = null;
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
        if (animsPlayed > 2)
        {
            if (duration >= 0.01f)
                newDuration = duration * Mathf.Pow(1 - 0.55f, 1.1f);
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
        ConvertAdjacentFlowers(validTiles, FlowerType.Fireweed, 3);
    }

    private void AdvanceBuckwheat()
    {
        List<Tile> validTiles = new List<Tile>();
        for (int i = 0; i < mapWidth; i++)
            for (int j = 0; j < mapHeight; j++)
                if (tiles[i, j].Flower == FlowerType.Buckwheat)
                        validTiles.Add(tiles[i, j]);
        SpreadToAdjacentTiles(validTiles, FlowerType.Buckwheat, 5);
    }

    public void IncreaseMapSize()
    {
        mapWidth++;
        mapHeight++;
        FlowerType[,] flowers = new FlowerType[mapWidth, mapHeight];
        bool[,] hives = new bool[mapWidth, mapHeight];
        for (int i = 0; i < mapWidth - 1; i++)
        {
            for (int j = 0; j < mapHeight - 1; j++)
            {
                flowers[i, j] = tiles[i, j].Flower;
                hives[i, j] = tiles[i, j].HasHive;
            }
        }

        ClearAllTiles();
        tiles = new Tile[mapWidth, mapHeight];
        GeneratePlot(false, true);
        GameObject.Find("GridRenderer").GetComponent<GridRenderer>().Reload();
        player.CenterCamera();
        ClearFlowers();

        for (int i = 0; i < mapWidth - 1; i++)
        {
            for (int j = 0; j < mapHeight - 1; j++)
            {
                tiles[i, j].Flower = flowers[i, j];
                tiles[i, j].HasHive = hives[i, j];
            }
        }

        ClearOverlappingTrees();
    }

    private void ClearAllTiles()
    {
        for (int i = 0; i < mapWidth - 1; i++)
        {
            for (int j = 0; j < mapHeight - 1; j++)
            {
                tiles[i, j].Flower = FlowerType.Empty;
                Destroy(tiles[i, j].gameObject);
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
        List<bool> hiveData = new List<bool>();

        //loop through each tile, calling it's save function
        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                flowerData.Add(tiles[i, j].Flower);
                hiveData.Add(tiles[i, j].HasHive);
            }
        }

        data.flowerData = flowerData;
        data.hiveData = hiveData;
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
                tiles[i, j].HasHive = data.hiveData[count];
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
    public List<bool> hiveData;
    public int width;
    public int height;
}