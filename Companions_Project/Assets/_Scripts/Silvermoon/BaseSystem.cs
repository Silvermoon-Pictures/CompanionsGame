using UnityEngine;

namespace Silvermoon.Core
{
    public abstract class BaseSystem : MonoBehaviour, ISystem
    {
        protected virtual void Initialize() { }
        protected virtual void Cleanup() { }

        void ISystem.Initialize()
        {
            Initialize();
        }

        void ISystem.Cleanup()
        {
            Cleanup();
        }
    }

    public abstract class BaseSystem<T> : BaseSystem where T : BaseSystem
    {
        public static T Instance { get; private set; }

        public void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(gameObject);
            else
                Instance = this as T;
        }
    }

}
