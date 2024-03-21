using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Gameplay/Npc/NpcData", fileName = "NpcData")]
public class NpcData : ScriptableObject
{
    public List<ActionAsset> Actions;
}
