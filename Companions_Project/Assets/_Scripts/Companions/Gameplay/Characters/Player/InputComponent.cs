using System.Collections;
using Companions.Systems;
using UnityEngine;

public class InputComponent : MonoBehaviour
{
    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool SprintInput { get; private set; }
    public bool JumpTriggered { get; private set; }

    private void OnEnable()
    {
        GameInputSystem.onMove += OnMove;
        GameInputSystem.onLook += OnLook;
        GameInputSystem.onSprint += OnSprint;
        GameInputSystem.onJump += OnJump;
    }

    private void OnDisable()
    {
        GameInputSystem.onMove -= OnMove;
        GameInputSystem.onLook -= OnLook;
        GameInputSystem.onSprint -= OnSprint;
        GameInputSystem.onJump -= OnJump;
    }

    private void OnMove(Vector2 value)
    {
        MoveInput = value;
    }

    private void OnLook(Vector2 value)
    {
        LookInput = value;
    }

    private void OnSprint(bool active)
    {
        SprintInput = active;
    }

    private void OnJump()
    {
        JumpTriggered = true;
    }

    private void LateUpdate()
    {
        JumpTriggered = false;
    }
}
