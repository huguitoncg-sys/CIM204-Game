using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SlugAnimation : MonoBehaviour
{
    [SerializeField] private Sprite[] frames;
    [SerializeField, Min(1)] private float framesPerSecond = 8f;
    private SpriteRenderer visual;
    private Vector3 previous;
    private float elapsed;
    public void Configure(Sprite[] value)
    {
        frames = value;
        var renderer = GetComponent<SpriteRenderer>();
        renderer.sprite = frames[0];
        renderer.color = Color.white;
        renderer.flipX = false;
    }
    private void Awake() { visual = GetComponent<SpriteRenderer>(); previous = transform.position; }
    private void LateUpdate()
    {
        Vector3 delta = transform.position - previous;
        previous = transform.position;
        if (Time.timeScale <= 0 || frames == null || frames.Length == 0) return;
        if (Mathf.Abs(delta.x) > 0.001f) visual.flipX = delta.x < 0;
        elapsed += Time.deltaTime;
        visual.sprite = frames[(int)(elapsed * framesPerSecond) % frames.Length];
    }
}
