using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace Rotwang.Sintel.Core.Player
{
    [RequireComponent(typeof(CapsuleCollider), typeof(Rigidbody))]
    public class PlayerLedgeGrab : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private PlayerLocomotion locomotion;
        [SerializeField] private Animator _anim;
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private CapsuleCollider capsule;
        [SerializeField] private Transform meshTransform;
        [SerializeField] private LayerMask groundLayer;

        [Header("Ledge Grab Settings")]
        [SerializeField] private float hangOffsetY = -1.5f;
        [SerializeField] private float hangOffsetZ = -0.1f;
        [SerializeField] private float grabCheckHeightTop = 1.5f;
        [SerializeField] private float grabCheckHeightBottom = 0.7f;

        private Vector3 originalColliderCenter;
        private float originalColliderHeight;
        private Quaternion originalMeshRotation;
        private Coroutine hangingCoroutine;

        private bool isHanging;
        public bool IsHanging => isHanging;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _anim ??= GetComponentInChildren<Animator>();
            capsule = capsule ?? GetComponent<CapsuleCollider>();
            locomotion = locomotion ?? GetComponent<PlayerLocomotion>();

            originalColliderHeight = capsule.height;
            originalColliderCenter = capsule.center;
            originalMeshRotation = meshTransform.localRotation;
        }

        private void FixedUpdate()
        {
            HandleGrab();
        }
        private void HandleGrab()
        {
            if (_rb.linearVelocity.y < 0 && !isHanging)
            {
                Vector3 lineDownStart = (transform.position + Vector3.up * 1.5f) + transform.forward;
                Vector3 LineDownEnd = (transform.position + Vector3.up * 0.7f) + transform.forward;
                Physics.Linecast(lineDownStart, LineDownEnd, out RaycastHit downHit, groundLayer);
                Debug.DrawLine(lineDownStart, LineDownEnd);

                if (downHit.collider != null)
                {
                    Vector3 lineFwdStart = new(transform.position.x, downHit.point.y - 0.1f, transform.position.z);
                    Vector3 LineFwdEnd = new Vector3(transform.position.x, downHit.point.y - 0.1f, transform.position.z) + transform.forward;
                    Physics.Linecast(lineFwdStart, LineFwdEnd, out RaycastHit fwdHit, groundLayer);
                    Debug.DrawLine(lineFwdStart, LineFwdEnd);

                    if (fwdHit.collider != null)
                    {
                        _rb.useGravity = false;
                        _rb.linearVelocity = Vector3.zero;

                        isHanging = true;
                        _anim.SetBool(AnimatorParams.IsHanging, isHanging);
                        Vector3 hangPos = new(fwdHit.point.x, downHit.point.y, fwdHit.point.z);
                        Vector3 offset = transform.forward * hangOffsetZ + transform.up * hangOffsetY;
                        hangPos += offset;
                        transform.position = hangPos;
                        transform.forward = -fwdHit.normal;
                        locomotion.CanMove = false;
                    }
                }
            }
        }
    }
}
