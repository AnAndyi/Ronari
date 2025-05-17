using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchingDirections : MonoBehaviour
{
    public ContactFilter2D castFilter;
    public float groundDistance = 0.05f;
    public float wallDistance = 0.2f;
    public float ceilingDistance = 0.05f;

    private CapsuleCollider2D touchingCol; // Default collider for this GameObject
    private PlayerController playerController; // Optional reference for player-specific logic
    Animator animator;

    RaycastHit2D[] groundHits = new RaycastHit2D[5];
    RaycastHit2D[] wallHits = new RaycastHit2D[5];
    RaycastHit2D[] ceilingHits = new RaycastHit2D[5];

    [SerializeField]
    private bool _isGrounded;
    public bool IsGrounded
    {
        get
        {
            return _isGrounded;
        }
        private set {
            _isGrounded = value;
            animator.SetBool(AnimationStrings.isGrounded, value);
        }
    }

    [SerializeField]
    private bool _isOnWall;
    public bool IsOnWall
    {
        get
        {
            return _isOnWall;
        }
        private set
        {
            _isOnWall = value;
            animator.SetBool(AnimationStrings.isOnWall, value);
        }
    }
    [SerializeField]
    private bool _isOnCeiling;
    private Vector2 wallCheckDirection => gameObject.transform.localScale.x > 0 ? Vector2.right :Vector2.left;

    public bool IsOnCeiling
    {
        get
        {
            return _isOnCeiling;
        }
        private set
        {
            _isOnCeiling = value;
            animator.SetBool(AnimationStrings.isOnCeiling, value);
        }
    }
    private void Awake()
    {
        touchingCol = GetComponent<CapsuleCollider2D>();
        playerController = GetComponent<PlayerController>(); // Only present on player
        animator = GetComponent<Animator>();
        if (touchingCol == null)
        {
            Debug.LogError($"No CapsuleCollider2D found on {gameObject.name}!");
        }
    }
    
    void FixedUpdate()
    {
        CapsuleCollider2D activeCollider = touchingCol; // Default to local collider

        // If this is the player, use the active collider from PlayerController
        if (playerController != null && playerController.ActiveCollider != null)
        {
            activeCollider = playerController.ActiveCollider;
        }

        if (activeCollider != null)
        {
            IsGrounded = activeCollider.Cast(Vector2.down, castFilter, groundHits, groundDistance) > 0;
            IsOnWall = activeCollider.Cast(wallCheckDirection, castFilter, wallHits, wallDistance) > 0;
            IsOnCeiling = activeCollider.Cast(Vector2.up, castFilter, ceilingHits, ceilingDistance) > 0;
        }
        else
        {
            Debug.LogWarning($"No active collider available for {gameObject.name}! Falling back to IsGrounded = false.");
            IsGrounded = false;
            IsOnWall = false;
            IsOnCeiling = false;
        }
    }
}

