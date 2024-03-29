using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;


public class CharacterController2D : MonoBehaviour
{
    [HideInInspector] public Rigidbody2D rigidBody;

    [Header("Checks")]
    public LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform wallCheck;
    [SerializeField] private Transform attackCheck;
    public bool grounded;
    public bool walled;
    const float checkRadius = .2f;


    [Header("Movement")]
    public float speed = 5;
    [SerializeField] private float smoothing = 0.1f;
    public float deadZone = 0.1f;
    private Vector3 zero = Vector3.zero;


    [Header("Player")]
    public float hurtTimeout = 1f;
    public float deathTimeout = 3f;

    [Header("Attacks")]
    public float attackRadius = .3f;
    public float attackGravityCancel = 5f;

    [Header("PrimaryAttack")]
    public float primaryAttackDamage = 5f;
    public float primaryAttackTimeout = 0.3f;
    public float primaryAttackSpeed = 2f;

    [Header("SecondaryAttack")]
    public float secondaryAttackDamage = 10f;
    public float secondaryAttackTimeout = 1f;
    public float secondaryAttackSpeed = 1f;

    [Header("Jump")]
    public bool doubleJump;
    [SerializeField] private float jumpForce = 800f;
    [SerializeField] private float doubleJumpMultiplier = 0.8f;
    public float jumpTimeout = 0.35f;
    public float maxFallSpeed = 25f;

    [Header("Dash")]
    [SerializeField] private float dashForce = 50f;
    public float dashTimeout = 0.25f;
    public bool canDash;

    [Header("Inventory")]
    public Inventory inventory;


    [Header("Grapple")]
    public bool canGrapple = true;
    public float grappleRange = 10f;
    [SerializeField] private float grappleSpeed = 25f;
    [SerializeField] private float grappleDeadzone = 0.25f;
    [SerializeField] private float grappleCooldown = 3f;
    public float grappleTimeout = 0.3f;
    [Header("Interact")]
    public float interactDistance = 2;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        inventory = new Inventory(24);
    }

    private void FixedUpdate()
    {
        grounded = false;
        walled = false;

        CheckGrounded();
        CheckWall();
    }


    private void CheckGrounded()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(groundCheck.position, checkRadius, groundLayer);

        foreach (var collider in colliders)
        {
            if (collider.gameObject != gameObject)
            {
                grounded = true;
                doubleJump = true;
                break;
            }
        }
    }

    private void CheckWall()
    {
        if (!grounded)
        {
            Collider2D[] collidersWall = Physics2D.OverlapCircleAll(wallCheck.position, checkRadius, groundLayer);

            foreach (var collider in collidersWall)
            {
                if (collider.gameObject != null)
                {
                    walled = true;
                    break;
                }
            }
        }
    }

    public void Move(float move)
    {
        move *= speed * 100 * Time.fixedDeltaTime;
        Vector3 targetVelocity = new Vector2(move, rigidBody.velocity.y);
        rigidBody.velocity = Vector3.SmoothDamp(rigidBody.velocity, targetVelocity, ref zero, smoothing);
        Flip(move);
    }

    public void Dash()
    {
        canDash = false;
        rigidBody.velocity = new Vector2(transform.localScale.x * dashForce, 0);
    }

    public void Jump(bool isDoubleJump = false, bool isWallJump = false)
    {
        rigidBody.velocity = new Vector2(rigidBody.velocity.x, 0f); // Reset vertical velocity

        float horizontalForce = (isWallJump) ? -transform.localScale.x * jumpForce * 1.2f : 0f;
        float verticalForce = jumpForce * (isDoubleJump ? doubleJumpMultiplier : 1f);

        rigidBody.AddForce(new Vector2(horizontalForce, verticalForce));
    }

    public void Flip(float move)
    {
        if (move == 0) return;
        if (Mathf.Sign(transform.localScale.x) != Mathf.Sign(move))
            transform.localScale = new Vector3(Mathf.Sign(move), 1, 1);
    }

    public void Attack(float damageAmount)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(attackCheck.position, attackRadius);
        for (int i = 0; i < colliders.Length; i++)
            if (colliders[i].gameObject != gameObject)
            {
                if (colliders[i].gameObject.tag == "Enemy")
                {
                    StartCoroutine(FlashEffect(colliders[i].gameObject));
                }
                //StartCoroutine(FlashEffect(colliders[i].gameObject));
                colliders[i].GetComponent<IDamageable>()?.Damage(damageAmount);
            }                
    }

    public void GrapplePull(Vector2 targetPosition)
    {
        Vector2 direction = (targetPosition - (Vector2)transform.position);

        if (Vector2.Distance(transform.position, targetPosition) > grappleDeadzone)
            rigidBody.MovePosition((Vector2)transform.position + (grappleSpeed * Time.deltaTime * direction.normalized));
    }

    public void StartGrappleCooldown()
    {
        StartCoroutine(GrappleCooldown());
    }

    private IEnumerator GrappleCooldown()
    {
        yield return new WaitForSeconds(grappleCooldown);
        canGrapple = true;
    }

    public void Interact()
    {
        Collider2D[] nearbyObjects = Physics2D.OverlapCircleAll(transform.position, interactDistance);

        foreach (Collider2D col in nearbyObjects)
        {
            InteractableInterface interactableInterface = col.transform.GetComponent<InteractableInterface>();
            interactableInterface?.Interact();

            if (interactableInterface != null)
            {
                return;
            }
        }
    }

    private IEnumerator FlashEffect(GameObject enemy)
    {
        // Assuming your enemy has a SpriteRenderer component
        SpriteRenderer enemyRenderer = enemy.GetComponent<SpriteRenderer>();

        // Store the original color of the enemy
        Color originalColor = enemyRenderer.color;

        // Set the enemy's color to red for a brief period
        enemyRenderer.color = Color.red;

        // Wait for a short duration
        yield return new WaitForSeconds(0.1f); // Adjust the duration as needed

        // Restore the original color of the enemy
        enemyRenderer.color = originalColor;
    }
}
