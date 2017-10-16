using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{

    private bool _open = false;
    private bool _isTriggered = false;
    private GameObject _pivot;
    private Coroutine _openingCoroutine;
    private float _openAngle; 

    void Update()
    {
        if (_isTriggered)
        {
            _open = !_open;
            if (_openingCoroutine != null) StopCoroutine(_openingCoroutine);
            _openingCoroutine = StartCoroutine(RotateDoor(_open ? _openAngle : 0));
            _isTriggered = false;
        }
    }

    IEnumerator RotateDoor(float targetAngle)
    {
        var start = _pivot.transform.rotation.eulerAngles.y;
        var target = start > 180 ? targetAngle + 360 : targetAngle;
        var time = 0f;
        while (time < 1.0f)
        {
            time += Time.deltaTime;
            var angle = Mathf.Lerp(start, target, time);
            _pivot.transform.rotation = Quaternion.Euler(0, angle, 0);
            yield return 0;
        }
    }

    public void Set(GameObject pivot, float startAngle, float sweepAngle)
    {
        _pivot = pivot;
        if (startAngle > 80 && startAngle < 100) _openAngle = sweepAngle;
        else _openAngle = -sweepAngle;
    }

    private void OnTriggerEnter(Collider other)
    {
        _isTriggered = true;
        Destroy(other.gameObject);
    }
}
