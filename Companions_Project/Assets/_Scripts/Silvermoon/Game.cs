using System;
using System.Collections;
using System.Collections.Generic;
using Silvermoon.Utils;

namespace Silvermoon.Core
{
    public interface ICoreComponent { }
    
    public interface ISystem : ICoreComponent
    {
        void Initialize() { }
        void Cleanup() { }
    }

    
    public interface IGame
    {
        IEnumerator Initialize(GameSettings settings);
        void Quit();
    }
    
    public static class Game
    {
        public static IEnumerable<Type> AllRequiredSystems()
        {
            foreach((Type systemType, RequiredSystemAttribute attribute) in ReflectionHelper.AllTypesWithAttribute<RequiredSystemAttribute>())
            {
                yield return systemType;
            }
        }
    }
    
    [System.Serializable]
    public class GameSettings
    {
        public bool simulate;
    }

    
    [AttributeUsage(AttributeTargets.Class)]
    public class RequiredSystemAttribute : System.Attribute
    { 
        
    }
}