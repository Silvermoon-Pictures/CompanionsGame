using Companions.Systems;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private float sensitivity = 2.0f;
    [SerializeField] private float maxlookAngle = 85.0f;

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
        yaw += lookInput.x * sensitivity * Time.deltaTime;
        pitch -= lookInput.y * sensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, -maxlookAngle, maxlookAngle);
        
        transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
    }
}
