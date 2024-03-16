using Companions.Systems;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private Transform body;
    [SerializeField] private float sensitivity = 2.0f;
    [SerializeField] private float xAngleLimit = 85.0f;
    [SerializeField] private float yAngleLimit = 85.0f;
    
    private Vector2 lookInput;

    private float pitch;
    private float yaw;
    
    private void OnEnable()
    {
        GameInputSystem.onLook += OnLook;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnDisable()
    {
        GameInputSystem.onLook -= OnLook;
    }

    private void OnLook(Vector2 value)
    {
        lookInput = value;
    }

    private void Update()
    {
        float mouseX = lookInput.x * sensitivity * Time.deltaTime;
        float mouseY = lookInput.y * sensitivity * Time.deltaTime;

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -90f, 47f);
        transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        body.Rotate(Vector3.up * mouseX);

        lookInput = Vector3.zero;
    }
}
