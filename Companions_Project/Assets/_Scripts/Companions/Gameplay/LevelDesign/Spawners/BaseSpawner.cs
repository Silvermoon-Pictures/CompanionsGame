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
    
    void ICompanionComponent.Initialize(GameContext context)
    {
        // PrespawnObjects();
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
            Prespawn(spawnData);
        }
    }

    public void DoSpawn()
    {
        IEnumerable<ObjectSpawnData> spawnDatas = GetObjectsToSpawnInternal();
        foreach (ObjectSpawnData spawnData in spawnDatas)
        {
            var obj = Instantiate(spawnData.prefab, spawnData.position, spawnData.rotation);
            PostProcess(obj);
        }
    }
    
    private void PostProcess(GameObject go)
    {
        IEnumerable<ICoreComponent> components = go.GetComponentsInChildren<ICoreComponent>(true);
        foreach(ICoreComponent component in components)
        {
            ComponentSystem.TrackComponent(component);
        }
    }

    private void Prespawn(ObjectSpawnData spawnData)
    {
        if (spawnData.prefab == null)
        {
            throw new DesignException($"Spawner {gameObject.name} has an invalid prefab assigned. Please check!");
            return;
        }

        var obj = Instantiate(spawnData.prefab, spawnData.position, spawnData.rotation);
        //obj.SetActive(false);
        
        // foreach (var comp in obj.GetComponentsInChildren<ICoreComponent>())
        //     ComponentSystem.TrackComponent(comp);

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
