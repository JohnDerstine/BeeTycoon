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

    public bool complete;

    [SerializeField]
    AudioSource source;

    [SerializeField]
    AudioClip audio;

    [SerializeField]
    AudioClip modifierSound;

    bool timeComplete;

    public IEnumerator ShakeModifier(VisualElement modifierElement, float duration)
    {
        float timeLapsed = 0.0f;
        VisualElement hex = modifierElement.Q<VisualElement>("Hex");
        VisualElement icon = modifierElement.Q<VisualElement>("Icon");
        float adjustAmount = 5;
        int degrees = 29;

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

    public IEnumerator AnimateNectar(float amount, Hive h, Vector3 worldPos, float duration)
    {
        List<TemplateContainer> globs = new List<TemplateContainer>();
        source.clip = audio;

        //source.pitch = 0.5f;
        int rounded = Mathf.CeilToInt(amount / 10);

        for (int i = 0; i < rounded; i++)
        {
            TemplateContainer glob = honeyGlobIcon.Instantiate();
            glob.style.position = Position.Absolute;
            glob.style.visibility = Visibility.Hidden;
            glob.Q("Glob").style.width = 52;
            glob.Q("Glob").style.height = 52;
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

            StartCoroutine(ToPoint(glob, top, left, 0.5f, false, h));

            StartCoroutine(WaitDeltaTime(0.05f));
            yield return new WaitWhile(() => !timeComplete);
            timeComplete = false;
        }

        StartCoroutine(WaitDeltaTime(0.6f * duration));
        yield return new WaitWhile(() => !timeComplete);
        timeComplete = false;

        //source.pitch = 0.25f;
        foreach (TemplateContainer glob in globs)
        {
            Vector3 pos = Camera.main.WorldToScreenPoint(h.transform.position);
            float xOffset = 36;
            float yOffset = 48;
            float top = Screen.height - pos.y - yOffset;
            float left = pos.x - xOffset;
            StartCoroutine(ToPoint(glob, top, left, 1 - duration, true, h));
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

    private IEnumerator ToPoint(TemplateContainer glob, float top, float left, float t, bool destroyOnEnd, Hive h)
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
