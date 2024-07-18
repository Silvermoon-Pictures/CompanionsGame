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
    [field: SerializeField]
    public NpcData NpcData { get; private set; }

    public List<GameObject> visualPrefabs;
        
    private NpcBrain brain;
    
    public MovementComponent MovementComponent { get; private set; }

    public NpcAction Action { get; private set; }
    private NpcFSM stateMachine;

    public bool HasAction => Action.actionData != null;
    public bool ShouldMove { get; set; }
    public bool ExecuteAction => HasAction && !ShouldMove && !WaitForTarget();

    internal NpcFSMContext stateMachineContext;

    private void Awake()
    {
        MovementComponent = GetComponent<MovementComponent>();
        stateMachineContext = new(0f)
        {
            animator = GetComponentInChildren<Animator>(),
            targetPosition = transform.position
        };
        
        stateMachine = NpcFSM.Make(this);
        
        SetupMovement();


        
        brain = new NpcBrain(this);
        Action = new NpcAction();

        GameObject visual = visualPrefabs[UnityEngine.Random.Range(0, visualPrefabs.Count)];
        Instantiate(visual, transform);
    }

    void Start()
    {
        if (NpcData == null)
            throw new DesignException($"NpcData on {name} is not set!");
        
        Decide();
    }

    public void Decide()
    {
        Action.Reset();
        
        var decisionData = brain.Decide();
        if (decisionData.action == null || (decisionData.target == null && !decisionData.randomPosition.HasValue))
            return;

        Action.actionData = decisionData.action;
        if (decisionData.target != null)
            Action.target = decisionData.target;
        else
            Action.randomPosition = decisionData.randomPosition;
    }

    private bool IsInTargetRange()
    {
        if (Action.target == null && !Action.randomPosition.HasValue)
            return false;
        
        float distance = Vector3.Distance(Action.TargetPosition, transform.position);
        return distance <= Action.actionData.Range;
    }

    public void GoTo(Vector3 destination, float stoppingDistance)
    {
        stateMachineContext.targetPosition = destination;
        stateMachineContext.stoppingDistance = stoppingDistance;
    }

    private bool WaitForTarget()
    {
        return Action.IsValid && Action.WaitForTarget && !IsInTargetRange();
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
        
    }

    public void Lift(LiftableComponent liftableComponent)
    {
        liftableComponent.transform.position = transform.position + 5 * transform.forward;
        liftableComponent.transform.SetParent(transform);
    }
}
