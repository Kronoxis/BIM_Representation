using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LayerToggle : MonoBehaviour
{
    public GameObject Layer;

    private void Start()
    {
        GetComponent<Toggle>().onValueChanged.AddListener(ToggleLayer);
    }

    public void ToggleLayer(bool enable)
    {
        Layer.SetActive(enable);
    }
}
