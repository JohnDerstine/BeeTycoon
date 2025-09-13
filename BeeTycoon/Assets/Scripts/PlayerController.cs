using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    GameObject hivePrefab;

    GameController game;

    MapLoader map;

    UIDocument ui;

    [SerializeField]
    HoneyMarket honeyMarket;

    HexMenu hexMenu;

    [SerializeField]
    public Sprite honey;

    [SerializeField]
    private GameObject standObject;

    [SerializeField]
    private VisualTreeAsset hiveUI;

    [SerializeField]
    private Material darkHive;

    [SerializeField]
    private Material lightHive;

    private Glossary glossary;

    //[SerializeField]
    //private VisualTreeAsset hiveUI;

    private TemplateContainer activeUI;
    public Hive currentHive;

    public List<Hive> hives = new List<Hive>();

    private Label moneyLabel;

    private GameObject selectedItem = null;
    public Texture2D selectedItemSprite;
    private GameObject hoverObject = null;
    private bool hovering;

    private GameObject objectToMove;
    private Vector3 storedPos;
    private bool pickedUpThisFrame = false;
    private Tile storedTile;

    private int money = 60;
    public int moneyEarned = 0;
    public int moneySpent = 0;
    public Dictionary<FlowerType, List<float>> inventory = new Dictionary<FlowerType, List<float>>();

    public bool fromSave;

    public GameObject SelectedItem
    {
        get { return selectedItem; }
        set
        {
            foreach (Hive h in hives)
                h.gameObject.GetComponent<MeshRenderer>().material = lightHive;
            selectedItem = value;
            if (value == null)// && selectedItem != null)
            {
                hovering = false;
                hoverObject = null;
                UnityEngine.Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                


                if (objectToMove != null)
                {
                    if (objectToMove.TryGetComponent<Hive>(out Hive h))
                        h.hiveTile.HasHive = true;

                    objectToMove.transform.position = storedPos;
                    objectToMove = null;
                }
            }
            else if (value != null)
            {
                foreach (Hive h in hives)
                    HighlightHives(selectedItem, h);
                if (selectedItem.tag == "Placeable" || selectedItem.tag == "Super")
                    hoverObject = Instantiate(selectedItem, new Vector3(-100, -100, -100), Quaternion.identity);
                else
                {

                    if (selectedItem.tag == "Bee")
                    {
                        hoverObject = Instantiate(selectedItem);
                        StartCoroutine(hoverObject.GetComponent<QueenBee>().TransferStats(selectedItem.GetComponent<QueenBee>()));
                    }
                }
                hovering = true;
            }
        }
    }

    public GameObject ObjectToMove
    {
        get { return objectToMove; }
        set
        {
            if (value == null)
            {
                objectToMove.transform.position = storedPos;
                storedTile = null;
            }
            else
            {
                storedPos = value.transform.position;

                pickedUpThisFrame = true;
                if (value.TryGetComponent<Hive>(out Hive h))
                    h.hiveTile.HasHive = false;
            }
            objectToMove = value;
        }
    }

    public int Money
    {
        get { return money; }
        set
        {
            money += value;
            moneyLabel.text = "$" + money;

            if (honeyMarket.marketOpen)
                honeyMarket.marketTemplate.Q<Label>("MoneyLabel").text = "$" + money;

            if (value < 0)
                moneySpent += value;
            else
                moneyEarned += value;
        }
    }

    public int HivesCount
    {
        get { return hives.Count; }
    }

    // Start is called before the first frame update
    void Start()
    {
        game = GameObject.Find("GameController").GetComponent<GameController>();
        map = GameObject.Find("MapLoader").GetComponent<MapLoader>();
        ui = GameObject.Find("UIDocument").GetComponent<UIDocument>();
        hexMenu = ui.gameObject.GetComponent<HexMenu>();
        glossary = ui.gameObject.GetComponent<Glossary>();

        if (!fromSave)
        {
            var values = System.Enum.GetValues(typeof(FlowerType));
            foreach (var v in values)
            {
                FlowerType fType = (FlowerType)v;
                inventory.Add(fType, new List<float> { 0, 0, 0, 0 }); //total, low, medium, high
            }
        }

        CenterCamera();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (glossary.open)
                glossary.CloseGlossary();
            else if (honeyMarket.marketOpen)
                honeyMarket.CloseMarket();
            else if (SelectedItem != null)
            {
                Destroy(hoverObject);
                SelectedItem = null;
            }
            else
                hexMenu.CloseTab();
        }

        if (Input.GetMouseButtonDown(1))
        {
            SelectedItem = null;
            
            if (ObjectToMove != null)
                ObjectToMove = null;
        }

        if (Input.GetKeyDown(KeyCode.G))
            glossary.OpenGlossary("Species");

        pickedUpThisFrame = false;
        //Checks to see if selected item is placed
        if (SelectedItem != null)
            checkForClick();

        if (objectToMove != null && !pickedUpThisFrame)
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            //If a tile is clicked while holding a placeable object, place the object
            if (Physics.Raycast(ray, out var tileHit, 1000, LayerMask.GetMask("Tile")))
                objectToMove.transform.position = tileHit.point;
            CheckForPlacement();
        }



        //Displays selected item under mouse
        if (hovering && hoverObject != null && selectedItem != null)
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (hoverObject.tag == "Placeable")
            {
                if (Physics.Raycast(ray, out var hit, 1000, LayerMask.GetMask("Tile")))
                {
                    if (hit.collider.gameObject.TryGetComponent<Tile>(out Tile t))
                    {
                        if (hoverObject.TryGetComponent<Hive>(out Hive h))
                            hoverObject.transform.position = new Vector3(t.gameObject.transform.position.x, t.gameObject.transform.position.y + 0.5f, t.gameObject.transform.position.z);
                        else
                            hoverObject.transform.position = t.gameObject.transform.position;
                    }
                }
            }
            else
            {
                if (Physics.Raycast(ray, out var hit, 1000))
                    hoverObject.transform.position = hit.point + new Vector3(0, 1.5f, 0);
            }
        }
        //Map Controls
        CheckZoom();
        PanCamera();
    }

    //Check for user clicks on various GameObjects or when an object from the hex menu is selected
    private void checkForClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            //If a tile is clicked while holding a placeable object, place the object
            if (Physics.Raycast(ray, out var tileHit, 1000, LayerMask.GetMask("Tile")))
            {
                if (tileHit.collider.gameObject.TryGetComponent<Tile>(out Tile t))
                {
                    if (hoverObject != null && hoverObject.tag == "Placeable")
                    {
                        hoverObject.transform.position = t.gameObject.transform.position;
                        if (hoverObject.TryGetComponent(out Hive h))
                        {
                            hoverObject.transform.position += new Vector3(0, 0.5f, 0);
                            hives.Add(h);
                            Money = -hoverObject.GetComponent<Cost>().Price;
                            h.x = (int)t.transform.position.x;
                            h.y = (int)t.transform.position.z;
                            SelectedItem = null;
                            h.Placed = true;
                            h.SetUpTemplate();
                            t.HasHive = true;
                            h.hiveTile = t;
                        }
                        else if (hoverObject.TryGetComponent<Cost>(out Cost c))
                        {
                            Destroy(hoverObject);
                            hoverObject = null;
                            if (c.ftype != FlowerType.Empty)
                            {
                                t.Flower = c.ftype;
                                if (hexMenu.flowersOwned[c.ftype] <= 0)
                                {
                                    Money = -c.Price;
                                    hexMenu.RefreshMenuLists();
                                    hexMenu.OpenTab(3, hexMenu.open4, false);
                                }
                                else
                                {
                                    hexMenu.flowersOwned[c.ftype]--;
                                    hexMenu.RefreshMenuLists();
                                    hexMenu.OpenTab(3, hexMenu.open4, false);
                                }
                            }

                            SelectedItem = null;
                        }
                        return;
                    }
                    else if (selectedItem.tag == "Shovel" && t.Flower != FlowerType.Empty)
                    {
                        Debug.Log(t.FlowerObject.GetComponent<Cost>().ftype);
                        storedTile = t;
                        ObjectToMove = t.FlowerObject;
                    }
                }
            }
            if (Physics.Raycast(ray, out var hiveHit, 1000, LayerMask.GetMask("Hive")))
            {
                //If a hive is clicked with an item, apply the item's effect
                if (hiveHit.collider.gameObject.TryGetComponent<Hive>(out Hive h))
                {

                    if (selectedItem.tag != "Placeable" && selectedItem.tag != "Dolly" && selectedItem.tag != "Shovel" && selectedItem.tag != "HiveTool" && selectedItem.tag != "Smoker")
                    {
                        int cost = selectedItem.GetComponent<Cost>().Price;
                        Money = -cost;
                        if (selectedItem.TryGetComponent(out QueenBee queen))
                        {
                            h.Populate(queen);
                            //Money = -hoverObject.GetComponent<Cost>().Price;
                            hexMenu.beeObjectList.Remove(SelectedItem);
                            hexMenu.beeSprites.Remove(selectedItemSprite);
                            hexMenu.tab1ItemCount--;
                            hexMenu.RefreshMenuLists();
                            hexMenu.OpenTab(0, hexMenu.open1, false);
                        }
                        else if (selectedItem.tag == "Super")
                        {
                            if (h.Size < 5)
                            {
                                h.Size = 1;
                                GameObject newLevel = Instantiate(selectedItem, h.gameObject.transform);
                                newLevel.transform.localPosition = new Vector3(0, h.Size - 1, 0);
                            }
                        }
                        else if (selectedItem.tag == "Frame")
                        {
                            if (h.Frames < 10)
                            {
                                h.Frames++;
                            }
                        }
                        else if (selectedItem.tag == "Sugar")
                        {
                            if (!h.hasSugar)
                            {
                                h.AddSugarWater();
                            }
                        }
                        else if (selectedItem.tag == "Reducer")
                        {
                            if (!h.hasReducer)
                            {
                                h.hasReducer = true;
                                if (h.Condition == "Mice")
                                    h.CureCondition();
                            }
                        }
                        else if (selectedItem.tag == "Stand")
                        {
                            if (!h.hasStand)
                            {
                                h.hasStand = true;
                                Instantiate(standObject, h.transform.position, Quaternion.identity);
                                h.transform.position += Vector3.up;
                            }
                        }
                        else if (selectedItem.tag == "Repellant")
                        {
                            if (h.Condition == "Mites")
                                h.CureCondition();
                            h.repellantTurns = 4;
                        }
                        else if (selectedItem.tag == "Insulation")
                        {
                            if (!h.hasReducer)
                            {
                                h.hasInsulation = true;
                            }
                        }
                        else if (selectedItem.tag == "Emergency" && (h.Condition == "Freezing" || h.Condition == "Starving"))
                        {
                            h.CureCondition();
                        }

                        Destroy(hoverObject);
                        SelectedItem = null;
                        selectedItemSprite = null;
                    }

                    if (SelectedItem != null && selectedItem.tag == "Dolly" && h.hiveTile.HasHive == true)
                    {
                        ObjectToMove = h.gameObject;
                    }
                    else if (SelectedItem != null && selectedItem.tag == "HiveTool" && h.Condition == "Glued")
                    {
                        h.CureCondition();
                    }
                    else if (SelectedItem != null && selectedItem.tag == "Smoker" && h.Condition == "Aggrevated")
                    {
                        h.CureCondition();
                    }
                }
            }
        }
    }

    private void CheckForPlacement()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            //If trash is clicked, delete flower
            if (Physics.Raycast(ray, out var trashHit, 1000, LayerMask.GetMask("Trash")))
            {
                Destroy(objectToMove);
                objectToMove = null;
                storedTile.Flower = FlowerType.Empty;
                Debug.Log("Trashed object");
                return;
            }

            //If a tile is clicked while holding a placeable object, place the object
            if (Physics.Raycast(ray, out var tileHit, 1000, LayerMask.GetMask("Tile")))
            {
                if (tileHit.collider.gameObject.TryGetComponent<Tile>(out Tile t))
                {
                    if (t.Flower == FlowerType.Empty && !t.HasHive)
                    {
                        if (objectToMove.TryGetComponent<Hive>(out Hive h))
                        {
                            h.hiveTile = t;
                            t.HasHive = true;
                            h.x = (int)t.transform.position.x;
                            h.y = (int)t.transform.position.y;
                            Debug.Log("Put down object");
                        }
                        else
                        {
                            t.Flower = objectToMove.GetComponent<Cost>().ftype;
                            storedTile.Flower = FlowerType.Empty;
                            Debug.Log("Put down object");
                        }
                        ObjectToMove = null;
                    }
                }
            }
        }
    }

    private void HighlightHives(GameObject item, Hive h)
    {
        switch (item.tag)
        {
            case "HiveTool":
                if (h.Condition != "Glued")
                    h.gameObject.GetComponent<MeshRenderer>().material = darkHive;
                break;
            case "Smoker":
                if (h.Condition != "Aggrevated")
                    h.gameObject.GetComponent<MeshRenderer>().material = darkHive;
                break;
            case "Insulation":
                if (!h.hasInsulation)
                    h.gameObject.GetComponent<MeshRenderer>().material = darkHive;
                break;
            case "Repellant":
                if (h.hasRepellant)
                    h.gameObject.GetComponent<MeshRenderer>().material = darkHive;
                break;
            case "Reducer":
                if (!h.hasReducer)
                    h.gameObject.GetComponent<MeshRenderer>().material = darkHive;
                break;
            case "Sugar":
                if (!h.hasSugar)
                    h.gameObject.GetComponent<MeshRenderer>().material = darkHive;
                break;
            case "Stand":
                if (!h.hasStand)
                    h.gameObject.GetComponent<MeshRenderer>().material = darkHive;
                break;
            case "Super":
                if (h.Size >= 5)
                    h.gameObject.GetComponent<MeshRenderer>().material = darkHive;
                break;
            case "Frame":
                if (h.Frames >= 10)
                    h.gameObject.GetComponent<MeshRenderer>().material = darkHive;
                break;
            case "Bee":
                if (h.Condition != "Dead")
                    h.gameObject.GetComponent<MeshRenderer>().material = darkHive;
                break;
            case "Emergency":
                if (h.Condition != "Freezing" && h.Condition != "Starving")
                    h.gameObject.GetComponent<MeshRenderer>().material = darkHive;
                break;
        }

    }

    public void OnTurnIncrement()
    {
        foreach (Hive h in hives)
            h.UpdateHive();
        honeyMarket.UpdateMarket();
    }

    public void OpenHiveUI(TemplateContainer template, VisualTreeAsset hiveUI, Hive hive)
    {
        Debug.Log(hive.canBeOpened + " " + game.CurrentState + " " + selectedItem);
        if (selectedItem != null || game.CurrentState == GameStates.Paused || game.CurrentState == GameStates.TurnEnd || hive.canBeOpened == false)
            return;

        Debug.Log("opening hive");
        //Check to see if the same hive is being clicked to close the hiveUI
        bool reclicked = false;
        if (activeUI == template && activeUI != null)
            reclicked = true;

        //Close any other open hive UI
        if (activeUI != null)
            CloseHiveUI(hive);

        if (reclicked)
            return;

        hive.isOpen = true;

        //Setup template
        if (template == null)
        {
            template = hiveUI.Instantiate();
            template.style.top = 50f;
            template.style.left = -100f;
            template.style.scale = new StyleScale(new Scale(new Vector3(0.8f, 0.8f, 1)));
            template.style.position = Position.Absolute;
        }

        //Add template to the UI document and save the template through the hive
        ui.rootVisualElement.Q("Right").Add(template);

        hive.template = template;
        activeUI = template;
        currentHive = hive;
    }

    public void CloseHiveUI(Hive hive)
    {
        if (activeUI == null)
            return;

        hive.CheckCancelAnim();

        ui.rootVisualElement.Q("Right").Remove(activeUI);
        activeUI = null;
        currentHive = null;
        hive.isOpen = false;
    }

    public void ReloadUI()
    {
        moneyLabel = ui.rootVisualElement.Q<Label>("Money");
        moneyLabel.text = "$" + money;
    }

    #region Camera Control
    private void CheckZoom()
    {
        Vector3 cameraPos = Camera.main.transform.position;
        Vector3 scrollY = new Vector3(0, Input.mouseScrollDelta.y * Time.deltaTime * 75f, 0);
        if (cameraPos.y - scrollY.y > 2.25f * map.mapWidth)
            Camera.main.transform.position = new Vector3(cameraPos.x, 2.25f * map.mapWidth, cameraPos.z);
        else if (cameraPos.y - scrollY.y < map.mapWidth)
            Camera.main.transform.position = new Vector3(cameraPos.x, map.mapWidth, cameraPos.z);
        else
            Camera.main.transform.position -= scrollY;
    }

    private void PanCamera()
    {
        Vector3 cameraPos = Camera.main.transform.position;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
        {
            float sens = Input.GetKey(KeyCode.A) ? -10f : 10f;
            Vector3 panZ = new Vector3(0, 0, sens * Time.deltaTime);
            if (cameraPos.z + panZ.z > map.mapHeight * .75f * 2f)
                Camera.main.transform.position = new Vector3(cameraPos.x, cameraPos.y, map.mapHeight * .75f * 2f);
            else if (cameraPos.z + panZ.z < 0)
                Camera.main.transform.position = new Vector3(cameraPos.x, cameraPos.y, 0f);
            else
                Camera.main.transform.position += panZ;
        }

        cameraPos = Camera.main.transform.position;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S))
        {
            float sens = Input.GetKey(KeyCode.W) ? -10f : 10f;
            Vector3 panX = new Vector3(sens * Time.deltaTime, 0, 0);
            if (cameraPos.x + panX.x > map.mapWidth * 2f)
                Camera.main.transform.position = new Vector3(map.mapWidth * 2f, cameraPos.y, cameraPos.z);
            else if (cameraPos.x + panX.x < 0)
                Camera.main.transform.position = new Vector3(0f, cameraPos.y, cameraPos.z);
            else
                Camera.main.transform.position += panX;
        }
    }

    public void CenterCamera()
    {
        int offset = (map.mapWidth % 2 == 0) ? 2 : 3;
        Camera.main.transform.position = new Vector3(map.mapWidth + (0.15f * Mathf.Pow(map.mapWidth, 1.75f)), 2.25f * map.mapWidth, (map.mapWidth / 2) + offset);
    }
    #endregion

    #region Save Load
    public void Save(ref PlayerSaveData data)
    {
        var values = System.Enum.GetValues(typeof(FlowerType));
        data.savedMoney = money;

        List<int> xs = new List<int>();
        List<int> ys = new List<int>();
        List<bool> hasSugar = new List<bool>();
        List<bool> hasReducer = new List<bool>();
        List<bool> hasStand = new List<bool>();
        List<bool> hasRepellant = new List<bool>();
        List<bool> hasInsulation = new List<bool>();
        List<float> honey = new List<float>();
        List<float> honeyPurity = new List<float>();
        List<FlowerType> honeyType = new List<FlowerType>();
        List<float> comb = new List<float>();
        List<int> combSizeCap = new List<int>();
        List<float> nectar = new List<float>();
        List<float> nectarGain = new List<float>();
        List<float> possibleNectar = new List<float>();
        List<float> population = new List<float>();
        List<int> size = new List<int>();
        List<string> condition = new List<string>();
        List<float> nectarValues = new List<float>();


        //Queen
        List<bool> nullQueen = new List<bool>();
        List<bool> finishedGenerating = new List<bool>();
        List<float> constructionMult = new List<float>();
        List<float> productionMult = new List<float>();
        List<float> collectionMult = new List<float>();
        List<float> resilienceMult = new List<float>();
        List<float> aggressivnessMult = new List<float>();
        List<string> species = new List<string>();
        List<int> age = new List<int>();
        List<float> grade = new List<float>();
        List<string> quirks = new List<string>();
        List<int> quirksCount = new List<int>();

        //honey inventory
        List<float> honeyInventory = new List<float>();

        foreach (Hive h in hives)
        {
            xs.Add(h.x);
            ys.Add(h.y);
            hasSugar.Add(h.hasSugar);
            hasReducer.Add(h.hasReducer);
            hasStand.Add(h.hasStand);
            hasRepellant.Add(h.hasRepellant);
            hasInsulation.Add(h.hasInsulation);
            honey.Add(h.honey);
            honeyPurity.Add(h.honeyPurity);
            honeyType.Add(h.honeyType);
            comb.Add(h.comb);
            combSizeCap.Add(h.combSizeCap);
            nectar.Add(h.nectar);
            nectarGain.Add(h.nectarGain);
            possibleNectar.Add(h.nectarGain);
            population.Add(h.population);
            size.Add(h.Size);
            condition.Add(h.Condition);

            //Queen
            nullQueen.Add(h.queen.nullQueen);
            finishedGenerating.Add(h.queen.finishedGenerating);
            constructionMult.Add(h.queen.constructionMult);
            productionMult.Add(h.queen.productionMult);
            collectionMult.Add(h.queen.collectionMult);
            resilienceMult.Add(h.queen.resilienceMult);
            aggressivnessMult.Add(h.queen.aggressivnessMult);
            species.Add(h.queen.species);
            age.Add(h.queen.age);
            grade.Add(h.queen.grade);
            foreach (string s in h.queen.quirks)
                quirks.Add(s);
            quirksCount.Add(h.queen.quirks.Count);

            //nectar values
            foreach (var v in values)
            {
                FlowerType fType = (FlowerType)v;
                nectarValues.Add(h.nectarValues[fType]);
            }
        }

        foreach (var v in values)
        {
            FlowerType fType = (FlowerType)v;
            for (int i = 0; i < 4; i++)
                honeyInventory.Add(inventory[fType][i]);
        }


        data.hiveCount = hives.Count;
        data.xs = xs;
        data.ys = ys;
        data.hasSugar = hasSugar;
        data.hasReducer = hasReducer;
        data.hasStand = hasStand;
        data.hasRepellant = hasRepellant;
        data.hasInsulation = hasInsulation;
        data.honey = honey;
        data.honeyPurity = honeyPurity;
        data.honeyType = honeyType;
        data.comb = comb;
        data.combSizeCap = combSizeCap;
        data.nectar = nectar;
        data.nectarGain = nectarGain;
        data.possibleNectar = possibleNectar;
        data.population = population;
        data.size = size;
        data.condition = condition;

        data.nectarValues = nectarValues;

        //Queen
        data.nullQueen = nullQueen;
        data.finishedGenerating = finishedGenerating;
        data.constructionMult = constructionMult;
        data.productionMult = productionMult;
        data.collectionMult = collectionMult;
        data.resilienceMult = resilienceMult;
        data.aggressivnessMult = aggressivnessMult;
        data.species = species;
        data.age = age;
        data.grade = grade;
        data.quirks = quirks;
        data.quirksCount = quirksCount;

        data.honeyInventory = honeyInventory;
    }

    public void Load(PlayerSaveData data)
    {
        fromSave = true;
        game = GameObject.Find("GameController").GetComponent<GameController>();
        map = GameObject.Find("MapLoader").GetComponent<MapLoader>();
        ui = GameObject.Find("UIDocument").GetComponent<UIDocument>();
        hexMenu = ui.gameObject.GetComponent<HexMenu>();
        money = data.savedMoney;
        var values = System.Enum.GetValues(typeof(FlowerType));
        int k = 0;
        for (int i = 0; i < data.hiveCount; i++)
        {
            Hive hive = null;
            if (i < data.hiveCount)
            {
                Vector3 pos = new Vector3(data.xs[i], 0, data.ys[i]);
                GameObject hiveObject = Instantiate(hivePrefab, pos, Quaternion.identity);
                hive = hiveObject.GetComponent<Hive>();
                hive.fromSave = true;
                hive.placed = true;
                hive.x = data.xs[i];
                hive.y = data.ys[i];
                hive.hiveTile = map.tiles[data.xs[i] / 2, data.ys[i] / 2];
                hive.hasSugar = data.hasSugar[i];
                hive.hasReducer = data.hasReducer[i];
                hive.hasStand = data.hasStand[i];
                if (data.hasStand[i])
                {
                    Instantiate(standObject, hive.transform.position, Quaternion.identity);
                    hive.transform.position += Vector3.up;
                }
                hive.hasRepellant = data.hasRepellant[i];
                hive.hasInsulation = data.hasInsulation[i];
                hive.honey = data.honey[i];
                hive.honeyPurity = data.honeyPurity[i];
                hive.honeyType = data.honeyType[i];
                hive.comb = data.comb[i];
                hive.combSizeCap = data.combSizeCap[i];
                hive.nectar = data.nectar[i];
                hive.nectarGain = data.nectarGain[i];
                hive.possibleNectar = data.possibleNectar[i];
                hive.population = data.population[i];
                hive.Size = data.size[i];
                hive.Condition = data.condition[i];

                foreach (var v in values)
                {
                    FlowerType fType = (FlowerType)v;
                    hive.nectarValues[fType] = data.nectarValues[k];
                    k++;
                }
            }

            //hive queens
            QueenBee queen = hive.gameObject.GetComponent<QueenBee>();
            queen.fromSave = true;
            SetQueens(queen, data, i);
            hive.queen = queen;
            OpenHiveUI(null, hiveUI, hive);
            hive.SetUpTemplate();
            CloseHiveUI(hive);
            hive.LoadPopulate();
            hive.UpdateMeters();
            hives.Add(hive);
        }

        //honey inventory
        int count = 0;
        foreach (var v in values)
        {
            FlowerType fType = (FlowerType)v;
            List<float> inv = new List<float>() { data.honeyInventory[count], data.honeyInventory[count + 1], data.honeyInventory[count + 2], data.honeyInventory[count + 3] };
            inventory[fType] = inv;
            count += 4;
        }
    }

    private void SetQueens(QueenBee queen, PlayerSaveData data, int i)
    {
        if (data.nullQueen[i] == false)
        {
            queen.nullQueen = false;
            queen.finishedGenerating = true;
            queen.constructionMult = data.constructionMult[i];
            queen.productionMult = data.productionMult[i];
            queen.collectionMult = data.collectionMult[i];
            queen.resilienceMult = data.resilienceMult[i];
            queen.aggressivnessMult = data.aggressivnessMult[i];
            queen.species = data.species[i];
            queen.age = data.age[i];
            queen.grade = data.grade[i];

            int count = 0;
            for (int j = 0; count < data.quirksCount[i]; j = 0)
            {
                if (data.quirks.Count == 0)
                    break;
                queen.quirks.Add(data.quirks[j]);
                data.quirks.RemoveAt(j);
                count++;
            }
        }
        else
        {
            queen.nullQueen = true;
            queen.finishedGenerating = true;
        }
    }
    #endregion
}

[System.Serializable]
public struct PlayerSaveData
{
    public int savedMoney;

    public int hiveCount;
    public List<int> xs;
    public List<int> ys;
    public List<bool> hasSugar;
    public List<bool> hasReducer;
    public List<bool> hasStand;
    public List<bool> hasRepellant;
    public List<bool> hasInsulation;
    public List<float> honey;
    public List<float> honeyPurity;
    public List<FlowerType> honeyType;
    public List<float> comb;
    public List<int> combSizeCap;
    public List<float> nectar;
    public List<float> nectarGain;
    public List<float> possibleNectar;
    public List<float> population;
    public List<int> size;
    public List<string> condition;

    public List<float> nectarValues;

    //queen
    public List<bool> nullQueen;
    public List<bool> finishedGenerating;
    public List<float> constructionMult;
    public List<float> productionMult;
    public List<float> collectionMult;
    public List<float> resilienceMult;
    public List<float> aggressivnessMult;
    public List<string> species;
    public List<int> age;
    public List<float> grade;
    public List<string> quirks;
    public List<int> quirksCount;

    //honey inventory
    public List<float> honeyInventory;
}
