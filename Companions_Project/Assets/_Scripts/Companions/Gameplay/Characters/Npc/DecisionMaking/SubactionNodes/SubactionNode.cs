using System.Collections;
using UnityEngine;



public class SubactionNode : ScriptableObject
{
    [HideInInspector]
    public SubactionNode nextNode;

    protected virtual float GetDuration() => 0f;

    public virtual IEnumerator Execute(SubactionContext context)
    {
        yield break;
    }
}