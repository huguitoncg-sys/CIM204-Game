using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(PlayerMovement))]
public class PlayerCharacterAnimation : MonoBehaviour
{
    [SerializeField] private Sprite[] frames;
    [SerializeField, Min(1f)] private float framesPerSecond = 8f;
    [SerializeField, Min(0.1f)] private float forwardIdleFramesPerSecond = 3f;
    [SerializeField] private Sprite[] swingDown, swingLeft, swingUp, swingRight;
    private Sprite[] activeSwing;
    private float swingStarted, swingDuration;
    public void ConfigureSwings(Sprite[] down, Sprite[] left, Sprite[] up, Sprite[] right)
    { swingDown = down; swingLeft = left; swingUp = up; swingRight = right; }
    public bool PlaySwing(Vector2 direction, float duration)
    {
        activeSwing = direction == Vector2.left ? swingLeft : direction == Vector2.right ? swingRight :
            direction == Vector2.up ? swingUp : swingDown;
        if (activeSwing == null || activeSwing.Length == 0) return false;
        facing = direction;
        swingStarted = Time.time;
        swingDuration = Mathf.Max(0.1f, duration);
        previousSequence = -1;
        return true;
    }
    private SpriteRenderer visual;
    private PlayerMovement movement;
    private PlayerMelee melee;
    private Vector3 previousPosition;
    private Vector2 facing = Vector2.down;
    private float clock, stepUntil;
    private int previousSequence = -1;

    public void Configure(Sprite[] sprites)
    {
        frames = sprites;
        var renderer = GetComponent<SpriteRenderer>();
        renderer.sprite = frames[0];
        renderer.color = Color.white;
        renderer.flipX = renderer.flipY = false;
    }

    private void Awake()
    {
        visual = GetComponent<SpriteRenderer>();
        movement = GetComponent<PlayerMovement>();
        melee = GetComponent<PlayerMelee>();
        previousPosition = transform.position;
    }

    private void LateUpdate()
    {
        if (frames == null || frames.Length < 19) return;
        Vector2 delta = transform.position - previousPosition;
        previousPosition = transform.position;
        if (Time.timeScale == 0f) return;
        bool moved = delta.sqrMagnitude > 0.000001f;
        if (moved) stepUntil = Time.time + 0.14f;
        if (activeSwing != null && activeSwing.Length > 0)
        {
            float elapsed = Time.time - swingStarted;
            if (elapsed < swingDuration)
            {
                visual.sprite = activeSwing[Mathf.Min(activeSwing.Length - 1, (int)(elapsed / swingDuration * activeSwing.Length))];
                return;
            }
            activeSwing = null;
        }
        if (movement.enabled && melee != null)
            facing = melee.Facing;
        else if (moved)
            facing = Mathf.Abs(delta.x) > Mathf.Abs(delta.y)
                ? (delta.x > 0 ? Vector2.right : Vector2.left)
                : (delta.y > 0 ? Vector2.up : Vector2.down);
        if (moved) stepUntil = Time.time + 0.14f;
        bool walking = Time.time < stepUntil;
        int start, count;
        if (facing == Vector2.left) { start = 6; count = walking ? 5 : 1; }
        else if (facing == Vector2.right) { start = 14; count = walking ? 5 : 1; }
        else if (facing == Vector2.up) { start = 11; count = walking ? 3 : 1; }
        else { start = walking ? 3 : 0; count = 3; }
        int sequence = start * 10 + count;
        if (sequence != previousSequence) { clock = 0; previousSequence = sequence; }
        float playbackRate = !walking && facing == Vector2.down ? forwardIdleFramesPerSecond : framesPerSecond;
        visual.sprite = frames[start + (int)(clock * playbackRate) % count];
        clock += Time.deltaTime;
    }
}




