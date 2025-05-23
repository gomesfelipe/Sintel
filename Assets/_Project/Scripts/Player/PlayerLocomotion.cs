using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Rotwang.Sintel.Core.Player
{
    [RequireComponent(typeof(CapsuleCollider), typeof(Rigidbody))]
    public class PlayerLocomotion : MonoBehaviour
    {
        [SerializeField] protected InputHandler _inputHandler;
        protected Camera _camera;
        protected Rigidbody _rb;
        [SerializeField] protected Animator _anim;
        private CapsuleCollider _collider; 
        public GameObject followTransform;
        [SerializeField] private Transform meshTransform;
        public bool CanMove = true;
        protected Vector3 moveDirection = Vector3.zero;
        public float moveSpeed = 5f, crouchSpeed=2f, sprintSpeed = 8f, rotationSpeed = 500f, rotationPower = 3f, rotationLerp = 0.5f;

        [SerializeField] private float turnInPlaceThreshold = 15f, turnInPlaceInterpSpeed = 10f, maxTurnInPlaceAngle = 120f, turnInPlaceResetSpeed = 5f;

        [SerializeField] private bool isSprinting = false, isCrouching = false, isHanging = false, crouchToggledThisFrame = false;
        private float originalColliderHeight;
        [SerializeField] private float crouchedHeight = 0.875f, crouchedCenterY = 0.4425f;
        private Vector3 originalColliderCenter, originalMeshLocalPosition;
        private Quaternion originalMeshRotation;
        private Coroutine crouchCoroutine;

        public float jumpForce = 5f, groundCheckDistance = 0.2f;
        protected bool isGrounded;
        public bool IsGrounded => isGrounded;
        public int SurfaceType { get; private set; } = 0; // 0 = indefinido, 1 = grass, 2 = wood

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
            originalMeshLocalPosition = meshTransform.localPosition;
            originalMeshRotation = meshTransform.localRotation;
        }
        private void FixedUpdate()
        {
            HandleActions();
            CheckGroundStatus();
            HandleTurnInPlace();
        }
        #region Input Handling
        protected void HandleActions()
        {
            HandleMovement();
            HandleRotation();
            HandleJump();
            HandleCrouchInput();
        }
        private void HandleCrouchInput()
        {
            if (_inputHandler.CrouchPressed && !crouchToggledThisFrame)
            {
                ToggleCrouch();
                crouchToggledThisFrame = true;
                _inputHandler.ClearCrouch();
            }
        }
        #endregion
        #region Movement
        private void HandleTurnInPlace()
        {
            if (_inputHandler.Move.sqrMagnitude > 0.01f || !isGrounded || !CanMove || isHanging)
            {
                _anim.SetFloat(AnimatorParams.TurnInPlaceDeg, 0f);
                return;
            }

            float angleDeg = Mathf.DeltaAngle(transform.eulerAngles.y, _camera.transform.eulerAngles.y);
            float angleNormalized = Mathf.Clamp(angleDeg / 180f, -1f, 1f);

            float targetAngle = Mathf.Abs(angleDeg) > turnInPlaceThreshold ? angleNormalized : 0f;

            // Suaviza o valor atual da anima��o
            float currentTurn = _anim.GetFloat(AnimatorParams.TurnInPlaceDeg);
            float newTurn = Mathf.Lerp(currentTurn, targetAngle, Time.deltaTime * turnInPlaceInterpSpeed);

            _anim.SetFloat(AnimatorParams.TurnInPlaceDeg, newTurn);
        }

        private void HandleMovement()
        {
            if (!CanMove) return;

            float currentSpeed = isSprinting ? sprintSpeed : isCrouching ? crouchSpeed : moveSpeed;

            if (_inputHandler.Move.sqrMagnitude < 0.01f)
            {
                moveDirection = Vector3.zero;
                _anim.SetFloat(AnimatorParams.Speed, 0);
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
            _anim.SetFloat(AnimatorParams.Speed, isSprinting ? 1.0f : (_inputHandler.Move.sqrMagnitude > 0 ? 0.5f : 0f));
        }

        private void HandleRotation()
        {
            if (moveDirection == Vector3.zero || isHanging || !CanMove) return;
            Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            _rb.rotation = Quaternion.RotateTowards(_rb.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }
        #endregion

        private void HandleJump()
        {
                if (_inputHandler.JumpPressed && (isGrounded || Mathf.Approximately(_rb.linearVelocity.y, 0)))
                {
                    if (isHanging)
                    {
                        _rb.useGravity = true;
                        isHanging = false;
                        _anim.SetBool(AnimatorParams.IsHanging, isHanging);
                    //meshTransform.localPosition = originalMeshLocalPosition;
                    meshTransform.localRotation = originalMeshRotation;
                    _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, jumpForce, _rb.linearVelocity.z);
                        StartCoroutine(EnableCanMove(0.25f));
                        if (crouchCoroutine != null)
                                StopCoroutine(crouchCoroutine);
                            crouchCoroutine = StartCoroutine(SmoothCrouchTransition(_collider.height, originalColliderHeight, _collider.center, originalColliderCenter, 0.15f));
                    }
                    else
                    {
                        _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, jumpForce, _rb.linearVelocity.z);
                    }
                    if (isCrouching) { isCrouching = !isCrouching; }
                }

                if (Mathf.Abs(_rb.linearVelocity.x) + Mathf.Abs(_rb.linearVelocity.z) > 0.1f) transform.forward = new Vector3(_rb.linearVelocity.x, 0, _rb.linearVelocity.z);
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
    private IEnumerator EnableCanMove(float watiTime)
        {
            yield return new WaitForSeconds(watiTime);
            CanMove = true;
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
        if (hitGround)
        {
                string tag = hit.collider.tag;
            if (tag == "Grass") SurfaceType = 1;
            else if (tag == "Wood") SurfaceType = 2;
            else SurfaceType = 0;
        }
        Debug.DrawRay(origin, Vector3.down * (groundCheckDistance + offset), hitGround ? Color.green : Color.red);

        isGrounded = hitGround;
        _anim.SetBool(AnimatorParams.IsGrounded, isGrounded);
    }
}
}
