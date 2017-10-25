using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Toggler : MonoBehaviour
{
    public void ToggleObject(GameObject obj)
    {
        obj.SetActive(!obj.activeInHierarchy);
    }

    public void EnableAll(GameObject ContentParent)
    {
        for (int i = 0; i < ContentParent.transform.childCount; ++i)
        {
            var child = ContentParent.transform.GetChild(i);
            if (child.name == "LayerTemplate") continue;
            child.GetComponent<Toggle>().isOn = true;
        }
    }
}
