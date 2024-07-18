using Silvermoon.Movement;
using Silvermoon.Utils;
using UnityEngine;
using System;

public class StaminaComponent : MonoBehaviour
{
    public float maxStamina = 100f;
    [ReadOnly]
    public float currentStamina = 100f;
    public float depletionRate = 5f;
    public float regenerationRate = 10f;
    public float exhaustionThreshold = 20f;
    public float coolOffThreshold = 40f;
    public bool depleteWhenMoving = false;
    private bool IsMoving => movementComponent.Velocity.WithY(0).magnitude > 0f;
    private bool CanDeplete => (depleteWhenMoving && IsMoving) || depletionTriggered;

    private MovementComponent movementComponent;

    public event Action onStaminaDepleted;
    public event Action OnStaminaRecovered;

    private bool isExhausted = false;
    private bool depletionTriggered = false;

    private void Awake()
    {
        movementComponent = GetComponent<MovementComponent>();
    }

    private void Update()
    {
        UpdateStamina();
    }

    private void UpdateStamina()
    {
        if (CanDeplete)
        {
            currentStamina -= depletionRate * Time.deltaTime;
        }
        else
        {
            currentStamina += regenerationRate * Time.deltaTime;
        }

        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
    }

    private void DepleteStamina()
    {
        currentStamina -= depletionRate * Time.deltaTime;

        if (currentStamina <= exhaustionThreshold)
        {
            isExhausted = true;
        }

        if (currentStamina <= 0)
        {
            currentStamina = 0;
            onStaminaDepleted?.Invoke();
        }
    }

    private void RegenerateStamina()
    {
        currentStamina += regenerationRate * Time.deltaTime;

        if (currentStamina >= coolOffThreshold && isExhausted)
        {
            isExhausted = false;
            OnStaminaRecovered?.Invoke();
        }
    }

    private void ClampStamina()
    {
        
    }

    public float GetPercentage()
    {
        return currentStamina / maxStamina;
    }

    public void StartDepleting()
    {
        depletionTriggered = true;
    }

    public void StopDepleting()
    {
        depletionTriggered = false;
    }

    public void ModifyDepletionRate(float newRate)
    {
        depletionRate = newRate;
    }

    public void ModifyRegenerationRate(float newRate)
    {
        regenerationRate = newRate;
    }
}
