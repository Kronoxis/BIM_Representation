using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViveControllerGrabObject : MonoBehaviour
{
    private SteamVR_TrackedController _trackedObj;
    private GameObject _collidingObject;
    private GameObject _objectInHand;

    private SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input((int) _trackedObj.controllerIndex); }
    }

    private void Awake()
    {
        _trackedObj = GetComponent<SteamVR_TrackedController>();
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
        if (_collidingObject || !col.GetComponent<Rigidbody>()) return;
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

        var joint = AddFixedJoint();
        joint.connectedBody = _objectInHand.GetComponent<Rigidbody>();
    }

    private FixedJoint AddFixedJoint()
    {
        FixedJoint joint = gameObject.AddComponent<FixedJoint>();
        joint.breakForce = 20000;
        joint.breakTorque = 20000;
        return joint;
    }

    // Releasing Object
    private void ReleaseObject()
    {
        var joint = GetComponent<FixedJoint>();
        if (joint)
        {
            joint.connectedBody = null;
            Destroy(joint);

            _objectInHand.GetComponent<Rigidbody>().velocity = Controller.velocity;
            _objectInHand.GetComponent<Rigidbody>().angularVelocity = Controller.angularVelocity;
        }
        _objectInHand = null;
    }
}
