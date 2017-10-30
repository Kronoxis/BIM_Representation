using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class VRUISlider : MonoBehaviour
{
    private BoxCollider _boxCollider;
    private RectTransform _rectTransform;

    private void Start()
    {
        ValidateCollider();
    }

    private void ValidateCollider()
    {
        _rectTransform = GetComponent<RectTransform>();

        _boxCollider = GetComponent<BoxCollider>();
        if (_boxCollider == null)
        {
            _boxCollider = gameObject.AddComponent<BoxCollider>();
        }

        _boxCollider.size = _rectTransform.sizeDelta;
    }
}