using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ResourcePopup : MonoBehaviour
{
    [SerializeField]
    private UIDocument document;

    [SerializeField]
    private VisualTreeAsset elementToSpawn;

    public void DisplayPopup(Vector3 position, int amount, float duration)
    {
        position.z -= 1.5f;
        position.x -= 1f;
        Vector3 worldPos = position;
        TemplateContainer activePopup = elementToSpawn.Instantiate();
        position = Camera.main.WorldToScreenPoint(position);
        activePopup.style.top = Screen.height - position.y;
        activePopup.style.left = position.x;

        VisualElement icon = activePopup.Q<VisualElement>("Icon");
        Label amountLabel = activePopup.Q<Label>("Amount");
        Label plus = activePopup.Q<Label>("plus");
        amountLabel.text = amount.ToString();
        //icon.style.width = amount * 2.4f * (20 / amount); //Attempts at scaling by amount perfectly
        //icon.style.height = amount * 2.4f * (20 / amount);
        //icon.style.marginRight = -8 + (amount / 10);
        //plus.style.fontSize = amount * 1.8f;
        //amountLabel.style.fontSize = amount * 1.8f;

        if (amount >= 10 && amount < 20)
        {
            icon.style.width = 36;
            icon.style.height = 36;
            icon.style.marginRight = -8;
            plus.style.fontSize = 24;
            amountLabel.style.fontSize = 24;
        }
        else if (amount >= 20 && amount < 30)
        {
            icon.style.width = 48;
            icon.style.height = 48;
            icon.style.marginRight = -10;
            plus.style.fontSize = 32;
            amountLabel.style.fontSize = 32;
        }
        else if (amount >= 30)
        {
            icon.style.width = 64;
            icon.style.height = 64;
            icon.style.marginRight = -12;
            plus.style.fontSize = 36;
            amountLabel.style.fontSize = 36;
        }
        activePopup.style.position = Position.Absolute;
        

        document.rootVisualElement.Q("Base").Add(activePopup);
        StartCoroutine(AdvancePopup(activePopup, worldPos, duration));
    }

    private IEnumerator AdvancePopup(TemplateContainer popup, Vector3 worldPos, float duration)
    {
        float adjustAmount = 0.5f;
        float fadeAmonunt = 0.01f;
        int cycles = 0;
        float yAdjust = 0f;
        VisualElement icon = popup.Q<VisualElement>("Icon");
        Label amount = popup.Q<Label>("Amount");
        Label plus = popup.Q<Label>("plus");

        yield return new WaitForFixedUpdate();
        while (icon.resolvedStyle.unityBackgroundImageTintColor.a > 0)
        {
            Vector3 position = worldPos;
            position = Camera.main.WorldToScreenPoint(position);
            popup.style.top = Screen.height - position.y - yAdjust;
            popup.style.left = position.x;
            adjustAmount = (0.1f - duration) * 10 * 1.5f; //1.5f is just an arbitrary modifier
            yAdjust += adjustAmount;
            fadeAmonunt = (0.06f - duration);

            if (cycles > 50)
            {
                icon.style.unityBackgroundImageTintColor = icon.resolvedStyle.unityBackgroundImageTintColor - new Color(0, 0, 0, fadeAmonunt);

                amount.style.color = amount.resolvedStyle.color - new Color(0, 0, 0, fadeAmonunt);
                amount.style.unityTextOutlineColor = amount.resolvedStyle.color - new Color(1, 1, 1, fadeAmonunt);

                plus.style.color = plus.resolvedStyle.color - new Color(0, 0, 0, fadeAmonunt);
                plus.style.unityTextOutlineColor = plus.resolvedStyle.color - new Color(1, 1, 1, fadeAmonunt);
            }

            yield return new WaitForSeconds(0.005f);
            cycles++;
        }

        document.rootVisualElement.Q("Base").Remove(popup);
    }
}
