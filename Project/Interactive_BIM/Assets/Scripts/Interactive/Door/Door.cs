using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : Interaction
{
    public bool Open = false;
    public float OpenAngle; 
    private Coroutine _openingCoroutine;

    void Update()
    {
        if (_isTriggered)
        {
            Open = !Open;
            if (_openingCoroutine != null) StopCoroutine(_openingCoroutine);
            _openingCoroutine = StartCoroutine(RotateDoor(Open ? OpenAngle : 0));
            _isTriggered = false;
        }
    }

    IEnumerator RotateDoor(float targetAngle)
    {
        var start = transform.rotation.eulerAngles.y;
        var target = start > 180 ? targetAngle + 360 : targetAngle;
        var time = 0f;
        while (time < 1.0f)
        {
            time += Time.deltaTime;
            var angle = Mathf.Lerp(start, target, time);
            transform.rotation = Quaternion.Euler(0, angle, 0);
            yield return 0;
        }
    }

    public void Set(GameObject pivot, float startAngle, float sweepAngle)
    {
        if ((startAngle > 80 && startAngle < 100) || (startAngle > 260 && startAngle < 280)) OpenAngle = sweepAngle;
        else OpenAngle = -sweepAngle;
    }

    public void OpenToPoint(Vector3 lookAtPoint)
    {
        //var currentDir = -transform.right;
        var targetDir = lookAtPoint - transform.position;
        targetDir.y = 0;
        targetDir.Normalize();
        transform.right = -targetDir;
        var angle = transform.localEulerAngles.y;
        angle = OpenAngle > 0 ? Mathf.Clamp(angle, 0, OpenAngle) : Mathf.Clamp(angle, OpenAngle, 0);
        transform.localEulerAngles = new Vector3(0, angle, 0);
    }
}