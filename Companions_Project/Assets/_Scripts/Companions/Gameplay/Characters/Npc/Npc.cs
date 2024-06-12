using System;
using System.Collections.Generic;
using Companions.Common;
using Companions.StateMachine;
using Silvermoon.Core;
using Silvermoon.Movement;
using Silvermoon.Utils;
using UnityEngine;

public partial class Npc : MonoBehaviour, ITargetable, ICompanionComponent
{
    private static readonly int IsMovingParam = Animator.StringToHash("isMoving");
    
    [field: SerializeField]
    public NpcData NpcData { get; private set; }

    public List<GameObject> visualPrefabs;
        
    private NpcBrain brain;
    
    public MovementComponent MovementComponent { get; private set; }

    public NpcAction Action { get; private set; }
    private NpcFSM stateMachine;

    public bool HasAction { get; set; }
    public bool ShouldMove { get; set; }
    public bool ExecuteAction => HasAction && !ShouldMove && !WaitForTarget();

    private NpcFSMContext stateMachineContext;

    private Animator animator;

    private void Awake()
    {
        stateMachine = NpcFSM.Make(this);
        
        GameObject visual = visualPrefabs[UnityEngine.Random.Range(0, visualPrefabs.Count)];
        Instantiate(visual, transform);

        animator = GetComponentInChildren<Animator>();
        stateMachineContext = new(0f);
        stateMachineContext.animator = animator;
        
        brain = new NpcBrain(this);
        Action = new NpcAction();
        SetupMovement();
    }

    void ICompanionComponent.WorldLoaded()
    {
        if (NpcData == null)
            throw new DesignException($"NpcData on {name} is not set!");

        Decide();
    }

    public void Decide()
    {
        HasAction = false;
        Action.Reset();
        
        var decisionData = brain.Decide();
        if (decisionData.action == null || (decisionData.target == null && !decisionData.randomPosition.HasValue))
            return;

        Action.actionData = decisionData.action;
        if (decisionData.target != null)
            Action.target = ((Component)decisionData.target).gameObject;
        else
            Action.randomPosition = decisionData.randomPosition;

        HasAction = true;
        ShouldMove = !IsInTargetRange() && !Action.WaitForTarget;
    }

    private bool IsInTargetRange()
    {
        if (Action.target == null && !Action.randomPosition.HasValue)
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
        stateMachineContext.dt = Time.deltaTime;
        stateMachineContext.velocity = MovementComponent.Velocity;
        stateMachineContext.executeAction = ExecuteAction;
        stateMachineContext.shouldMove = ShouldMove;
        stateMachineContext.waitForTarget = WaitForTarget();
        
        stateMachine.Transition(stateMachineContext);
        stateMachine.Update(stateMachineContext);
        stateMachine.PostUpdate(stateMachineContext);
    }

    private void UpdateAnimations()
    {
        if (MovementComponent.Velocity.magnitude > 0)
        {
            animator.SetBool(IsMovingParam, true);
        }
        else
        {
            animator.SetBool(IsMovingParam, false);
        }
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
