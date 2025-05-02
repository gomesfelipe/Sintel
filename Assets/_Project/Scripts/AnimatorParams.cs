using UnityEngine;

public static class AnimatorParams
{
    public static readonly int Speed = Animator.StringToHash("Speed");
    public static readonly int Jumping = Animator.StringToHash("Jumping");
    public static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
    public static readonly int IsCrouching = Animator.StringToHash("IsCrouching");
    public static readonly int AttackTrigger = Animator.StringToHash("Attack");
    public static readonly int IsBlocking = Animator.StringToHash("IsBlocking");
    public static readonly int IsHanging = Animator.StringToHash("IsHanging");
    public static readonly int HoldingTorch = Animator.StringToHash("HoldingTorch");
    public static readonly int AttackTorch = Animator.StringToHash("TorchAttack");
}
