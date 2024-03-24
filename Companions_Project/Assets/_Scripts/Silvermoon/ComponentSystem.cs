using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

namespace Silvermoon.Core
{
    public static class ComponentSystem<T> where T : ICoreComponent
    {
        public static T Instance
        {
            get
            {
                var components = ComponentSystem.GetAllComponents<T>();
                foreach (var component in components)
                    return component;
                
                throw new Exception($"Component {typeof(T)} not found");
            }
        }
        
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
        private Dictionary<Type, ComponentGroup> componentGroups = new();
        private List<ICoreComponent> allComponents = new();

        public static void TrackComponent(ICoreComponent component)
        {
            Type type = component.GetType();

            var components = Instance.allComponents;
            var componentGroups = Instance.componentGroups;
            
            if(!componentGroups.ContainsKey(type))
                componentGroups[type] = new ComponentGroup { Type = type };
            foreach (var (groupType, componentGroup) in componentGroups)
            {
                if(groupType.IsAssignableFrom(type))
                    componentGroup.Add(component);
            }
            
            components.Add(component);
            componentGroups[type].Add(component);
        }

        public static void UntrackAll()
        {
            Instance.componentGroups.Clear();
            Instance.allComponents.Clear();
        }
        
        public static IEnumerable<T> GetAllComponents<T>(bool includeInactive = false) where T : ICoreComponent
            => GetAllComponents(typeof(T), includeInactive).Cast<T>();
        
        public static IEnumerable<ICoreComponent> GetAllComponents(Type type, bool includeInactive = false)
        {
            foreach (var coreComponent in GetAllComponents(type, Vector3.zero, -1, null, includeInactive)) 
                yield return coreComponent;
        }
        
        public static IEnumerable<ICoreComponent> GetAllComponents(Type type, Vector3 position = new(), float radius = -1, Func<Component, bool> filter = null, bool includeInactive = false)
        {
            if (Instance == null)
                yield break;

            Instance.EnsureComponentGroupExists(type);

            if (!Instance.componentGroups.ContainsKey(type))
                yield break;
            
            for (int i = Instance.componentGroups[type].Components.Count - 1; i >= 0; i--)
            {
                var component = Instance.componentGroups[type].Components.ElementAt(i);
                Component comp = component as Component;
                if (comp == null)
                    throw new Exception($"ComponentSystem - Illegal action on {component}: Attempting to get a component that does not inherit from Component");

                if (IsInvalid(comp, position, radius, filter, includeInactive))
                    continue;

                yield return component;
            }
        }
        
        public static ICoreComponent GetClosestTarget(Type type, Vector3 position, float radius = -1f, Func<Component, bool> filter = null, bool includeInvalidTargets = false)
        {
            IEnumerator<ICoreComponent> targetIterator = GetAllComponents(type, position, radius, filter, includeInvalidTargets).GetEnumerator();
            if (!targetIterator.MoveNext())
                return null;
            

            // TODO Omer: Avoid these casts
            Component closestTarget = (Component)targetIterator.Current;
            float closestDistance = Vector3.SqrMagnitude(position - closestTarget.transform.position);

            while (targetIterator.MoveNext())
            {
                Component current = (Component)targetIterator.Current;
                float currentDistance = Vector3.SqrMagnitude(current.transform.position - position);
                if (currentDistance > closestDistance)
                    continue;

                closestTarget = current;
                closestDistance = currentDistance;
            }

            return (ICoreComponent)closestTarget;
        }

        private static bool IsInvalid(Component comp, Vector3 position, float radius, Func<Component, bool> filter, bool includeInactive)
        {
            if (!includeInactive && !comp.gameObject.activeInHierarchy)
                return true;

            if (!(filter?.Invoke(comp) ?? true))
                return true;
            if (radius <= float.Epsilon)
                return false;
                
            float distance = Vector3.Distance(comp.transform.position, position);
            bool isOutsideRadius = distance > radius;
            return isOutsideRadius;
        }
        
        private void EnsureComponentGroupExists(Type type)
        {
            if (componentGroups.ContainsKey(type)) 
                return;
            
            componentGroups[type] = new ComponentGroup { Type = type };

            foreach (var component in allComponents)
            {
                if (type.IsAssignableFrom(component.GetType()))
                {
                    componentGroups[type].Add(component);
                }
            }
        }
        
        public static ComponentGroup GetComponentGroup<T>()
        {
            Instance.EnsureComponentGroupExists(typeof(T));
            return Instance.componentGroups[typeof(T)];
        }
        
        public static int GetComponentCount<T>()
        {
            return GetComponentCount(typeof(T));
        }
        
        public static int GetComponentCount(Type type, bool inactiveIncluded = true)
        {
            if (!Instance.componentGroups.ContainsKey(type))
                return 0;
            
            if (!inactiveIncluded)
            {
                int count = 0;
                foreach (var comp in GetAllComponents(type, includeInactive: false))
                {
                    count++;
                }

                return count;
            }
            
            
            return Instance.componentGroups.Count;
        }
    }
    
    public class ComponentGroup
    {
        public HashSet<ICoreComponent> Components { get; private set; } = new HashSet<ICoreComponent>();
        public Type Type { get; internal set; }
        
        public event EventHandler<ICoreComponent> OnComponentAdded;
        public event EventHandler<ICoreComponent> OnComponentRemoved;
        
        public void Add(ICoreComponent component)
        {
            Components.Add(component);
            OnComponentAdded?.Invoke(this, component);
        }

        public void Remove(ICoreComponent component)
        {
            Components.Remove(component);
            OnComponentRemoved?.Invoke(this, component);
        }
    }
}

