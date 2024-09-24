using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Silvermoon.Utils
{
    public static class UnityUtils
    {
        public static GameObject GetRootGameObject(this GameObject target, Func<GameObject, bool> filter = null)
        {
            Transform parent = target.transform;
            while (parent != null)
            {
                Transform root = parent;
                parent = parent.parent;
                if (parent == null || (filter?.Invoke(root.gameObject) ?? true))
                    return root.gameObject;
            }

            return null;
        }
        
        public static GameObject GetRootGameObject(this Transform target) => GetRootGameObject(target.gameObject);
    }
    
    public static class ReflectionHelper
    {
        private static Dictionary<Assembly, List<string>> assemblyReferences = new();
        
        public static IEnumerable<Assembly> AllAssembliesReferencing(Assembly assembly)
        {
            foreach (Assembly other in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (other.ReferencesAssembly(assembly))
                {
                    yield return other;
                }
            }
        }
        
        public static IEnumerable<(Type type, T attribute)> AllTypesWithAttribute<T>() where T : Attribute
        {
            Assembly typeAssembly = Assembly.GetAssembly(typeof(T));
            foreach (var assembly in ReflectionHelper.AllAssembliesReferencing(typeAssembly))
            {
                foreach (var type in assembly.DefinedTypes)
                {
                    Attribute attribute = type.GetCustomAttribute(typeof(T));
                    if (attribute != null)
                        yield return (type, (T)attribute);
                }
            }
        }
        
        public static IEnumerable<TypeInfo> AllTypesDerivingFrom(Type baseType)
        {
            Assembly typeAssembly = Assembly.GetAssembly(baseType);
            foreach (var assembly in ReflectionHelper.AllAssembliesReferencing(typeAssembly))
            {
                foreach (var type in assembly.DefinedTypes)
                    if (baseType.IsAssignableFrom(type))
                        yield return type;
            }
        }
        
        public static bool ReferencesAssembly(this Assembly assembly, Assembly other)
        {
            if (!assemblyReferences.TryGetValue(assembly, out List<string> referencedAssemblies))
            {
                referencedAssemblies = new List<string>();
                CacheAssemblyReference(ref referencedAssemblies, assembly);
                assemblyReferences[assembly] = referencedAssemblies;
            }
            
            return referencedAssemblies.Contains(other.GetName().FullName);
        }

        private static void CacheAssemblyReference(ref List<string> referencedAssemblies, Assembly targetAssembly)
        {   
            // Assumes self references
            referencedAssemblies.Add(targetAssembly.GetName().FullName);
                
            var asses = targetAssembly.GetReferencedAssemblies();
            foreach (AssemblyName assName in asses)
            {
                referencedAssemblies.Add(assName.FullName);
            }
        }
    }
}
