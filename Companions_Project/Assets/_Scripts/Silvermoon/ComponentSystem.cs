using System.Collections.Generic;
using System;
using UnityEngine;

namespace Silvermoon.Core
{
    public static class ComponentSystem<T> where T : ICoreComponent
    {
        public static IEnumerable<T> Components
        {
            get
            {
                var components = ComponentSystem.GetAllComponents<T>();
                foreach (var component in components)
                    yield return component;
            }
        }
    }
    
    [RequiredSystem]
    public class ComponentSystem : BaseSystem<ComponentSystem>
    {
        private Dictionary<Type, HashSet<ICoreComponent>> componentMap = new();
        private List<ICoreComponent> allComponents = new();

        public static void TrackComponent(ICoreComponent component)
        {
            Type type = component.GetType();
            Instance.allComponents.Add(component);

            foreach ((Type mapType, HashSet<ICoreComponent> mapComponents) in Instance.componentMap)
            {
                if (mapType.IsAssignableFrom(type))
                {
                    mapComponents.Add(component);
                }
            }
        }

        public static void UntrackAll()
        {
            Instance.componentMap.Clear();
        }
        
        public static IEnumerable<T> GetAllComponents<T>(bool includeInactive = false) where T : ICoreComponent 
        {
            Type type = typeof(T);
            if (Instance == null)
                yield break;

            Instance.EnsureComponentMapExists(type);

            var components = Instance.componentMap;
            foreach (T component in components[type])
            {
                Component comp = component as Component;
                if (comp == null)
                    throw new Exception($"ComponentSystem - Illegal action on {component}: Attempting to get a component that does not inherit from Component");
                
                if (includeInactive || comp.gameObject.activeInHierarchy)
                    yield return component;
            }
        }
        
        private void EnsureComponentMapExists(Type type)
        {
            if (!Instance.componentMap.ContainsKey(type))
            {
                Instance.componentMap[type] = new HashSet<ICoreComponent>();

                foreach (var component in Instance.allComponents)
                {
                    if (type.IsAssignableFrom(component.GetType()))
                    {
                        Instance.componentMap[type].Add(component);
                    }
                }
            }
        }
    }
}

