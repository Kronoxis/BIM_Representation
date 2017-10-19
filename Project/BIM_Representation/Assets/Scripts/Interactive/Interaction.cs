using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interaction : MonoBehaviour {

    protected bool _isTriggered = false;

    public void Trigger()
    {
        _isTriggered = !_isTriggered;
    }
}
