using UnityEngine;

/// <summary>
/// Вращение излучателя мышью/клавишами.
/// X/Z — поворот по двум осям, R — вернуть исходное положение.
/// </summary>
public class EmitterRotator : MonoBehaviour
{
    public float rotationSpeed = 80f;
    public bool allowMouse = true;

    private Vector3 startEuler;

    private void Start()
    {
        startEuler = transform.eulerAngles;
    }

    private void Update()
    {
        float yaw = Input.GetAxis("Horizontal");
        float pitch = Input.GetAxis("Vertical");

        transform.Rotate(Vector3.up, yaw * rotationSpeed * Time.deltaTime, Space.World);
        transform.Rotate(Vector3.right, -pitch * rotationSpeed * Time.deltaTime, Space.Self);

        if (Input.GetKeyDown(KeyCode.R))
            transform.eulerAngles = startEuler;

        if (allowMouse && Input.GetMouseButton(0))
        {
            float dx = Input.GetAxis("Mouse X");
            float dy = Input.GetAxis("Mouse Y");

            transform.Rotate(Vector3.up, dx * rotationSpeed * 0.15f, Space.World);
            transform.Rotate(Vector3.right, -dy * rotationSpeed * 0.15f, Space.Self);
        }
    }
}
