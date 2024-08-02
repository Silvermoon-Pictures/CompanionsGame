using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(SplineContainer))]
public class PathComponent : MonoBehaviour
{
    private SplineContainer splinePath;
    private float length;
    public float Length => length;

    private void Awake()
    {
        splinePath = GetComponent<SplineContainer>();
        length = splinePath.CalculateLength();
    }

    public Vector3 GetPosition(float percentage)
    {
        var position = splinePath.EvaluatePosition(percentage);
        return new Vector3(position.x, position.y, position.z);
    }
}
