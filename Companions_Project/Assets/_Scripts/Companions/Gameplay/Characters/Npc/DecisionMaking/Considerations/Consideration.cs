using System;
using Companions.Common;
using UnityEngine;

public class ConsiderationContext
{
    public Npc npc;
    public EventArgs callback;
}

public abstract class Consideration
{
    public abstract float CalculateScore(ConsiderationContext context);
}

public class WeightedConsideration : Consideration
{
    public Consideration Consideration;
    public AnimationCurve Curve = new(new Keyframe(0.0f, 0f), new Keyframe(1.0f, 1f));
    public float MaxValue = 1f;
    public float Weight = 1f;
    
    public override float CalculateScore(ConsiderationContext context)
    {
        return Curve.Evaluate(Consideration.CalculateScore(context) / MaxValue) * Weight;
    }
}

public class PriorityConsideration : Consideration
{
    public Consideration Consideration;

    public override float CalculateScore(ConsiderationContext context)
    {
        return Consideration.CalculateScore(context);
    }
}