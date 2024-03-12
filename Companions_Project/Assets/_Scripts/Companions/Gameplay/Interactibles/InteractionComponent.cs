using System;
using UnityEngine;

public class InteractionComponent : MonoBehaviour
{
    private void Awake()
    {
        
    }

    public void Interact()
    {
        // TODO OK: Implement scriptable Game Effects that can be created in an editor window
        Debug.Log($"{name} is interacted with.");
    }
}
