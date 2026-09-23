using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerMelee : MonoBehaviour
{
    [SerializeField, Min(0.1f)] private float attackRadius = 1.9f;
    [SerializeField, Min(0.05f)] private float attackCooldown = 0.35f;
    [SerializeField, Min(0.02f)] private float visualDuration = 0.16f;
    private readonly List<Collider2D> overlaps = new List<Collider2D>(16);
    private readonly HashSet<TriangleEnemy> hitEnemies = new HashSet<TriangleEnemy>();
    private PlayerMovement movement;
    private Transform swing;
    private PolygonCollider2D hitShape;
    private MeshRenderer visual;
    private Mesh mesh;
    private Material material;
    private float readyAt, hideAt;
    private Vector2 facing = Vector2.right;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        GameObject effect = new GameObject("Melee Half Circle");
        swing = effect.transform;
        swing.SetParent(transform, false);
        const int segments = 32;
        Vector2[] outline = new Vector2[segments + 2];
        Vector3[] vertices = new Vector3[segments + 2];
        Color[] colors = new Color[vertices.Length];
        int[] triangles = new int[segments * 3];
        outline[0] = Vector2.zero;
        for (int i = 0; i <= segments; i++)
        {
            float angle = (-90f + 180f * i / segments) * Mathf.Deg2Rad;
            outline[i + 1] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            vertices[i + 1] = outline[i + 1];
        }
        for (int i = 0; i < vertices.Length; i++) colors[i] = new Color(1f, 0.85f, 0.3f, 0.42f);
        for (int i = 0; i < segments; i++)
        {
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = i + 2;
        }
        mesh = new Mesh { name = "Melee semicircle" };
        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        effect.AddComponent<MeshFilter>().sharedMesh = mesh;
        visual = effect.AddComponent<MeshRenderer>();
        material = new Material(Shader.Find("Sprites/Default"));
        visual.sharedMaterial = material;
        SpriteRenderer playerSprite = GetComponent<SpriteRenderer>();
        if (playerSprite != null)
        {
            visual.sortingLayerID = playerSprite.sortingLayerID;
            visual.sortingOrder = playerSprite.sortingOrder + 1;
        }
        hitShape = effect.AddComponent<PolygonCollider2D>();
        hitShape.isTrigger = true;
        hitShape.SetPath(0, outline);
        hitShape.enabled = false;
        visual.enabled = false;
    }

    private void Update()
    {
        if (visual.enabled && Time.time >= hideAt) visual.enabled = false;
        if (!movement.enabled) return; // Grid puzzle owns the controls while active.
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;
        bool left = keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed;
        bool right = keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed;
        bool up = keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed;
        bool down = keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed;
        // A newly pressed direction wins when moving diagonally.
        if (up && !down && (keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame)) facing = Vector2.up;
        else if (down && !up && (keyboard.sKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame)) facing = Vector2.down;
        else if (left && !right && (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame)) facing = Vector2.left;
        else if (right && !left && (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)) facing = Vector2.right;
        else if (left != right && up == down) facing = right ? Vector2.right : Vector2.left;
        else if (up != down && left == right) facing = up ? Vector2.up : Vector2.down;
        if (keyboard.spaceKey.wasPressedThisFrame && Time.time >= readyAt) Attack();
    }

    private void Attack()
    {
        readyAt = Time.time + attackCooldown;
        hideAt = Time.time + visualDuration;
        swing.localRotation = Quaternion.Euler(0, 0, Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg);
        // Keep radius in world units even if the player has been resized.
        swing.localScale = new Vector3(attackRadius / Mathf.Abs(transform.lossyScale.x), attackRadius / Mathf.Abs(transform.lossyScale.y), 1);
        visual.enabled = true;
        hitShape.enabled = true;
        Physics2D.SyncTransforms();
        ContactFilter2D filter = new ContactFilter2D();
        filter.NoFilter();
        filter.useTriggers = true;
        overlaps.Clear();
        hitShape.Overlap(filter, overlaps);
        hitShape.enabled = false;
        hitEnemies.Clear();
        foreach (Collider2D collider in overlaps)
        {
            TriangleEnemy enemy = collider.GetComponentInParent<TriangleEnemy>();
            if (enemy != null && hitEnemies.Add(enemy)) enemy.TakeDamage(1);
        }
    }

    private void OnDisable()
    {
        if (visual != null) visual.enabled = false;
        if (hitShape != null) hitShape.enabled = false;
    }

    private void OnDestroy()
    {
        if (mesh != null) Destroy(mesh);
        if (material != null) Destroy(material);
    }
}


