using System.Collections.Generic;
using Companions.Common;
using Silvermoon.Core;
using UnityEngine;

namespace Companions.Systems
{
    [RequiredSystem]
    public class InventorySystem : BaseSystem<InventorySystem>, ICompanionComponent
    {
        private Dictionary<GameObject, InventoryComponent> inventoryComponents = new(); 
        
        void ICompanionComponent.WorldLoaded()
        {
            foreach (var inventoryComponent in ComponentSystem<InventoryComponent>.Components)
            {
                inventoryComponents.Add(inventoryComponent.gameObject, inventoryComponent);
            }
        }

        protected override void Cleanup()
        {
            base.Cleanup();
            
            inventoryComponents.Clear();
        }

        public static bool GetInventoryComponent(GameObject entity, out InventoryComponent inventoryComponent)
        {
            if (Instance.inventoryComponents.TryGetValue(entity, out inventoryComponent))
                return true;

            if (!entity.TryGetComponent(out inventoryComponent))
                return false;

            Instance.inventoryComponents.Add(entity, inventoryComponent);
            return true;
        }

        public static bool IsInventoryFull(GameObject entity)
        {
            if (!GetInventoryComponent(entity, out InventoryComponent inventoryComponent))
                return false;

            return inventoryComponent.IsFull();
        }

        public static bool HasPlaceInInventory(GameObject entity)
        {
            return !IsInventoryFull(entity);
        }

        public static bool IsInventoryFull(GameObject entity, InventoryType type)
        {
            if (!GetInventoryComponent(entity, out InventoryComponent inventoryComponent))
                return false;

            return inventoryComponent.IsFull(type);
        }
        
        public static bool IsInventoryEmpty(GameObject entity, InventoryType type)
        {
            if (!GetInventoryComponent(entity, out InventoryComponent inventoryComponent))
                return false;

            return inventoryComponent.IsEmpty(type);
        }

        public static int GetEmptyWeightAmount(GameObject entity, InventoryType type)
        {
            if (!GetInventoryComponent(entity, out InventoryComponent inventoryComponent))
                return 0;

            return inventoryComponent.GetEmptyWeightAmount(type);
        }
    }
}
