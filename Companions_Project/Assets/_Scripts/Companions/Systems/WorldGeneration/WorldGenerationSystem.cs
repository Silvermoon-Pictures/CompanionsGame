using System;
using System.Collections;
using Companions.Common;
using Silvermoon.Core;
using UnityEngine;

namespace Companions.Systems
{
    [RequiredSystem]
    public class WorldGenerationSystem : BaseSystem<WorldGenerationSystem>
    {
        public event EventHandler OnWorldIsLoaded;

        protected override void Initialize(GameContext context)
        {
            base.Initialize(context);
            StartCoroutine(GenerateWorld(context));
        }

        private IEnumerator GenerateWorld(GameContext context)
        {
            // TODO OK: Create World Factory
            var worldGenerationConfig = ConfigurationSystem.GetConfig<WorldGenerationConfig>();
            if (worldGenerationConfig.World != null)
            {
                FactoryInstruction instruction =
                    new FactoryInstruction(worldGenerationConfig.World, Vector3.zero, Quaternion.identity);
                context.AddInstruction(instruction);
            }

            yield return context.game.Factory.ProcessQueue();

            yield return new WaitForSeconds(1f);

            OnWorldIsLoaded?.Invoke(this, EventArgs.Empty);
            NotifyWorldGeneration();
        }

        private void NotifyWorldGeneration()
        {
            foreach (ICompanionComponent worshipSystem in ComponentSystem<ICompanionComponent>.Components)
                worshipSystem.WorldLoaded();
        }
    }
}
