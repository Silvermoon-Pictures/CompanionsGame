using System;
using System.Collections;
using System.Collections.Generic;
using Silvermoon.Utils;

namespace Silvermoon.Core
{
    public interface ICompanionComponent { }

    public interface ISystem  : ICompanionComponent
    {
        void Initialize() { }
        void Cleanup() { }
    }
    
    public interface IGame
    {
        IEnumerator Initialize();
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
    
    [AttributeUsage(AttributeTargets.Class)]
    public class RequiredSystemAttribute : System.Attribute
    { 
        
    }
}