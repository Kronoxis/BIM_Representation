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

}
