using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("References")]
    public Rigidbody Rigidbody { get; private set; }
    public Animator Animator { get; private set; }
    public CapsuleCollider Collider { get; private set; }
    public InputHandler InputHandler { get; private set; }

    [Header("States")]
    public bool IsGrounded { get; set; }
    public bool IsCrouching { get; set; }
    public bool IsSprinting => InputHandler?.SprintHeld == true;
    public bool IsChargingAttack => InputHandler?.FireCharging == true;

    public Vector2 MoveInput => InputHandler?.Move ?? Vector2.zero;
    public Vector2 LookInput => InputHandler?.Look ?? Vector2.zero;

    private void Awake()
    {
        Rigidbody ??= GetComponent<Rigidbody>();
        Animator ??= GetComponentInChildren<Animator>();
        Collider ??= GetComponent<CapsuleCollider>();
        InputHandler ??= GetComponent<InputHandler>();
    }
}
