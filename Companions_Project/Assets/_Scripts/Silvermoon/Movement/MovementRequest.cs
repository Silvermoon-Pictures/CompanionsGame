using UnityEngine;

namespace Silvermoon.Movement
{
    public abstract class MovementRequest
    {
        public abstract bool Done { get; }
        public abstract Vector3 Evaluate(MovementContext context, float totalTime);
    }
}
