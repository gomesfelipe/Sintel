using UnityEngine;

public static class AnimatorParams
{
    public static readonly int Speed = Animator.StringToHash("Speed");
    public static readonly int Jumping = Animator.StringToHash("Jumping");
    public static readonly int IsGrounded = Animator.StringToHash("IsGrounded");
    public static readonly int IsCrouching = Animator.StringToHash("IsCrouching");
}
