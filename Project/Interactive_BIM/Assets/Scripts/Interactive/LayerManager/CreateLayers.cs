using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateLayers : MonoBehaviour
{
    public GameObject ContentParent;
    public GameObject LayerTemplate;

    public int Y = -30;
    public int DeltaY = 30; 

    private void Start()
    {
        foreach (var layerName in LayerLibrary.GetLayerNames())
        {
            // Create Toggle
            var toggle = Instantiate(LayerTemplate, ContentParent.transform);

            // Set text to layer name
            toggle.name = layerName;
            toggle.transform.GetComponentInChildren<Text>().text = layerName.Substring(3);

            // Set Toggle Position
            var pos = toggle.GetComponent<RectTransform>().localPosition;
            toggle.GetComponent<RectTransform>().localPosition = new Vector3(pos.x, Y, pos.z);

            // Show Toggle
            toggle.SetActive(true);

            // Increase Content height
            var size = ContentParent.GetComponent<RectTransform>().sizeDelta;
            ContentParent.GetComponent<RectTransform>().sizeDelta = new Vector2(size.x, size.y + DeltaY);

            // Layer Toggle script
            toggle.GetComponent<LayerToggle>().Layer = LayerLibrary.GetLayer(layerName);

            // Move y down
            Y -= DeltaY;
        }
    }
}
