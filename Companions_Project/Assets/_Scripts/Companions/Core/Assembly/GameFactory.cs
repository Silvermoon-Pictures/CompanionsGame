using System;
using System.Collections;
using System.Collections.Generic;
using Silvermoon.Core;
using UnityEngine;
using Object = UnityEngine.Object;

public class GameFactory : IFactory
{
    private readonly GameContext context;

    public event EventHandler<GameObject> ObjectCreated;
    
    public GameFactory(GameContext context)
    {
        this.context = context;
    }

    public Queue<FactoryInstruction> instructions = new();

    public IEnumerator ProcessQueue()
    {
        while (instructions.Count > 0)
        {
            var instruction = instructions.Dequeue();
            GameObject obj = Object.Instantiate(instruction.prefab, instruction.position, instruction.rotation);
            ObjectCreated?.Invoke(this, obj);
            instruction.callback?.Invoke(obj);
        }

        yield break;
    }

    public void AddInstruction(FactoryInstruction instruction)
    {
        instructions.Enqueue(instruction);
    }
}
