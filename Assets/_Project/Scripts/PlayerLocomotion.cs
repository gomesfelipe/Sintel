using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

[RequireComponent(typeof(CapsuleCollider),typeof(Rigidbody))] 
public class PlayerLocomotion : MonoBehaviour
{
    protected Rigidbody _rb;
    [SerializeField] protected Animator _anim;
    public Vector2 _move, _look;
    protected Vector3 moveDirection = Vector3.zero;
    public float aimValue, fireValue;

    public float rotationPower = 3f, rotationLerp = 0.5f;

    public float moveSpeed = 5f, rotationSpeed = 500f, burstSpeed;
    public GameObject followTransform;

    public GameObject projectile;

    private bool m_Charging;

    public float jumpForce = 5f; // Força do pulo
    private bool isGrounded; // Verifica se o jogador está no chão
    public LayerMask groundLayer; // Camada do chão para verificação
    public float groundCheckDistance = 0.2f; // Distância para verificar se está no chão

    private void Awake()
    {
        _rb ??= GetComponent<Rigidbody>();
        _anim = GetComponentInChildren<Animator>();
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

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded)
        {
            _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
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
        if (_move.x == 0 && _move.y == 0 || _move.sqrMagnitude < 0.01) 
        {
            moveDirection = Vector3.zero; // Garante que a direção do movimento é zerada quando não há entrada
            return;
        }
        Vector3 forward = Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;

        // Zero out the y component of the camera's forward and right vectors to keep the movement strictly horizontal
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        // Calcula a dire��o de movimento relativa � c�mera
        moveDirection = ((forward * _move.y) + (right * _move.x)).normalized;

        _rb.MovePosition(_rb.position + (moveSpeed * Time.fixedDeltaTime * moveDirection));

        if (moveDirection != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }

        _anim.SetFloat("Speed", moveDirection.magnitude);
    }

    private void HandleRotation()
    {
        if (moveDirection == Vector3.zero) return; // Não rotaciona se não há direção de movimento

        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        _rb.rotation = Quaternion.Slerp(_rb.rotation, targetRotation, rotationLerp * Time.fixedDeltaTime);
    }

    private void CheckGroundStatus()
    {
        isGrounded = Physics.Raycast(transform.position, -Vector3.up, groundCheckDistance, groundLayer);
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
        var transform = this.transform;
        var newProjectile = Instantiate(projectile);
        newProjectile.transform.position = transform.position + transform.forward * 0.6f;
        newProjectile.transform.rotation = transform.rotation;
        const int size = 1;
        newProjectile.transform.localScale *= size;
        newProjectile.GetComponent<Rigidbody>().mass = Mathf.Pow(size, 3);
        newProjectile.GetComponent<Rigidbody>().AddForce(transform.forward * 20f, ForceMode.Impulse);
        newProjectile.GetComponent<MeshRenderer>().material.color =
            new Color(Random.value, Random.value, Random.value, 1.0f);
    }
}
