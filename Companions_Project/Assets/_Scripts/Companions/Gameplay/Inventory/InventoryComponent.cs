using System.Collections.Generic;
using Silvermoon.Core;

using UnityEngine;

public class InventoryComponent : MonoBehaviour, ICoreComponent
{
    public List<Inventory> inventories = new();

    public bool TryAddToInventory(InventoryType inventoryType, Item item)
    {
        Inventory inventory = inventories.Find(inventory => inventory.type == inventoryType);
        if (inventory == null)
            return false;

        if (inventory.items.Count > inventory.maxItemAmount)
            return false;

        inventory.Add(item);

        return true;
    }

    public Item TakeFromInventory(InventoryType inventoryType)
    {
        if (!GetInventory(inventoryType, out Inventory inventory))
            return null;

        return inventory.Take();
    }

    public void TransferInventory(InventoryComponent otherInventory, InventoryType type)
    {
        if (!GetInventory(type, out Inventory inventory))
            return;

        foreach (Item item in inventory.TakeAll())
        {
            otherInventory.TryAddToInventory(type, item);
        }
    }
    
    public bool IsFull()
    {
        foreach (var inventory in inventories)
        {
            if (inventory.IsFull())
                return true;
        }

        return false;
    }

    public bool IsFull(InventoryType inventoryType)
    {
        if (!GetInventory(inventoryType, out Inventory inventory))
            return true;

        return inventory.IsFull();
    }

    public int GetEmptySpotAmount(InventoryType type)
    {
        if (!GetInventory(type, out Inventory inventory))
            return 0;

        return inventory.GetEmptySpotAmount();
    }

    private bool GetInventory(InventoryType type, out Inventory inventory)
    {
        inventory = inventories.Find(inventory => inventory.type == type);
        return inventory != null;
    }
}
