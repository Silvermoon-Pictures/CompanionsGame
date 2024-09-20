using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubactionContext
{
    public Npc npc;
    public Animator animator;
    public GameObject target;
    public BlackboardComponent blackboard;
    public EventArgs actionCallback;
}

[Serializable]
public partial class ActionGraphNode
{
    [SerializeField] private string guid;
    [SerializeField] private Rect position;
    [SerializeField] private string nextNodeId;

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

    public void SetPosition(Vector2 pos)
    {
        position.position = pos;
    }

    public void SetY(float y)
    {
        position.y = y;
    }

    public virtual IEnumerator Execute(SubactionContext context)
    {
        yield break;
    }
}
