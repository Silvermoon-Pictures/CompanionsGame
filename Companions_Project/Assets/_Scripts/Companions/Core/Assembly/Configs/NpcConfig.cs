using UnityEngine;

[CreateAssetMenu(menuName = "Companions/Configs/NpcConfig", fileName = "NpcConfig")]
public class NpcConfig : BaseConfig
{
    [field: SerializeField]
    public float DecisionMakingInterval { get; private set; } = 0.5f;
    [field: SerializeField]
    public float DecisionMakingDistanceThreshold { get; private set; } = 50f;
    public float DecisionMakingDistanceThresholdSqr =>
        DecisionMakingDistanceThreshold * DecisionMakingDistanceThreshold;
}
