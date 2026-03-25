using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed = 5f;

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 movement;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        Debug.Log("Update running");
        movement.x = Keyboard.current.dKey.isPressed ? 1 :
                     Keyboard.current.aKey.isPressed ? -1 : 0;
        movement.y = Keyboard.current.wKey.isPressed ? 1 :
                     Keyboard.current.sKey.isPressed ? -1 : 0;
        Debug.Log(movement);
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement.normalized * speed * Time.fixedDeltaTime);
        UpdateAnimation();
    }

    void UpdateAnimation()
    {
        bool isMoving = movement != Vector2.zero;
        animator.SetBool("isMoving", isMoving);
        if (!isMoving) return;
        animator.SetFloat("moveX", movement.x);
        animator.SetFloat("moveY", movement.y);
    }
}