using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    GameObject hivePrefab;

    [SerializeField]
    MapLoader map;

    [SerializeField]
    UIDocument ui;

    private List<Hive> hives = new List<Hive>();

    private bool placing;
    private VisualElement root;
    private VisualElement left;
    private CustomVisualElement tab1;
    private CustomVisualElement tab2;
    private CustomVisualElement tab3;
    private CustomVisualElement tab4;

    [SerializeField]
    Texture2D hex;

    [SerializeField]
    StyleSheet tabStyle;

    int tabItemsPerRow = 7;

    private List<CustomVisualElement> tabHexes = new List<CustomVisualElement>();
    private List<CustomVisualElement> tabs = new List<CustomVisualElement>();
    private CustomVisualElement activeTab;

    private Manipulator close;
    private Manipulator open1;
    private Manipulator open2;
    private Manipulator open3;
    private Manipulator open4;

    // Start is called before the first frame update
    void Start()
    {
        root = ui.rootVisualElement;
        left = root.Q<VisualElement>("Left");

        close = new Clickable(close => CloseTab());
        open1 = new Clickable(open => OpenTab(tab1, open1));
        open2 = new Clickable(open => OpenTab(tab2, open2));
        open3 = new Clickable(open => OpenTab(tab3, open3));
        open4 = new Clickable(open => OpenTab(tab4, open4));

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
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
            foreach (Hive h in hives)
                h.UpdateHive();
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            map.GenerateFlowers();
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            placing = !placing;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseTab();
        }

            if (placing)
            checkForClick();
    }

    private void checkForClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            PointerEventData pointer = new PointerEventData(EventSystem.current);
            pointer.position = Input.mousePosition;
            pointer.position = new Vector3(pointer.position.x, Screen.height - pointer.position.y);

            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out var hit, 1000, LayerMask.GetMask("Tile")))
            {
                Debug.Log("clicked");
                if (hit.collider.gameObject.TryGetComponent<Tile>(out Tile t))
                {
                    GameObject temp = Instantiate(hivePrefab, t.transform.position, Quaternion.identity);
                    hives.Add(temp.GetComponent<Hive>());
                    temp.GetComponent<Hive>().x = (int)t.transform.position.x;
                    temp.GetComponent<Hive>().x = (int)t.transform.position.z;
                    Debug.Log((int)t.transform.position.x + " " + (int)t.transform.position.z);
                    placing = false;
                }
            }
        }
    }

    private void OpenTab(CustomVisualElement tab, Manipulator open)
    {
        if (activeTab != null)
        {
            CloseTab();
        }

        activeTab = tab;
        tab.AddManipulator(close);
        tab.RemoveManipulator(open);
        foreach (CustomVisualElement t in tabs)
        {
            if (t != tab)
                t.style.unityBackgroundImageTintColor = new Color(0.36f, 0.21f, 0.17f, 0.875f);
            else
                t.style.unityBackgroundImageTintColor = new Color(1f, 1f, 0f, 1f);
        }


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

        for (int i = 0; i < tabItemsPerRow; i++)
        {
            CustomVisualElement hex = new CustomVisualElement();
            CustomVisualElement lastHex = tabHexes[tabHexes.Count - 1];
            hex.styleSheets.Add(tabStyle);
            float modifier = (i == 0) ? 1 : .625f;
            float hexTop = (tab1.resolvedStyle.height * i * modifier) + (tab1.resolvedStyle.height * .5f) + (tab1.resolvedStyle.marginBottom * .5f);
            hex.style.top = hexTop;
            StyleLength hexLeft = (i % 2) == 0 ? lastHex.resolvedStyle.left - (tab1.resolvedStyle.width * .125f) : lastHex.resolvedStyle.left + (tab1.resolvedStyle.width * .24f);
            hex.style.left = hexLeft;

            hex.style.height = tab1.resolvedStyle.height;
            hex.style.width = tab1.resolvedStyle.width;
            left.Add(hex);
            tabHexes.Add(hex);
        }
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
}
