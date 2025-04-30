using UnityEngine;
using Unify.UnifiedRenderer;

namespace Rotwang.Sintel.Core.Player
{
public class PlayerAnimationManager : MonoBehaviour
{
    [SerializeField] protected PlayerManager _manager;
    [SerializeField] private PlayerLocomotion playerLocomotion;
    private TorchController torchController;
    [SerializeField] protected Color baseTorchColor, burnedTorchColor;
    [SerializeField] private UnifiedRenderer torchRenderer;
    [SerializeField] protected AudioSource _audioSource;
    [SerializeField] private AudioClip[] grassClips;
    [SerializeField] private AudioClip[] woodClips;
    private void Start()
    {
        _manager ??= GetComponentInParent<PlayerManager>();
        playerLocomotion ??= GetComponentInParent<PlayerLocomotion>();
        torchController ??= GetComponentInParent<TorchController>();
    }
    #region  Animation Events

    public void PlayFootstepSound()
    {
        if (playerLocomotion == null || _audioSource == null) return;

        int type = playerLocomotion.SurfaceType;
        AudioClip clip = null;

        if (type == 1 && grassClips.Length > 0)
            clip = grassClips[Random.Range(0, grassClips.Length)];
        else if (type == 2 && woodClips.Length > 0)
            clip = woodClips[Random.Range(0, woodClips.Length)];
        if (clip != null)
            _audioSource.PlayOneShot(clip);
    }
    public void GrabTorch()
    {
        if (torchRenderer != null)
        {
            torchRenderer.TrySetPropertyValue("_BaseColor", baseTorchColor);
            torchRenderer.ApplyPropertiesToBlock();
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
}

