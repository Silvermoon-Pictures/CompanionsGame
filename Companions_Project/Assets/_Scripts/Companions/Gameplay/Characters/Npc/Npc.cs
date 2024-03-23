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

    public NpcAction Action { get; private set; }
    private NpcFSM stateMachine;

    public bool HasAction { get; set; }
    public bool ShouldMove { get; set; }
    public bool ExecuteAction => HasAction && !ShouldMove && !WaitForTarget();

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
        if (newAction == null || newTarget == null)
            return;

        Action.actionData = newAction;
        Action.target = ((Component)newTarget).gameObject;

        HasAction = true;
        ShouldMove = !IsInTargetRange() && !Action.WaitForTarget;
    }

    private bool IsInTargetRange()
    {
        if (Action.target == null)
            return false;
        
        float distance = Vector3.Distance(Action.TargetPosition, transform.position);
        return distance <= Action.actionData.Range;
    }

    private bool WaitForTarget()
    {
        return Action.IsValid && Action.WaitForTarget && !IsInTargetRange();
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
            shouldMove = ShouldMove,
            waitForTarget = WaitForTarget()
        };
        
        stateMachine.Transition(context);
        stateMachine.Update(context);
        stateMachine.PostUpdate(context);
    }

    private void UpdateAnimations()
    {
        
    }

    public void Lift(LiftableComponent liftableComponent)
    {
        liftableComponent.transform.position = transform.position + 5 * transform.forward;
        liftableComponent.transform.SetParent(transform);
    }

    private void OnReachedTarget()
    {
        ShouldMove = false;
    }
}
