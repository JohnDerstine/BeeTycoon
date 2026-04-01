using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using Unity.VisualScripting;

public class MapLoader : MonoBehaviour
{
    [SerializeField]
    public List<GameObject> flowerList;

    [SerializeField]
    public GameController game;

    [SerializeField]
    UIDocument document;

    [SerializeField]
    GameObject hive;

    [SerializeField]
    private GameObject trash;
    [SerializeField]
    private GameObject market;
    [SerializeField]
    private GameObject shed;
    [SerializeField]
    private GameObject glossary;

    [SerializeField]
    private List<Material> leafMats = new List<Material>();
    [SerializeField]
    private List<Material> grassMats = new List<Material>();
    [SerializeField]
    private List<Material> tileMats = new List<Material>();
    [SerializeField]
    private List<Material> tuftMats = new List<Material>();
    [SerializeField]
    private Material water;

    private int spawnChance = 4;
    public int mapWidth = 12;
    public int mapHeight = 16;
    private int foliageDensityMin = 5;
    private int foliageDensityMax = 12;

    public Tile[,] tiles;

    [SerializeField]
    private GameObject fence;
    [SerializeField]
    private GameObject grassTile;
    [SerializeField]
    private GameObject roadTile;
    [SerializeField]
    private GameObject outOfBoundsTile;
    [SerializeField]
    private GameObject outOfBoundsTree;
    [SerializeField]
    private GameObject grass;
    [SerializeField]
    private GameObject mushroom;

    private List<GameObject> trees = new List<GameObject>();
    private List<GameObject> oobTiles = new List<GameObject>();
    private List<GameObject> tileList = new List<GameObject>();
    private List<GameObject> tufts = new List<GameObject>();

    private GameObject trashObject;
    private GameObject marketObject;
    private GameObject glossaryObject;
    private GameObject shedObject;
    private PlayerController player;
    private HexMenu hexMenu;

    private List<Tile> leftChoices = new List<Tile>();
    private List<Tile> rightChoices = new List<Tile>();

    void Awake()
    {
        tiles = new Tile[mapWidth, mapHeight];
        GeneratePlot(false, false, true);
    }

    public void GameStart(bool fromSave)
    {
        tiles = new Tile[mapWidth, mapHeight];
        hexMenu = GameObject.Find("UIDocument").GetComponent<HexMenu>();
        GeneratePlot(fromSave, false, true);
    }

    private void GeneratePlot(bool fromSave, bool reload, bool generateFlowers)
    {
        trees.Clear();
        tufts.Clear();
        oobTiles.Clear();
        tileList.Clear();

        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                GameObject temp = Instantiate(grassTile, new Vector3(i * 2, 0, j * 2), Quaternion.identity);
                tiles[i, j] = temp.GetComponent<Tile>();
                Material[] m = new Material[1] { tileMats[1] };
                temp.GetComponent<MeshRenderer>().materials = m;
                tiles[i, j].map = this;
                tiles[i, j].Flower = (FlowerType)0;
                tiles[i, j].x = i;
                tiles[i, j].y = j;
                tileList.Add(temp);
                if (j >= 4 && j <= 11)
                {
                    if (i >= 0 && i <= 7)
                    {
                        Material[] m2 = new Material[1] { tileMats[0] };
                        if (j != 4 && j != 11 && i < 6)
                        {
                            tiles[i, j].alive = true;
                            tiles[i, j].gameObject.GetComponent<MeshRenderer>().materials = m2;
                        }
                        else if (Random.Range(0, 3) == 0)
                        {
                            tiles[i, j].alive = true;
                            tiles[i, j].gameObject.GetComponent<MeshRenderer>().materials = m2;
                        }
                    }
                }
            }
        }

        GenerateLake();
        CleanupEdges();

        //GenerateFoliage();
        GenerateBorder();
        if (!fromSave)
        {
            if (generateFlowers)
            StartCoroutine(GenerateFlowers());
        }
        else
            SaveSystem.Load();

        if (game.CurrentState != GameStates.Menu && !reload)
        {
            player = GameObject.Find("PlayerController").GetComponent<PlayerController>();
            player.Setup();
        }

        if (!fromSave && !reload && game.CurrentState != GameStates.Menu)
        {
            int randX = Random.Range(0, 6);
            int randY = Random.Range(5, 11);
            GameObject temp = Instantiate(hive, new Vector3(randX * 2, 0.5f, randY * 2), Quaternion.identity);
            Hive h = temp.GetComponent<Hive>();
            player.hives.Add(h);
            h.Placed = true;
            h.queen = h.GetComponent<QueenBee>();
            tiles[randX, randY].HasHive = true;
            tiles[randX, randY].hive = h;
            h.hiveTile = tiles[randX, randY];
            h.x = randX; h.y = randY;
            //h.x = (int)tiles[randX, randY].transform.position.x;
            //h.y = (int)tiles[randX, randY].transform.position.y;
        }

        GenerateBuildings();
    }

    private void CleanupEdges()
    {
        Material[] m = new Material[1] { tileMats[1] };
        Material[] m2 = new Material[1] { tileMats[0] };
        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                if (tiles[i, j].alive && GetAdjTileCount(i, j) <= 1)
                {
                    tiles[i, j].alive = false;
                    tiles[i, j].gameObject.GetComponent<MeshRenderer>().materials = m;
                    i = 0;
                    j = 0;
                }

                if (!tiles[i, j].alive && GetAdjTileCount(i, j) >= 3)
                {
                    tiles[i, j].alive = true;
                    tiles[i, j].gameObject.GetComponent<MeshRenderer>().materials = m2;
                }
            }
        }
    }

    private int GetAdjTileCount(int i, int j)
    {
        int count = 0;
        if (i < 11 && tiles[i + 1, j].alive)
            count++;
        if (j < 11 && tiles[i, j + 1].alive)
            count++;
        if (i > 0 && tiles[i - 1, j].alive)
            count++;
        if (j > 0 && tiles[i, j - 1].alive)
            count++;
        return count;
    }

    private void GenerateBuildings()
    {
        trashObject = Instantiate(trash, new Vector3(-3, 0, 11), Quaternion.Euler(0, 90, 0));
        marketObject = Instantiate(market, new Vector3(-6, 0.5f, 15.125f), Quaternion.Euler(0, 0, 0));
        shedObject = Instantiate(shed, new Vector3(-2.5f, 0, 19), Quaternion.Euler(270, 180, 0));
        //glossaryObject = Instantiate(glossary, new Vector3(3, 0, -3), Quaternion.identity);

        for (int i = 0; i < 16; i++)
            Instantiate(fence, new Vector3(-6 - i, 0, 13), Quaternion.Euler(0, 180, 0));
        for (int i = 0; i < 20; i++)
            Instantiate(fence, new Vector3(-2 - i, 0, 16.9f), Quaternion.Euler(0, 180, 0));

        ClearOverlappingTrees();
    }

    private void GenerateLake()
    {
        Material[] m = new Material[1] { water };
        int posX = Random.Range(2, mapWidth - 2);
        int posY;
        if (posX > 4 && posX < 11)
            posY = Random.Range(7, mapHeight - 2);
        else
            posY = Random.Range(2, mapHeight - 2);

        tiles[posX, posY].GetComponent<MeshRenderer>().materials = m;
        tiles[posX, posY].water = true;
        tiles[posX, posY].alive = false;
        Debug.Log("x: " + posX + " y: " + posY);


        int randX = Random.Range(2, 5);
        int randY = Random.Range(2, 5);
        Debug.Log("randx: " + randX + " randy: " + randY);

        for (int i = posY; i < posY + randY; i++)
        {
            for (int j = posX; j < posX + randX; j++)
            {
                if (i < 12 && i >= 0 && j < 16 && j >= 0)
                {
                    tiles[i, j].GetComponent<MeshRenderer>().materials = m;
                    tiles[i, j].water = true;
                    tiles[i, j].alive = false;
                }
            }
        }
        
        int newPosX = Random.Range(posX, posX + randX);
        int newPosY = Random.Range(posY, posY + randY);

        int randX2 = Random.Range(2, 5);
        int randY2 = Random.Range(2, 5);
        Debug.Log("randx2: " + randX2 + " randy2: " + randY2);

        int offset = Random.Range(-randY + 1, 1);

        for (int i = newPosY + offset; i < newPosY + randY2 + offset; i++)
        {
            for (int j = newPosX; j < newPosX + randX2; j++)
            {
                if (i < 12 && i >= 0 && j < 16 && j >= 0)
                {
                    tiles[i, j].GetComponent<MeshRenderer>().materials = m;
                    tiles[i, j].water = true;
                    tiles[i, j].alive = false;
                }
            }
        }
    }    

    private void ClearOverlappingTrees()
    {
        List<GameObject> toRemove = new List<GameObject>();

        foreach (GameObject tree in trees)
        {
            if (tree.GetComponent<Collider>().bounds.Intersects(trashObject.GetComponent<Collider>().bounds))
                toRemove.Add(tree);
            if (tree.GetComponent<Collider>().bounds.Intersects(shedObject.GetComponent<Collider>().bounds) && !toRemove.Contains(tree))
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
            for (int x = 0; x < mapWidth + i + 2; x++) //Left right
            {
                Vector3 offset = new Vector3(Random.Range(-0.2f, 0.2f), 0, Random.Range(-0.2f, 0.2f));
                int rotatationRand = Random.Range(0, 360);
                oobTiles.Add(Instantiate(outOfBoundsTile, new Vector3(mapWidth * 2 - x * 2 + i, 0, -2 - i), Quaternion.identity));
                oobTiles.Add(Instantiate(outOfBoundsTile, new Vector3(mapWidth * 2 - x * 2 + i, 0, mapHeight * 2 + i), Quaternion.identity));
                trees.Add(Instantiate(outOfBoundsTree, new Vector3(mapWidth * 2 - x * 2 + i, 0, mapHeight * 2 + i) + offset, Quaternion.Euler(0, rotatationRand, 0)));
                trees.Add(Instantiate(outOfBoundsTree, new Vector3(mapWidth * 2 - x * 2 + i, 0, -2 - i) + offset, Quaternion.Euler(0, rotatationRand, 0)));
            }
            for (int y = 0; y < mapHeight + i + 2; y++) //Top bottom
            {
                Vector3 offset = new Vector3(Random.Range(-0.2f, 0.2f), 0, Random.Range(-0.2f, 0.2f));
                int rotatationRand = Random.Range(0, 360);
                if (y - (i / 2) != 8 && y - (i / 2) != 9) //For dirt path
                {
                    oobTiles.Add(Instantiate(outOfBoundsTile, new Vector3(-2 - i, 0, mapHeight * 2 - y * 2 + i), Quaternion.identity));
                    //if (y - (i / 2) != 6 && y - (i / 2) != 7 && y - (i / 2) != 10 && y - (i / 2) != 11)
                        trees.Add(Instantiate(outOfBoundsTree, new Vector3(-2 - i, 0, mapHeight * 2 - y * 2 + i) + offset, Quaternion.Euler(0, rotatationRand, 0)));
                }
                else
                {
                    Instantiate(roadTile, new Vector3(-2 - i, 0, mapHeight * 2 - y * 2 + i), Quaternion.identity);
                }
                oobTiles.Add(Instantiate(outOfBoundsTile, new Vector3(mapWidth * 2 + i, 0, mapHeight * 2 - y * 2 + i), Quaternion.identity));
                trees.Add(Instantiate(outOfBoundsTree, new Vector3(mapWidth * 2 + i, 0, mapHeight * 2 - y * 2 + i) + offset, Quaternion.Euler(0, rotatationRand, 0)));
            }
        }

        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                if (!tiles[i, j].alive && !tiles[i, j].water)
                {
                    Vector3 offset = new Vector3(Random.Range(-0.2f, 0.2f), 0, Random.Range(-0.2f, 0.2f));
                    int rotatationRand = Random.Range(0, 360);
                    GameObject tree = Instantiate(outOfBoundsTree, new Vector3(i * 2, 0, j * 2) + offset, Quaternion.Euler(0, rotatationRand, 0));
                    trees.Add(tree);
                    tiles[i, j].tree = tree;
                }
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
            if (!tile.GetComponent<Tile>().water && tile.GetComponent<Tile>().alive)
            {
                tile.GetComponent<MeshRenderer>().materials = tileMaterial;
                tile.GetComponent<Tile>().seasonColor = tileMats[grassIndex];
            }
            else if (!tile.GetComponent<Tile>().alive && !tile.GetComponent<Tile>().water)
                tile.GetComponent<MeshRenderer>().materials = oobTileMaterial;
            else if (tile.GetComponent<Tile>().water)
                tile.GetComponent<MeshRenderer>().material = water;
        }

        Material[] tuftMaterial = new Material[1];
        tuftMaterial[0] = tuftMats[grassIndex];
        foreach (GameObject tuft in tufts)
        {
            tuft.GetComponent<MeshRenderer>().materials = tuftMaterial;
        }
    }

    public IEnumerator GenerateFlowers()
    {
        var values = System.Enum.GetValues(typeof(FlowerType));
        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                if (tiles[i, j].alive && Random.Range(0, spawnChance) == 0)
                {
                    bool possible = false;
                    if (game.CurrentState == GameStates.Menu && tiles[i, j].Flower == FlowerType.Empty)
                    {
                        FlowerType rand = FlowerType.Empty;
                        while (!possible)
                        {
                            rand = (FlowerType)Random.Range(2, values.Length);
                            if ((i != mapWidth - 1 && j != mapHeight - 1) || (rand != FlowerType.Orange && rand != FlowerType.Tupelo))
                                possible = true;
                        }
                        tiles[i, j].Flower = rand;
                    }
                    else if (tiles[i, j].Flower == FlowerType.Empty)
                    {
                        FlowerType rand = FlowerType.Empty;
                        while (!possible)
                        {
                            rand = hexMenu.availableFTypes[Random.Range(0, hexMenu.availableFTypes.Count)];
                            if ((i != mapWidth - 1 && j != mapHeight - 1 && tiles[i,j].Check234()) || (rand != FlowerType.Orange && rand != FlowerType.Tupelo))
                                possible = true;
                        }
                        tiles[i, j].Flower = rand;
                    }
                    yield return new WaitForFixedUpdate();
                }
            }
        }

        ClearHiveFlowers();
    }

    private void ClearHiveFlowers()
    {
        if (player == null)
            return;

        foreach (Hive h in player.hives)
            h.hiveTile.Flower = FlowerType.Empty;
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

    #region GetTileHelpers
    private void ConvertAdjacentFlowers(List<Tile> validTiles, FlowerType fType, int chance)
    {
        foreach (Tile t in validTiles)
        {
            if (t.x + 1 < mapWidth && tiles[t.x + 1, t.y].Flower != FlowerType.Empty && tiles[t.x + 1, t.y].Flower != FlowerType.Tupelo && tiles[t.x + 1, t.y].Flower != FlowerType.Orange && tiles[t.x + 1, t.y].Flower != FlowerType.Fireweed && !tiles[t.x + 1, t.y].HasHive) //Fired weed exception
                if (Random.Range(0, chance) == 0)
                    tiles[t.x + 1, t.y].Flower = fType;
            if (t.x - 1 >= 0 && tiles[t.x - 1, t.y].Flower != FlowerType.Empty && tiles[t.x + 1, t.y].Flower != FlowerType.Tupelo && tiles[t.x + 1, t.y].Flower != FlowerType.Orange && tiles[t.x - 1, t.y].Flower != FlowerType.Fireweed && !tiles[t.x - 1, t.y].HasHive)
                if (Random.Range(0, chance) == 0)
                    tiles[t.x - 1, t.y].Flower = fType;
            if (t.y + 1 < mapHeight && tiles[t.x, t.y + 1].Flower != FlowerType.Empty && tiles[t.x + 1, t.y].Flower != FlowerType.Tupelo && tiles[t.x + 1, t.y].Flower != FlowerType.Orange && tiles[t.x, t.y + 1].Flower != FlowerType.Fireweed && !tiles[t.x, t.y + 1].HasHive)
                if (Random.Range(0, chance) == 0)
                    tiles[t.x, t.y + 1].Flower = fType;
            if (t.y - 1 >= 0 && tiles[t.x, t.y - 1].Flower != FlowerType.Empty && tiles[t.x + 1, t.y].Flower != FlowerType.Tupelo && tiles[t.x + 1, t.y].Flower != FlowerType.Orange && tiles[t.x, t.y - 1].Flower != FlowerType.Fireweed && !tiles[t.x, t.y - 1].HasHive)
                if (Random.Range(0, chance) == 0)
                    tiles[t.x, t.y - 1].Flower = fType;
        }
    }

    private void SpreadToAdjacentTiles(List<Tile> validTiles, FlowerType fType, int chance)
    {
        foreach (Tile t in validTiles)
        {
            if (t.x + 1 < mapWidth && tiles[t.x + 1, t.y].Flower == FlowerType.Empty && !tiles[t.x + 1, t.y].HasHive && !tiles[t.x + 1, t.y].water && tiles[t.x + 1, t.y].alive)
                if (Random.Range(0, chance) == 0)
                    tiles[t.x + 1, t.y].Flower = fType;
            if (t.x - 1 >= 0 && tiles[t.x - 1, t.y].Flower == FlowerType.Empty && !tiles[t.x - 1, t.y].HasHive && !tiles[t.x - 1, t.y].water && tiles[t.x - 1, t.y].alive)
                if (Random.Range(0, chance) == 0)
                    tiles[t.x - 1, t.y].Flower = fType;
            if (t.y + 1 < mapHeight && tiles[t.x, t.y + 1].Flower == FlowerType.Empty && !tiles[t.x, t.y + 1].HasHive && !tiles[t.x, t.y + 1].water && tiles[t.x, t.y + 1].alive)
                if (Random.Range(0, chance) == 0)
                    tiles[t.x, t.y + 1].Flower = fType;
            if (t.y - 1 >= 0 && tiles[t.x, t.y - 1].Flower == FlowerType.Empty && !tiles[t.x, t.y - 1].HasHive && !tiles[t.x, t.y - 1].water && tiles[t.x, t.y - 1].alive)
                if (Random.Range(0, chance) == 0)
                    tiles[t.x, t.y - 1].Flower = fType;
        }
    }

    public List<Tile> GetAdjacentFlowers(FlowerType fType, int i, int j)
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

    public List<Tile> GetDiagonalFlowers(FlowerType fType, int i, int j)
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

    public List<Tile> GetAdjacentTiles(int i, int j)
    {
        List<Tile> adjTiles = new List<Tile>();
        if (i + 1 < mapWidth)
            adjTiles.Add(tiles[i + 1, j]);
        if (i - 1 >= 0)
            adjTiles.Add(tiles[i - 1, j]);
        if (j + 1 < mapHeight)
            adjTiles.Add(tiles[i, j + 1]);
        if (j - 1 >= 0)
            adjTiles.Add(tiles[i, j - 1]);
        return adjTiles;
    }

    public List<Tile> GetDiagonalTiles(int i, int j)
    {
        List<Tile> diagTiles = new List<Tile>();
        if (i + 1 < mapWidth && j + 1 < mapHeight)
            diagTiles.Add(tiles[i + 1, j + 1]);
        if (i + 1 < mapWidth && j - 1 >= 0)
            diagTiles.Add(tiles[i + 1, j - 1]);
        if (i - 1 >= 0 && j + 1 < mapHeight)
            diagTiles.Add(tiles[i - 1, j + 1]);
        if (i - 1 >= 0 && j - 1 >= 0)
            diagTiles.Add(tiles[i - 1, j - 1]);
        return diagTiles;
    }

    public List<Tile> GetEmptyTiles()
    {
        List<Tile> emptyTiles = new List<Tile>();

        foreach (Tile t in tiles)
            if (t.Flower == FlowerType.Empty && !t.HasHive)
                emptyTiles.Add(t);

        return emptyTiles;
    }

    #endregion

    #region AdvanceFlowers
    public void AdvanceFlowerStates()
    {
        AdvanceFireweed();
        AdvanceBuckwheat();
        AdvanceDandelion();

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

    private void AdvanceDandelion()
    {
        List<Tile> newTiles = new List<Tile>();
        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                if (tiles[i, j].Flower == FlowerType.Dandelion && !newTiles.Contains(tiles[i, j]))
                {
                    tiles[i, j].Flower = FlowerType.Empty;
                    List<Tile> empty = GetEmptyTiles();
                    int index = Random.Range(0, empty.Count);
                    empty[index].Flower = FlowerType.Dandelion;
                    newTiles.Add(empty[index]);
                }
            }
        }
    }

    #endregion

    public void IncreaseMapSize(string dir)
    {
        if (dir == "right")
            IncreaseHelper(0, 6, 1, 11, true);
        else if (dir == "left")
            IncreaseHelper(0, 6, -1, 4, true);
        else if (dir == "down")
            IncreaseHelper(4, 11, 1, 6, false);
    }

    private void IncreaseHelper(int randStartMin, int randStartMax, int jMod, int jLimitStart, bool horizontal)
    {
        int tilesGained = 0;

        while (tilesGained < 15)
        {
            int j = 0;
            int randStartY = Random.Range(randStartMin, randStartMax);
            int randLength = Random.Range(3, 6);
            while (randLength > 1)
            {
                if (tilesGained > 18)
                    break;

                for (int i = randStartY; i < randStartY + randLength; i++)
                {
                    int x, y;
                    if (horizontal)
                    {
                        x = i - Mathf.FloorToInt(randLength / 2);
                        y = jLimitStart + j;
                    }
                    else
                    {
                        x = jLimitStart + j;
                        y = i - Mathf.FloorToInt(randLength / 2);
                    }

                    if (x >= 0 && y >= 0 && x < 12 && y < 16 && !tiles[x, y].alive && !tiles[x, y].water)
                    {
                        Tile t = tiles[x, y];
                        t.alive = true;
                        Destroy(t.tree);
                        t.tree = null;
                        tilesGained++;
                    }
                }
                j += jMod;

                randLength -= Random.Range(0, 4);
            }
        }

        SeasonRecolor();
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
        List<Hive> hives = new List<Hive>();

        //loop through each tile, calling it's save function
        for (int i = 0; i < mapWidth; i++)
        {
            for (int j = 0; j < mapHeight; j++)
            {
                flowerData.Add(tiles[i, j].Flower);
                hiveData.Add(tiles[i, j].HasHive);
                hives.Add(tiles[i, j].hive);
            }
        }

        data.flowerData = flowerData;
        data.hiveData = hiveData;
        data.hives = hives;
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
                tiles[i, j].hive = data.hives[count];
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
    public List<Hive> hives;
    public int width;
    public int height;
}