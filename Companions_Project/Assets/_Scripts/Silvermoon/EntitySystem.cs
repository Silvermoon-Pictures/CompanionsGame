using System.Collections.Generic;

namespace Silvermoon.Core
{
    [RequiredSystem]
    public class EntitySystem : BaseSystem<EntitySystem>
    {
        private List<Entity> entities;
        
        public static void Track(Entity entity)
        {
            Instance.entities.Add(entity);
        }
        
        public static void Untrack(Entity entity)
        {
            Instance.entities.Remove(entity);
        }
    }
}
