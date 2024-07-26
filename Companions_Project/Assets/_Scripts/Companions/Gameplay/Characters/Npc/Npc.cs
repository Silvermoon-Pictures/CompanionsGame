using System;
using System.Collections;
using System.Collections.Generic;
using Companions.Common;
using Companions.StateMachine;
using Silvermoon.Core;
using Silvermoon.Movement;
using Silvermoon.Utils;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public partial class Npc : MonoBehaviour, ITargetable, ICompanionComponent
{
    [field: SerializeField]
    public NpcData NpcData { get; private set; }
    
    private NpcBrain brain;
    
    public MovementComponent MovementComponent { get; private set; }

    public NpcAction Action => stateMachineContext.currentActionData;
    private NpcFSM stateMachine;

    public bool HasAction => stateMachineContext.currentActionData != null;

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
            stoppingDistance = 1f,
        };
        
        stateMachine = NpcFSM.Make(this);
        
        SetupMovement();
        
        brain = new NpcBrain(this);
    }

    void ICompanionComponent.WorldLoaded()
    {
        if (NpcData == null)
            throw new DesignException($"NpcData on {name} is not set!");
        
        Decide();
    }

    public ActionAsset Decide()
    {
        stateMachineContext.executeAction = false;
        var action = brain.Decide();
        if (action == null)
            return null;
        if (HasAction && action.name == Action.actionData.name)
            return null;

        stateMachineContext.previousActionData = Action;
        stateMachineContext.currentActionData = new NpcAction(action);
        
        stateMachineContext.executeAction = true;

        return action;
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
