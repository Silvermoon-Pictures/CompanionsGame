using Companions.Systems;
using Silvermoon.Core;
using UnityEngine;

public class Item : MonoBehaviour, ICoreComponent, IAvailablity
{
    [field: SerializeField]
    public int Weight { get; private set; }
    
    public void OnAddedToInventory()
    {
        gameObject.SetActive(false);
    }

    // TODO Omer: Figure out what happens here
    public void OnRemovedFromInventory()
    {
        //gameObject.SetActive(true);
    }

    public bool IsAvailable(GameObject querier)
    {
        return InventorySystem.GetEmptyWeightAmount(querier, InventoryType.Hand) >= Weight;
    }
}
