using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Configs", fileName = "MapGenConfig")]
public class MapGenerationConfig : ScriptableObject
{
    public List<AssetData> Assets;
}

[System.Serializable]
public struct AssetData
{
    public GameObject asset;
    public int amount;
}
