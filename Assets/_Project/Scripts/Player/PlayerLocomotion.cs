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
    public bool CanMove = true;
    protected Vector3 moveDirection = Vector3.zero;
    public float rotationPower = 3f, rotationLerp = 0.5f;
    public float moveSpeed = 5f, crouchSpeed=2f, sprintSpeed = 8f, rotationSpeed = 500f;
    public GameObject followTransform;

    private bool isSprinting = false, isCrouching = false;
    private bool crouchToggledThisFrame = false;
    private float originalColliderHeight;
    [SerializeField] private float crouchedHeight = 0.875f;
    [SerializeField] private float crouchedCenterY = 0.4425f;
    private Vector3 originalColliderCenter;
    private Coroutine crouchCoroutine;

    public float jumpForce = 5f, groundCheckDistance = 0.2f;
    private bool isGrounded;
    public LayerMask groundLayer;

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
        if (_inputHandler.CrouchPressed && !crouchToggledThisFrame)
        {
            ToggleCrouch();
            crouchToggledThisFrame = true;
            _inputHandler.ClearCrouch();
        }
    }

    private void HandleMovement()
    {
        if (!CanMove) { return; }
        float currentSpeed = isSprinting ? sprintSpeed : isCrouching ? crouchSpeed : moveSpeed;

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
        _anim.SetFloat(AnimatorParams.Speed, isSprinting ? 1.0f : (_inputHandler.Move.sqrMagnitude > 0 ? 0.5f : 0f)); // 1 to sprint, 0.5 to walk
    }

    private void HandleRotation()
    {
        if (moveDirection == Vector3.zero) return;
        Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
        _rb.rotation = Quaternion.RotateTowards(_rb.rotation, toRotation, rotationSpeed * Time.deltaTime);
    }

    private void HandleJump()
    {
        if (_inputHandler.JumpPressed && isGrounded)
        {
            _anim.SetTrigger(AnimatorParams.Jumping);
            _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            if (isCrouching) { isCrouching = !isCrouching; }
        }
    }

    private void ToggleCrouch()
    {
        isCrouching = !isCrouching;
        _anim.SetBool(AnimatorParams.IsCrouching, isCrouching);

        if (crouchCoroutine != null)
            StopCoroutine(crouchCoroutine);

        float targetHeight = isCrouching ? crouchedHeight : originalColliderHeight;
        Vector3 targetCenter = isCrouching
            ? new Vector3(originalColliderCenter.x, crouchedCenterY, originalColliderCenter.z)
            : originalColliderCenter;

        crouchCoroutine = StartCoroutine(SmoothCrouchTransition(_collider.height, targetHeight, _collider.center, targetCenter, 0.15f));
    }

    private IEnumerator SmoothCrouchTransition(float startHeight, float endHeight, Vector3 startCenter, Vector3 endCenter, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            _collider.height = Mathf.Lerp(startHeight, endHeight, t);
            _collider.center = Vector3.Lerp(startCenter, endCenter, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        _collider.height = endHeight;
        _collider.center = endCenter;
    }

    private void CheckGroundStatus()
    {
        if (_collider == null) return;

        float radius = _collider.radius * 0.95f;
        float offset = 0.03f;

        // Base real do capsule considerando o centro e altura
        Vector3 capsuleBottom = transform.position + _collider.center - Vector3.up * (_collider.height / 2f - radius);
        Vector3 origin = capsuleBottom + Vector3.up * offset;

        // Faz o SphereCast
        bool hitGround = Physics.SphereCast(origin, radius, Vector3.down, out RaycastHit hit, groundCheckDistance + offset, groundLayer);

        Debug.DrawRay(origin, Vector3.down * (groundCheckDistance + offset), hitGround ? Color.green : Color.red);

        isGrounded = hitGround;
        _anim.SetBool(AnimatorParams.IsGrounded, isGrounded);
    }

}
