using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField, Min(0f)] private float moveSpeed = 5f;
    private Rigidbody2D body;
    private Vector2 direction;

    private void Awake() => body = GetComponent<Rigidbody2D>();

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        direction = Vector2.zero;
        if (keyboard == null) return;

        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) direction.y += 1f;
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) direction.y -= 1f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) direction.x -= 1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) direction.x += 1f;

        // Keep diagonal movement the same speed as horizontal/vertical movement.
        direction = direction.normalized;
    }

    private void FixedUpdate()
    {
        body.MovePosition(body.position + direction * moveSpeed * Time.fixedDeltaTime);
    }
}
