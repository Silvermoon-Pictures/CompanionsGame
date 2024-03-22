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

    public enum WeightType
    {
        Tiny,
        Small,
        Medium,
        Large,
        Huge,
        Gargantuan
    }
}