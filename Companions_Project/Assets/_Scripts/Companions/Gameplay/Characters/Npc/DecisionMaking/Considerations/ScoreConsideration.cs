using UnityEngine;

public class ScoreConsideration : Consideration
{
    public float Score;


    public override float CalculateScore(ConsiderationContext context)
    {
        return Score;
    }
}
