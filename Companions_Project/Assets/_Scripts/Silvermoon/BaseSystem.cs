using UnityEngine;

namespace Silvermoon.Core
{
    public abstract class BaseSystem : MonoBehaviour, ISystem
    {
        protected GameContext GameContext { get; private set; }
        
        protected virtual void Preload(GameContext context) { }
        protected virtual void Initialize(GameContext context) { }
        protected virtual void Cleanup() { }

        void ISystem.Preload(GameContext context)
        {
            Preload(context);
        }
        
        void ISystem.Initialize(GameContext context)
        {
            GameContext = context;
            
            Initialize(context);
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
