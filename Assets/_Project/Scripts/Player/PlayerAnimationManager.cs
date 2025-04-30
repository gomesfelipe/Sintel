using UnityEngine;
using Unify.UnifiedRenderer;
public class PlayerAnimationManager : MonoBehaviour
{
    [SerializeField] protected AudioSource _audioSource;
    private TorchController torchController;
    [SerializeField] protected Color baseTorchColor, burnedTorchColor;
    [SerializeField] private UnifiedRenderer torchRenderer;
    private void Start()
    {
        torchController ??= GetComponentInParent<TorchController>();
    }
    #region  Animation Events
    public void GrabTorch()
    {
        if (torchRenderer != null)
        {
            torchRenderer.TrySetPropertyValue("_BaseColor", baseTorchColor);
            torchRenderer.ApplyPropertiesToBlock(); // <- FORÇA atualização visual
        }

        if (torchController != null && torchController.IsTorchActive())
        {
            torchController.UpdateTorchVisual(true);
        }
    }

    public void SetTorchOnFire()
    {
        if (torchRenderer != null)
        {
            torchRenderer.TrySetPropertyValue("_BaseColor", burnedTorchColor);
            torchRenderer.ApplyPropertiesToBlock();
        }

        if (torchController != null)
        {
            torchController.StartConsumingFuel();
        }
    }
    #endregion
}
