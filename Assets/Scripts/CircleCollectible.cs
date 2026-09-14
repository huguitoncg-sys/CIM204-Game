using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class CircleCollectible : MonoBehaviour
{
    [SerializeField] private CollectibleHUD hud;
    private bool collected;

    public void SetHUD(CollectibleHUD value) => hud = value;

    private void Awake()
    {
        // Prefab assets cannot store references to a HUD in a scene.
        if (hud == null) hud = FindFirstObjectByType<CollectibleHUD>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || other.GetComponentInParent<PlayerMovement>() == null) return;
        // Retry in case the HUD was created after this coin.
        if (hud == null) hud = FindFirstObjectByType<CollectibleHUD>();
        if (hud == null)
        {
            Debug.LogWarning("Cannot collect coin: the scene needs a CollectibleHUD component.", this);
            return;
        }
        collected = true;
        hud.AddCollectible();
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
