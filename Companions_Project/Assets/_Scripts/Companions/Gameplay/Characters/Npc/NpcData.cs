using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Companions/Npc/NpcData", fileName = "NpcData")]
public class NpcData : ScriptableObject
{
    public List<ActionAsset> Actions;
}
