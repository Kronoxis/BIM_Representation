using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViveControllerGrabObject : MonoBehaviour
{
    private SteamVR_TrackedController _trackedController;
    private GameObject _collidingObject;
    private GameObject _objectInHand;

    private SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input((int)_trackedController.controllerIndex); }
    }

    private void Awake()
    {
        _trackedController = GetComponent<SteamVR_TrackedController>();
    }

    private void Update()
    {
        if (Controller.GetHairTriggerDown())
        {
            if (_collidingObject)
            {
                GrabObject();
            }
        }

        if (Controller.GetHairTrigger())
        {
            if (_objectInHand)
            {
                _objectInHand.GetComponent<Door>().OpenToPoint(_trackedController.transform.position);
            }
        }

        if (Controller.GetHairTriggerUp())
        {
            if (_objectInHand)
            {
                ReleaseObject();
            }
        }
    }

    // Colliding Object
    private void SetCollidingObject(Collider col)
    {
        if (_collidingObject || !col.GetComponent<Rigidbody>())
        {
            // Check parent
            if (!col.transform.parent.GetComponent<Rigidbody>())
                return;
            _collidingObject = col.transform.parent.gameObject;
            return;
        }
        _collidingObject = col.gameObject;
    }

    public void OnTriggerEnter(Collider other)
    {
        SetCollidingObject(other);
    }

    public void OnTriggerStay(Collider other)
    {
        SetCollidingObject(other);
    }

    public void OnTriggerExit(Collider other)
    {
        _collidingObject = null;
    }

    // Grabbing Object
    private void GrabObject()
    {
        _objectInHand = _collidingObject;
        _collidingObject = null;
    }

    // Releasing Object
    private void ReleaseObject()
    {
        _objectInHand.GetComponent<Rigidbody>().velocity = Controller.velocity;
        _objectInHand.GetComponent<Rigidbody>().angularVelocity = Controller.angularVelocity;
    }
}
