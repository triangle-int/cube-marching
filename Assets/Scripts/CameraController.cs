using System;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float moveSpeed;

    private Transform _transform;

    private void Start()
    {
        _transform = transform;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Mouse2))
        {
            var eulerAngles = _transform.eulerAngles;
            eulerAngles.x -= Input.GetAxisRaw("Mouse Y") * rotationSpeed * Time.deltaTime;
            eulerAngles.y += Input.GetAxisRaw("Mouse X") * rotationSpeed * Time.deltaTime;
            _transform.rotation = Quaternion.Euler(eulerAngles);
            
            var xMove = Input.GetAxisRaw("Horizontal") * moveSpeed * Time.deltaTime;
            var yMove = (Convert.ToInt32(Input.GetKey(KeyCode.E)) - Convert.ToInt32(Input.GetKey(KeyCode.Q))) * moveSpeed * Time.deltaTime;
            var zMove = Input.GetAxisRaw("Vertical") * moveSpeed * Time.deltaTime;
            _transform.Translate(xMove, yMove, zMove);
        }
    }
}
