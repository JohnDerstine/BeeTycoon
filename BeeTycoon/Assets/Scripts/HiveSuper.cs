using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiveSuper : MonoBehaviour
{
    void OnMouseDown()
    {
        if (gameObject.transform.parent == null)
            return;

        gameObject.transform.parent.TryGetComponent<Hive>(out Hive h);
        if (h != null)
            h.childOnMouseDown();
    }
}
