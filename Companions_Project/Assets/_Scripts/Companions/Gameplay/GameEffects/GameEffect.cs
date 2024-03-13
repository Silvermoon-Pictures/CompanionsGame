using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Gameplay/Game Effect", fileName = "Game Effect")]
public class GameEffect : ScriptableObject
{
    [SerializeField]
    public List<GameEffectBehavior> behaviors = new();

    public void Execute(GameEffectContext context)
    {
        foreach (var behavior in behaviors)
            behavior.Execute(context);
    }
}
