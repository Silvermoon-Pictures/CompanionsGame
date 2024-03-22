using System;
using System.Collections;
using System.Collections.Generic;
using Silvermoon.Utils;
using UnityEngine;

namespace Silvermoon.Core
{
    public interface ICoreComponent { }
    
    public interface ISystem : ICoreComponent
    {
        void Preload(GameContext context) { }
        void Initialize(GameContext context) { }
        void Cleanup() { }
    }

    
    public interface IGame
    {
        IEnumerator Initialize(GameSettings settings);
        void Quit();
        IFactory Factory { get; }
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

    public class GameContext
    {
        public IGame game;
        public GameSettings settings;

        public void AddInstruction(FactoryInstruction instruction)
        {
            game.Factory.AddInstruction(instruction);
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