using System;
using System.Collections;
using UnityEngine;

namespace Silvermoon.Core
{
    public interface IFactory
    {
        void ProcessQueue();
        void AddInstruction(FactoryInstruction instruction);
    }

    public class FactoryInstruction
    {
        public GameObject prefab;
        public Vector3 position;
        public Quaternion rotation;

        public Action<GameObject> callback;
    
        public FactoryInstruction(GameObject prefab, Vector3 position, Quaternion rotation, Action<GameObject> callback = null)
        {
            this.prefab = prefab;
            this.position = position;
            this.rotation = rotation;
            this.callback = callback;
        }
    };

}
