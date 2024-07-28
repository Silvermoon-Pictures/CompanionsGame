using UnityEngine;

[CreateAssetMenu(fileName = "PlayerConfig", menuName = "Companions/Configs/PlayerConfig")]
public class PlayerConfig : BaseConfig
{
    [field: SerializeField]
    public LayerMask InteractionLayerMask { get; private set; }
}
