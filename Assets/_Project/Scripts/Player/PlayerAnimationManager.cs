using UnityEngine;

public class PlayerAnimationManager : MonoBehaviour
{
    private TorchController torchController;
    private void Start()
    {
        torchController ??= GetComponentInParent<TorchController>();
    }
    #region  Animation Events
    public void GrabTorch()
    {
        if (torchController != null && torchController.IsTorchActive())
        {
            torchController.UpdateTorchVisual(true);
        }
    }

    public void SetTorchOnFire()
    {

    }
    #endregion
}
