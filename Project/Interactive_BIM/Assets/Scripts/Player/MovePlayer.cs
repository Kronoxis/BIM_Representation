using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePlayer : MonoBehaviour
{
    public float MoveSpeed = 3.0f;
    public float SprintSpeed = 6.0f;
    public float JumpSpeed = 8.0f;
    public float Gravity = 20.0f;
    public float Height = 1.75f;
    public float CrouchHeight = 0.5f;

    private CharacterController _controller;
    private Camera _camera;
    private Transform _cameraTransform;

    private Vector3 _moveDir = Vector3.zero;
    private bool _isFly = false;
    private Coroutine _crouchCoroutine;
    private bool _isCrouch = false;

    private void Start()
    {
        _controller = GetComponent<CharacterController>();
        _camera = GetComponentInChildren<Camera>();
        _cameraTransform = _camera.GetComponent<Transform>();

        _cameraTransform.localPosition = new Vector3(_cameraTransform.localPosition.x, Height - 0.1f, _cameraTransform.localPosition.z);
        _controller.height = Height;
        _controller.center = new Vector3(_controller.center.x, Height / 2, _controller.center.z);
    }

    private void Update()
    {
        Move();
        Rotate();
        Crouch();
        Interact();
    }

    private void Move()
    {
        var speed = (Input.GetKey(KeyCode.LeftShift) ? SprintSpeed : MoveSpeed) * (_isCrouch ? 0.5f : 1);
        var horizontal = Input.GetAxis("Horizontal") * speed;
        var vertical = Input.GetAxis("Vertical") * speed;
        var fly = Input.GetAxis("Fly") * speed;

        if (Mathf.Abs(fly) > 0.05f || _isFly)
        {
            _isFly = true;
            _moveDir = new Vector3(horizontal, fly, vertical);
            _moveDir = transform.TransformDirection(_moveDir);
            if (Input.GetButtonUp("Jump")) _isFly = false;
        }
        else
        {
            if (_controller.isGrounded)
            {
                _moveDir = new Vector3(horizontal, 0, vertical);
                _moveDir = transform.TransformDirection(_moveDir);
                if (Input.GetButtonUp("Jump"))
                    _moveDir.y = JumpSpeed;
            }
            else
            {
                _moveDir = new Vector3(horizontal, _moveDir.y, vertical);
                _moveDir = transform.TransformDirection(_moveDir);
                _moveDir.y -= Gravity * Time.deltaTime;
            }
        }
        _controller.Move(_moveDir * Time.deltaTime);
    }

    private void Rotate()
    {
        var yaw = Input.GetAxis("Mouse X");
        var pitch = -Input.GetAxis("Mouse Y");
        transform.Rotate(new Vector3(0, yaw, 0));
        _cameraTransform.Rotate(new Vector3(pitch, 0, 0));
    }

    private void Crouch()
    {
        var crouch = Input.GetButtonUp("Crouch");
        if (crouch)
        {
            if (_crouchCoroutine != null) StopCoroutine(_crouchCoroutine);
            _isCrouch = !_isCrouch;
            _crouchCoroutine = StartCoroutine(MoveView(_isCrouch ? CrouchHeight : Height));
        }
    }

    private void Interact()
    {
        var interact = Input.GetButtonUp("Fire1");
        if (interact)
        {
            RaycastHit hit;
            if (Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, out hit, 5, -1,
                QueryTriggerInteraction.Collide))
            {
                var interaction = hit.collider.gameObject.GetComponent<Interaction>();
                if (!interaction)
                {
                    if (hit.collider.transform.parent)
                        interaction = hit.collider.transform.parent.gameObject.GetComponent<Interaction>();
                }
                if (interaction) interaction.Trigger();
            }
        }
    }

    IEnumerator MoveView(float targetHeight)
    {
        var startY = _cameraTransform.localPosition.y;
        var endY = targetHeight - 0.1f;
        var startHeight = _controller.height;
        var endHeight = targetHeight;
        var startCenterY = _controller.center.y;
        var endCenterY = targetHeight / 2;
        var time = 0f;
        while (time < 1.0f)
        {
            time += Time.deltaTime * 4;
            var newY = Mathf.Lerp(startY, endY, time);
            var newHeight = Mathf.Lerp(startHeight, endHeight, time);
            var newCenterY = Mathf.Lerp(startCenterY, endCenterY, time);
            _cameraTransform.localPosition = new Vector3(_cameraTransform.localPosition.x, newY, _cameraTransform.localPosition.z);
            _controller.height = newHeight;
            _controller.center = new Vector3(_controller.center.x, newCenterY, _controller.center.z);
            yield return 0;
        }
    }
}
