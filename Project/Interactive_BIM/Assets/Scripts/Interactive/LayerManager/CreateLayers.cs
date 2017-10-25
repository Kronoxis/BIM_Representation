using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreateLayers : MonoBehaviour
{
    public GameObject Meshes;
    public GameObject ContentParent;
    public GameObject LayerTemplate;

    private int _y = -12;
    private int _deltaY = 30; 

    private void Start()
    {
        for (int i = 0; i < Meshes.transform.childCount; ++i)
        {
            var model = Meshes.transform.GetChild(i);
            for (int j = 0; j < model.childCount; ++j)
            {
                // Create Toggle
                var toggle = Instantiate(LayerTemplate, ContentParent.transform);

                // Set text to layer name
                var layer = model.GetChild(j);
                toggle.name = layer.name;
                toggle.transform.Find("Label").GetComponent<Text>().text = layer.name.Substring(3);

                // Set Toggle Position
                var pos = toggle.GetComponent<RectTransform>().localPosition;
                toggle.GetComponent<RectTransform>().localPosition = new Vector3(pos.x, _y, pos.z);
                
                // Show Toggle
                toggle.SetActive(true);

                // Increase Content height
                var size = ContentParent.GetComponent<RectTransform>().sizeDelta;
                ContentParent.GetComponent<RectTransform>().sizeDelta = new Vector2(size.x, size.y + _deltaY);

                // Layer Toggle script
                toggle.GetComponent<LayerToggle>().Layer = layer.gameObject;

                // Move y down
                _y -= _deltaY;
            }
        }
    }
}
