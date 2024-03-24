using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;

public enum InventoryType
{
    None,
    Pocket,
    Hand,
    Bag
}

[System.Serializable]
public class Inventory
{
    public InventoryType type;
    [FormerlySerializedAs("maxItemAmount")] public int maxWeight;
    
    [ReadOnly]
    public List<Item> items = new();

    private int currentWeight;

    public void Add(Item item)
    {
        items.Add(item);
        currentWeight += item.Weight;
        item.OnAddedToInventory();
    }

    public Item Take()
    {
        return TakeInternal();
    }

    public bool HasCapacity(Item item) => currentWeight + item.Weight <= maxWeight;

    private Item TakeInternal()
    {
        Item item = items[0];
        currentWeight -= item.Weight;
        item.OnRemovedFromInventory();
        items.Remove(item);
        return item;
    }

    public IEnumerable<Item> TakeAll()
    {
        for (int i = items.Count - 1; i >= 0; i--)
        {
            Item item = TakeInternal();
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

