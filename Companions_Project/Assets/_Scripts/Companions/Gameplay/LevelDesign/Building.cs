using System.Collections.Generic;
using Silvermoon.Core;
using UnityEngine;

public class Building : MonoBehaviour, ICoreComponent
{
    public List<GameObject> phases = new();

    private int currentPhase = 0;
    
    public void Upgrade()
    {
        if (currentPhase == phases.Count)
            return;

        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
        
        phases[currentPhase].SetActive(true);
        currentPhase++;
    }
}
