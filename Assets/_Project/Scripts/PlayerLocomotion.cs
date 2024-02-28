using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

[RequireComponent(typeof(CapsuleCollider),typeof(Rigidbody))] 
public class PlayerLocomotion : MonoBehaviour
{
    protected Rigidbody _rb;
    [SerializeField] protected Animator _anim;
    private CapsuleCollider _collider;
    public Vector2 _move, _look;
    protected Vector3 moveDirection = Vector3.zero;
    public float aimValue, fireValue;

    public float rotationPower = 3f, rotationLerp = 0.5f;

    public float moveSpeed = 5f, sprintSpeed = 8f, rotationSpeed = 500f, burstSpeed;
    public GameObject followTransform;

    private bool isCrouching = false; // Agachamento
    private float originalColliderHeight; // Altura original do collider
    private Vector3 originalColliderCenter; // Centro original do collider

    public float jumpForce = 5f;
    private bool isGrounded;
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.2f;

    public GameObject projectile;
    private bool m_Charging;

    private bool isSprinting = false; // Controle do sprint
    private void Awake()
    {
        _rb ??= GetComponent<Rigidbody>();
        _anim = GetComponentInChildren<Animator>();
        _collider ??= GetComponent<CapsuleCollider>();
        originalColliderHeight = _collider.height;
        originalColliderCenter = _collider.center;
    }
    private void FixedUpdate()
    {
        HandleMovement();
        HandleRotation();
        CheckGroundStatus();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _move = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        _look = context.ReadValue<Vector2>();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        isSprinting = context.performed;
        //_anim.SetBool("IsSprinting", isSprinting);
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded)
        {
            _anim.SetTrigger("Jumping");
            _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ToggleCrouch();
        }
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                if (context.interaction is SlowTapInteraction)
                {
                    StartCoroutine(BurstFire((int)(context.duration * burstSpeed)));
                }
                else
                {
                    Fire();
                }
                m_Charging = false;
                break;

            case InputActionPhase.Started:
                if (context.interaction is SlowTapInteraction)
                    m_Charging = true;
                break;

            case InputActionPhase.Canceled:
                m_Charging = false;
                break;
        }
    }

    public void OnGUI()
    {
        if (m_Charging)
            GUI.Label(new Rect(100, 100, 200, 100), "Charging...");
    }


    private void HandleMovement()
    {
        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;

        if (_move.sqrMagnitude < 0.01)
        {
            moveDirection = Vector3.zero;
            _anim.SetFloat("Speed", 0); // Idle
            return;
        }

        Vector3 forward = Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        moveDirection = ((forward * _move.y) + (right * _move.x)).normalized;

        _rb.MovePosition(_rb.position + (currentSpeed * Time.fixedDeltaTime * moveDirection));

        if (moveDirection != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            _rb.rotation = Quaternion.RotateTowards(_rb.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }

        // Ajusta o valor de Speed no Animator com base no estado de sprint
        _anim.SetFloat("Speed", isSprinting ? 1.0f : (_move.sqrMagnitude > 0 ? 0.5f : 0f)); // 1 para sprint, 0.5 para andar
    }

    private void HandleRotation()
    {
        if (moveDirection == Vector3.zero) return; // Não rotaciona se não há direção de movimento

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        _rb.rotation = Quaternion.Slerp(_rb.rotation, targetRotation, rotationLerp * Time.fixedDeltaTime);
    }

    private void ToggleCrouch()
    {
        isCrouching = !isCrouching;
        _anim.SetBool("IsCrouching", isCrouching);

        if (isCrouching)
        {
            // Reduzir a altura do collider pela metade ao agachar
            _collider.height = originalColliderHeight / 2;
            _collider.center = new Vector3(originalColliderCenter.x, originalColliderCenter.y / 2, originalColliderCenter.z);
        }
        else
        {
            // Restaurar a altura original do collider ao levantar
            _collider.height = originalColliderHeight;
            _collider.center = originalColliderCenter;
        }
    }

    private void CheckGroundStatus()
    {
        Vector3 rayStart = transform.position + Vector3.up * 0.1f; // Ajusta levemente para cima para evitar colisão com o próprio jogador
        bool hitGround = Physics.Raycast(rayStart, -Vector3.up, out RaycastHit hit, groundCheckDistance, groundLayer);
        Debug.DrawRay(rayStart, -Vector3.up * groundCheckDistance, hitGround ? Color.green : Color.red);

        isGrounded = hitGround;
        _anim.SetBool("IsGrounded", isGrounded);
    }


    private IEnumerator BurstFire(int burstAmount)
    {
        for (var i = 0; i < burstAmount; ++i)
        {
            Fire();
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void Fire()
    {

    }
}
