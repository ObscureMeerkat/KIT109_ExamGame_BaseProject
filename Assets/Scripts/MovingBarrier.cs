using UnityEngine;

// Moves a barrier back and forth along a fixed axis around its starting position.
// Uses a Kinematic Rigidbody2D + MovePosition so the motion is part of the
// physics step and projectiles bounce off it correctly.
// Set the Rigidbody2D's Body Type to Kinematic in the Inspector.
[RequireComponent(typeof(Rigidbody2D))]
public class MovingBarrier : MonoBehaviour
{
    [SerializeField] Vector2 moveDirection = Vector2.up;  // axis to travel along
    [SerializeField] float distance = 3f;                 // how far from the start point each way
    [SerializeField] float speed = 2f;

    Rigidbody2D rb;
    Vector2 startPos;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        startPos = rb.position;   // captured fresh on every scene load, so reload resets it
    }

    void FixedUpdate()
    {
        // Smooth ping-pong around the start position.
        float offset = Mathf.Sin(Time.time * speed) * distance;
        Vector2 target = startPos + moveDirection.normalized * offset;
        rb.MovePosition(target);
    }
}