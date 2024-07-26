using Companions.Systems;
using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private Transform body;
    [SerializeField] private Transform head;
    [SerializeField]
    [Range(0f, 50f)]
    private float sensitivity = 2.0f;
    [SerializeField]
    [MinMaxSlider(-90f, 90f)]
    private Vector2 verticalClamp = new Vector2(-85f, 40f);
    
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

    private void LateUpdate()
    {
        // Handle camera rotation based on mouse movement
        float mouseX = lookInput.x * sensitivity * Time.deltaTime;
        float mouseY = lookInput.y * sensitivity * Time.deltaTime;

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, verticalClamp.x, verticalClamp.y);
        body.Rotate(Vector3.up * mouseX);
        head.transform.localRotation = Quaternion.Euler(pitch, body.transform.eulerAngles.y, body.transform.eulerAngles.z);

        lookInput = Vector3.zero;
    }
}
