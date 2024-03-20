using System;
using System.Collections.Generic;
using Companions.Common;
using Silvermoon.Core;
using Silvermoon.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

public class ObjectSpawnData
{
    public GameObject prefab;
    public Vector3 position;
    public Quaternion rotation = Quaternion.identity;
}

public abstract class BaseSpawner : MonoBehaviour, ICompanionComponent
{
    [SerializeField] private int amount = 1;

    private List<GameObject> prespawned = new();
    
    protected abstract IEnumerable<ObjectSpawnData> GetObjectsToSpawn();

    private GameContext context;
    
    void ICompanionComponent.Initialize(GameContext context)
    {
        this.context = context;
    }

    private void PrespawnObjects()
    {
        IEnumerable<ObjectSpawnData> spawnDatas = GetObjectsToSpawnInternal();
        foreach (ObjectSpawnData spawnData in spawnDatas)
        {
            Prespawn(spawnData);
        }
    }
    
    void ICompanionComponent.WorldLoaded()
    {
        Spawn();
    }

    private void Spawn()
    {
        IEnumerable<ObjectSpawnData> spawnDatas = GetObjectsToSpawnInternal();
        foreach (ObjectSpawnData spawnData in spawnDatas)
        {
            var instruction = new FactoryInstruction(spawnData.prefab, spawnData.position, spawnData.rotation, SetupObject);
            context.AddInstruction(instruction);
        }
    }
    
    protected virtual void SetupObject(GameObject go) { }

    private void Prespawn(ObjectSpawnData spawnData)
    {
        if (spawnData.prefab == null)
        {
            throw new DesignException($"Spawner {gameObject.name} has an invalid prefab assigned. Please check!");
            return;
        }

        var obj = Instantiate(spawnData.prefab, spawnData.position, spawnData.rotation);
        prespawned.Add(obj);
    }
    
    private IEnumerable<ObjectSpawnData> GetObjectsToSpawnInternal()
    {
        for (int i = 0; i < amount; i++)
        {
            foreach(var obj in GetObjectsToSpawn())
                yield return obj;
        }
    }
    
}
