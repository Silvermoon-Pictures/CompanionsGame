using Silvermoon.Core;
using UnityEngine;

public class Entity : MonoBehaviour
{
    void OnEnable()
    {
        EntitySystem.Track(this);
    }

    private void OnDisable()
    {
        EntitySystem.Untrack(this);
    }
}
