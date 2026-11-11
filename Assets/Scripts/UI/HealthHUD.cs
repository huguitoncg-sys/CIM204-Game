using UnityEngine;
using UnityEngine.UI;

public class HealthHUD : MonoBehaviour
{
    [SerializeField] private Text label;
    private int health = 3;
    public int Health => health;

    public void SetLabel(Text value) { label = value; Refresh(); }
    private void Awake() { health = 3; Refresh(); }
    public void TakeDamage()
    {
        if (health == 0) return;
        health = Mathf.Max(0, health - 1);
        var player = FindAnyObjectByType<PlayerMovement>();
        if (player != null) DamageFlash.Play(player.gameObject);
        Refresh();
    }
    private void LateUpdate()
    {
        if (health != 0 || GameOverPopup.IsOpen) return;
        var popup = GetComponent<GameOverPopup>();
        if (popup == null) popup = gameObject.AddComponent<GameOverPopup>();
        popup.Show(label != null ? label.font : null);
    }
    public bool RestoreHealth(int amount = 1)
    {
        if (amount <= 0 || health <= 0 || health >= 3) return false;
        health = Mathf.Min(3, health + amount);
        Refresh();
        return true;
    }
    private void Refresh()
    {
        if (label != null) label.text = "Health: " + health;
    }
}



