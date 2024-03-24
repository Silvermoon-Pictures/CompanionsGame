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

        if (!inventory.HasCapacity(item))
            return false;
        
        inventory.Add(item);
        return true;
    }

    public bool TryGetFromInventory(InventoryType inventoryType, out Item item)
    {
        item = null;
        if (!GetInventory(inventoryType, out Inventory inventory))
            return false;

        if (!inventory.HasItem())
            return false;
        
        item = inventory.Take();
        return true;
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
    
    public bool IsEmpty(InventoryType inventoryType)
    {
        if (!GetInventory(inventoryType, out Inventory inventory))
            return true;

        return inventory.IsEmpty();
    }

    public int GetEmptyWeightAmount(InventoryType type)
    {
        if (!GetInventory(type, out Inventory inventory))
            return 0;

        return inventory.GetEmptyWeightAmount();
    }

    private bool GetInventory(InventoryType type, out Inventory inventory)
    {
        inventory = inventories.Find(inventory => inventory.type == type);
        return inventory != null;
    }
}
