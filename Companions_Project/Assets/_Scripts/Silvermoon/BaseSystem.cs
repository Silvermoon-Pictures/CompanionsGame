using UnityEngine;

namespace Silvermoon.Core
{
    public abstract class BaseSystem : MonoBehaviour, ISystem
    {
        protected virtual void Initialize()
        {
            
        }

        void ISystem.Initialize()
        {
            Initialize();
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
