using System;
using Silvermoon.Movement;
using Silvermoon.Utils;
using UnityEngine;

public class StaminaComponent : MonoBehaviour
{
    public float maxStamina;
    public float depletionRate = 1f;
    public float increaseRate = 1f;

    [ReadOnly]
    public float stamina;
    
    private MovementComponent movementComponent;
    private bool IsMoving => movementComponent.Velocity.WithY(0f).magnitude > float.Epsilon;

    private void OnEnable()
    {
        movementComponent = GetComponent<MovementComponent>();
        stamina = maxStamina;
    }

    private void Update()
    {
        UpdateStamina();
    }

    private void UpdateStamina()
    {
        if (IsMoving)
            stamina -= depletionRate * Time.deltaTime;
        else
            stamina += increaseRate * Time.deltaTime;

        stamina = Mathf.Clamp(stamina, 0f, maxStamina);
    }

    public float GetPercentage()
    {
        return stamina / maxStamina;
    }
}
