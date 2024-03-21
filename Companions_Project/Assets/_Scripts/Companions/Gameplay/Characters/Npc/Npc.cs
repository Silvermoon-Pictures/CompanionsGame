using System;
using Silvermoon.Core;
using Silvermoon.Movement;
using Silvermoon.Utils;
using UnityEngine;

public partial class Npc : MonoBehaviour, ITargetable, ICoreComponent
{
    [field: SerializeField]
    public NpcData NpcData { get; private set; }
    private NpcBrain brain;
    
    public MovementComponent MovementComponent { get; private set; }

    public GameObject target;

    public bool rock;

    private NpcAction currentAction;

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
        currentAction = new NpcAction();

        Decide();
    }

    private void Decide()
    {
        currentAction.actionData = brain.Decide(out var newTarget);
        target = ((Component)newTarget).gameObject;
        
        if (!IsInActionRange())
        {
            UpdateDestination(target.transform.position);
            return;
        }
        
        ExecuteCurrentAction();
    }

    private bool IsInActionRange()
    {
        float distance = Vector3.Distance(target.transform.position, transform.position);
        return distance <= currentAction.actionData.Range;
    }

    private GameEffectContext CreateContext()
    {
        var context = new GameEffectContext()
        {
            instigator = gameObject,
            target = target.gameObject
        };

        return context;
    }

    public void ExecuteCurrentAction()
    {
        currentAction.actionData.Execute(CreateContext());
    }

    public bool IsCarryingRock()
    {
        return rock;
    }

    private void Update()
    {
        if (target == null)
            return;
        
        if (IsInActionRange())
            StopMoving();
        else
            UpdateDestination(target.transform.position);
    }
}
