using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : BaseSpawner
{
    [SerializeField]
    private GameObject prefab;
    
    protected override IEnumerable<ObjectSpawnData> GetObjectsToSpawn()
    {
        yield return new ObjectSpawnData { prefab = prefab, position = transform.position};
    }
}
