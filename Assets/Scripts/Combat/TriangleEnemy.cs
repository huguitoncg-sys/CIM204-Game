using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
public class TriangleEnemy : MonoBehaviour
{
    [Header("Patrol and Detection")]
    [SerializeField, Min(0f)] private float patrolDistance = 2f;
    [SerializeField, Min(0f)] private float patrolSpeed = 1.5f;
    [SerializeField, Min(0f)] private float chaseSpeed = 2.5f;
    [SerializeField, Min(0.1f)] private float detectionRadius = 3f;
    [Header("Hit Reaction")]
    [SerializeField, Min(0f)] private float stunDuration = 0.35f;
    [SerializeField, Min(0f)] private float knockbackSpeed = 8f;
    [SerializeField, Min(0.1f)] private float knockbackDeceleration = 32f;
    private float stunnedUntil;
    private Vector2 knockbackVelocity;
    private readonly System.Collections.Generic.List<RaycastHit2D> knockbackHits = new System.Collections.Generic.List<RaycastHit2D>();
    private HealthHUD hud;
    private int hitPoints = 3;
    private bool dead, chasing, returning;
    public bool IsDefeated => dead;
    private Rigidbody2D body;
    private PlayerMovement player;
    private SpriteRenderer homeRoom;
    private ReferenceDungeonMap dungeon;
    private Vector2 home;
    private int patrolDirection = 1;
    private LineRenderer ring;
    private Material ringMaterial;
    private readonly Vector3[] ringPoints = new Vector3[64];
    private Vector2 colliderExtents;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        if (body == null) body = gameObject.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.useFullKinematicContacts = true;
        home = body.position;
        colliderExtents = GetComponent<PolygonCollider2D>().bounds.extents;
        GameObject circle = new GameObject("Detection Circle");
        circle.transform.SetParent(transform, false);
        ring = circle.AddComponent<LineRenderer>();
        ring.useWorldSpace = true;
        ring.loop = true;
        ring.positionCount = ringPoints.Length;
        ring.widthMultiplier = 0.035f;
        ringMaterial = new Material(Shader.Find("Sprites/Default"));
        ring.sharedMaterial = ringMaterial;
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            ring.sortingLayerID = sprite.sortingLayerID;
            ring.sortingOrder = sprite.sortingOrder + 1;
        }
    }

    private void Start()
    {
        player = FindFirstObjectByType<PlayerMovement>();
        dungeon = FindFirstObjectByType<ReferenceDungeonMap>();
        foreach (SpriteRenderer candidate in FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None))
        {
            if (candidate.gameObject.scene != gameObject.scene || !candidate.name.StartsWith("Background Rectangle")) continue;
            if (Contains(candidate.bounds, home)) { homeRoom = candidate; break; }
        }
    }

    private static bool Contains(Bounds bounds, Vector2 point)
    {
        return point.x >= bounds.min.x && point.x <= bounds.max.x && point.y >= bounds.min.y && point.y <= bounds.max.y;
    }

    private Vector2 KeepInsideRoom(Vector2 point)
    {
        if (homeRoom == null) return point;
        Bounds bounds = homeRoom.bounds;
        point.x = Mathf.Clamp(point.x, bounds.min.x + colliderExtents.x, bounds.max.x - colliderExtents.x);
        point.y = Mathf.Clamp(point.y, bounds.min.y + colliderExtents.y, bounds.max.y - colliderExtents.y);
        return point;
    }

    private void FixedUpdate()
    {
        if (dead) return;
        if (Time.time < stunnedUntil) { UpdateKnockback(); return; }
        Vector2 position = body.position;
        bool canChase = player != null && player.gameObject.activeInHierarchy &&
            ((Vector2)player.transform.position - position).sqrMagnitude <= detectionRadius * detectionRadius &&
            (homeRoom == null || Contains(homeRoom.bounds, player.transform.position));
        if (chasing && !canChase) returning = true;
        chasing = canChase;
        Vector2 target;
        float speed;
        if (chasing)
        {
            target = player.transform.position;
            speed = chaseSpeed;
        }
        else if (returning)
        {
            target = KeepInsideRoom(home);
            speed = patrolSpeed;
            if (Vector2.Distance(position, target) <= 0.05f) returning = false;
        }
        else
        {
            target = KeepInsideRoom(home + Vector2.right * patrolDirection * patrolDistance);
            speed = patrolSpeed;
            if (Vector2.Distance(position, target) <= 0.05f)
            {
                patrolDirection *= -1;
                target = KeepInsideRoom(home + Vector2.right * patrolDirection * patrolDistance);
            }
        }
                Vector2 next = KeepInsideRoom(Vector2.MoveTowards(position, target, speed * Time.fixedDeltaTime));
        if (dungeon != null && !dungeon.Walkable(next, colliderExtents))
        {
            Vector2 horizontal = new Vector2(next.x, position.y);
            Vector2 vertical = new Vector2(position.x, next.y);
            if (horizontal != position && dungeon.Walkable(horizontal, colliderExtents)) next = horizontal;
            else if (vertical != position && dungeon.Walkable(vertical, colliderExtents)) next = vertical;
            else { next = position; if (!chasing) { returning = false; patrolDirection *= -1; } }
        }
        body.MovePosition(next);
    }

    private void LateUpdate()
    {
        Color color = chasing ? new Color(1f, 0.22f, 0.15f, 0.75f) : new Color(1f, 0.65f, 0.3f, 0.45f);
        ring.startColor = ring.endColor = color;
        for (int i = 0; i < ringPoints.Length; i++)
        {
            float angle = i * Mathf.PI * 2f / ringPoints.Length;
            ringPoints[i] = transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * detectionRadius;
        }
        ring.SetPositions(ringPoints);
    }

    private void UpdateKnockback()
    {
        Vector2 position = body.position;
        Vector2 delta = knockbackVelocity * Time.fixedDeltaTime;
        knockbackVelocity = Vector2.MoveTowards(knockbackVelocity, Vector2.zero, knockbackDeceleration * Time.fixedDeltaTime);
        float distance = delta.magnitude;
        if (distance <= 0.0001f) return;
        var filter = ContactFilter2D.noFilter;
        filter.useTriggers = false;
        knockbackHits.Clear();
        Physics2D.BoxCast(position, colliderExtents * 2f, 0f, delta.normalized, filter, knockbackHits, distance);
        foreach (var hit in knockbackHits)
        {
            if (hit.collider == null || hit.collider.attachedRigidbody == body ||
                hit.collider.GetComponentInParent<PlayerMovement>() != null ||
                hit.collider.GetComponentInParent<TriangleEnemy>() != null) continue;
            distance = Mathf.Min(distance, Mathf.Max(0f, hit.distance - 0.02f));
        }
        Vector2 target = KeepInsideRoom(position + delta.normalized * distance);
        // Small steps also guard against crossing a narrow map wall at low frame rates.
        int steps = Mathf.Max(1, Mathf.CeilToInt(Vector2.Distance(position, target) / 0.1f));
        Vector2 next = position;
        for (int i = 1; i <= steps; i++)
        {
            Vector2 sample = Vector2.Lerp(position, target, (float)i / steps);
            if (dungeon != null && !dungeon.Walkable(sample, colliderExtents)) break;
            next = sample;
        }
        if ((next - position).sqrMagnitude < 0.000001f) knockbackVelocity = Vector2.zero;
        body.MovePosition(next);
    }

    public void TakeDamage(int damage)
    {
        Vector2 direction = player != null ? (Vector2)(transform.position - player.transform.position) : Vector2.down;
        TakeDamage(damage, direction);
    }

    public void TakeDamage(int damage, Vector2 hitDirection)
    {
        if (dead || damage <= 0) return;
        stunnedUntil = Time.time + stunDuration;
        knockbackVelocity = (hitDirection.sqrMagnitude > 0.0001f ? hitDirection.normalized : Vector2.down) * knockbackSpeed;
        body.linearVelocity = Vector2.zero;
        hitPoints = Mathf.Max(0, hitPoints - damage);
        if (hitPoints == 0)
        {
            dead = true;
            var inventory = player != null ? player.GetComponent<PlayerBombs>() : FindAnyObjectByType<PlayerBombs>();
            bool dropHeart = Random.Range(0, 3) == 0;
            bool dropBomb = inventory != null && inventory.IsUnlocked && Random.Range(0, 3) == 0;
            // Separate simultaneous drops visually while keeping both within pickup reach.
            if (dropHeart) HeartPickup.Spawn(transform.position, dropBomb ? 0.4f : 0f);
            if (dropBomb) BombPickup.Spawn(transform.position, inventory.circleSprite);
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (dead || Time.time < stunnedUntil || other.GetComponentInParent<PlayerMovement>() == null) return;
        if (hud == null) hud = FindFirstObjectByType<HealthHUD>();
        if (hud != null) hud.TakeDamage();
        else Debug.LogWarning("Triangle Enemy needs a HealthHUD in the scene.", this);
    }

    private void OnDestroy()
    {
        if (ringMaterial != null) Destroy(ringMaterial);
    }
}








