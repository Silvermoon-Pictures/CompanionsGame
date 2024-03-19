using Silvermoon.Core;
using UnityEngine;

namespace Companions.Common
{
    public interface ICompanionComponent : ICoreComponent
    {
        void Initialize(GameContext context) { }
        void Cleanup() { }
        void WorldLoaded() { }
    }
}