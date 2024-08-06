using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ScriptedActionSequence", menuName = "Companions/Npc/ScriptedActionSequence")]
public class ScriptedActionSequenceAsset : ScriptableObject
{
    public List<ScriptedActionAsset> scriptedActionQueue = new();
}
