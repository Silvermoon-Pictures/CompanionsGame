using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubactionContext
{
    public Npc npc;
    public Animator animator;
    public GameObject target;
    public DictionaryComponent dictionaryComponent;
}

[Serializable]
public class ActionGraphNode
{
    [SerializeField] private string guid;
    [SerializeField] private Rect position;
    [SerializeField] private string nextNodeId;

    public string typeName;

    public void SetNextNode(string nextNodeGuid) => this.nextNodeId = nextNodeGuid;


    public string Id => guid;
    public Rect Position => position;
    public string NextNodeId => nextNodeId;

    public ActionGraphNode()
    {
        NewGUID();
    }

    private void NewGUID()
    {
        guid = Guid.NewGuid().ToString();
    }

    public void SetPosition(Rect pos)
    {
        position = pos;
    }

    public virtual IEnumerator Execute(SubactionContext context)
    {
        yield break;
    }
}
