using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LayerToggle : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    public GameObject Layer;
    public Button ModeButton;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(ToggleLayer);
    }

    public void ToggleLayer()
    {
        Layer.GetComponentsInChildren<MeshRenderer>(true).ToList()
            .ForEach(x =>
            {
                GetComponent<ToggleObject>().Toggle(x, VRInput.Mode);
            });
        GetComponent<Button>().colors = ModeButton.colors;
        GetComponentInChildren<Text>().color = GetComponent<Button>().colors.highlightedColor;
    }

    public void OnSelect(BaseEventData e)
    {
        GetComponentInChildren<Text>().color = GetComponent<Button>().colors.highlightedColor;
    }

    public void OnDeselect(BaseEventData e)
    {
        GetComponentInChildren<Text>().color = Color.black;
    }
}
