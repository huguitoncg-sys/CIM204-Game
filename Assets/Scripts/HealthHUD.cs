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
        health = Mathf.Max(0, health - 1);
        Refresh();
    }
    private void Refresh()
    {
        if (label != null) label.text = "Health: " + health;
    }
}
