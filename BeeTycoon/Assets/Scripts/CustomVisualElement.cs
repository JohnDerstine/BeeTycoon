using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UIElements;

public class CustomVisualElement : VisualElement
{
    public new class UxmlFactory : UxmlFactory<CustomVisualElement, UxmlTraits> { }

    public float alphaHitTestMinThreshold { get; set; }

    bool DefaultContains(Vector2 local)
    {
        return local.x >= 0 && local.y >= 0 &&
               local.x <= resolvedStyle.width &&
               local.y <= resolvedStyle.height;
    }

    bool SpriteSupportsAlphaHitTest(Texture2D texture)
    {
        return texture != null &&
               !GraphicsFormatUtility.IsCrunchFormat(texture.format) &&
               texture.isReadable;
    }

    public override bool ContainsPoint(Vector2 local)
    {
        alphaHitTestMinThreshold = 0.5f;

        Background b = resolvedStyle.backgroundImage;

        Texture2D tex = b.texture;
        if (tex == null && b.sprite != null)
        {
            tex = b.sprite.texture;
        }

        if (!SpriteSupportsAlphaHitTest(tex))
        {
            Debug.LogWarning("Sprite Doesn't support pixel read");
            alphaHitTestMinThreshold = 0;
            return DefaultContains(local);
        }

        // Convert local coordinates to texture space.
        float x = (local.x / resolvedStyle.width);
        float y = 1 - (local.y / resolvedStyle.height); // Texture UV (0,0) is bottom-left

        try
        {
            var a = tex.GetPixelBilinear(x, y).a;
            return a >= alphaHitTestMinThreshold;
        }
        catch (UnityException e)
        {
            Debug.LogError(
                "Using alphaHitTestThreshold greater than 0 on Image whose sprite texture cannot be read. " +
                e.Message + " Also make sure to disable sprite packing for this sprite.");
            alphaHitTestMinThreshold = 0;
            return DefaultContains(local);
        }
    }
}
