using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePlayer : MonoBehaviour
{
    public float MoveSpeed = 3.0f;
    public float SprintSpeed = 6.0f;
    public float JumpSpeed = 8.0f;
    public float Gravity = 20.0f;

    public GameObject Interaction;

    private CharacterController _controller;
    private Camera _camera;
    private Transform _cameraTransform;

    private Vector3 _moveDir = Vector3.zero;
    private bool _isFly = false;

    private void Start()
    {
        _controller = GetComponent<CharacterController>();
        _camera = GetComponentInChildren<Camera>();
        _cameraTransform = _camera.GetComponent<Transform>();
    }

    private void Update()
    {
        // Move
        var speed = Input.GetKey(KeyCode.LeftShift) ? SprintSpeed : MoveSpeed;
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

        // Rotate
        var yaw = Input.GetAxis("Mouse X");
        var pitch = -Input.GetAxis("Mouse Y");
        transform.Rotate(new Vector3(0, yaw, 0));
        _cameraTransform.Rotate(new Vector3(pitch, 0, 0));

        // Interact
        var interact = Input.GetButtonUp("Fire1");
        if (interact)
        {
            var interaction = Instantiate(Interaction, _cameraTransform.position, Quaternion.identity);
            interaction.GetComponent<Rigidbody>().velocity = _cameraTransform.forward * 15;
            Destroy(interaction, 2);
        }
    }
}
