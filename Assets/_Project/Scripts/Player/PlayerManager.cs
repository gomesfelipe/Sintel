using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Rotwang.Sintel.Core.Player
{
    public class PlayerManager : MonoBehaviour
    {
        [Header("References")]
        public Rigidbody Rigidbody { get; private set; }
        public Animator Animator { get; private set; }
        public CapsuleCollider Collider { get; private set; }
        public InputHandler InputHandler { get; private set; }
        public TorchController TorchController { get; private set; }
        public Rig rig;
        public TwoBoneIKConstraint leftHandIK, rightHandIK;
        public Transform leftHandIKTarget, rightHandIKTarget;

        [Header("Interaction")]
        public float interactionDistance = 2f;
        public float capsuleRadius = 0.4f;
        public LayerMask interactableLayer;
        private IInteractable currentTarget;
        private Vector3 targetPoint;
        private Transform _playerTransform;

        [Header("States")]
        public bool IsGrounded { get; set; }
        public bool IsCrouching { get; set; }
        public bool IsSprinting => InputHandler?.SprintHeld == true;
        public bool IsChargingAttack => InputHandler?.FireCharging == true;

        public Vector2 MoveInput => InputHandler?.Move ?? Vector2.zero;
        public Vector2 LookInput => InputHandler?.Look ?? Vector2.zero;

        [SerializeField] protected AnimationClip setObjOnFire;

        private void Awake()
        {
            Rigidbody ??= GetComponent<Rigidbody>();
            Animator ??= GetComponentInChildren<Animator>();
            Collider ??= GetComponent<CapsuleCollider>();
            InputHandler ??= GetComponent<InputHandler>();
            TorchController ??= GetComponent<TorchController>();
            _playerTransform = transform;
        }

        private void Update()
        {
            DetectInteractable();

            if (InputHandler != null && InputHandler.InteractPressed)
            {
                TryInteract();
            }
        }

        private void DetectInteractable()
        {
            Vector3 baseOrigin = _playerTransform.position + Vector3.up * 0.1f;
            Vector3 topOrigin = _playerTransform.position + Vector3.up * 1.5f;
            Vector3 direction = _playerTransform.forward;

            if (Physics.CapsuleCast(baseOrigin, topOrigin, capsuleRadius, direction, out RaycastHit hit, interactionDistance, interactableLayer))
            {
                currentTarget = hit.collider.GetComponent<IInteractable>();
                targetPoint = hit.point;
            }
            else
            {
                currentTarget = null;
            }
        }

        private void TryInteract()
        {
            if (currentTarget == null || InputHandler == null) return;

            if (currentTarget is IInteractable interactable)
            {
                if (interactable.RequiresHoldToInteract())
                {
                    interactable.OnInteractHold(InputHandler.InteractPressed);
                }
                else if (InputHandler.InteractPressedDown)
                {
                    interactable.OnInteract();
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying) return;

            if (currentTarget != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(targetPoint, 0.2f);
                Gizmos.DrawLine(_playerTransform.position + Vector3.up * 0.1f, targetPoint);
            }
        }

        public void PlayAnimation(AnimationClip animationClip)
        {
            Animator.CrossFade(animationClip.name, 0.2f);
        }
    }
}
