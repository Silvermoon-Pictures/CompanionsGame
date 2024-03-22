using System;
using System.Collections.Generic;
using Companions.Core;
using Silvermoon.Core;
using UnityEngine;

namespace Companions.Systems
{
    [RequiredSystem]
    public class ConfigurationSystem : BaseSystem<ConfigurationSystem>
    {
        private Dictionary<Type, ScriptableObject> allConfigs = new();

        private ConfigurationComponent mainConfigurationComponent;

        protected override void Preload(GameContext context)
        {
            base.Preload(context);

            mainConfigurationComponent = ComponentSystem<ConfigurationComponent>.Instance;
            foreach (var config in mainConfigurationComponent.Configurations)
            {
                allConfigs[config.GetType()] = config;
            }
        }

        public static T GetConfig<T>() where T : ScriptableObject
        {
            return Instance.allConfigs[typeof(T)] as T;
        }
    }
}
