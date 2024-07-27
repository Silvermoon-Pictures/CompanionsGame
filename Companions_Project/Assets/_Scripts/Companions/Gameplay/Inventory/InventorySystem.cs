using System.Collections.Generic;
using Companions.Common;
using Silvermoon.Core;
using UnityEditor;
using UnityEngine;

namespace Companions.Systems
{
    [RequiredSystem]
    public class InventorySystem : BaseSystem<InventorySystem>, ICompanionComponent
    {
        private Dictionary<GameObject, List<InventoryComponent>> inventoryComponents = new(); 
        
        void ICompanionComponent.WorldLoaded()
        {
            foreach (var inventoryComponent in ComponentSystem<InventoryComponent>.Components)
            {
                if (!inventoryComponents.ContainsKey(inventoryComponent.Owner))
                    inventoryComponents.Add(inventoryComponent.Owner, new());
                
                inventoryComponents[inventoryComponent.Owner].Add(inventoryComponent);
            }
        }

        protected override void Cleanup()
        {
            base.Cleanup();
            
            inventoryComponents.Clear();
        }

        public static bool GetInventoryComponent(GameObject entity, InventoryType type, out InventoryComponent inventoryComponent)
        {
            inventoryComponent = null;
            if (!Instance.inventoryComponents.TryGetValue(entity, out List<InventoryComponent> inventories))
                return false;
            
            foreach (InventoryComponent inventory in inventories)
            {
                if (inventory.type == type)
                {
                    inventoryComponent = inventory;
                    return true;
                }
            }

            return false;
        }

        public static bool CanCarryItem(GameObject entity, LiftableComponent item, InventoryType type)
        {
            if (!GetInventoryComponent(entity, type, out InventoryComponent inventoryComponent))
                return false;
            
            return inventoryComponent.CanCarryItem(item);
        }

        public static void AddToInventory(GameObject entity, LiftableComponent item, InventoryType type)
        {
            if (!GetInventoryComponent(entity, type, out InventoryComponent inventoryComponent))
                return;
            
            inventoryComponent.Add(item);
        }

        public static bool IsInventoryFull(GameObject entity, InventoryType type)
        {
            if (!GetInventoryComponent(entity, type, out InventoryComponent inventoryComponent))
                return false;

            return inventoryComponent.IsFull();
        }
        
        public static bool IsInventoryEmpty(GameObject entity, InventoryType type)
        {
            if (!GetInventoryComponent(entity, type, out InventoryComponent inventoryComponent))
                return false;

            return inventoryComponent.IsEmpty();
        }

        public static int GetEmptyWeightAmount(GameObject entity, InventoryType type)
        {
            if (!GetInventoryComponent(entity, type, out InventoryComponent inventoryComponent))
                return 0;

            return inventoryComponent.GetEmptyWeightAmount();
        }

        public static void DropItem(GameObject entity, LiftableComponent itemToDrop)
        {
            if (!Instance.inventoryComponents.ContainsKey(entity))
                return;

            bool itemDropped = false;
            foreach (var inventory in Instance.inventoryComponents[entity])
            {
                if (itemDropped)
                    break;
                
                foreach (var item in inventory.items)
                {
                    if (item == itemToDrop)
                    {
                        inventory.Remove(item);
                        itemDropped = true;
                        break;
                    }
                }
            }
        }
    }
}
