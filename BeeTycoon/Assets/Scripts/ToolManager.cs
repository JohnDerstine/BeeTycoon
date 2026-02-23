using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolManager : MonoBehaviour
{
    [SerializeField]
    ShovelTool shovel;

    //[SerializeField]
    // SmokerTool smoker;

    [SerializeField]
    DollyTool dolly;
    //Add others later


    //private ShovelTool shovelTool;
    //private DollyTool dollyTool;

    //Object to Move
    [SerializeField]
    private GameObject holo;

    [SerializeField]
    private Material redHolo;

    [SerializeField]
    private Material greenHolo;

    [SerializeField]
    private MapLoader map;

    private GameObject objectToMove;
    private Vector3 storedPos;
    private Tile storedTile;
    private FlowerType storedFType;
    private GameObject activeHolo;
    bool pickedUpThisFrame;

    private ToolScript activeTool;

    public GameObject ObjectToMove
    {
        get { return objectToMove; }
        set
        {
            if (value == null)
            {
                objectToMove.transform.position = storedPos;
                storedTile.Flower = storedFType;
                storedTile = null;
                storedFType = FlowerType.Empty;
                Destroy(ObjectToMove);
            }
            else
            {
                Destroy(activeHolo);
                activeHolo = Instantiate(holo, value.transform, true); //holo hover for placeables
                value.TryGetComponent<Cost>(out Cost c);
                if (c != null && c.tree)
                {
                    activeHolo.transform.localScale = new Vector3(3, 3, 3);
                    activeHolo.transform.position = new Vector3(value.transform.position.x, 2f, value.transform.position.z);
                }
                storedPos = value.transform.position;

                pickedUpThisFrame = true;
                if (value.TryGetComponent<Hive>(out Hive h))
                {
                    h.hiveTile.HasHive = false;
                    h.hiveTile.hive = h;
                }
            }
            objectToMove = value;
        }
    }

    void Awake()
    {
        map = GameObject.Find("MapLoader").GetComponent<MapLoader>();
    }

    //Logic for tool functionality
    void Update()
    {
        pickedUpThisFrame = false;

        if (Input.GetMouseButtonDown(1))
        {
            if (ObjectToMove != null)
                ObjectToMove = null;
            activeTool = null;
        }

        if (activeTool == dolly || activeTool == shovel)
        {
            if (ObjectToMove == null)
                CheckForPickup();
            else
                CheckForPlacement();
        }

        if (objectToMove != null && !pickedUpThisFrame)
        {
            FollowCursor();
        }
        //Else if other tool active, CheckForUse()
    }

    public void SetActiveTool(GameObject item)
    {   
        switch (item.tag)
        {
            case "Dolly":
                activeTool = dolly;
                break;
            case "Shovel":
                activeTool = shovel;
                break;
            default:
                activeTool = null;
                break;
        }
    }

    public void SetToolNull(){ 
        activeTool = null; 
    }

    private void CheckForPickup()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out var tileHit, 1000, LayerMask.GetMask("Tile")))
            {
                if (tileHit.collider.gameObject.TryGetComponent<Tile>(out Tile t))
                {
                    if (activeTool == shovel && shovel.uses > 0 && t.Flower != FlowerType.Empty)
                    {
                        storedTile = t;
                        storedFType = t.Flower;
                        t.FlowerFixed = FlowerType.Empty;
                        ObjectToMove = t.FlowerObject;
                    }
                    else if (activeTool == dolly && dolly.uses > 0 && t.HasHive)
                    {
                        ObjectToMove = t.hive.gameObject;
                        storedTile = t;
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
            if (Physics.Raycast(ray, out var trashHit, 1000, LayerMask.GetMask("Trash")) && activeTool == shovel)
            {
                Destroy(objectToMove);
                shovel.uses--;
                CleanUpShovel();
                return;
            }

            if (Physics.Raycast(ray, out var tileHit, 1000, LayerMask.GetMask("Tile")))
            {
                if (tileHit.collider.gameObject.TryGetComponent<Tile>(out Tile t))
                {
                    if (activeTool == shovel)
                    {
                        if (t.Flower == FlowerType.Empty && !t.HasHive)
                        {
                            t.Flower = storedFType;
                            shovel.uses--;
                            CleanUpShovel(t);
                        }
                    }
                    else if (activeTool == dolly && t.Flower == FlowerType.Empty && !t.HasHive)
                    {
                        Hive h = objectToMove.GetComponent<Hive>();
                        h.hiveTile = t;
                        t.HasHive = true;
                        t.hive = h;
                        h.x = (int)t.transform.position.x;
                        h.y = (int)t.transform.position.z;
                        h.transform.position = t.transform.position;
                        h.transform.position += new Vector3(0, 0.5f, 0);
                        dolly.uses--;
                        CleanUpDolly(t);
                    }
                }
            }
        }
    }

    private void CleanUpShovel(Tile t = null)
    {
        if (activeHolo != null)
            Destroy(activeHolo);
        objectToMove = null;
        if (storedTile != t)
            storedTile.Flower = FlowerType.Empty;
        storedTile = null;
        storedFType = FlowerType.Empty;
    }

    private void CleanUpDolly(Tile t)
    {
        if (activeHolo != null)
            Destroy(activeHolo);
        objectToMove = null;
        if (storedTile != t)
        {
            storedTile.HasHive = false;
            storedTile.hive = null;
            storedTile = null;
        }
        storedFType = FlowerType.Empty;
    }

    private void FollowCursor()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        //If a tile is clicked while holding a placeable object, place the object
        if (Physics.Raycast(ray, out var tileHit, 1000, LayerMask.GetMask("Tile")))
        {
            if (objectToMove.GetComponent<Cost>().tree)
                activeHolo.transform.position = new Vector3(objectToMove.transform.position.x, 2f, objectToMove.transform.position.z);
            else
                activeHolo.transform.position = objectToMove.transform.position;

            if (tileHit.collider.gameObject.TryGetComponent<Tile>(out Tile t))
            {
                if (objectToMove.GetComponent<Cost>().tree)
                    objectToMove.transform.position = new Vector3(t.gameObject.transform.position.x + 1, t.gameObject.transform.position.y, t.gameObject.transform.position.z + 1);
                else
                    objectToMove.transform.position = t.gameObject.transform.position;

                if (objectToMove.GetComponent<Cost>().tree && (t.y == map.mapHeight - 1 || t.x == map.mapWidth - 1 || !t.Check234() || t.HasHive || t.Flower != FlowerType.Empty))
                    activeHolo.GetComponent<MeshRenderer>().material = redHolo;
                else if (t.HasHive || t.Flower != FlowerType.Empty)
                    activeHolo.GetComponent<MeshRenderer>().material = redHolo;
                else
                    activeHolo.GetComponent<MeshRenderer>().material = greenHolo;
            }
        }
        else if (Physics.Raycast(ray, out var hit2, 1000, LayerMask.GetMask("OOB")))
        {
            activeHolo.GetComponent<MeshRenderer>().material = redHolo;
            objectToMove.transform.position = hit2.point;
            if (objectToMove.GetComponent<Cost>().tree)
                activeHolo.transform.position = new Vector3(objectToMove.transform.position.x, objectToMove.transform.position.y + 2f, objectToMove.transform.position.z);
            else
                activeHolo.transform.position = objectToMove.transform.position;
        }
    }
}
