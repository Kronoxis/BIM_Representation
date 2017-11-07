using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class VRInput : MonoBehaviour
{
    #region Object Toggle
    public Canvas ObjectTogglePopupCanvas;
    public GameObject ObjectToggleMenu;
    public Text ObjectName;
    public Button ShowButton;
    public Button FadeButton;
    public Button HideButton;

    private GameObject _selected;
    private float _maxDistance = 10.0f;
    #endregion

    #region Layer Manager
    public Canvas LayerManagerPopupCanvas;
    public GameObject LayerManagerMenu;
    public Button ShowAllButton;
    public Button FadeAllButton;
    public Button HideAllButton;
    public Button ModeButton;

    private static ToggleObject.ToggleMode _mode = ToggleObject.ToggleMode.Hide;

    public static ToggleObject.ToggleMode Mode
    {
        get { return _mode; }
        set { _mode = value; }
    }
    #endregion

    #region Teleport
    public GameObject TeleportReticlePrefab;
    public Transform CameraRigTransform;
    public Transform HeadTransform;
    public Vector3 TeleportReticleOffset;
    public LayerMask TeleportMask;

    private GameObject _reticle;
    private bool _shouldTeleport;
    private Vector3 _hitPoint;
    #endregion

    private SteamVR_TrackedController _trackedController;

    private SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input((int)_trackedController.controllerIndex); }
    }

    private void Awake()
    {
        _trackedController = GetComponent<SteamVR_TrackedController>();
    }

    private void Start()
    {
        #region Object Toggle
        ObjectToggleMenu.SetActive(false);
        ShowButton.onClick.AddListener(Show);
        FadeButton.onClick.AddListener(Fade);
        HideButton.onClick.AddListener(Hide);
        #endregion

        #region Layer Manager
        LayerManagerMenu.SetActive(false);
        ShowAllButton.onClick.AddListener(ShowAll);
        FadeAllButton.onClick.AddListener(FadeAll);
        HideAllButton.onClick.AddListener(HideAll);
        ModeButton.onClick.AddListener(SwitchMode);
        #endregion

        #region Teleport
        _reticle = Instantiate(TeleportReticlePrefab);
        #endregion
    }

    private void Update()
    {
        #region Object Toggle
        if (Controller.GetHairTriggerUp())
        {
            // Hide menu when clicking anywhere
            if (ObjectToggleMenu.activeInHierarchy)
            {
                _selected = null;
                Popup(ObjectTogglePopupCanvas, ObjectToggleMenu, false);
            }
            // Select object and open menu
            else
            {
                RaycastHit hit;
                if (Physics.Raycast(_trackedController.transform.position, _trackedController.transform.forward, out hit, _maxDistance))
                {
                    // Only objects with MeshRenderer are valid
                    if (hit.collider.gameObject.GetComponent<MeshRenderer>())
                    {
                        _selected = hit.collider.gameObject;
                        var tags = hit.collider.GetComponent<MeshTags>();
                        ObjectName.text = tags.Name + " (" + tags.IfcType.Substring(3) + ")";

                        var dist = 1.0f;
                        RaycastHit hitCam;
                        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hitCam, 2.0f))
                            dist = hitCam.distance;
                        Popup(ObjectTogglePopupCanvas, ObjectToggleMenu, true, dist);
                        Popup(LayerManagerPopupCanvas, LayerManagerMenu, false);
                    }
                }
            }
        }
        #endregion

        #region Layer Manager
        if (Controller.GetPressUp(SteamVR_Controller.ButtonMask.ApplicationMenu))
        {
            var dist = 1.0f;
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, 2))
                dist = hit.distance;
            Popup(LayerManagerPopupCanvas, LayerManagerMenu, !LayerManagerMenu.activeInHierarchy, dist);
            Popup(ObjectTogglePopupCanvas, ObjectToggleMenu, false);
        }
        #endregion

        #region Teleport
        if (Controller.GetPress(SteamVR_Controller.ButtonMask.Touchpad))
        {
            RaycastHit hit;
            if (Physics.Raycast(_trackedController.transform.position, transform.forward, out hit, 100))
            {
                _hitPoint = hit.point;
                if (((1 << hit.collider.gameObject.layer) & TeleportMask) != 0)
                {
                    _reticle.SetActive(true);
                    _reticle.transform.position = _hitPoint + TeleportReticleOffset;
                    _shouldTeleport = true;
                }
                else
                {
                    _reticle.SetActive(false);
                    _shouldTeleport = false;
                }
            }
        }
        else
        {
            _reticle.SetActive(false);
        }

        if (Controller.GetPressUp(SteamVR_Controller.ButtonMask.Touchpad) && _shouldTeleport)
        {
            Teleport();
            Popup(LayerManagerPopupCanvas, LayerManagerMenu, false);
        }
        #endregion

        #region Interact
        if (Controller.GetPressUp(SteamVR_Controller.ButtonMask.Touchpad))
        {
            RaycastHit hit;
            if (Physics.Raycast(_trackedController.transform.position, transform.forward, out hit, 5, -1, QueryTriggerInteraction.Collide))
            {
                var interaction = hit.collider.gameObject.GetComponent<Interaction>();
                if (!interaction)
                {
                    if (hit.collider.transform.parent)
                        interaction = hit.collider.transform.parent.gameObject.GetComponent<Interaction>();
                }
                if (interaction)
                {
                    interaction.Trigger();
                }
            }
        }
        #endregion
    }

    private void Popup(Canvas canvas, GameObject menu, bool show, float maxDist = 1.0f)
    {
        if (show)
        {
            var tr = Camera.main.transform;
            canvas.GetComponent<RectTransform>().position = tr.position + tr.forward * (Mathf.Min(1.0f, maxDist) - 0.1f);
            canvas.GetComponent<RectTransform>().rotation =
                Quaternion.Euler(tr.rotation.eulerAngles.x, tr.rotation.eulerAngles.y, 0);
        }
        menu.SetActive(show);
    }

    #region Object Toggle
    private void Show()
    {
        StopAllCoroutines();
        StartCoroutine(Toggle(ToggleObject.ToggleMode.Show, _selected));
    }

    private void Fade()
    {
        StopAllCoroutines();
        StartCoroutine(Toggle(ToggleObject.ToggleMode.Fade, _selected));
    }

    private void Hide()
    {
        StopAllCoroutines();
        StartCoroutine(Toggle(ToggleObject.ToggleMode.Hide, _selected));
    }

    private IEnumerator Toggle(ToggleObject.ToggleMode mode, GameObject obj)
    {
        if (obj)
        {
            // Get Script
            var script = GetComponent<ToggleObject>();

            // Toggle Backface object and children
            if (obj.transform.parent)
                obj.transform.parent.GetComponentsInChildren<Transform>(true)
                    .Where(x => x.name == obj.name && x.GetComponent<MeshRenderer>()).ToList()
                    .ForEach(x =>
                    {
                        x.GetComponentsInChildren<Transform>(true).ToList()
                            .ForEach(y => script.Toggle(y.GetComponent<MeshRenderer>(), mode));
                    });
            yield return 0;
        }
    }
    #endregion

    #region Layer Manager
    private void ShowAll()
    {
        StopAllCoroutines();
        StartCoroutine(ToggleAll(ToggleObject.ToggleMode.Show));
    }

    private void FadeAll()
    {
        StopAllCoroutines();
        StartCoroutine(ToggleAll(ToggleObject.ToggleMode.Fade));
    }

    private void HideAll()
    {
        StopAllCoroutines();
        StartCoroutine(ToggleAll(ToggleObject.ToggleMode.Hide));
    }

    private IEnumerator ToggleAll(ToggleObject.ToggleMode mode)
    {
        int count = 0;
        foreach (var obj in LayerLibrary.GetLayers())
        {
            foreach (var mf in obj.GetComponentsInChildren<MeshFilter>(true))
            {
                ++count;
                if (count % 30 == 0) yield return StartCoroutine(Toggle(mode, mf.gameObject));
                else StartCoroutine(Toggle(mode, mf.gameObject));
            }

            // Edit button colors
            var panel = LayerManagerMenu.transform.Find("Panel");
            for (int i = 0; i < panel.childCount; ++i)
            {
                switch (mode)
                {
                    case ToggleObject.ToggleMode.Show:
                        panel.GetChild(i).GetComponent<Button>().colors = ShowAllButton.colors;
                        break;
                    case ToggleObject.ToggleMode.Fade:
                        panel.GetChild(i).GetComponent<Button>().colors = FadeAllButton.colors;
                        break;
                    case ToggleObject.ToggleMode.Hide:
                        panel.GetChild(i).GetComponent<Button>().colors = HideAllButton.colors;
                        break;
                }
            }
            yield return 0;
            count = 0;
        }
    }

    private void SwitchMode()
    {
        _mode = (ToggleObject.ToggleMode)(((int)_mode + 1) % 3);
        ModeButton.GetComponentInChildren<Text>().text = "Mode: " + _mode;
        switch (_mode)
        {
            case ToggleObject.ToggleMode.Show:
                ModeButton.colors = ShowAllButton.colors;
                break;
            case ToggleObject.ToggleMode.Fade:
                ModeButton.colors = FadeAllButton.colors;
                break;
            case ToggleObject.ToggleMode.Hide:
                ModeButton.colors = HideAllButton.colors;
                break;
        }
    }
    #endregion

    #region Teleport
    private void Teleport()
    {
        _shouldTeleport = false;
        _reticle.SetActive(false);
        Vector3 diff = CameraRigTransform.position - HeadTransform.position;
        diff.y = 0;
        CameraRigTransform.position = _hitPoint + diff;
    }
    #endregion
}
