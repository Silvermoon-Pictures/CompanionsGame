using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICompanionComponent { }

public interface ISystem  : ICompanionComponent
{
    void Initialize() { }
    void Cleanup() { }
}

public class EntitySystem : MonoBehaviour, ISystem
{
    
}
