using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class TestToggleObject : MonoBehaviour
{
    public Canvas PopupCanvas;
    public GameObject Menu;
    public Text ObjectName;
    public Button ShowButton;
    public Button FadeButton;
    public Button HideButton;

    private GameObject _selected;
    private float _maxDistance = 10.0f;

    private void Start()
    {
        Menu.SetActive(false);
        ShowButton.onClick.AddListener(Show);
        FadeButton.onClick.AddListener(Fade);
        HideButton.onClick.AddListener(Hide);
    }

    private void Update()
    {
        if (Input.GetButtonUp("Fire1"))
        {
            // Hide menu when clicking anywhere
            if (Menu.activeInHierarchy)
            {
                _selected = null;
                Popup(false);
            }
            // Select object and open menu
            else
            {
                var tr = Camera.main.transform;
                RaycastHit hit;
                if (Physics.Raycast(tr.position, tr.forward, out hit, _maxDistance))
                {
                    // Only objects with MeshRenderer are valid
                    if (hit.collider.gameObject.GetComponent<MeshRenderer>())
                    {
                        _selected = hit.collider.gameObject;
                        Popup(true, hit);
                    }
                }
            }
        }
    }

    private void Popup(bool show, RaycastHit hit = default(RaycastHit))
    {
        if (show)
        {
            ObjectName.text = hit.collider.GetComponent<MeshTags>().Name;

            var tr = Camera.main.transform;
            var posBegin = tr.position;
            var posEnd = hit.point;
            var pos = (posBegin + posEnd) / 2;
            //var pos = hit.point - Vector3.Scale(tr.forward, new Vector3(0.1f, 0.1f, 0.1f));
            PopupCanvas.GetComponent<RectTransform>().SetPositionAndRotation(pos, tr.rotation);

            var scale = (hit.distance / 2) / _maxDistance * 3.0f;
            Menu.GetComponent<RectTransform>().localScale = new Vector3(scale, scale, scale);
        }
        Menu.SetActive(show);
    }

    private void Show()
    {
        Toggle(ToggleObject.ToggleMode.Show);
    }

    private void Fade()
    {
        Toggle(ToggleObject.ToggleMode.Fade);
    }

    private void Hide()
    {
        Toggle(ToggleObject.ToggleMode.Hide);
    }

    private void Toggle(ToggleObject.ToggleMode mode)
    {
        if (_selected)
        {
            GetComponent<ToggleObject>().Toggle(_selected.GetComponent<MeshRenderer>(), mode);
            _selected.transform.parent.GetComponentsInChildren<Transform>()
                .Where(x => x.name == _selected.name && x != _selected.transform && x.GetComponent<MeshRenderer>()).ToList()
                .ForEach(x => GetComponent<ToggleObject>().Toggle(x.GetComponent<MeshRenderer>(), mode));
        }
    }
}
