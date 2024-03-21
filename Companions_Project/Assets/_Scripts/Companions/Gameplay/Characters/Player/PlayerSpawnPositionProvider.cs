using Silvermoon.Core;
using UnityEngine;

public class PlayerSpawnPositionProvider : MonoBehaviour, ICoreComponent
{
    public Transform spawnPosition;
    
    public Vector3 GetSpawnPosition()
    {
        return spawnPosition != null ? spawnPosition.position : transform.position;
    }
}
    
    
