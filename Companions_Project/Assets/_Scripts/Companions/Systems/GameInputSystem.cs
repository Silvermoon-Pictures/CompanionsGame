using System;
using Silvermoon.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Companions.Systems
{
    public class GameInputSystem : BaseSystem<GameInputSystem>
    {
        // TODO OK: Implement generic commands for input actions
        public static event Action<Vector2> onMove;
        public static event Action<Vector2> onLook;
        public static event Action onInteract;

        public void OnMovement(InputValue value)
        {
            onMove?.Invoke(value.Get<Vector2>());
        }
        
        public void OnLook(InputValue value)
        {
            onLook?.Invoke(value.Get<Vector2>());
        }

        public void OnInteract(InputValue value)
        {
            onInteract?.Invoke();
        }
    }
}
