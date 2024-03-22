using System;
using Companions.Common;
using Companions.StateMachine;
using Silvermoon.Core;
using Silvermoon.Movement;
using Silvermoon.Utils;
using UnityEngine;

public partial class Npc : MonoBehaviour, ITargetable, ICompanionComponent, ILifter
{
    [field: SerializeField]
    public NpcData NpcData { get; private set; }
    private NpcBrain brain;
    
    public MovementComponent MovementComponent { get; private set; }

    public bool rock;

    public NpcAction Action { get; private set; }
    private NpcFSM stateMachine;

    public bool HasAction { get; set; }
    public bool ShouldMove { get; set; }
    public bool ExecuteAction => HasAction && !ShouldMove;

    private void Awake()
    {
        MovementComponent = GetComponent<MovementComponent>();
        SetupMovement();
        stateMachine = NpcFSM.Make(this);
        
        brain = new NpcBrain(this);
        Action = new NpcAction();
    }

    void Start()
    {
        if (NpcData == null)
            throw new DesignException($"NpcData on {name} is not set!");

        Decide();
    }

    public void Decide()
    {
        HasAction = false;
        Action.Reset();
        
        var newAction = brain.Decide(out var newTarget);
        if (newAction == null)
            return;

        Action.actionData = newAction;
        if (newTarget != null)
            Action.target = ((Component)newTarget).gameObject;

        HasAction = true;
        ShouldMove = !IsInTargetRange() && !Action.WaitForTarget;
    }

    private bool IsInTargetRange()
    {
        if (!HasAction)
            return false;
        
        float distance = Vector3.Distance(Action.TargetPosition, transform.position);
        return distance <= Action.actionData.Range;
    }

    public bool WaitForTarget()
    {
        return Action.WaitForTarget && !IsInTargetRange();
    }

    public GameEffectContext CreateContext()
    {
        var context = new GameEffectContext()
        {
            instigator = gameObject,
            target = Action.target
        };

        return context;
    }

    private void ExecuteCurrentAction()
    {
        ShouldMove = false;
    }

    public bool IsCarryingRock()
    {
        return rock;
    }

    private void Update()
    {
        UpdateAnimations();
        UpdateStateMachine();
    }

    private void UpdateStateMachine()
    {
        var context = new NpcFSMContext(Time.deltaTime)
        {
            velocity = MovementComponent.Velocity,
            executeAction = ExecuteAction,
            shouldMove = ShouldMove
        };
        
        stateMachine.Transition(context);
        stateMachine.Update(context);
        stateMachine.PostUpdate(context);
    }

    private void UpdateAnimations()
    {
        
    }

    private bool CanMove()
    {
        return !HasAction || (HasAction && !Action.actionData.waitForTarget);
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
