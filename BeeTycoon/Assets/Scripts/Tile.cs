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
    public FlowerType Flower
    {
        get { return flower; }
        set
        {
            Destroy(flowerObject);
            if (value != FlowerType.Empty) //&& value != flower
                flowerObject = Instantiate(map.flowerList[(int)value], transform.position, Quaternion.identity);

            flower = value;
        }
    }

    public GameObject FlowerObject
    {
        get { return flowerObject; }
    }

    void Start()
    {
        matRenderer = GetComponent<MeshRenderer>();
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
