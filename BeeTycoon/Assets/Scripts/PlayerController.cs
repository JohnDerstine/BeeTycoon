using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    GameObject hivePrefab;

    [SerializeField]
    MapLoader map;

    private List<Hive> hives = new List<Hive>();

    private bool placing;

    // Start is called before the first frame update
    void Start()
    {
        
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
}
