using System.Collections;
using UnityEngine;

public class SubactionContext
{
    public Npc npc;
    public Animator animator;
    public GameObject target;
    public DictionaryComponent dictionaryComponent;
}

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