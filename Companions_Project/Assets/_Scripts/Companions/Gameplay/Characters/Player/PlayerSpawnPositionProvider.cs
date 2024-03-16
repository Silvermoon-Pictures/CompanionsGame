using System.Collections.Generic;
using Companions.Common;
using UnityEngine;

public class PlayerSpawnPositionProvider : MonoBehaviour, ICompanionComponent
{
    public Transform spawnPosition;
    
    public Vector3 GetSpawnPosition()
    {
        return spawnPosition != null ? spawnPosition.position : transform.position;
    }
}
    
    
