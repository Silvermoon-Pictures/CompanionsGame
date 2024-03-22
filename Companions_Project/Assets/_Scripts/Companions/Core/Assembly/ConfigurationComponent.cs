using System.Collections.Generic;
using Silvermoon.Core;
using UnityEngine;


namespace Companions.Core
{
    public class ConfigurationComponent : MonoBehaviour, ICoreComponent
    {
        [field: SerializeField]
        public List<ScriptableObject> Configurations { get; private set; }
    }
}
