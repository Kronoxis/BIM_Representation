using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(SteamVR_LaserPointer))]
public class VRUIInput : MonoBehaviour
{
    private SteamVR_LaserPointer _laserPointer;
    private SteamVR_TrackedController _trackedController;

    private void OnEnable()
    {
        _laserPointer = GetComponent<SteamVR_LaserPointer>();
        _laserPointer.PointerIn -= HandlePointerIn;
        _laserPointer.PointerIn += HandlePointerIn;
        _laserPointer.PointerOut -= HandlePointerOut;
        _laserPointer.PointerOut += HandlePointerOut;

        _trackedController = GetComponent<SteamVR_TrackedController>();
        if (_trackedController == null)
        {
            _trackedController = GetComponentInParent<SteamVR_TrackedController>();
        }
        _trackedController.PadClicked -= HandleTriggerClicked;
        _trackedController.PadClicked += HandleTriggerClicked;
    }

    private void HandleTriggerClicked(object sender, ClickedEventArgs e)
    {
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            ExecuteEvents.Execute(EventSystem.current.currentSelectedGameObject, new PointerEventData(EventSystem.current), ExecuteEvents.submitHandler);
        }
    }

    private void HandlePointerIn(object sender, PointerEventArgs e)
    {
        var selectable = e.target.GetComponent<Selectable>();
        if (selectable != null)
        {
            selectable.Select();
        }
    }

    private void HandlePointerOut(object sender, PointerEventArgs e)
    {

        var selectable = e.target.GetComponent<Selectable>();
        if (selectable != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}