using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
public class TriangleEnemy : MonoBehaviour
{
    [Header("Patrol and Detection")]
    [SerializeField, Min(0f)] private float patrolDistance = 2f;
    [SerializeField, Min(0f)] private float patrolSpeed = 1.5f;
    [SerializeField, Min(0f)] private float chaseSpeed = 2.5f;
    [SerializeField, Min(0.1f)] private float detectionRadius = 3f;
    private HealthHUD hud;
    private int hitPoints = 3;
    private bool dead, chasing, returning;
    private Rigidbody2D body;
    private PlayerMovement player;
    private SpriteRenderer homeRoom;
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
        body.MovePosition(KeepInsideRoom(Vector2.MoveTowards(position, target, speed * Time.fixedDeltaTime)));
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

    public void TakeDamage(int damage)
    {
        if (dead || damage <= 0) return;
        hitPoints = Mathf.Max(0, hitPoints - damage);
        if (hitPoints == 0)
        {
            dead = true;
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (dead || other.GetComponentInParent<PlayerMovement>() == null) return;
        if (hud == null) hud = FindFirstObjectByType<HealthHUD>();
        if (hud != null) hud.TakeDamage();
        else Debug.LogWarning("Triangle Enemy needs a HealthHUD in the scene.", this);
    }

    private void OnDestroy()
    {
        if (ringMaterial != null) Destroy(ringMaterial);
    }
}


