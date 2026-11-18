using UnityEngine;

public class DamageFlash : MonoBehaviour
{
    private SpriteRenderer visual;
    private Color normalColor;
    private float endsAt;
    private bool flashing, transient;
    public bool IsFlashing => flashing;
    public static void Play(GameObject target, bool finalHit = false)
    {
        var source = target.GetComponent<SpriteRenderer>();
        if (source == null) return;
        if (finalHit)
        {
            // Keep a brief visual on lethal hits without delaying death, rewards or collision removal.
            var echo = new GameObject("Damage Flash");
            echo.transform.SetPositionAndRotation(source.transform.position, source.transform.rotation);
            echo.transform.localScale = source.transform.lossyScale;
            var renderer = echo.AddComponent<SpriteRenderer>();
            renderer.sprite = source.sprite;
            renderer.sharedMaterial = source.sharedMaterial;
            renderer.sortingLayerID = source.sortingLayerID;
            renderer.sortingOrder = source.sortingOrder;
            renderer.flipX = source.flipX;
            renderer.flipY = source.flipY;
            renderer.color = Color.white;
            var flash = echo.AddComponent<DamageFlash>();
            flash.transient = true;
            flash.Begin(renderer);
            return;
        }
        var effect = target.GetComponent<DamageFlash>();
        if (effect == null) effect = target.AddComponent<DamageFlash>();
        effect.Begin(source);
    }
    private void Begin(SpriteRenderer renderer)
    {
        visual = renderer;
        if (!flashing) normalColor = renderer.color;
        flashing = true;
        endsAt = Time.unscaledTime + 0.14f;
        visual.color = new Color(1f, 0.12f, 0.12f, normalColor.a);
    }
    private void LateUpdate()
    {
        if (!flashing || Time.unscaledTime < endsAt) return;
        if (transient) { Destroy(gameObject); return; }
        Restore();
    }
    public void Cancel() => Restore();
    private void Restore()
    {
        if (flashing && visual != null) visual.color = normalColor;
        flashing = false;
    }
    private void OnDisable() => Restore();
}


