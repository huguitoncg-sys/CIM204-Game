using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
public class TriangleEnemy : MonoBehaviour
{
    private HealthHUD hud;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<PlayerMovement>() == null) return;
        // Resolve the scene HUD at runtime so every prefab instance works.
        if (hud == null) hud = FindFirstObjectByType<HealthHUD>();
        if (hud != null) hud.TakeDamage();
        else Debug.LogWarning("Triangle Enemy needs a HealthHUD in the scene.", this);
    }
}
