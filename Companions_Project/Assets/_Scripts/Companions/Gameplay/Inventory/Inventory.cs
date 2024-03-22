using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

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
    // TODO Omer: Change this to Weight
    public int maxItemAmount;
    
    [ReadOnly]
    public List<Item> items;

    public void Add(Item item)
    {
        items.Add(item);
        item.OnAddedToInventory();
    }

    public Item Take()
    {
        Item item = items[0];
        item.OnRemovedFromInventory();
        items.Remove(item);
        return item;
    }

    public IEnumerable<Item> TakeAll()
    {
        foreach (var item in items)
        {
            item.OnRemovedFromInventory();
            yield return item;
        }
        
        items.Clear();
    }

    public bool IsFull()
    {
        return items.Count == maxItemAmount;
    }

    public int GetEmptySpotAmount()
    {
        return maxItemAmount - items.Count;
    }
}

