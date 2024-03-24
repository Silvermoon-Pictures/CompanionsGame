using System;
using System.Collections.Generic;
using UnityEngine;

namespace Silvermoon.Movement
{
    public abstract class PriorityStateMachine
{
    public State CurrentState { get; private set; }

    private List<State> states;
    
    public void Update(StateMachineContext context)
    {
        CurrentState.Update(context);
    }

    public void PostUpdate(StateMachineContext context)
    {
        CurrentState.PostUpdate(context);
    }

    public void Transition(StateMachineContext context)
    {
        foreach (var state in states)
        {
            if (state == CurrentState)
            {
                if (!CurrentState.CanExit(context))
                    break;
                 
                continue;
            }
                
            if (!state.CanEnter(context)) 
                continue;
            
            CurrentState.OnExit(context);
            state.OnEnter(context);
            CurrentState = state;
            break;
        }
    }
    
    public PriorityStateMachine(List<State> states)
    {
        this.states = states;
        CurrentState = states[^1];
    }
}

public abstract class State
{
    public abstract bool CanEnter(StateMachineContext context);
    public abstract bool CanExit(StateMachineContext context);
    public virtual void OnEnter(StateMachineContext context) { }
    public virtual void OnExit(StateMachineContext context) { }
    public abstract void Update(StateMachineContext context);
    public virtual void PostUpdate(StateMachineContext context) { }
}

public abstract class StateMachineContext
{
    public StateMachineContext Empty => new NullContext();
    public float dt;

    public StateMachineContext(float deltaTime)
    {
        this.dt = deltaTime;
    }
}

public class NullContext : StateMachineContext
{
    public NullContext() : base(-1f) { }
}
}
