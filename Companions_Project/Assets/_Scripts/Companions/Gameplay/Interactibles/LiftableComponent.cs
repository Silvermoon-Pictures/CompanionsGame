using System;
using Companions.Systems;
using Sirenix.OdinInspector;
using UnityEngine;

public enum InventoryType
{
    None,
    Pocket,
    Hand,
    Bag
}

public class LiftableComponent : InteractionComponent
{
    public GameEffect DropGameEffect;

    private bool isLifted;

    [SerializeField, SuffixLabel("kg")]
    private float weight;
    public float Weight => weight;

    private void OnValidate()
    {
        if (weight < 0.005f)
            weight = 0.005f;
    }

    public void OnAddedToInventory()
    {
        //gameObject.SetActive(false);
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
    
    public void Drop(GameObject instigator)
    {
        Transform t = transform;
        t.SetParent(null);
        Vector3 pos = t.position;
        t.position = new Vector3(pos.x, instigator.transform.position.y, pos.z);
        
        var context = new GameEffectContext()
        {
            instigator = instigator,
            target = gameObject,
        };
        GameEffectSystem.Execute(DropGameEffect, context);
    }

    public void Lift(GameObject instigator, Transform attachSocket)
    {
        Transform t = transform;
        t.SetParent(attachSocket);
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;

        var context = new GameEffectContext()
        {
            instigator = instigator,
            target = gameObject,
        };
        GameEffectSystem.Execute(GameEffect, context);
    }
}