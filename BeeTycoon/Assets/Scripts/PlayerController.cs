using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    GameObject hivePrefab;

    private List<Hive> hives = new List<Hive>();

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
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            GameObject temp = Instantiate(hivePrefab, new Vector3(20, 1, 20), Quaternion.identity);
            hives.Add(temp.GetComponent<Hive>());
        }
    }
}
