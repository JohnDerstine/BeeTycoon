using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.UIElements;
using static Unity.VisualScripting.Member;

public class ResourcePopup : MonoBehaviour
{
    [SerializeField]
    private UIDocument document;

    [SerializeField]
    private VisualTreeAsset elementToSpawn;

    [SerializeField]
    private VisualTreeAsset honeyGlobIcon;

    [SerializeField]
    private VisualTreeAsset percentLabel;

    public bool complete;

    [SerializeField]
    AudioSource source;

    [SerializeField]
    AudioSource popupSource;

    [SerializeField]
    AudioClip audio;

    [SerializeField]
    AudioClip modProc;

    [SerializeField]
    AudioClip percentAudio;

    [SerializeField]
    AudioClip modifierSound;

    [SerializeField]
    Texture2D smallSprite;

    [SerializeField]
    Texture2D mediumSprite;

    [SerializeField]
    Texture2D LargeSprite;

    bool timeComplete;

    private float accumulatedNectar = 0;
    private float percent = 0;

    TemplateContainer activePercent;
    Hive currentHive;

    const int small = 1;
    const int medium = 10;
    const int large = 100;

    public float Percent
    {
        get { return percent; }
        set
        {
            if (value / 100 > Mathf.CeilToInt(percent / 100) && Mathf.CeilToInt(percent / 100) != 0)
            {
                AudioSource hiveSource = currentHive.GetComponent<AudioSource>();
                popupSource.pitch = 1f; // Should probably make seperate audio source
                popupSource.PlayOneShot(percentAudio, 1f);
                GameObject.Find("PlayerController").GetComponent<PlayerController>().RoyalJelly++;
            }

            percent = value;
        }
    }

    public void DisplayPercent(Hive h)
    {
        RemovePercent();
        currentHive = h;
        activePercent = percentLabel.Instantiate();
        activePercent.style.position = Position.Absolute;
        document.rootVisualElement.Add(activePercent);
        activePercent.RegisterCallback((GeometryChangedEvent evt) =>
        {
            Vector3 position = h.transform.position;
            position = Camera.main.WorldToScreenPoint(position);
            float top = Screen.height - position.y - 80 - activePercent.resolvedStyle.height / 2;
            float left = position.x - activePercent.resolvedStyle.width / 2;

            activePercent.style.top = top;
            activePercent.style.left = left;
        });
    }

    public void UpdatePercent(float amount)
    {
        currentHive.SetStatsModifiers();
        accumulatedNectar += amount;
        Percent = (accumulatedNectar * .01f / currentHive.maxHoneyProduction) * 100;
        if (currentHive.maxHoneyProduction != 0)
            activePercent.Q<Label>().text = Mathf.RoundToInt(percent).ToString() + "%"; //0.01f is conversion rate

        float colorValue = percent / 100;

        if (percent < 100)
        {
            activePercent.Q<Label>().style.color = new Color(1 - colorValue, 1, 0);
            activePercent.Q<Label>().style.fontSize = 32;
        }
        else if (percent >= 100 && percent < 200)
        {
            activePercent.Q<Label>().style.color = new Color(0, 1, colorValue);
            activePercent.Q<Label>().style.fontSize = 36;
        }
        else if (percent >= 200 && percent < 300)
        {
            activePercent.Q<Label>().style.color = new Color(0, 1 - colorValue, 1);
            activePercent.Q<Label>().style.fontSize = 40;
        }
        else if (percent >= 300 && percent < 400)
        {
            activePercent.Q<Label>().style.color = new Color(colorValue, 0, 1);
            activePercent.Q<Label>().style.fontSize = 44;
        }
        else if (percent >= 400 && percent < 500)
        {
            activePercent.Q<Label>().style.color = new Color(1, 0, 1 - colorValue);
            activePercent.Q<Label>().style.fontSize = 48;
        }
        else
        {
            activePercent.Q<Label>().style.color = new Color(1, 0, 0);
            activePercent.Q<Label>().style.fontSize = 52;
        }

        activePercent.RegisterCallback((GeometryChangedEvent evt) =>
        {
            Vector3 position = currentHive.transform.position;
            position = Camera.main.WorldToScreenPoint(position);
            float top = Screen.height - position.y - 80 - activePercent.resolvedStyle.height / 2;
            float left = position.x - activePercent.resolvedStyle.width / 2;

            activePercent.style.top = top;
            activePercent.style.left = left;
        });
    }

    private void RemovePercent()
    {
        if (activePercent != null)
        {
            if (document.rootVisualElement.Contains(activePercent))
                document.rootVisualElement.Remove(activePercent);
            activePercent = null;
            accumulatedNectar = 0;
            currentHive = null;
        }
    }

    public IEnumerator ShakeModifier(VisualElement modifierElement, float duration)
    {
        float timeLapsed = 0.0f;
        VisualElement hex = modifierElement.Q<VisualElement>("Hex");
        VisualElement icon = modifierElement.Q<VisualElement>("Icon");
        float adjustAmount = 5;
        int degrees = 29;

        popupSource.pitch = 2;
        popupSource.PlayOneShot(modProc);
        while (timeLapsed < duration * 3f)
        {
            Vector3 iconEuler = icon.transform.rotation.eulerAngles;
            Vector3 hexEuler = hex.transform.rotation.eulerAngles;
            if (iconEuler.z > 180)
                iconEuler.z = iconEuler.z - 360;

            if ((iconEuler.z > degrees && adjustAmount > 0) || (iconEuler.z < -degrees && adjustAmount < 0))
            {
                adjustAmount *= -1;
                degrees -= 5;
            }

            icon.transform.rotation = Quaternion.Euler(iconEuler.x, iconEuler.y, iconEuler.z + adjustAmount);
            hex.transform.rotation = Quaternion.Euler(hexEuler.x, hexEuler.y, hexEuler.z + adjustAmount);

            yield return new WaitForSeconds(Time.deltaTime);
            timeLapsed += Time.deltaTime;
        }

        Vector3 iconEuler2 = icon.transform.rotation.eulerAngles;
        Vector3 hexEuler2 = hex.transform.rotation.eulerAngles;
        icon.transform.rotation = Quaternion.Euler(iconEuler2.x, iconEuler2.y, 0);
        hex.transform.rotation = Quaternion.Euler(hexEuler2.x, hexEuler2.y, 0);
    }

    public IEnumerator AnimateNectar(int amount, Hive h, Vector3 worldPos, float duration)
    {
        List<TemplateContainer> globs = new List<TemplateContainer>();
        source.clip = audio;

        //source.pitch = 0.5f;

        //1/5/25
        //if amount > double tier amount, start using that tier icon.

        List<Texture2D> spritesToSpawn = new List<Texture2D>();
        List<int> amountList = new List<int>();
        int modifiedAmount;
        for (modifiedAmount = amount; modifiedAmount >= large; modifiedAmount -= large)
        {
            spritesToSpawn.Add(LargeSprite);
            amountList.Add(large);
        }
        for (int ignore = 0; modifiedAmount >= medium; modifiedAmount -= medium)
        {
            spritesToSpawn.Add(mediumSprite);
            amountList.Add(medium);
        }
        for (int ignore = 0; modifiedAmount >= small; modifiedAmount -= small)
        {
            spritesToSpawn.Add(smallSprite);
            amountList.Add(small);
        }

        for (int i = 0; i < spritesToSpawn.Count; i++)
        {
            TemplateContainer glob = honeyGlobIcon.Instantiate();
            glob.style.position = Position.Absolute;
            glob.style.visibility = Visibility.Hidden;
            glob.Q("Glob").style.width = 52;
            glob.Q("Glob").style.height = 52;
            glob.Q("Glob").style.backgroundImage = spritesToSpawn[i];
            document.rootVisualElement.Q<VisualElement>("Base").Add(glob);
            globs.Add(glob);

            Vector3 position = worldPos;
            position = Camera.main.WorldToScreenPoint(position);
            float startTop = Screen.height - position.y;
            float startLeft = position.x;

            source.Play();

            yield return new WaitForEndOfFrame(); //let resolved style update

            float dir = Random.Range(0, 359);
            float radius = Random.Range(25, 50);
            float xOffset = 36;
            float yOffset = 48;

            float top = startTop - yOffset + Mathf.Sin(dir) * radius;
            float left = startLeft - xOffset + Mathf.Cos(dir) * radius;
            glob.style.visibility = Visibility.Visible;

            glob.style.top = startTop;
            glob.style.left = startLeft;

            yield return new WaitForEndOfFrame(); //let resolved style update

            StartCoroutine(ToPoint(glob, top, left, 0.5f, false, h, amount, amountList[i]));

            StartCoroutine(WaitDeltaTime(0.05f));
            yield return new WaitWhile(() => !timeComplete);
            timeComplete = false;
        }

        StartCoroutine(WaitDeltaTime(0.6f * duration));
        yield return new WaitWhile(() => !timeComplete);
        timeComplete = false;

        //source.pitch = 0.25f;
        for (int i = 0; i < amountList.Count; i++)
        {
            Vector3 pos = Camera.main.WorldToScreenPoint(h.transform.position);
            float xOffset = 36;
            float yOffset = 48;
            float top = Screen.height - pos.y - yOffset;
            float left = pos.x - xOffset;
            StartCoroutine(ToPoint(globs[i], top, left, 1 - duration, true, h, amount, amountList[i]));
            StartCoroutine(WaitDeltaTime(0.075f));
            yield return new WaitWhile(() => !timeComplete);
            timeComplete = false;

            //if (activePulse != null)
            //    StopCoroutine(activePulse);
            //activePulse = StartCoroutine(Pulse());
        }

        globs.Clear();
        complete = true;
    }

    private IEnumerator ToPoint(TemplateContainer glob, float top, float left, float t, bool destroyOnEnd, Hive h, float amount, int globs)
    {
        while (Mathf.Abs(glob.resolvedStyle.left - left) >= 10 || Mathf.Abs(glob.resolvedStyle.top - top) >= 10)
        {
            glob.style.left = Mathf.Lerp(glob.resolvedStyle.left, left, t);
            glob.style.top = Mathf.Lerp(glob.resolvedStyle.top, top, t);
            yield return new WaitForSeconds(0.05f);
        }

        if (destroyOnEnd)
        {
            AudioSource hiveSource = h.GetComponent<AudioSource>();
            hiveSource.pitch = 0.25f;
            hiveSource.PlayOneShot(audio, 0.4f);
            UpdatePercent(amount / globs);
            if (document.rootVisualElement.Q<VisualElement>("Base").Contains(glob))
                document.rootVisualElement.Q<VisualElement>("Base").Remove(glob);
        }
    }

    public IEnumerator WaitDeltaTime(float time)
    {
        float timeLapsed = 0.0f;
        while (timeLapsed < time)
        {
            timeLapsed += Time.deltaTime;
            yield return new WaitForSeconds(Time.deltaTime);
        }
        timeComplete = true;
    }
}
