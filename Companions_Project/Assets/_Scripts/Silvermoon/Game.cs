using System;
using System.Collections;
using System.Collections.Generic;
using Silvermoon.Utils;

namespace Silvermoon.Core
{
    public interface ICoreComponent
    {
        void Initialize() { }
    }
    
    public interface IGame
    {
        IEnumerator Initialize();
        void Quit();
    }

    public interface ISystem : ICoreComponent
    {
        new void Initialize() { }
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
    
    [AttributeUsage(AttributeTargets.Class)]
    public class RequiredSystemAttribute : System.Attribute
    { 
        
    }
}