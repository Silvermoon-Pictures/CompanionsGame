using Silvermoon.Movement;
using Silvermoon.Utils;
using UnityEngine;

public partial class Npc : MonoBehaviour
{
    [field: SerializeField]
    public NpcData NpcData { get; private set; }
    private NpcBrain brain;
    
    public MovementComponent MovementComponent { get; private set; }

    public ActionAsset currentAction;

    public Transform target;

    public bool rock;

    private void Awake()
    {
        MovementComponent = GetComponent<MovementComponent>();
        SetupMovement();
    }

    void Start()
    {
        if (NpcData == null)
            throw new DesignException($"NpcData on {name} is not set!");
        
        brain = new NpcBrain(this);
        currentAction = brain.Decide();
        
        var context = new GameEffectContext()
        {
            instigator = gameObject,
            target = gameObject
        };
        currentAction.gameEffectOnStart.Execute(context);
    }

    public bool IsCarryingRock()
    {
        return rock;
    }
}
