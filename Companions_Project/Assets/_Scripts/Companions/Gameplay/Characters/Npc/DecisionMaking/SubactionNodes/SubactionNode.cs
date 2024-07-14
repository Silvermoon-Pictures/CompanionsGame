using System.Collections;
using UnityEngine;

public class SubactionNode : ScriptableObject
{
    [HideInInspector]
    public SubactionNode nextNode;

    public float Duration => GetDuration();

    protected virtual float GetDuration() => 0f;

    public virtual IEnumerator Execute(Npc npc)
    {
        yield break;
    }
}