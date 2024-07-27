using System.Collections.Generic;
using System.Linq;
using Silvermoon.Core;
using Sirenix.OdinInspector;
using UnityEngine;

public class InventoryComponent : MonoBehaviour, ICoreComponent
{
    [SerializeField]
    private GameObject owner;
    public GameObject Owner => owner;
    
    public InventoryType type;
    [SuffixLabel("kg")]
    public int maxWeight;
    [SerializeField]
    private int maxNumberOfItems = 1;
    
    [ReadOnly]
    public List<LiftableComponent> items = new();

    [SerializeField, ReadOnly]
    private int currentWeight;

    public void Add(LiftableComponent item)
    {
        currentWeight += item.Weight;
        items.Add(item);
        item.OnAddedToInventory();
    }
    
    public void Remove(LiftableComponent item)
    {
        currentWeight -= item.Weight;
        items.Remove(item);
        item.OnRemovedFromInventory();
    }

    public LiftableComponent Take()
    {
        return TakeInternal();
    }

    public bool HasCapacity(LiftableComponent item) => currentWeight + item.Weight <= maxWeight;

    private LiftableComponent TakeInternal()
    {
        LiftableComponent item = items[0];
        currentWeight -= item.Weight;
        item.OnRemovedFromInventory();
        items.Remove(item);
        return item;
    }

    public IEnumerable<LiftableComponent> TakeAll()
    {
        for (int i = items.Count - 1; i >= 0; i--)
        {
            LiftableComponent item = TakeInternal();
            yield return item;
        }
        
        items.Clear();
    }

    public int GetCurrentWeight()
    {
        return items.Sum(item => item.Weight);
    }

    public bool IsFull()
    {
        return GetCurrentWeight() >= maxWeight;
    }

    public bool CanCarryItem(LiftableComponent item)
    {
        if (items.Count >= maxNumberOfItems)
            return false;
        
        return GetEmptyWeightAmount() >= item.Weight;
    }
    
    public bool IsEmpty()
    {
        return items.Count == 0;
    }
    
    public bool HasItem()
    {
        return !IsEmpty();
    }

    public int GetEmptyWeightAmount()
    {
        return maxWeight - GetCurrentWeight();
    }
}
