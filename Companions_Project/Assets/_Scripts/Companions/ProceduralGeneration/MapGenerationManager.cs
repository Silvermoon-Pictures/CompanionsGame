using System.Collections;
using UnityEngine;
using Silvermoon.Core;

public class MapGenerationManager : BaseSystem<MapGenerationManager>
{
    public MapGenerationConfig mapGenConfig;

    public static IEnumerator GenerateMap()
    {
        foreach (var data in Instance.mapGenConfig.assets)
        {
            for (int i = 0; i < data.amount; i++)
            {
                Vector3 position = Vector2.zero + 10 * Random.insideUnitCircle;
                Instantiate(data.asset, position, Quaternion.identity);
            }
        }

        yield return BuildNavMeshAsync(GameManager.Instance.NavMeshSurface);
    }
}
