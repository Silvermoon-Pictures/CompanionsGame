using Sirenix.OdinInspector;
using UnityEngine;

public class ScoreConsideration : Consideration
{
    public bool useRandom;
    [HideIf("useRandom")]
    public float Score;

    [ShowIf("useRandom"), MinMaxSlider(0f, 1f, true)]
    public Vector2 randomScore;


    public override float CalculateScore(ConsiderationContext context)
    {
        if (useRandom)
            return Random.Range(randomScore.x, randomScore.y);
            
        return Score;
    }
}
