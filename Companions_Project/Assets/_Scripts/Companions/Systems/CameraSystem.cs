using Cinemachine;
using Silvermoon.Core;
using UnityEngine;

namespace Companions.Systems
{
    [RequiredSystem]
    public class CameraSystem : BaseSystem<CameraSystem>
    {
        private Camera mainCamera;
        public static Camera Camera => Instance.mainCamera;
        private CameraConfig cameraConfig;

        public static CinemachineBrain Brain => CinemachineCore.Instance.GetActiveBrain(0);

        protected override void Initialize(GameContext context)
        {
            base.Initialize(context);
            cameraConfig = ConfigurationSystem.GetConfig<CameraConfig>();
            mainCamera = Instantiate(cameraConfig.CameraPrefab).GetComponent<Camera>();
        }

        protected override void Cleanup()
        {
            base.Cleanup();
            DestroyImmediate(mainCamera.gameObject);
        }
    }
}

