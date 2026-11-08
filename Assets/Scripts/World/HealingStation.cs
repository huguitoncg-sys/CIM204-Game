using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class HealingStation : MonoBehaviour
{
    private HealthHUD health;
    private void Awake()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
        health = FindAnyObjectByType<HealthHUD>();
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (Time.timeScale <= 0 || GameOverPopup.IsOpen ||
            other.GetComponentInParent<PlayerMovement>() == null) return;
        if (health == null) health = FindAnyObjectByType<HealthHUD>();
        if (health != null) health.RestoreHealth(3);
    }
}
