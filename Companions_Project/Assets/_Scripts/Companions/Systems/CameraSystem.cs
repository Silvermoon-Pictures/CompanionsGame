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
        public static bool HasBrain => CinemachineCore.Instance.BrainCount > 0;

        public static CinemachineBrain Brain => CinemachineCore.Instance.GetActiveBrain(0);

        private CinemachineVirtualCamera mainVirtualCamera;
        public static CinemachineVirtualCamera MainVirtualCamera => Instance.mainVirtualCamera;

        protected override void Initialize(GameContext context)
        {
            base.Initialize(context);
            cameraConfig = ConfigurationSystem.GetConfig<CameraConfig>();
            mainCamera = FindObjectOfType<Camera>();
            if (mainCamera == null)
                mainCamera = Instantiate(cameraConfig.CameraPrefab).GetComponent<Camera>();

            mainVirtualCamera = mainCamera.GetComponentInChildren<CinemachineVirtualCamera>();
        }

        protected override void Cleanup()
        {
            base.Cleanup();
            DestroyImmediate(mainCamera.gameObject);
        }

        public static void EnableCamera()
        {
            MainVirtualCamera.gameObject.SetActive(true);
        }

        public static void DisableCamera()
        {
            MainVirtualCamera.gameObject.SetActive(false);
        }
    }
}

