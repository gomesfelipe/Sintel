using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class InputHandler : MonoBehaviour
{
    public Vector2 Move { get; private set; }
    public Vector2 Look { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool CrouchPressed { get; private set; }
    public bool SprintHeld { get; private set; }
    public bool FirePressed { get; private set; }
    public bool FireCharging { get; private set; }
    public float FireChargeDuration { get; private set; }
    public bool InteractPressed { get; private set; }
    public bool EquipWeapon { get; private set; }

    public void OnMove(InputAction.CallbackContext context) => Move = context.ReadValue<Vector2>();
    public void OnLook(InputAction.CallbackContext context) => Look = context.ReadValue<Vector2>();
    public void OnJump(InputAction.CallbackContext context) => JumpPressed = context.performed;
    public void OnCrouch(InputAction.CallbackContext context) => CrouchPressed = context.performed;
    public void OnSprint(InputAction.CallbackContext context) => SprintHeld = context.performed;

    public void OnInteract(InputAction.CallbackContext context) => InteractPressed = context.performed;
    public void OnEquipWeapon(InputAction.CallbackContext context) => EquipWeapon = context.performed;
    public void OnFire(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started:
                if (context.interaction is SlowTapInteraction)
                {
                    FireCharging = true;
                    FireChargeDuration = 0f;
                }
                else
                {
                    FirePressed = true;
                }
                break;

            case InputActionPhase.Performed:
                FirePressed = true;
                FireCharging = false;
                break;

            case InputActionPhase.Canceled:
                FireCharging = false;
                FirePressed = false;
                FireChargeDuration = 0f;
                break;
        }
    }

    public void ClearFire()
    {
        FirePressed = false;
    }
}
