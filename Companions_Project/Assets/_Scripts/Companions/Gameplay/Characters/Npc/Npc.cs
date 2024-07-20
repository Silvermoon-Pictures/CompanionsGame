using System;
using System.Collections.Generic;
using Companions.Common;
using Companions.StateMachine;
using Silvermoon.Core;
using Silvermoon.Movement;
using Silvermoon.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

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
    public bool ExecuteAction => HasAction;

    internal NpcFSMContext stateMachineContext;
    internal DictionaryComponent dictionaryComponent;

    private void Awake()
    {
        // GameObject visual = visualPrefabs[UnityEngine.Random.Range(0, visualPrefabs.Count)];
        // Instantiate(visual, transform);
        
        MovementComponent = GetComponent<MovementComponent>();
        dictionaryComponent = GetComponent<DictionaryComponent>();
        stateMachineContext = new(0f)
        {
            animator = GetComponentInChildren<Animator>(),
            targetPosition = transform.position,
            stoppingDistance = 1f
        };
        
        stateMachine = NpcFSM.Make(this);
        
        SetupMovement();
        
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
        var decisionData = brain.Decide();
        if (decisionData.action == null)
            return;
        if (HasAction && decisionData.action.name == Action.actionData.name)
            return;
        if (decisionData.target == null && !decisionData.randomPosition.HasValue)
            return;

        Action.Reset();
        Action.actionData = decisionData.action;
        Action.Subactions = new(Action.actionData.SubactionQueue);
        if (decisionData.target != null)
            Action.target = decisionData.target;
        else
            Action.randomPosition = decisionData.randomPosition;

        stateMachineContext.executeAction = true;
    }

    public void GoTo(Vector3 destination, float stoppingDistance)
    {
        stateMachineContext.targetPosition = destination;
        stateMachineContext.stoppingDistance = stoppingDistance;
    }

    private void Update()
    {
        UpdateAnimations();
        UpdateStateMachine();
    }

    private void UpdateStateMachine()
    {
        stateMachineContext.dt = Time.deltaTime;
        
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
