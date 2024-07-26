using System.Collections.Generic;
using Silvermoon.Movement;
using UnityEngine;
using UnityEngine.AI;

public partial class Npc
{
    private WalkingMoveState walkingMoveState;
    
    private void SetupMovement()
    {
        var navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.updatePosition = false;
        navMeshAgent.updateRotation = true;

        walkingMoveState = new WalkingMoveState(MovementComponent, navMeshAgent, stateMachineContext);

        List<State> states = new()
        {
            new RequestMoveState(MovementComponent, navMeshAgent),
            walkingMoveState,
            new IdleMoveState(MovementComponent)
        };
        
        MovementComponent.Initialize(states);
    }

    public void UpdateDestination(Vector3 position)
    {
        walkingMoveState.UpdateDestination(position);
    }
}
