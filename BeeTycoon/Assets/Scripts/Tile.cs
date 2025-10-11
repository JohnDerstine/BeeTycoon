using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public MapLoader map;

    private FlowerType flower = FlowerType.Empty;
    private GameObject flowerObject;
    public bool completed;
    private bool hasHive;

    [SerializeField]
    private Material greenMat;
    [SerializeField]
    private Material yellowMat;
    [SerializeField]
    private Material blueMat;

    MeshRenderer matRenderer;

    public Material currentMat;

    public int x;
    public int y;
    public int lastGain;
    Tile original;

    public Tile Original
    {
        get { return original; }
    }
    public bool HasHive
    {
        get { return hasHive; }
        set
        {
            if (value)
                Flower = FlowerType.Empty;
            hasHive = value;
        }
    }

    public FlowerType FlowerFixed
    {
        get { return flower; }
        set
        {
            if ((flower == FlowerType.Orange || flower == FlowerType.Tupelo) && this != original)
            {
                original.FlowerFixed = value;
                return;
            }

            SetFlower(FlowerType.Empty);

            flower = value;
        }
    }
    public FlowerType Flower
    {
        get { return flower; }
        set
        {
            if ((flower == FlowerType.Orange || flower == FlowerType.Tupelo) && this != original)
            {
                original.Flower = value;
                return;
            }

            Destroy(flowerObject);

            SetFlower(value);

            flower = value;
        }
    }

    private void SetFlower(FlowerType current)
    {
        if (current != FlowerType.Empty && current != FlowerType.Orange && current != FlowerType.Tupelo) //&& value != flower
            flowerObject = Instantiate(map.flowerList[(int)current], transform.position, Quaternion.identity);
        else if (current != FlowerType.Empty)
        {
            original = this;
            flowerObject = Instantiate(map.flowerList[(int)current], new Vector3(transform.position.x + 1, transform.position.y, transform.position.z + 1), Quaternion.identity);
            map.tiles[x + 1, y].original = this;
            map.tiles[x, y + 1].original = this;
            map.tiles[x + 1, y + 1].original = this;
            map.tiles[x + 1, y].flower = current;
            map.tiles[x, y + 1].flower = current;
            map.tiles[x + 1, y + 1].flower = current;
            map.tiles[x + 1, y].flowerObject = flowerObject;
            map.tiles[x, y + 1].flowerObject = flowerObject;
            map.tiles[x + 1, y + 1].flowerObject = flowerObject;
        }

        if ((flower == FlowerType.Orange || flower == FlowerType.Tupelo) && current == FlowerType.Empty)
        {
            original = null;
            map.tiles[x + 1, y].original = null;
            map.tiles[x, y + 1].original = null;
            map.tiles[x + 1, y + 1].original = null;
            map.tiles[x + 1, y].flower = current;
            map.tiles[x, y + 1].flower = current;
            map.tiles[x + 1, y + 1].flower = current;
            map.tiles[x + 1, y].flowerObject = null;
            map.tiles[x, y + 1].flowerObject = null;
            map.tiles[x + 1, y + 1].flowerObject = null;
        }
    }

    public GameObject FlowerObject
    {
        get 
        {
            if (this != original && original != null)
                return original.flowerObject;
            return flowerObject; 
        }
    }

    void Start()
    {
        matRenderer = GetComponent<MeshRenderer>();
    }

    public bool Check234()
    {
        List<Tile> tiles234 = new List<Tile>() { map.tiles[x + 1, y], map.tiles[x, y + 1], map.tiles[x + 1, y + 1] };
        foreach (Tile t in tiles234)
            if (t.hasHive || t.flower != FlowerType.Empty)
                return false;
        return true;
    }

    public IEnumerator Animate(FlowerType fType, float strength, float duration, bool primary, AudioSource audio)
    {
        if (flower != fType)
            yield return new WaitForEndOfFrame();
        else
        {
            //change tile color
            if (primary)
                matRenderer.material = yellowMat;
            else
                matRenderer.material = blueMat;



            if (!primary)
            {
                yield return new WaitForSeconds(Random.Range(0.025f, 0.049f));
                audio.PlayOneShot(audio.clip, 0.5f);
            }
            else
            {
                yield return new WaitForSeconds(Random.Range(0.05f, 0.075f));
                audio.PlayOneShot(audio.clip);
            }

            for (int i = 0; i < 5; i++)
            {
                flowerObject.transform.localScale += new Vector3(0, 0.25f * strength, 0);
                yield return new WaitForSeconds(duration);
            }

            for (int i = 0; i < 5; i++)
            {
                flowerObject.transform.localScale -= new Vector3(0, 0.25f * strength, 0);
                yield return new WaitForSeconds(duration);
            }

            //revert tile color
            matRenderer.material = currentMat;
        }

        completed = true;
    }

    private void OnMouseUp()
    {
        if (Input.GetMouseButtonUp(1) && Flower != FlowerType.Empty)
            GameObject.Find("UIDocument").GetComponent<Glossary>().OpenGlossary("Flowers");
    }
}
