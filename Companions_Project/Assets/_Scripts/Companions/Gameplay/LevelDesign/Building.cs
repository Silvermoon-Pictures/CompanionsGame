using Silvermoon.Core;
using UnityEngine;

public class Building : MonoBehaviour, ICoreComponent
{
    public GameObject secondPhase;

    public void Upgrade()
    {
        secondPhase.SetActive(true);
    }
}
