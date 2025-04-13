using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public MapLoader map;

    private FlowerType flower;
    private GameObject flowerObject;
    public bool completed;

    public FlowerType Flower
    {
        get { return flower; }
        set
        {
            Destroy(flowerObject);
            if (value != FlowerType.Empty && value != flower)
                flowerObject = Instantiate(map.flowerList[(int)value], transform.position, Quaternion.identity);
            flower = value;
        }
    }

    public IEnumerator Animate(FlowerType fType, float strength, float duration)
    {
        if (flower != fType)
            yield return new WaitForEndOfFrame();
        else
        {
            for (int i = 0; i < 5; i++)
            {
                flowerObject.transform.localScale += new Vector3(0.25f * strength, 0.25f * strength, 0.25f * strength);
                yield return new WaitForSeconds(duration);
            }

            for (int i = 0; i < 5; i++)
            {
                flowerObject.transform.localScale -= new Vector3(0.25f * strength, 0.25f * strength, 0.25f * strength);
                yield return new WaitForSeconds(duration);
            }
        }

        completed = true;
    }
}
