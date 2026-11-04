using UnityEngine;
using UnityEngine.InputSystem;

public interface IPlayerInteraction
{
    void Interact();
}

public class PlayerInteraction : MonoBehaviour
{
    public IPlayerInteraction Target { get; set; }
    private void LateUpdate()
    {
        if (FirstRoomEncounter.BlocksGameplayInput) return;
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            Target?.Interact();
    }
}

