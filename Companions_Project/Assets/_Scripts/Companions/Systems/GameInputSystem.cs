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
        public static event Action onJump;
        public static event Action<bool> onSprint;
        public static event Action onTogglePOV;
        public static PlayerInput PlayerInput { get; set; }

        protected override void Initialize(GameContext context)
        {
            base.Initialize(context);

            CutsceneSystem.CutsceneStarted += OnCutsceneStarted;
            CutsceneSystem.CutsceneStopped += OnCutsceneStopped;
        }

        private void OnCutsceneStarted(object director, EventArgs e)
        {
            SwitchToGameplayInputLayer();
        }
        
        private void OnCutsceneStopped(object director, EventArgs e)
        {
            SwitchToCutsceneInputLayer();
        }

        protected override void Cleanup()
        {
            base.Cleanup();
            
            CutsceneSystem.CutsceneStarted -= OnCutsceneStarted;
            CutsceneSystem.CutsceneStopped -= OnCutsceneStopped;
        }

        private static void SwitchToCutsceneInputLayer()
        {
            PlayerInput.SwitchCurrentActionMap("Cutscene");
        }
        
        private static void SwitchToGameplayInputLayer()
        {
            PlayerInput.SwitchCurrentActionMap("Gameplay");
        }

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

        public void OnJump(InputValue value)
        {
            onJump?.Invoke();
        }

        public void OnSprint(InputValue value)
        {
            onSprint?.Invoke(value.isPressed);
        }

        public void OnTogglePOV(InputValue value)
        {
            onTogglePOV?.Invoke();
        }
    }
}
