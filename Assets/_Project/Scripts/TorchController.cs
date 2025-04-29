using UnityEngine;

public class TorchController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject torchVisual;
    [SerializeField] private InputHandler inputHandler;
    [SerializeField] private Animator animator;

    [Header("Torch Settings")]
    public bool hasTorch = false;
    public bool torchActive = false;
    public float maxFuel = 100f, fuelConsumptionRate = 5f, attackFuelCost = 10f, burnEffectiveness = 1f;

    private float currentFuel;

    private void Awake()
    {
        inputHandler ??= GetComponent<InputHandler>();
        animator ??= GetComponentInChildren<Animator>();
        currentFuel = maxFuel;
        UpdateTorchVisual();
    }

    private void Update()
    {
        if (hasTorch && torchActive && inputHandler.FirePressed)
        {
            ToggleTorch();
            inputHandler.ClearFire();
        }


    }
    private void LateUpdate()
    {
        if (torchActive)
        {
            ConsumeFuelOverTime();
        }
    }

    private void ToggleTorch()
    {
        if (!torchActive)
        {
            if (currentFuel > 0)
            {
                torchActive = true;
                UpdateTorchVisual();
            }
            else
            {
                Debug.Log("No fuel to active the torch!");
            }
        }
        else
        {
            torchActive = false;
            UpdateTorchVisual();
        }

        UpdateAnimator();
    }

    private void UpdateTorchVisual()
    {
        if (torchVisual != null)
            torchVisual.SetActive(torchActive);
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
            torchActive = false;
            UpdateTorchVisual();
            UpdateAnimator();
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
            Debug.Log("Objeto começou a queimar pela tocha!");
        }
        else
        {
            Debug.Log("O objeto não é queimável.");
        }
    }


    public void Refuel(float amount)
    {
        currentFuel = Mathf.Clamp(currentFuel + amount, 0, maxFuel);
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
