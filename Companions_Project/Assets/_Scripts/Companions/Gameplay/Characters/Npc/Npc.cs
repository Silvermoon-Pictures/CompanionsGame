using System;
using Companions.StateMachine;
using Silvermoon.Core;
using Silvermoon.Movement;
using Silvermoon.Utils;
using UnityEngine;

public partial class Npc : MonoBehaviour, ITargetable, ICoreComponent, ILifter
{
    [field: SerializeField]
    public NpcData NpcData { get; private set; }
    private NpcBrain brain;
    
    public MovementComponent MovementComponent { get; private set; }

    public GameObject target;

    public bool rock;

    public NpcAction CurrentAction { get; private set; }
    private NpcFSM stateMachine;

    public bool ExecutingAction { get; private set; }

    private void Awake()
    {
        MovementComponent = GetComponent<MovementComponent>();
        SetupMovement();
        stateMachine = NpcFSM.Make(this);
    }

    void Start()
    {
        if (NpcData == null)
            throw new DesignException($"NpcData on {name} is not set!");
        
        brain = new NpcBrain(this);
        CurrentAction = new NpcAction();

        Decide();
    }

    public void Decide()
    {
        var newAction = brain.Decide(out var newTarget);
        if (newAction == null)
        {
            ExecutingAction = true;
            return;
        }

        CurrentAction.actionData = newAction;
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
        return distance <= CurrentAction.actionData.Range;
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
        CurrentAction.actionData.Execute(CreateContext());
        ExecutingAction = true;
    }

    public bool IsCarryingRock()
    {
        return rock;
    }

    private void Update()
    {
        var context = new NpcFSMContext(Time.deltaTime)
        {
            velocity = MovementComponent.Velocity,
            executingAction = ExecutingAction
        };
        
        stateMachine.Transition(context);
        stateMachine.Update(context);
        stateMachine.PostUpdate(context);
        
        UpdateMovement();
        UpdateAnimations();
    }

    private void UpdateAnimations()
    {
        
    }

    private void UpdateMovement()
    {
        if (ExecutingAction || target == null)
            return;

        if (IsInActionRange())
            StopMoving();
        else
            UpdateDestination(target.transform.position);
    }

    public void Lift(LiftableComponent liftableComponent)
    {
        liftableComponent.transform.position = transform.position + 5 * transform.forward;
        liftableComponent.transform.SetParent(transform);
        
        rock = true;
    }

    private void OnReachedTarget()
    {
        ExecuteCurrentAction();
    }
}
