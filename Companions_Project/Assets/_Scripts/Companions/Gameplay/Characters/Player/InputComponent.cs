using Companions.Systems;
using UnityEngine;

public class InputComponent : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    
    private void OnEnable()
    {
        GameInputSystem.onMove += OnMove;
        GameInputSystem.onLook += OnLook;
    }
    
    private void OnDisable()
    {
        GameInputSystem.onMove -= OnMove;
        GameInputSystem.onLook -= OnLook;
    }

    private void OnMove(Vector2 value)
    {
        MoveInput = value;
    }
    
    private void OnLook(Vector2 value)
    {
        LookInput = value;
    }
}
