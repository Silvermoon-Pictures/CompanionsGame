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
        navMeshAgent.updateRotation = false;

        walkingMoveState = new WalkingMoveState(MovementComponent, navMeshAgent);

        List<State> states = new()
        {
            walkingMoveState,
            new IdleMoveState(MovementComponent)
        };
        
        MovementComponent.Initialize(states);
    }

    public void UpdateDestination(Vector3 position)
    {
        walkingMoveState.UpdateDestination(position);
    }
    
    public void StopMoving()
    {
        walkingMoveState.StopMoving();
    }
}
