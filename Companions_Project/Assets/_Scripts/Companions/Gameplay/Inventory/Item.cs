using Silvermoon.Core;
using UnityEngine;

public class Item : MonoBehaviour, ICoreComponent
{
    [field: SerializeField]
    public int Weight { get; private set; }
    
    public void OnAddedToInventory()
    {
        gameObject.SetActive(false);
    }

    public void OnRemovedFromInventory()
    {
        gameObject.SetActive(true);
    }
}
