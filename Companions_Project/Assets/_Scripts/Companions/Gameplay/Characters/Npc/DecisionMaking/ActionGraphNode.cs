using System;
using UnityEngine;

[Serializable]
public class ActionGraphNode
{
    [SerializeField] private string guid;
    [SerializeField] private Rect position;

    public string typeName;

    public string Id => guid;
    public Rect Position => position;

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
}
