public interface IInteractable
{
    void OnInteract(); 
    void OnInteractHold(bool isHolding);
    bool RequiresHoldToInteract();
    float HoldDuration();
}