using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

[RequireComponent(typeof(CapsuleCollider), typeof(Rigidbody))]
public class PlayerLocomotion : MonoBehaviour
{
    [SerializeField] protected InputHandler _inputHandler;
    protected Camera _camera;
    protected Rigidbody _rb;
    [SerializeField] protected Animator _anim;
    private CapsuleCollider _collider;
    public Vector2 _move, _look;
    protected Vector3 moveDirection = Vector3.zero;
    public float rotationPower = 3f, rotationLerp = 0.5f;
    public float moveSpeed = 5f, sprintSpeed = 8f, rotationSpeed = 500f;
    public GameObject followTransform;

    private bool isCrouching = false; // Agachamento
    private float originalColliderHeight; // Altura original do collider
    private Vector3 originalColliderCenter; // Centro original do collider

    public float jumpForce = 5f;
    private bool isGrounded;
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.2f;

    private bool isSprinting = false; 
    private void Awake()
    {
        _inputHandler ??= GetComponent<InputHandler>();
        _camera ??= Camera.main;
        _rb ??= GetComponent<Rigidbody>();
        _anim ??= GetComponentInChildren<Animator>();
        _collider ??= GetComponent<CapsuleCollider>();
        originalColliderHeight = _collider.height;
        originalColliderCenter = _collider.center;
    }
    private void FixedUpdate()
    {
        HandleActions();
        CheckGroundStatus();
    }
    protected void HandleActions()
    {
        HandleMovement();
        HandleRotation();
        HandleJump();
        if (_inputHandler.CrouchPressed)
        {
            ToggleCrouch();
        }

    }

    private void HandleMovement()
    {
        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;

        if (_inputHandler.Move.sqrMagnitude < 0.01)
        {
            moveDirection = Vector3.zero;
            _anim.SetFloat(AnimatorParams.Speed, 0); // Idle
            return;
        }

        Vector3 forward = _camera.transform.forward;
        Vector3 right = _camera.transform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        moveDirection = ((forward * _inputHandler.Move.y) + (right * _inputHandler.Move.x)).normalized;

        _rb.MovePosition(_rb.position + (currentSpeed * Time.fixedDeltaTime * moveDirection));

        if (moveDirection != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            _rb.rotation = Quaternion.RotateTowards(_rb.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }
        _anim.SetFloat(AnimatorParams.Speed, isSprinting ? 1.0f : (_inputHandler.Move.sqrMagnitude > 0 ? 0.5f : 0f)); // 1 to sprint, 0.5 to walk
    }

    private void HandleRotation()
    {
        if (moveDirection == Vector3.zero) return;
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        _rb.rotation = Quaternion.Slerp(_rb.rotation, targetRotation, rotationLerp * Time.fixedDeltaTime);
    }

    private void HandleJump()
    {
        if (_inputHandler.JumpPressed && isGrounded)
        {
            _anim.SetTrigger(AnimatorParams.Jumping);
            _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
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
        _anim.SetBool(AnimatorParams.IsGrounded, isGrounded);
    }
}
