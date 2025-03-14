using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    GameObject hivePrefab;

    List<List<Texture2D>> spriteList = new List<List<Texture2D>>();

    List<List<GameObject>> objectList = new List<List<GameObject>>();

    [SerializeField]
    List<Texture2D> flowerSprites = new List<Texture2D>();

    [SerializeField]
    List<GameObject> flowerObjectList = new List<GameObject>();

    [SerializeField]
    MapLoader map;

    [SerializeField]
    UIDocument ui;

    [SerializeField]
    HoneyMarket honeyMarket;

    [SerializeField]
    public Sprite honey;

    //[SerializeField]
    //private VisualTreeAsset hiveUI;

    private TemplateContainer activeUI;

    private List<Hive> hives = new List<Hive>();

    private VisualElement root;
    private VisualElement left;
    private CustomVisualElement tab1;
    private CustomVisualElement tab2;
    private CustomVisualElement tab3;
    private CustomVisualElement tab4;

    private int tab1ItemCount = 4;
    private int tab2ItemCount = 4;
    private int tab3ItemCount = 4;
    private int tab4ItemCount = 4;

    [SerializeField]
    Texture2D hex;

    [SerializeField]
    StyleSheet tabStyle;

    [SerializeField]
    StyleSheet itemStyle;

    int tabItemsPerRow = 8;

    private List<CustomVisualElement> tabHexes = new List<CustomVisualElement>();
    private List<CustomVisualElement> tabs = new List<CustomVisualElement>();
    private List<int> tabItemCounts = new List<int>();
    private CustomVisualElement activeTab;

    private Manipulator close;
    private Manipulator open1;
    private Manipulator open2;
    private Manipulator open3;
    private Manipulator open4;

    private GameObject selectedItem = null;
    private GameObject hoverObject = null;
    private bool hovering;

    public GameObject SelectedItem
    {
        get { return selectedItem; }
        set
        {
            selectedItem = value;
            if (value == null)
            {
                hovering = false;
                hoverObject = null;
            }
            else
            {
                hoverObject = Instantiate(selectedItem, new Vector3(-100, -100, -100), Quaternion.identity);
                hovering = true;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        root = ui.rootVisualElement;
        left = root.Q<VisualElement>("Left");

        close = new Clickable(close => CloseTab());
        open1 = new Clickable(open => OpenTab(0, open1));
        open2 = new Clickable(open => OpenTab(1, open2));
        open3 = new Clickable(open => OpenTab(2, open3));
        open4 = new Clickable(open => OpenTab(3, open4));

        tab1 = root.Q<CustomVisualElement>("tab1");
        tab1.AddManipulator(open1);
        tabs.Add(tab1);
        tab2 = root.Q<CustomVisualElement>("tab2");
        tab2.AddManipulator(open2);
        tabs.Add(tab2);
        tab3 = root.Q<CustomVisualElement>("tab3");
        tab3.AddManipulator(open3);
        tabs.Add(tab3);
        tab4 = root.Q<CustomVisualElement>("tab4");
        tab4.AddManipulator(open4);
        tabs.Add(tab4);

        tabItemCounts.Add(tab1ItemCount);
        tabItemCounts.Add(tab2ItemCount);
        tabItemCounts.Add(tab3ItemCount);
        tabItemCounts.Add(tab4ItemCount);

        spriteList.Add(flowerSprites);
        spriteList.Add(flowerSprites);
        spriteList.Add(flowerSprites);
        spriteList.Add(flowerSprites);

        objectList.Add(flowerObjectList);
        objectList.Add(flowerObjectList);
        objectList.Add(flowerObjectList);
        objectList.Add(flowerObjectList);

        Debug.Log(honey.texture.name);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            foreach (Hive h in hives)
                h.UpdateHive();
            honeyMarket.UpdateMarket();
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            map.GenerateFlowers();
        }
        //if (Input.GetKeyDown(KeyCode.UpArrow))
        //{
        //    OpenHiveUI();
        //}
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            SelectedItem = hivePrefab;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (SelectedItem != null)
                SelectedItem = null;
            else if (activeUI != null)
                CloseHiveUI();
            else
                CloseTab();
        }

        if (SelectedItem != null && hoverObject != null)
            checkForClick();

        if (hovering && hoverObject != null && selectedItem != null)
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit, 1000, LayerMask.GetMask("Tile")))
            {
                if (hit.collider.gameObject.TryGetComponent<Tile>(out Tile t))
                {
                    hoverObject.transform.position = t.gameObject.transform.position;
                }
            }
        }

        //Map Controls
        CheckZoom();
        PanCamera();
    }

    private void checkForClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out var hit, 1000, LayerMask.GetMask("Tile")))
            {
                if (hit.collider.gameObject.TryGetComponent<Tile>(out Tile t))
                {
                    hoverObject.transform.position = t.gameObject.transform.position;
                    if (hoverObject.TryGetComponent(out Hive h))
                    {
                        hives.Add(h);
                        h.x = (int)t.transform.position.x;
                        h.y = (int)t.transform.position.z;
                    }
                    SelectedItem = null;
                }
            }
        }
    }

    #region Hex Tab Menus
    private void OpenTab(int num, Manipulator open)
    {
        //Close any open tabs
        if (activeTab != null)
        {
            CloseTab();
        }

        CustomVisualElement tab = tabs[num];

        int items = tabItemCounts[tabs.IndexOf(tab)];
        int itemsInRow = 0;
        int rows = 1;

        activeTab = tab;
        tab.AddManipulator(close);
        tab.RemoveManipulator(open);
        
        //Grey out other tabs, highlight clicked tab
        foreach (CustomVisualElement t in tabs)
        {
            if (t != tab)
                t.style.unityBackgroundImageTintColor = new Color(0.36f, 0.21f, 0.17f, 0.875f);
            else
                t.style.unityBackgroundImageTintColor = new Color(1f, 1f, 0f, 1f);
        }

        //Create hex items that belong to that tab
        for (int i = 0; i < items; i++)
        {
            //hex to be added
            CustomVisualElement hex = new CustomVisualElement();

            //Separate calculations for first item of each row
            if (i % tabItemsPerRow == 0)
                SpawnTopHex(num, i);
            else
            {
                //Get the last hex created
                CustomVisualElement lastHex = tabHexes[tabHexes.Count - 1];
                hex.styleSheets.Add(tabStyle);

                //Calculate and set position of new hex to be flush with the previous hexes
                //NOTE: tab1.resolvedStyle is used as the basis calculations since it is placed before runtime
                //and the resolvedStyle of it will give back dimensions that take into account scaling with screen size.
                float hexTop = (tab1.resolvedStyle.height * itemsInRow * .625f) + (tab1.resolvedStyle.height * .5f) + (tab1.resolvedStyle.marginBottom * .5f);
                hex.style.top = hexTop;
                StyleLength hexLeft = (itemsInRow % 2) == 0 ? lastHex.resolvedStyle.left - (tab1.resolvedStyle.width * .125f) + (tab1.resolvedStyle.width * .75f * (rows - 1)) 
                    : lastHex.resolvedStyle.left + (tab1.resolvedStyle.width * .24f) + (tab1.resolvedStyle.width * .75f * (rows - 1));
                hex.style.left = hexLeft;

                //Set the width and height
                hex.style.height = tab1.resolvedStyle.height;
                hex.style.width = tab1.resolvedStyle.width;

                //Add to UI container and the list of active hexes
                left.Add(hex);
                tabHexes.Add(hex);

                //Add Icon to each hex item
                VisualElement icon = new VisualElement();
                icon.styleSheets.Add(itemStyle);
                icon.style.backgroundImage = spriteList[num][i];
                int list = num; //For some reason, when the clickable event is triggered, it goes back to find
                int item = i; //what num and i are equal to retroactivly. This causes index out of bounds. Store values as ints to avoid.
                hex.AddManipulator(new Clickable(e => SelectItem(objectList[list][item])));
                hex.Add(icon);
                itemsInRow++;
            }

            //Start a new row when end of the row is reached.
            if (itemsInRow == tabItemsPerRow - 1)
            {
                itemsInRow = -1;
                rows++;
            }
        }
    }

    private void SpawnTopHex(int num, int i)
    {
        CustomVisualElement starterHex = new CustomVisualElement();
        starterHex.styleSheets.Add(tabStyle);
        float itemTop = tab1.resolvedStyle.top;
        starterHex.style.top = itemTop;
        StyleLength itemLeft = tab1.resolvedStyle.width * .25f;
        starterHex.style.left = itemLeft;

        starterHex.style.height = tab1.resolvedStyle.height;
        starterHex.style.width = tab1.resolvedStyle.width;
        left.Add(starterHex);
        tabHexes.Add(starterHex);

        VisualElement icon = new VisualElement();
        icon.styleSheets.Add(itemStyle);
        icon.style.backgroundImage = spriteList[num][i];
        starterHex.AddManipulator(new Clickable(e => SelectItem(objectList[num][i])));
        starterHex.Add(icon);
    }

    private void CloseTab()
    {
        if (activeTab == null)
            return;
        else
        {
            foreach (CustomVisualElement t in tabs)
                t.style.unityBackgroundImageTintColor = Color.white;
            foreach (CustomVisualElement hex in tabHexes)
                left.Remove(hex);

            activeTab.RemoveManipulator(close);

            if (activeTab == tab1)
                activeTab.AddManipulator(open1);
            else if (activeTab == tab2)
                activeTab.AddManipulator(open2);
            else if (activeTab == tab3)
                activeTab.AddManipulator(open3);
            else if (activeTab == tab4)
                activeTab.AddManipulator(open4);

            activeTab = null;
            tabHexes.Clear();
        }
    }

    private void SelectItem(GameObject item)
    {
        SelectedItem = item;
    }
    #endregion

    public void OpenHiveUI(TemplateContainer template, VisualTreeAsset hiveUI, Hive hive)
    {
        //Check to see if the same hive is being clicked to close the hiveUI
        bool reclicked = false;
        if (activeUI == template && activeUI != null)
            reclicked = true;

        if (activeUI != null)
            CloseHiveUI();

        if (reclicked)
            return;

        if (template == null)
        {
            template = hiveUI.Instantiate();
            template.style.top = 0f;
            template.style.left = 0f;
            template.style.scale = new StyleScale(new Scale(new Vector3(0.8f, 0.8f, 1)));
        }

        ui.rootVisualElement.Q("Right").Add(template);

        hive.template = template;
        activeUI = template;
    }

    public void CloseHiveUI()
    {
        ui.rootVisualElement.Q("Right").Remove(activeUI);
        activeUI = null;
    }

    #region Camera Control
    private void CheckZoom()
    {
        Vector3 cameraPos = Camera.main.transform.position;
        Vector3 scrollY = new Vector3(0, Input.mouseScrollDelta.y * Time.deltaTime * 75f, 0);
        if (cameraPos.y - scrollY.y > 22.5)
            Camera.main.transform.position = new Vector3(cameraPos.x, 22.5f, cameraPos.z);
        else if (cameraPos.y - scrollY.y < 10)
            Camera.main.transform.position = new Vector3(cameraPos.x, 10f, cameraPos.z);
        else
            Camera.main.transform.position -= scrollY;
    }

    private void PanCamera()
    {
        Vector3 cameraPos = Camera.main.transform.position;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S))
        {
            float sens = Input.GetKey(KeyCode.S) ? -10f : 10f;
            Vector3 panZ = new Vector3(0, 0, sens * Time.deltaTime);
            if (cameraPos.z + panZ.z > map.mapHeight * .75f * 2f)
                Camera.main.transform.position = new Vector3(cameraPos.x, cameraPos.y, map.mapHeight * .75f * 2f);
            else if (cameraPos.z + panZ.z < 0)
                Camera.main.transform.position = new Vector3(cameraPos.x, cameraPos.y, 0f);
            else
                Camera.main.transform.position += panZ;
        }

        cameraPos = Camera.main.transform.position;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
        {
            float sens = Input.GetKey(KeyCode.A) ? -10f : 10f;
            Vector3 panX = new Vector3(sens * Time.deltaTime, 0, 0);
            if (cameraPos.x + panX.x > map.mapWidth * 2f)
                Camera.main.transform.position = new Vector3(map.mapWidth * 2f, cameraPos.y, cameraPos.z);
            else if (cameraPos.x + panX.x < 0)
                Camera.main.transform.position = new Vector3(0f, cameraPos.y, cameraPos.z);
            else
                Camera.main.transform.position += panX;
        }
    }
    #endregion
}
