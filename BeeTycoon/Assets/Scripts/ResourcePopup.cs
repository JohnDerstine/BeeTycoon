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

    public void DisplayPopup(Vector3 position, int amount, float duration, Hive h, Vector3 tPos)
    {
        position.z -= 1.5f;
        position.x -= 1f;
        Vector3 worldPos = position;
        TemplateContainer activePopup = elementToSpawn.Instantiate();
        position = Camera.main.WorldToScreenPoint(position);
        activePopup.style.top = Screen.height - position.y;
        activePopup.style.left = position.x;

        VisualElement hex = activePopup.Q<VisualElement>("Hex");
        VisualElement icon = activePopup.Q<VisualElement>("Icon");

        hex.style.width = 96;
        hex.style.height = 96;
        icon.style.width = 64;
        icon.style.height = 64;
        activePopup.style.position = Position.Absolute;

        document.rootVisualElement.Q().Add(activePopup);

        AudioSource modifierSource = GameObject.Find("UnlockTracker").GetComponent<AudioSource>();
        modifierSource.pitch = 2f;
        modifierSource.PlayOneShot(modifierSound);

        StartCoroutine(AdvancePopup(activePopup, worldPos, duration, amount, h, tPos));
    }

    private IEnumerator AdvancePopup(TemplateContainer popup, Vector3 worldPos, float duration, float amount, Hive h, Vector3 tPos)
    {
        float timeLapsed = 0.0f;
        //float adjustAmount;
        float fadeAmonunt;
        //float yAdjust = 0f;
        VisualElement hex = popup.Q<VisualElement>("Hex");
        VisualElement icon = popup.Q<VisualElement>("Icon");

        yield return new WaitForFixedUpdate();
        while (timeLapsed < duration * 1.5f)
        {
            Vector3 position = worldPos;
            position = Camera.main.WorldToScreenPoint(position);
            popup.style.top = Screen.height - position.y;// - yAdjust;
            popup.style.left = position.x + 18;
            //adjustAmount = 0.75f / duration; //1.5f is just an arbitrary modifier
            //yAdjust += adjustAmount;
            fadeAmonunt = 0.05f / duration;

            if (timeLapsed > duration * 0.75f)
            {
                icon.style.unityBackgroundImageTintColor = icon.resolvedStyle.unityBackgroundImageTintColor - new Color(0, 0, 0, fadeAmonunt);
                hex.style.unityBackgroundImageTintColor = hex.resolvedStyle.unityBackgroundImageTintColor - new Color(0, 0, 0, fadeAmonunt);
            }

            yield return new WaitForSeconds(Time.deltaTime);
            timeLapsed += Time.deltaTime;
        }

        if (document.rootVisualElement.Q().Contains(popup))
            document.rootVisualElement.Q().Remove(popup);

        StartCoroutine(AnimateNectar(amount, h, tPos, duration));
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

        StartCoroutine(WaitDeltaTime(0.75f * duration));
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
            StartCoroutine(ToPoint(glob, top, left, 0.5f, true, h));
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
