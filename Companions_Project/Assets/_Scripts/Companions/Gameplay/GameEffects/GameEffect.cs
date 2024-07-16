using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Companions/Game Effect", fileName = "Game Effect")]
public class GameEffect : ScriptableObject
{
    [SerializeField]
    public List<GameEffectBehavior> behaviors = new();

    public void Execute(GameEffectContext context)
    {
        GameEffectSystem.Execute(behaviors, context);
    }

    public IEnumerator ExecuteCoroutine(GameEffectContext context)
    {
        yield return GameEffectSystem.ExecuteCoroutine(this, context);
    }
}
