using UnityEngine;
using System;

public class TorchController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] protected PlayerManager _manager;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject torchVisual, flamesVisual;
    [SerializeField] private InputHandler inputHandler;

    [Header("Torch Settings")]
    public bool hasTorch = false, torchActive = false;
    private bool isConsumingFuel = false;
    public float maxFuel = 100f, fuelConsumptionRate = 5f, attackFuelCost = 10f, burnEffectiveness = 1f;
    private float currentFuel;
    public event Action OnTorchActivated, OnTorchDeactivated;

    private void Awake()
    {
        inputHandler ??= GetComponent<InputHandler>();
        animator ??= GetComponentInChildren<Animator>();
        currentFuel = maxFuel;
        UpdateTorchVisual(false);
    }

    private void Update()
    {
        if (hasTorch && inputHandler.EquipWeapon)
        {
            ToggleTorch();
            if (torchActive)
            {
                UpdateTorchVisual(false);
                UpdateAnimator();
            }   
        }
    }
    private void LateUpdate()
    {
        if (torchActive && isConsumingFuel)
        {
            ConsumeFuelOverTime();
        }
    }
    private void SetTorchState(bool active)
    {
        torchActive = active;
        UpdateTorchVisual(active);
        UpdateAnimator();

        if (active)
            OnTorchActivated?.Invoke();
        else
            OnTorchDeactivated?.Invoke();
    }

    private void ToggleTorch()
    {
        if (!torchActive && currentFuel > 0)
        {
            SetTorchState(true);
        }
        else if (torchActive)
        {
            SetTorchState(false);
        }
        else
        {
            Debug.Log("No fuel to activate the torch!");
        }
    }

    public void UpdateTorchVisual(bool visible)
    {
        if (torchVisual != null)
            torchVisual.SetActive(visible);
    }

    private void UpdateAnimator()
    {
        if (animator != null)
        {
            animator.SetBool(AnimatorParams.HoldingTorch, torchActive);
        }
    }
    private void ConsumeFuelOverTime()
    {
        currentFuel -= fuelConsumptionRate * Time.deltaTime;
        burnEffectiveness = Mathf.Clamp01(currentFuel / maxFuel);

        if (currentFuel <= 0)
        {
            currentFuel = 0;
            SetTorchState(false);
            StopConsumingFuel();
        }
    }
    public void TorchAttack()
    {
        if (!torchActive) return;

        if (currentFuel >= attackFuelCost)
        {
            currentFuel -= attackFuelCost;
            animator?.SetTrigger(AnimatorParams.AttackTorch);
            Debug.Log("Torch stab done!");
        }
        else
        {
            Debug.Log("No fuel to realize a torch stab!");
        }
    }

    public void BurnObject(GameObject target)
    {
        if (!torchActive || currentFuel <= 0) return;

        if (target.TryGetComponent<IBurnable>(out var burnable))
        {
            burnable.StartBurn();
            Debug.Log("Objeto comecou a queimar pela tocha!");
        }
        else
        {
            Debug.Log("The object can't burn.");
        }
    }

    public void Refuel(float amount)
    {
        currentFuel = Mathf.Clamp(currentFuel + amount, 0, maxFuel);
    }
    public void StartConsumingFuel()
    {
        isConsumingFuel = true;
        flamesVisual.SetActive(true);
    }

    public void StopConsumingFuel()
    {
        isConsumingFuel = false;
        flamesVisual.SetActive(false);
    }
    public bool IsTorchActive()
    {
        return torchActive;
    }
    public bool HasFuel()
    {
        return currentFuel > 0;
    }
}
